using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace IV.Menu_Scene
{
    class CommandScene
    {
        private readonly Texture2D texture;
        private readonly Texture2D title;


        public CommandScene(ContentManager content)
        {
            texture = content.Load<Texture2D>("Textures\\MENU\\Graphics\\Commands\\menu_commands");
            title = content.Load<Texture2D>("Textures\\MENU\\Graphics\\Menu_T_Commands");
        }

        public void Draw(SpriteBatch sBatch)
        {
            sBatch.Draw(texture, new Rectangle(0, 0, GameSettings.WindowWidth, GameSettings.WindowHeight),
                        new Rectangle(0, 0, texture.Width, texture.Height),
                        Color.White);
            sBatch.Draw(title,
                        new Rectangle(0, 0, GameSettings.WindowWidth*800/1600, GameSettings.WindowHeight*84/900),
                        Color.White);
        }
    }

    class AudioScene
    {
        private readonly Texture2D texture;
        private readonly Texture2D slider;
        private readonly Texture2D title;

        private readonly MenuSelector selector;
        private Vector2 sondFxPosition;
        private Vector2 volPosition;
       

        private bool isSoundFxSelected;
        private readonly float minX;
        private readonly float maxX;

        private KeyboardState oldState;

        private readonly SoundManager soundManager;

        private readonly float slideValue = (.4f*GameSettings.WindowWidth)/100f;
       
        public AudioScene(ContentManager content, SoundManager soundManager)
        {
            texture = content.Load<Texture2D>("Textures\\MENU\\Graphics\\Audio\\menu_audio");
            slider = content.Load<Texture2D>("Textures\\MENU\\Graphics\\Audio\\audio_selector");
            title = content.Load<Texture2D>("Textures\\MENU\\Graphics\\Menu_T_audio");
            
            sondFxPosition = new Vector2((62.5f * GameSettings.WindowWidth) / 100f,
                                           (26f*GameSettings.WindowHeight)/100f);
            volPosition = new Vector2((62.5f * GameSettings.WindowWidth) / 100f,
                                           (48f * GameSettings.WindowHeight) / 100f);

            selector = new MenuSelector(content, (1.5f*GameSettings.WindowWidth)/100f,
                                        (26f*GameSettings.WindowHeight)/100f,
                                        (48f*GameSettings.WindowHeight)/100f);
            minX = sondFxPosition.X;
            maxX = 89.3f*GameSettings.WindowWidth/100f;
            isSoundFxSelected = true;

            sondFxPosition.X = ((((MathHelper.Clamp(GameSettings.SoundFx, 0, 1)*100))*(maxX - minX))/100)+minX;
            volPosition.X = ((((MathHelper.Clamp(GameSettings.MusicVol, 0, 1) * 100)) * (maxX - minX)) / 100) + minX;
            this.soundManager = soundManager;
        }

        public void Reset()
        {
            sondFxPosition.X = ((((MathHelper.Clamp(GameSettings.SoundFx, 0, 1) * 100)) * (maxX - minX)) / 100) + minX;
            volPosition.X = ((((MathHelper.Clamp(GameSettings.MusicVol, 0, 1) * 100)) * (maxX - minX)) / 100) + minX;
        }

        public void Update(GameTime gameTime)
        {
            var keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Left))
            {
                if (isSoundFxSelected)
                {
                    sondFxPosition.X -= slideValue;
                    GameSettings.SoundFx = (((sondFxPosition.X - minX)*100)/(maxX - minX))/100;
                }
                else
                {
                    volPosition.X -= slideValue;
                    GameSettings.MusicVol = (((volPosition.X - minX) * 100) / (maxX - minX)) / 100;
                    MediaPlayer.Volume = MathHelper.Clamp(GameSettings.MusicVol, 0, 1);
                }
            }
            else if (keyState.IsKeyDown(Keys.Right))
            {
                if (isSoundFxSelected)
                {
                    sondFxPosition.X += slideValue;
                    GameSettings.SoundFx = (((sondFxPosition.X - minX)*100)/(maxX - minX))/100;
                }
                else
                {
                    volPosition.X += slideValue;
                    GameSettings.MusicVol = (((volPosition.X - minX)*100)/(maxX - minX))/100;
                    MediaPlayer.Volume = MathHelper.Clamp(GameSettings.MusicVol, 0, 1);
                }
            }
            else if ((keyState.IsKeyDown(Keys.Up) && oldState.IsKeyUp(Keys.Up)))
            {
                if (selector.MoveBack())
                {
                    isSoundFxSelected = !isSoundFxSelected;
                    soundManager.PlaySound("chose_button");
                }
            }
            else if ((keyState.IsKeyDown(Keys.Down)&& oldState.IsKeyUp(Keys.Down)))
            {
                if (selector.MoveNext())
                {
                    isSoundFxSelected = !isSoundFxSelected;
                    soundManager.PlaySound("chose_button");
                }
            }

            selector.Update(gameTime);
            sondFxPosition.X = MathHelper.Clamp(sondFxPosition.X, minX, maxX);
            volPosition.X = MathHelper.Clamp(volPosition.X, minX, maxX);

            oldState = keyState;
        }

        public void Draw(SpriteBatch sBatch)
        {
            sBatch.Draw(texture, new Rectangle(0, 0, GameSettings.WindowWidth, GameSettings.WindowHeight),
                        new Rectangle(0, 0, texture.Width, texture.Height),
                        Color.White);
            sBatch.Draw(slider,
                        new Rectangle((int) sondFxPosition.X, (int) sondFxPosition.Y,
                                      GameSettings.WindowWidth*80/1600,
                                      GameSettings.WindowHeight*80/900),
                        Color.White);
            sBatch.Draw(slider,
                        new Rectangle((int) volPosition.X, (int) volPosition.Y,
                                      GameSettings.WindowWidth*80/1600,
                                      GameSettings.WindowHeight*80/900),
                        Color.White);
            selector.Draw(sBatch);
            sBatch.Draw(title,
                        new Rectangle(0, 0, GameSettings.WindowWidth*800/1600, GameSettings.WindowHeight*84/900),
                        Color.White);

        }

    }
    class MenuSelector
        {
            private Texture2D texture;
            private Vector2 position;
            private readonly float[] indices;
            private int currentIndex;

            private TimeSpan time;
            private bool moveNextRequest;
            private bool moveBackRequest;

            public MenuSelector(ContentManager content,float xPosition,params float[] indices)
            {
                this.indices = indices;
                texture = content.Load<Texture2D>("Textures\\MENU\\Graphics\\Menu_selector");
                position = new Vector2(xPosition, indices[0]);
                currentIndex = 0;
            }

            public MenuSelector(Texture2D texture, float xPosition, params float[] indices)
            {
                this.indices = indices;
                this.texture = texture;
                position = new Vector2(xPosition, indices[0]);
                currentIndex = 0;
            }

            public void SetTexture(Texture2D tex)
            {
                texture = tex;
            }

            public void Update(GameTime gameTime)
            {
                if(!moveNextRequest && !moveBackRequest) return;

                if(moveNextRequest)
                {
                    time += gameTime.ElapsedGameTime;
                    if(time > TimeSpan.FromMilliseconds(1))
                    {
                        time = TimeSpan.Zero;
                        position.Y += 15;
                        if(position.Y >= indices[currentIndex])
                        {
                            moveNextRequest = false;
                            position.Y = indices[currentIndex];
                        }
                    }
                }
                if(moveBackRequest)
                {
                    time += gameTime.ElapsedGameTime;
                    if(time > TimeSpan.FromMilliseconds(1))
                    {
                        time = TimeSpan.Zero;
                        position.Y -= 15;
                        if(position.Y <= indices[currentIndex])
                        {
                            moveBackRequest = false;
                            position.Y = indices[currentIndex];
                        }
                    }
                }
                
            }

            public bool MoveNext()
            {
                currentIndex++;
                if (currentIndex >= indices.Length)
                {
                    currentIndex --;
                    return false;
                }
                moveNextRequest = true;
                return true;
            }

            public bool MoveBack()
            {
                currentIndex--;
                if (currentIndex < 0)
                {
                    currentIndex++;
                    return false;
                }
                moveBackRequest = true;
                return true;
            }

            public void Draw(SpriteBatch sBatch)
            {
                sBatch.Draw(texture,
                            new Rectangle((int) position.X, (int) position.Y,
                                          GameSettings.WindowWidth*80/1600,
                                          GameSettings.WindowHeight*80/900),
                            Color.White);
            }
   }

    class VideoScene
    {
        private readonly Texture2D texture;
        private readonly Texture2D res1024;
        private readonly Texture2D res1200;
        private readonly Texture2D res1228;
        private readonly Texture2D res1280;
        private readonly Texture2D res1366;
        private readonly Texture2D res1440;
        private readonly Texture2D res1600;
        private readonly Texture2D checkBoxTex;
        private readonly Texture2D checkBoxNotCheckedTex;
        private readonly Texture2D title;

        private readonly MenuSelector selector;
        private KeyboardState oldState;

        private Vector2 checkBoxPosition;
        private Vector2 resPosition;

        private bool isfullSelected;
        private bool isCheckBoxChecked;
        private int resIndex;

        private readonly SoundManager soundManager;

        public VideoScene(ContentManager content, SoundManager soundManager)
        {
            texture = content.Load<Texture2D>("Textures\\MENU\\Graphics\\Video\\menu_video");
            res1024 = content.Load<Texture2D>("Textures\\MENU\\Graphics\\Video\\Video_1024x768");
            res1200 = content.Load<Texture2D>("Textures\\MENU\\Graphics\\Video\\Video_1200x900");
            res1228 = content.Load<Texture2D>("Textures\\MENU\\Graphics\\Video\\Video_1228x768");
            res1280 = content.Load<Texture2D>("Textures\\MENU\\Graphics\\Video\\Video_1280x720");
            res1366 = content.Load<Texture2D>("Textures\\MENU\\Graphics\\Video\\Video_1366x768");
            res1440 = content.Load<Texture2D>("Textures\\MENU\\Graphics\\Video\\Video_1440x900");
            res1600 = content.Load<Texture2D>("Textures\\MENU\\Graphics\\Video\\Video_1600x900");
            checkBoxTex = content.Load<Texture2D>("Textures\\MENU\\Graphics\\Video\\Video_checked");
            checkBoxNotCheckedTex = content.Load<Texture2D>("Textures\\MENU\\Graphics\\Video\\Video_NOTchecked");
            title = content.Load<Texture2D>("Textures\\MENU\\Graphics\\Menu_Video");

            checkBoxPosition = new Vector2((50f * GameSettings.WindowWidth) / 100f,
                                           (17f * GameSettings.WindowHeight) / 100f);

            resPosition = new Vector2((63f * GameSettings.WindowWidth) / 100f,
                                           (45.5f * GameSettings.WindowHeight) / 100f);

            selector = new MenuSelector(content, (1.5f * GameSettings.WindowWidth) / 100f,
                                        (26f * GameSettings.WindowHeight) / 100f,
                                        (48f * GameSettings.WindowHeight) / 100f);
            isfullSelected = true;
            isCheckBoxChecked = GameSettings.IsFullScreenMode;
            GetResolution();
            this.soundManager = soundManager;
        }

        public void Update(GameTime gameTime)
        {
            var keyState = Keyboard.GetState();

            if(keyState.IsKeyDown(Keys.Left) && oldState.IsKeyUp(Keys.Left))
            {
                if (isfullSelected) isCheckBoxChecked = !isCheckBoxChecked;
                else
                {
                    resIndex++;
                    if (resIndex > 6) resIndex = 0;

                    SetResolution();
                }
                soundManager.PlaySound("options_change");
                GameSettings.IsFullScreenMode = isCheckBoxChecked;
            }
            else if(keyState.IsKeyDown(Keys.Right) && oldState.IsKeyUp(Keys.Right))
            {
                if (isfullSelected) isCheckBoxChecked = !isCheckBoxChecked;
                else
                {
                    resIndex--;
                    if (resIndex < 0) resIndex = 6;

                    SetResolution();
                }
                soundManager.PlaySound("options_change");
                GameSettings.IsFullScreenMode = isCheckBoxChecked;
            }
            else if ((keyState.IsKeyDown(Keys.Up) && oldState.IsKeyUp(Keys.Up)))
            {
                if (selector.MoveBack())
                {
                    isfullSelected = !isfullSelected;
                    soundManager.PlaySound("chose_button");
                }
            }
            else if ((keyState.IsKeyDown(Keys.Down) && oldState.IsKeyUp(Keys.Down)))
            {
                if (selector.MoveNext())
                {
                    isfullSelected = !isfullSelected;
                    soundManager.PlaySound("chose_button");
                }
            }

            selector.Update(gameTime);

            oldState = keyState;
        }

        public void Draw(SpriteBatch sBatch)
        {
            sBatch.Draw(texture, new Rectangle(0, 0, GameSettings.WindowWidth, GameSettings.WindowHeight),
                        new Rectangle(0, 0, texture.Width, texture.Height),
                        Color.White);

            sBatch.Draw(isCheckBoxChecked ? checkBoxTex : checkBoxNotCheckedTex,
                        new Rectangle((int) checkBoxPosition.X, (int) checkBoxPosition.Y,
                                      GameSettings.WindowWidth*160/1600,
                                      GameSettings.WindowHeight*160/900),
                        Color.White);
            Texture2D res;
            switch (resIndex)
            {
                case 0:
                    res = res1024;
                    break;
                case 1:
                    res = res1200;
                    break;
                case 2:
                    res = res1228;
                    break;
                case 3:
                    res = res1280;
                    break;
                case 4:
                    res = res1366;
                    break;
                case 5:
                    res = res1440;
                    break;
                case 6:
                    res = res1600;
                    break;
                default:
                    res = res1024;
                    break;
            }
            sBatch.Draw(res,
                        new Rectangle((int) resPosition.X, (int) resPosition.Y,
                                      GameSettings.WindowWidth*420/1600,
                                      GameSettings.WindowHeight*100/900),
                        Color.White);

            selector.Draw(sBatch);
            sBatch.Draw(title,
                        new Rectangle(0, 0, GameSettings.WindowWidth * 800 / 1600, GameSettings.WindowHeight * 84 / 900),
                        Color.White);
        }

        void GetResolution()
        {
            switch (GameSettings.WindowWidth)
            {
                case 1024:
                    resIndex = 0;
                    break;
                case 1200:
                    resIndex = 1;
                    break;
                case 1228:
                    resIndex = 2;
                    break;
                case 1280:
                    resIndex = 3;
                    break;
                case 1366:
                    resIndex = 4;
                    break;
                case 1440:
                    resIndex = 5;
                    break;
                case 1600:
                    resIndex = 6;
                    break;
                default:
                    resIndex = 3;
                    break;
            }
        }

        void SetResolution()
        {
            switch (resIndex)
            {
                case 0:
                    GameSettings.SavedWindowWidth = 1024;
                    GameSettings.SavedWindowHeight = 768;
                    break;
                case 1:
                    GameSettings.SavedWindowWidth = 1200;
                    GameSettings.SavedWindowHeight = 900;
                    break;
                case 2:
                    GameSettings.SavedWindowWidth = 1228;
                    GameSettings.SavedWindowHeight = 768;
                    break;
                case 3:
                    GameSettings.SavedWindowWidth = 1280;
                    GameSettings.SavedWindowHeight = 720;
                    break;
                case 4:
                    GameSettings.SavedWindowWidth = 1366;
                    GameSettings.SavedWindowHeight = 768;
                    break;
                case 5:
                    GameSettings.SavedWindowWidth = 1440;
                    GameSettings.SavedWindowHeight = 900;
                    break;
                case 6:
                    GameSettings.SavedWindowWidth = 1600;
                    GameSettings.SavedWindowHeight = 900;
                    break;
            }
        }
    }
}
