using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace IV.Menu_Scene.Extras
{
    public class ExtrasScene
    {
        private readonly Texture2D texture;
        private readonly MenuSelector selector;

        private KeyboardState oldState;
        private int extrasIndex;
        private readonly SoundManager soundManager;

        public bool FirstEnter { get; set; }

        private readonly AcheivementsView acheivementsView;
        private readonly PhotosView photosView;
        private readonly StatView statView;
        private readonly StoreView storeView;
        private readonly ModesView modesView;

        public ExtrasScene(ContentManager content, SoundManager soundManager)
        {
            texture = content.Load<Texture2D>("Textures\\MENU\\Graphics\\Extras\\Extras_dashboard");

            selector = new MenuSelector(content, (1.4f*GameSettings.WindowWidth)/100f,
                                        (31.4f*GameSettings.WindowHeight)/100f,
                                        (39.4f*GameSettings.WindowHeight)/100f,
                                        (47.6f*GameSettings.WindowHeight)/100f,
                                        (55.4f*GameSettings.WindowHeight)/100f,
                                        (61.7f*GameSettings.WindowHeight)/100f);

            this.soundManager = soundManager;

            acheivementsView = new AcheivementsView(content, new Vector2((39.8f*GameSettings.WindowWidth)/100f,
                                                                         (20.7f*GameSettings.WindowHeight)/100f));

            photosView = new PhotosView(content, new Vector2((39.8f*GameSettings.WindowWidth)/100f,
                                                                         (20.7f*GameSettings.WindowHeight)/100f));

            statView = new StatView(content, new Vector2((39.2f*GameSettings.WindowWidth)/100f,
                                                                         (20.7f*GameSettings.WindowHeight)/100f));

            storeView = new StoreView(content, new Vector2((39.8f * GameSettings.WindowWidth) / 100f,
                                                                         (20.7f * GameSettings.WindowHeight) / 100f));
            modesView = new ModesView(content, new Vector2((39.8f * GameSettings.WindowWidth) / 100f,
                                                                         (20.7f * GameSettings.WindowHeight) / 100f));
        }

        public void RefreshAchievements()
        {
            acheivementsView.Refresh();
        }

        public void Update(GameTime gameTime)
        {
            var keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Keys.Up) && oldState.IsKeyUp(Keys.Up) && !acheivementsView.isFocused)
            {
                extrasIndex--;
                if (extrasIndex < 0)
                    extrasIndex = 0;
                selector.MoveBack();
                soundManager.PlaySound("chose_button");
            }
            else if (keyState.IsKeyDown(Keys.Down) && oldState.IsKeyUp(Keys.Down) && !acheivementsView.isFocused)
            {
                extrasIndex++;
                if (extrasIndex > 4)
                    extrasIndex = 4;
                selector.MoveNext();
                soundManager.PlaySound("chose_button");
            }

            if (keyState.IsKeyDown(Keys.Right) && oldState.IsKeyUp(Keys.Right) && !acheivementsView.isFocused)
            {
                if (extrasIndex == 0)
                {
                    acheivementsView.FirstEnter = true;
                    acheivementsView.isFocused = true;
                }else if (extrasIndex == 1)
                {
                    photosView.isFocused = true;
                }else if (extrasIndex == 2)
                {
                    statView.isFocused = true;
                }
            }

            oldState = keyState;

            if (extrasIndex == 0)
            {
                acheivementsView.Update(gameTime);
            }else if (extrasIndex == 1)
            {
                photosView.Update(gameTime);
            }else if (extrasIndex == 2)
            {
                statView.Update(gameTime);
            }else if (extrasIndex == 3)
            {
                modesView.Update(gameTime);
            }else if (extrasIndex == 4)
            {
                storeView.Update(gameTime);
            }

            selector.Update(gameTime);
        }

        public void Draw(SpriteBatch sBatch)
        {
            sBatch.Draw(texture, new Rectangle(0, 0, GameSettings.WindowWidth, GameSettings.WindowHeight),
                        new Rectangle(0, 0, texture.Width, texture.Height),
                        Color.White);
            selector.Draw(sBatch);

            if (extrasIndex == 0)
            {
                acheivementsView.Draw(sBatch);
            }else if (extrasIndex == 1)
            {
                photosView.Draw(sBatch);
            }else if (extrasIndex == 2)
            {
                statView.Draw(sBatch);
            }else if (extrasIndex == 3)
            {
                modesView.Draw(sBatch);
            }else if (extrasIndex == 4)
            {
                storeView.Draw(sBatch);
            }
        } 
    }
}