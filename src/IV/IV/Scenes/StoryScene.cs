using System;
using System.Collections.Generic;
using IV.Menu_Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace IV.Scenes
{
    class StoryScene : GameScene
    {
        private readonly SpriteBatch spriteBatch;
        private List<List<StoryImagePart>> parts;
        private KeyboardState oldState;
        private int bigIndex;
        private int smallIndex;
        private TimeSpan timeToNextPart;
        private const double LoopSeconds = 4;

        private int storyIndex;

        private TimeSpan timeToSkip = TimeSpan.FromSeconds(LoopSeconds);
        private SpriteFont font;

        public event EventHandler<MenuEventArgs> OnStartingGame;

        private Song backMusic;

        public StoryScene(Game game, int storyIndex) : base(game)
        {
            this.storyIndex = storyIndex;
            spriteBatch = (SpriteBatch) Game.Services.GetService(typeof (SpriteBatch));

            timeToNextPart = TimeSpan.FromSeconds(LoopSeconds);

        }

        public void LoadContent(ContentManager content)
        {
            font = content.Load<SpriteFont>("Fonts\\font");
            if (storyIndex == 0)
            {
                #region textures

                var textures = new List<List<Texture2D>>
                                   {
                                       new List<Texture2D>
                                           {
                                               content.Load<Texture2D>("Story\\image-1"),
                                           },
                                       new List<Texture2D>
                                           {
                                               content.Load<Texture2D>("Story\\Mine1"),
                                               content.Load<Texture2D>("Story\\mine2")
                                           },
                                       new List<Texture2D>
                                           {
                                               content.Load<Texture2D>("Story\\mine3"),
                                               content.Load<Texture2D>("Story\\mine4")
                                           },
                                       new List<Texture2D>
                                           {
                                               content.Load<Texture2D>("Story\\image-4"),
                                           },
                                       new List<Texture2D>
                                           {
                                               content.Load<Texture2D>("Story\\mine5"),
                                               content.Load<Texture2D>("Story\\mine6")
                                           },
                                       new List<Texture2D>
                                           {
                                               content.Load<Texture2D>("Story\\mine7"),
                                               content.Load<Texture2D>("Story\\mine8")
                                           },
                                   };
                parts = new List<List<StoryImagePart>>
                            {
                                new List<StoryImagePart>
                                    {
                                        new StoryImagePart(new Vector2((0*GameSettings.WindowWidth)/100f,
                                                                       (0*GameSettings.WindowHeight)/100f),
                                                           textures[0][0])
                                    },
                                new List<StoryImagePart>
                                    {
                                        new StoryImagePart(new Vector2((0*GameSettings.WindowWidth)/100f,
                                                                       (0*GameSettings.WindowHeight)/100f),
                                                           textures[1][0]),
                                        new StoryImagePart(new Vector2((39.0178f*GameSettings.WindowWidth)/100f,
                                                                       (0*GameSettings.WindowHeight)/100f),
                                                           textures[1][1])
                                    },
                                new List<StoryImagePart>
                                    {
                                        new StoryImagePart(new Vector2((0*GameSettings.WindowWidth)/100f,
                                                                       (0*GameSettings.WindowHeight)/100f),
                                                           textures[2][0]),
                                        new StoryImagePart(new Vector2((40.3f*GameSettings.WindowWidth)/100f,
                                                                       (0*GameSettings.WindowHeight)/100f),
                                                           textures[2][1])
                                    },
                                new List<StoryImagePart>
                                    {
                                        new StoryImagePart(new Vector2((0*GameSettings.WindowWidth)/100f,
                                                                       (0*GameSettings.WindowHeight)/100f),
                                                           textures[3][0])
                                    },
                                new List<StoryImagePart>
                                    {
                                        new StoryImagePart(new Vector2((0*GameSettings.WindowWidth)/100f,
                                                                       (0*GameSettings.WindowHeight)/100f),
                                                           textures[4][0]),
                                        new StoryImagePart(new Vector2((46.6f*GameSettings.WindowWidth)/100f,
                                                                       (0*GameSettings.WindowHeight)/100f),
                                                           textures[4][1])
                                    },
                                new List<StoryImagePart>
                                    {
                                        new StoryImagePart(new Vector2((0*GameSettings.WindowWidth)/100f,
                                                                       (0*GameSettings.WindowHeight)/100f),
                                                           textures[5][0]),
                                        new StoryImagePart(new Vector2((36.8f*GameSettings.WindowWidth)/100f,
                                                                       (0*GameSettings.WindowHeight)/100f),
                                                           textures[5][1])
                                    },
                            };

                #endregion

                backMusic = content.Load<Song>("Audio\\Music\\SKID ROW");
                MediaPlayer.Stop();
                MediaPlayer.Play(backMusic);


            }else if (storyIndex == 1)
            {
                var textures = new List<List<Texture2D>>
                                   {
                                       new List<Texture2D>
                                           {
                                               content.Load<Texture2D>("Story\\image-7"),
                                           }
                                   };
                parts = new List<List<StoryImagePart>>
                            {
                                new List<StoryImagePart>
                                    {
                                        new StoryImagePart(new Vector2((0*GameSettings.WindowWidth)/100f,
                                                                       (0*GameSettings.WindowHeight)/100f),
                                                           textures[0][0])
                                    }
                            };

            }
            parts[0][0].Start();
        }

        public override void Hide()
        {
            MediaPlayer.Stop();
            base.Hide();
        }

        public override void Update(GameTime gameTime)
        {
            var keyBoardState = Keyboard.GetState();

            if (MediaPlayer.State != MediaState.Playing && backMusic != null)
                MediaPlayer.Play(backMusic);

            if (keyBoardState.IsKeyDown(Keys.Left) && oldState.IsKeyUp(Keys.Left))
            {
                if (bigIndex > 0)
                {
                    foreach (var part in parts[bigIndex])
                    {
                        part.Initialize();
                    }
                    bigIndex--;
                    smallIndex = parts[bigIndex].Count;

                    timeToNextPart = TimeSpan.FromSeconds(LoopSeconds);
                }

            }
            else if (keyBoardState.IsKeyDown(Keys.Right) && oldState.IsKeyUp(Keys.Right))
            {
                if (smallIndex < parts[bigIndex].Count - 1)
                {
                    parts[bigIndex][smallIndex].ForceToShow();
                    smallIndex++;
                    parts[bigIndex][smallIndex].Start();

                    timeToNextPart = TimeSpan.FromSeconds(LoopSeconds);
                }
                else
                {
                    if (smallIndex < parts[bigIndex].Count)
                    {
                        parts[bigIndex][smallIndex].ForceToShow();
                        smallIndex++;

                        timeToNextPart = TimeSpan.FromSeconds(LoopSeconds);
                    }
                    else if (bigIndex < parts.Count - 1)
                    {
                        smallIndex = 0;
                        bigIndex++;

                        timeToNextPart = TimeSpan.FromSeconds(LoopSeconds);

                        parts[bigIndex][0].Start();

                    }
                }
            }
            else if (keyBoardState.IsKeyDown(Keys.Enter) && oldState.IsKeyUp(Keys.Enter))
            {
                //if (bigIndex == parts.Count - 1)
                if (OnStartingGame != null)
                    OnStartingGame(this, new MenuEventArgs {IsNewGame = true, IsSelecting = false, LevelIndex = 0});
            }

            timeToNextPart -= gameTime.ElapsedGameTime;
            if (timeToNextPart <= TimeSpan.Zero)
            {
                timeToNextPart = TimeSpan.FromSeconds(LoopSeconds);

                if (smallIndex < parts[bigIndex].Count - 1)
                {
                    smallIndex++;
                    parts[bigIndex][smallIndex].Start();
                }
                else
                {
                    if (smallIndex < parts[bigIndex].Count)
                    {
                        parts[bigIndex][smallIndex].Start();
                        smallIndex++;
                    }
                    /*else if (bigIndex < parts.Count - 1)
                    {
                        smallIndex = 0;
                        bigIndex++;

                        parts[bigIndex][0].Start();
                    }
                    else if (bigIndex == parts.Count - 1)
                    {
                        if (OnStartingGame != null)
                            OnStartingGame(this,
                                           new MenuEventArgs {IsNewGame = true, IsSelecting = false, LevelIndex = 0});
                    }*/
                }
            }
            
            foreach (var part in parts[bigIndex])
            {
                part.Update(gameTime);
            }

            timeToSkip -= gameTime.ElapsedGameTime;

            oldState = keyBoardState;
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            var blend = GraphicsDevice.BlendState;
            var depth = GraphicsDevice.DepthStencilState;
            var sampler = GraphicsDevice.SamplerStates[0];

            spriteBatch.Begin();

            Draw(spriteBatch);

            spriteBatch.End();

            GraphicsDevice.BlendState = blend;
            GraphicsDevice.DepthStencilState = depth;
            GraphicsDevice.SamplerStates[0] = sampler;
        }

        public void Draw(SpriteBatch sBatch)
        {
            /*spriteBatch.DrawString(font, string.Format("Big Index : {0}", bigIndex), Vector2.Zero, Color.White);
            spriteBatch.DrawString(font, string.Format("Small Index : {0}", smallIndex), new Vector2(0, 30), Color.White);
            spriteBatch.DrawString(font, string.Format("Time : {0}", timeToNextPart.Seconds.ToString()), new Vector2(0, 60), Color.White);*/

           

            foreach (var part in parts[bigIndex])
            {
                part.Draw(spriteBatch);
            }

            const string message = "Enter to skip";
            var measure = font.MeasureString(message);

            if (timeToSkip > TimeSpan.Zero)
            {
                spriteBatch.DrawString(font, message,
                                       new Vector2(
                                           GameSettings.WindowWidth - measure.X - 10,
                                           GameSettings.WindowHeight - measure.Y - 5),
                                       Color.White);
            }
        }
    }
}
