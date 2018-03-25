using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Menu_Scene.Extras
{
    public class ModesView
    {
         private Texture2D texture;
        private Vector2 position;
        
        public ModesView(ContentManager content, Vector2 position)
        {
            this.position = position;
            texture = content.Load<Texture2D>("Textures\\MENU\\Graphics\\Extras\\Mods");
        }

        public void Update(GameTime gameTime)
        {
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture,
                             new Rectangle((int) position.X,
                                           (int) position.Y,
                                           GameSettings.WindowWidth*texture.Width/1600,
                                           GameSettings.WindowHeight*texture.Height/900), null, Color.White);
        }  
    }
}