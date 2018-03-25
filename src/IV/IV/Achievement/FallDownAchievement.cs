using Microsoft.Xna.Framework.Graphics;

namespace IV.Achievement
{
    public class FallDownAchievement : Achievement, ISubscriber<OnPlayerFallDown>
    {
        private Texture2D texture;

        public FallDownAchievement()
        {
            EventAggregator.Instance.Subscribe(this);
        }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            texture = content.Load<Texture2D>("Textures\\Achievements\\4-skyJump");
        }

        public void OnEvent(OnPlayerFallDown e)
        {
            if (DataStoreObject.FallDownAccomplished) return;

            DataStoreObject.FallDownAccomplished = true;
            FireAchievement(texture);
        }
    }
}