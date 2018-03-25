using System;
using IV.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace IV.Menu_Scene
{
    class ChapterSelect
    {
        private readonly Texture2D texture;
        private readonly Texture2D redTexture;
        private readonly Texture2D greenTexture;
        private readonly MenuSelector selector;
        public event EventHandler<MenuEventArgs> OnSelectLevel;
        private KeyboardState oldState;
        private int levelIndex;
        private SoundManager soundManager;

        public bool FirstEnter { get; set; }

        public ChapterSelect(ContentManager content, SoundManager soundManager)
        {
            texture = content.Load<Texture2D>("Textures\\MENU\\Graphics\\select_chap\\menu_select_chap");
            redTexture = content.Load<Texture2D>("Textures\\MENU\\Graphics\\select_chap\\red_button_lvl");
            greenTexture = content.Load<Texture2D>("Textures\\MENU\\Graphics\\select_chap\\green_button_lvl");

            selector = new MenuSelector(greenTexture, (2.8f*GameSettings.WindowWidth)/100f,
                                        (28.7f*GameSettings.WindowHeight)/100f, 
                                        (37.5f*GameSettings.WindowHeight)/100f,
                                        (58f*GameSettings.WindowHeight)/100f,
                                        (66.8f * GameSettings.WindowHeight) / 100f,
                                        (75.6f * GameSettings.WindowHeight) / 100f);

            this.soundManager = soundManager;
        }


        public void Update(GameTime gameTime)
        {
            var keyState = Keyboard.GetState();

            if(keyState.IsKeyDown(Keys.Up) && oldState.IsKeyUp(Keys.Up))
            {
                levelIndex--;
                if (levelIndex < 0)
                    levelIndex = 0;
                selector.MoveBack();
                selector.SetTexture(GameSettings.LevelIndex >= levelIndex ? greenTexture : redTexture);
                soundManager.PlaySound("chose_button");
            }
            else if(keyState.IsKeyDown(Keys.Down) && oldState.IsKeyUp(Keys.Down))
            {
                levelIndex++;
                if (levelIndex > 4)
                    levelIndex = 4;
                selector.MoveNext();
                selector.SetTexture(GameSettings.LevelIndex >= levelIndex ? greenTexture : redTexture);
                soundManager.PlaySound("chose_button");
            }/*
            else if ((keyState.IsKeyDown(Keys.Left) && oldState.IsKeyUp(Keys.Left))||
                (keyState.IsKeyDown(Keys.Right) && oldState.IsKeyUp(Keys.Right)))
            {
                if (OnSelectLevel != null && levelIndex <= GameSettings.LevelIndex)
                {
                    soundManager.PlaySound("selct_button");
                    OnSelectLevel(this, new MenuEventArgs {IsSelecting = true, LevelIndex = levelIndex});
                }
            }*/
            else if (keyState.IsKeyDown(Keys.Enter) && oldState.IsKeyUp(Keys.Enter))
            {
                if (FirstEnter)
                {
                    FirstEnter = false;
                }
                else
                {
                    if (OnSelectLevel != null && levelIndex <= GameSettings.LevelIndex)
                    {
                        soundManager.PlaySound("selct_button");
                        OnSelectLevel(this, new MenuEventArgs { IsSelecting = true, LevelIndex = levelIndex });
                    }
                }
            }
            oldState = keyState;
           

            selector.Update(gameTime);

        }

        public void Draw(SpriteBatch sBatch)
        {
            sBatch.Draw(texture, new Rectangle(0, 0, GameSettings.WindowWidth, GameSettings.WindowHeight),
                        new Rectangle(0, 0, texture.Width, texture.Height),
                        Color.White);
            selector.Draw(sBatch);
        }
    }
}
