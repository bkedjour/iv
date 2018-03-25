using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Menu_Scene
{
    public class StoryImagePart
    {
        private Vector2 position;
        private readonly Texture2D texture;

        private TimeSpan timer;
        private float alpha;

        private bool isStarted;

        public StoryImagePart(Vector2 position, Texture2D texture)
        {
            this.position = position;
            this.texture = texture;
            alpha = 0;
            isStarted = false;
        }

        public void Initialize()
        {
            timer = TimeSpan.Zero;
            alpha = 0;
            isStarted = false;
        }

        public void Start()
        {
            isStarted = true;
        }

        public void ForceToShow()
        {
            Start();
            alpha = 255;
        }

        public void Update(GameTime gameTime)
        {
            if (!isStarted) return;

            timer += gameTime.ElapsedGameTime;
            if (timer >= TimeSpan.FromMilliseconds(5) && alpha < 255)
            {
                timer = TimeSpan.Zero;
                alpha++;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture,
                             new Rectangle((int) position.X,
                                           (int) position.Y,
                                           (GameSettings.WindowWidth*texture.Width)/1920,
                                           (GameSettings.WindowHeight*texture.Height)/1080),
                             null, Color.White*(alpha/255f));
        }
    }
}