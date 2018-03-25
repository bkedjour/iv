using System;
using IV.Achievement;
using IV.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace IV
{
    public class IVGame : Game
    {
        readonly GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private GameScene currentScene;
        private ActionScene actionScene;
        private MenuScene menu;
        private StoryScene storyScene;
        private CreditScene creditScene;

        private SoundManager soundManager;
        private readonly GameSettings gameSettings;

        private bool actionSceneDispose;

        public IVGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            gameSettings = new GameSettings();
            gameSettings = gameSettings.Load();
            gameSettings.SetSettings();

            graphics.PreferredBackBufferWidth = GameSettings.WindowWidth;
            graphics.PreferredBackBufferHeight = GameSettings.WindowHeight;

            Exiting += IVGame_Exiting;
        }

        void IVGame_Exiting(object sender, EventArgs e)
        {
            gameSettings.Save();
            AchievementManager.Manager.Save();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService(typeof (SpriteBatch), spriteBatch);

            soundManager = new SoundManager();
            soundManager.LoadContent(Content);
            Services.AddService(typeof(SoundManager), soundManager);

            AchievementManager.Manager.LoadContent(Content);

            menu = new MenuScene(this);
            menu.OnGameStart += GameStarting;
            menu.OnCreditClicked += ShowCredit;
            menu.Initialize();
            menu.LoadContent(Content);
            Components.Add(menu);

            /*storyScene = new StoryScene(this,0);
            storyScene.Initialize();
            storyScene.LoadContent(Content);
            storyScene.OnStartingGame += GameStarting;
            Components.Add(storyScene);*/

            if(GameSettings.IsFullScreenMode)
                graphics.ToggleFullScreen();
            
            currentScene = menu;
            currentScene.Show();
            MediaPlayer.Volume = GameSettings.MusicVol;
        }

        private void GameStarting(object sender, MenuEventArgs e)
        {
            if (sender is MenuScene && e.IsNewGame)
            {
                storyScene = new StoryScene(this,0);
                storyScene.Initialize();
                storyScene.LoadContent(Content);
                storyScene.OnStartingGame += GameStarting;
                Components.Add(storyScene);
                SwitchScene(storyScene);
                
            }
            else
            {
                actionScene = new ActionScene(this, Content, gameSettings, e.LevelIndex, e.IsSelecting);
                Components.Add(actionScene);
                actionScene.OnBackToMenu += BackToMenu;
                actionScene.OnEndGame += ShowCredit;
                SwitchScene(actionScene);
                if (storyScene != null)
                {
                    storyScene.Dispose();
                    Components.Remove(storyScene);
                    storyScene = null;
                }
            }
        }

        void ShowCredit(object sender, EventArgs e)
        {
            MediaPlayer.Stop();
            if (creditScene != null)
            {
                SwitchScene(creditScene);
                actionSceneDispose = true;
            }
            else
            {
                creditScene = new CreditScene(this);
                creditScene.LoadContent(Content);
                creditScene.OnExit += CreditEnd;
                Components.Add(creditScene);
                SwitchScene(creditScene);
                actionSceneDispose = true;
            }
        }

        private void CreditEnd(object sender, EventArgs e)
        {
            SwitchScene(menu);
        }

        private void BackToMenu(object sender, EventArgs e)
        {
            SwitchScene(menu);
            actionSceneDispose = true;
        }

        protected override void Update(GameTime gameTime)
        {
            soundManager.Update(gameTime);
            AchievementManager.Manager.Update(gameTime);

            base.Update(gameTime);

            if(actionSceneDispose & actionScene != null)
            {
                actionScene.Dispose();
                actionScene = null;
                actionSceneDispose = false;
            }
            
        }

        void SwitchScene(GameScene scene)
        {
            currentScene.Hide();
            currentScene = scene;
            currentScene.Show();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            base.Draw(gameTime);

            spriteBatch.Begin();

            AchievementManager.Manager.Draw(spriteBatch);

            spriteBatch.End();
        }
    }
}
