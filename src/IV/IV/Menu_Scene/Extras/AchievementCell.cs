using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Menu_Scene.Extras
{
    public class AchievementCell
    {
        private readonly Texture2D texture;
        public Vector2 Position { get; set; }
        private readonly string description;
        private readonly Vector2 descriptionPosition;
        private readonly Texture2D lockedTexture;
        private bool isUnlocked;
        private readonly SpriteFont font;

        public bool IsSelected { get; set; }

        public AchievementCell(Texture2D texture, Vector2 position, string description, Vector2 descriptionPosition,
                               SpriteFont font, bool isUnlocked, Texture2D lockedTexture)
        {
            this.texture = texture;
            Position = position;
            this.description = description;
            this.descriptionPosition = descriptionPosition;
            this.font = font;
            this.isUnlocked = isUnlocked;
            this.lockedTexture = lockedTexture;
        }

        public void SetUnlocked(bool isunlocked)
        {
            isUnlocked = isunlocked;
        }

        public void Update(GameTime gameTime)
        {
            
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture,
                             new Rectangle((int) Position.X, (int) Position.Y,
                                           GameSettings.WindowWidth*texture.Width/1600,
                                           GameSettings.WindowHeight*texture.Height/900), null,
                             /*IsSelected ? Color.Green :*/ Color.White);
            if (IsSelected)
                spriteBatch.DrawString(font, description, descriptionPosition, Color.White);

            if (!isUnlocked)
            {
                spriteBatch.Draw(lockedTexture,
                                 new Rectangle((int) Position.X, (int) Position.Y,
                                               GameSettings.WindowWidth*texture.Width/1600,
                                               GameSettings.WindowHeight*texture.Height/900), null, Color.White);
            }
        }
    }
}