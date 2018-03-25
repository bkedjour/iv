using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace IV.Scenes
{
    public class GameScene : DrawableGameComponent
    {
        public List<GameComponent> Components { get; protected set; }

        public GameScene(Game game) : base(game)
        {
            Components = new List<GameComponent>();
            Enabled = false;
            Visible = false;
        }

        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < Components.Count; i++)
                if (Components[i].Enabled)
                    Components[i].Update(gameTime);
            
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            for (int i = 0; i < Components.Count; i++)
                if (Components[i] is DrawableGameComponent)
                    ((DrawableGameComponent) Components[i]).Draw(gameTime);
            base.Draw(gameTime);
        }

        public virtual void Show()
        {
            Enabled = true;
            Visible = true;
        }
        public virtual void Hide()
        {
            Enabled = false;
            Visible = false;
        }
    }
}