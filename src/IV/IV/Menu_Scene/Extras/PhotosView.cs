using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace IV.Menu_Scene.Extras
{
    public class PhotosView
    {
        private readonly Texture2D texture;
        private Vector2 position;
        private readonly Texture2D[] photos;

        private int photoIndex;
        private KeyboardState oldState;
        public bool FirstEnter { get; set; }

  
        public bool isFocused { get; set; }

        public PhotosView(ContentManager content, Vector2 position)
        {
            this.position = position;
            texture = content.Load<Texture2D>("Textures\\MENU\\Graphics\\Extras\\Photos");

            photos = new Texture2D[7];
            photos[0] = content.Load<Texture2D>("Textures\\MENU\\Graphics\\Extras\\Photos\\0-ITG");
            photos[1] = content.Load<Texture2D>("Textures\\MENU\\Graphics\\Extras\\Photos\\1-IV");
            photos[2] = content.Load<Texture2D>("Textures\\MENU\\Graphics\\Extras\\Photos\\2-Laspersky");
            photos[3] = content.Load<Texture2D>("Textures\\MENU\\Graphics\\Extras\\Photos\\3-Shorton");
            photos[4] = content.Load<Texture2D>("Textures\\MENU\\Graphics\\Extras\\Photos\\4-Machine_gun");
            photos[5] = content.Load<Texture2D>("Textures\\MENU\\Graphics\\Extras\\Photos\\5-Plasma_gun");
            photos[6] = content.Load<Texture2D>("Textures\\MENU\\Graphics\\Extras\\Photos\\6-Trojans");
        }

        public void Update(GameTime gameTime)
        {
           

            if (!isFocused) return;

            var currentState = Keyboard.GetState();

            if (currentState.IsKeyDown(Keys.Left) && oldState.IsKeyUp(Keys.Left))
            {
                photoIndex--;
                if (photoIndex <= 0)
                    photoIndex = 0;

            }else if (currentState.IsKeyDown(Keys.Right) && oldState.IsKeyUp(Keys.Right))
            {
                photoIndex++;
                if (photoIndex >= photos.Length)
                    photoIndex = photos.Length - 1;
            }

            
            oldState = currentState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture,
                             new Rectangle((int) position.X,
                                           (int) position.Y,
                                           GameSettings.WindowWidth*texture.Width/1600,
                                           GameSettings.WindowHeight*texture.Height/900), null, Color.White);

            spriteBatch.Draw(photos[photoIndex],
                             new Rectangle((int)position.X + (int)((2.41*GameSettings.WindowWidth)/100f),
                                           (int)position.Y + (int)((2.3f * GameSettings.WindowWidth) / 100f),
                                           GameSettings.WindowWidth * 810 / 1600,
                                           GameSettings.WindowHeight * 460 / 900), null, Color.White);
        }
    }
}