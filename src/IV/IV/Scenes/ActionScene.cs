using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using IV.Achievement;
using IV.Action_Scene;
using IV.Menu_Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using File = System.IO.File;

namespace IV.Scenes
{
    public class ActionScene : GameScene
    {
        private SpriteBatch sBatch;

        private Level level;
        private int levelIndex;
        private Model IVModel;
        string levelPath;
        private readonly ContentManager Content;

        Thread loadingContentThread;


        #region FPS calculation variables

        private double FPSlastTime;
        private double FPStotalSinceLast;
        private double FPStoDisplay;
        private int FPStotalFramesSinceLast;

        #endregion

        private SpriteFont font;
        private TimeSpan timeToDeath;

        private Texture2D loadingScreen;
        private Texture2D storyTexture;
        private bool isStoryShowing;

        private KeyboardState oldState;

        private List<Song> songs;
        private int songIndex;
        private readonly Random random;

        private readonly bool levelSelection;
        private PauseScene pauseScene;
        private bool gamePaused;
        public event EventHandler OnBackToMenu;
        public event EventHandler OnEndGame;

        private readonly SoundManager soundManager;
        private ObjectivesManager objectivesManager;

        //djmpost
        private RenderTarget2D PP_guisanteBlure;
        private Texture2D original_menu_output;
        private Effect pp_blure;
        //----

        private readonly GameSettings gameSettings;

        //In game story
        private StoryScene storyScene;

        public ActionScene(Game game, ContentManager content,GameSettings gameSettings,int levelIndex, bool levelSelection)
            : base(game)
        {
            Content = content;
            this.gameSettings = gameSettings;
            this.levelSelection = levelSelection;
            random = new Random();
            this.levelIndex = levelIndex;

            this.gameSettings = gameSettings;

            soundManager = (SoundManager) Game.Services.GetService(typeof (SoundManager));
        }

        void LoadTexture()
        {
            loadingScreen = Content.Load<Texture2D>("Textures\\Loading");
            storyTexture = Content.Load<Texture2D>("Story\\STORY_4");
        }

        void LoadTheContent()
        {
            IVModel = Content.Load<Model>("Models\\IV");
            font = Content.Load<SpriteFont>("Fonts\\font");
            objectivesManager = new ObjectivesManager(Content);
            pauseScene = new PauseScene(Content, soundManager,objectivesManager);
            pauseScene.OnGameExit += ExitGame;
            pauseScene.OnResumeGame += ResumeGame;
            pauseScene.OnMainMenu += MainMenu;
            pauseScene.OnResetLevel += ResetLevel;
            pauseScene.OnLoadCheckPoint += LoadCheckPoint;

            songs = new List<Song>
                        {
                            Content.Load<Song>("Audio\\Music\\BLiZZARD"),
                            Content.Load<Song>("Audio\\Music\\CRUDE"),
                            Content.Load<Song>("Audio\\Music\\DancyJM"),
                            Content.Load<Song>("Audio\\Music\\DYNAMITE"),
                            Content.Load<Song>("Audio\\Music\\Sitges Savepoint"),
                            Content.Load<Song>("Audio\\Music\\Spontaneous Devotion")
                        };
            songIndex = random.Next(6);
            

            //postprocess ----
            pp_blure = Content.Load<Effect>("Shaders\\GAME_blur");

            //------------------


            storyScene = new StoryScene(Game,1);
            storyScene.LoadContent(Content);
            storyScene.OnStartingGame += (s, e) => isStoryShowing = false;
        }

        private void LoadCheckPoint(object sender, EventArgs e)
        {
            if(!level.CanShowMenu) return;
            level.LoadCheckPoint();
            gamePaused = false;            
        }

        private void ResetLevel(object sender, EventArgs e)
        {
            gamePaused = false;
            levelIndex--;
            LoadLevel();
        }

        private void MainMenu(object sender, EventArgs e)
        {
            if (OnBackToMenu != null)
                OnBackToMenu(this, EventArgs.Empty);
        }

        private void ResumeGame(object sender, EventArgs e)
        {
            gamePaused = false;
        }

        private void ExitGame(object sender, EventArgs e)
        {
            Game.Exit();
        }

        public override void Initialize()
        {
            LoadTexture();
            sBatch = (SpriteBatch) Game.Services.GetService(typeof (SpriteBatch));
            //levelIndex = 0;
            loadingContentThread = new Thread(new ThreadStart(
                                                      delegate
                                                      {
                                                          LoadTheContent();
                                                          LoadNextLevel();
                                                          loadingContentThread = null;
                                                          MediaPlayer.Play(songs[songIndex]);
                                                      }))
            {
                Name = "Next Thread",
                Priority = ThreadPriority.Normal
            };

            loadingContentThread.Start();

            base.Initialize();

            PP_guisanteBlure = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                                                  GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color,
                                                  DepthFormat.Depth24);
        }

        private void LoadNextLevel()
        {
            levelPath = string.Empty;

            while (true)
            {
                levelPath = String.Format(@"Content/Levels/{0}.xml", ++levelIndex);
                if (File.Exists(levelPath))
                    break;

                if (levelIndex == 1)
                    throw new Exception("No levels found.");

                levelIndex = 0;
            }

            if (level != null)
                level.Dispose();
            level = null;
            GC.Collect();
            Components.Clear();
            Components = new List<GameComponent>();

            level = new Level(Game, levelPath, Components, IVModel, levelIndex);
            level.SetObjectivesManager(objectivesManager);

            isStoryShowing = levelIndex == 3;

            EventAggregator.Instance.Publish(new OnLevelStarted {Index = levelIndex});

            gameSettings.Save();
        }

        void LoadLevel()
        {
            if (levelIndex > 4 && OnEndGame != null)
            {
                OnEndGame(this, EventArgs.Empty);
                loadingContentThread = null;
                GameSettings.LevelIndex = 4;
                return;
            }
            objectivesManager.Reset();
            loadingContentThread = new Thread(new ThreadStart(
                                                      delegate
                                                      {
                                                          LoadNextLevel();
                                                          loadingContentThread = null;
                                                      }))
            {
                Name = "Next Thread",
                Priority = ThreadPriority.Normal
            };

            loadingContentThread.Start();
        }

        public override void Update(GameTime gameTime)
        {
            var state = Keyboard.GetState();
            
            if ((loadingContentThread == null && (level.LevelReached || level.ResetLevel)))
            {
                if (level.ResetLevel) levelIndex--;
                if (!levelSelection || (levelSelection && levelIndex > GameSettings.LevelIndex))
                {
                    GameSettings.LevelIndex = levelIndex;
                }

                EventAggregator.Instance.Publish(new OnLevelAccomplished { Index = levelIndex });

                LoadLevel();
            }

            if (loadingContentThread == null && !level.ResetLevel)
            {
                if (level.PlayerDead)
                {
                    timeToDeath += gameTime.ElapsedGameTime;
                    if (timeToDeath > TimeSpan.FromSeconds(0))
                    {
                        timeToDeath = TimeSpan.Zero;
                        level.LoadCheckPoint();
                    }
                }
                else
                {
                    if (isStoryShowing)
                    {
                        /*if(state.IsKeyDown(Keys.Enter) && oldState.IsKeyUp(Keys.Enter))
                            isStoryShowing = false;*/
                        storyScene.Update(gameTime);
                    }
                    else
                    {
                        if (state.IsKeyDown(Keys.Escape) && oldState.IsKeyUp(Keys.Escape) && level.CanShowMenu)
                        {
                            gamePaused = !gamePaused;
                            soundManager.PlaySound("echap_button");
                        }

                        if (!gamePaused)
                        {
                            if (state.IsKeyDown(Keys.R) && oldState.IsKeyUp(Keys.R))
                                LoadCheckPoint(this, EventArgs.Empty);

                            if (state.IsKeyDown(Keys.L) && oldState.IsKeyUp(Keys.L))
                            {
                                level.NextDebugTeleport();
                            }
                            
                            level.UpdateSpace(gameTime);
                            objectivesManager.Update(gameTime);

                            if (state.IsKeyDown(Keys.N) && oldState.IsKeyUp(Keys.N))
                            {
                                level.LevelReached = true;
                            }

                            if (state.IsKeyDown(Keys.P) && oldState.IsKeyUp(Keys.P))
                            {
                                var index = levelIndex - 2;
                                if (index > -1)
                                {
                                    levelIndex -= 2;
                                    level.LevelReached = true;
                                }
                            }

                            base.Update(gameTime);
                        }
                        else
                            pauseScene.Update(gameTime);
                        level.Update(gameTime);
                    }
                }

            }
            oldState = state;

            if (loadingContentThread == null && songs != null && MediaPlayer.State == MediaState.Stopped)
            {
                var old = songIndex;
                while (old == songIndex)
                    songIndex = random.Next(6);
                MediaPlayer.Play(songs[songIndex]);
            }
        }
        public override void Draw(GameTime gameTime)
        {
            if (loadingContentThread == null && !level.PlayerDead)
            {
                if (isStoryShowing)
                {
                    sBatch.Begin();

                    /*sBatch.Draw(storyTexture, new Rectangle(0, 0, GameSettings.WindowWidth, GameSettings.WindowHeight),
                                new Rectangle(0, 0, storyTexture.Width, storyTexture.Height),
                                Color.White);*/
                    storyScene.Draw(sBatch);

                    sBatch.End();
                }
                else
                {

                    GraphicsDevice.SetRenderTarget(PP_guisanteBlure);
                    GraphicsDevice.BlendState = BlendState.Opaque;
                    GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                    GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

                    GraphicsDevice.Clear(Color.Black);
                    GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;

                    level.Draw(gameTime);
                    base.Draw(gameTime);

                    GraphicsDevice.SetRenderTarget(null);
                    GraphicsDevice.Clear(Color.Black);
                    original_menu_output = PP_guisanteBlure;

                    sBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                    pp_blure.CurrentTechnique.Passes[0].Apply();
                    pp_blure.Parameters["textureToRender"].SetValue(original_menu_output);
                    sBatch.Draw(original_menu_output, Vector2.Zero, Color.White);
                    sBatch.End();

                    sBatch.Begin();
#if DEBUG

                    #region calculate FPS

                    FPStotalSinceLast += gameTime.ElapsedGameTime.TotalSeconds;
                    FPStotalFramesSinceLast++;
                    if (gameTime.TotalGameTime.TotalSeconds - FPSlastTime > .25 &&
                        gameTime.ElapsedGameTime.TotalSeconds > 0)
                    {
                        double avg = FPStotalSinceLast/FPStotalFramesSinceLast;
                        FPSlastTime = gameTime.TotalGameTime.TotalSeconds;
                        FPStoDisplay = Math.Round(1/avg, 1);
                        FPStotalSinceLast = 0;
                        FPStotalFramesSinceLast = 0;
                    }
                    sBatch.DrawString(font, "FPS: " + FPStoDisplay, new Vector2(0, GameSettings.WindowHeight - 40),
                                      Color.White);

                    #endregion
                    
#endif

                    level.Draw2DContent(sBatch);

                    objectivesManager.Draw(sBatch);

                    sBatch.End();

                    if (gamePaused)
                    {
                        sBatch.Begin();
                        pauseScene.Draw(sBatch, levelIndex);
                        sBatch.End();
                    }
                }
            }
            else
            {
                sBatch.Begin();

                sBatch.Draw(loadingScreen, new Vector2(((GameSettings.WindowWidth - loadingScreen.Width)/2.0f),
                                                       (GameSettings.WindowHeight - loadingScreen.Height)/2.0f),
                            Color.White);

                sBatch.End();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (level != null)
                level.Dispose();
            level = null;
            GC.Collect();
            Components.Clear();
            Components = new List<GameComponent>();
            MediaPlayer.Stop();
        }
    }
}