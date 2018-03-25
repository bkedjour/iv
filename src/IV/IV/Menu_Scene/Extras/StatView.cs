using IV.Achievement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace IV.Menu_Scene.Extras
{
    public class StatView
    {
        private readonly Texture2D texture;
        private Vector2 position;
        private SpriteFont font;

        private KeyboardState oldState;
        public bool FirstEnter { get; set; }

  
        public bool isFocused { get; set; }

        public StatView(ContentManager content, Vector2 position)
        {
            this.position = position;
            texture = content.Load<Texture2D>("Textures\\MENU\\Graphics\\Extras\\Statistics");
            font = content.Load<SpriteFont>("Fonts\\gameFont");

        }

        public void Update(GameTime gameTime)
        {
            if (!isFocused) return;

            var currentState = Keyboard.GetState();

            oldState = currentState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture,
                             new Rectangle((int) position.X,
                                           (int) position.Y,
                                           GameSettings.WindowWidth*texture.Width/1600,
                                           GameSettings.WindowHeight*texture.Height/900), null, Color.White);

            spriteBatch.DrawString(font,
                                   string.Format("Enemy killed: {0}",
                                                 AchievementManager.Manager.AchievementsStatus.EnemyKilled),
                                   new Vector2(position.X + ((3.5f*GameSettings.WindowWidth)/100f),
                                               position.Y + ((6f*GameSettings.WindowHeight)/100f)),
                                   Color.White);
            spriteBatch.DrawString(font,
                                   string.Format("Jump times: {0}",
                                                 AchievementManager.Manager.AchievementsStatus.JumpCount),
                                   new Vector2(position.X + ((3.5f*GameSettings.WindowWidth)/100f),
                                               position.Y + ((13f*GameSettings.WindowHeight)/100f)),
                                   Color.White);
            spriteBatch.DrawString(font,
                                   string.Format("Super jump times: {0}",
                                                 AchievementManager.Manager.AchievementsStatus.SuperJumpCount),
                                   new Vector2(position.X + ((3.5f*GameSettings.WindowWidth)/100f),
                                               position.Y + ((20f*GameSettings.WindowHeight)/100f)),
                                   Color.White);

            spriteBatch.DrawString(font,
                                   string.Format("Death times: {0}",
                                                 AchievementManager.Manager.AchievementsStatus.PlayerDeath),
                                   new Vector2(position.X + ((3.5f*GameSettings.WindowWidth)/100f),
                                               position.Y + ((27f*GameSettings.WindowHeight)/100f)),
                                   Color.White);

            spriteBatch.DrawString(font,
                                   string.Format("Cero fired: {0}",
                                                 AchievementManager.Manager.AchievementsStatus.CeroCount),
                                   new Vector2(position.X + ((3.5f*GameSettings.WindowWidth)/100f),
                                               position.Y + ((34f*GameSettings.WindowHeight)/100f)),
                                   Color.White);

            spriteBatch.DrawString(font,
                                   string.Format("Rejected times: {0}",
                                                 AchievementManager.Manager.AchievementsStatus.RejectedCounter),
                                   new Vector2(position.X + ((3.5f*GameSettings.WindowWidth)/100f),
                                               position.Y + ((41f*GameSettings.WindowHeight)/100f)),
                                   Color.White);

        } 
    }
}