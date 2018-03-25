using Microsoft.Xna.Framework.Graphics;

namespace IV.Achievement
{
    public class IdleFor30SecAchievement : Achievement, ISubscriber<OnIdle>
    {
        private Texture2D texture;

        public IdleFor30SecAchievement()
        {
            EventAggregator.Instance.Subscribe(this);
        }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            texture = content.Load<Texture2D>("Textures\\Achievements\\10-Statue");
        }

        public void OnEvent(OnIdle e)
        {
            if (DataStoreObject.IdleFor30SecAccomplished) return;

            DataStoreObject.IdleFor30SecAccomplished = true;
            FireAchievement(texture);
        }
    }
}