using Microsoft.Xna.Framework.Graphics;

namespace IV.Achievement
{
    public class Level5AccomplishedAchievement : Achievement, ISubscriber<OnLevelAccomplished>
    {
        private Texture2D texture;

        public Level5AccomplishedAchievement()
        {
            EventAggregator.Instance.Subscribe(this);
        }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            texture = content.Load<Texture2D>("Textures\\Achievements\\12-The_Winner");
        }

        public void OnEvent(OnLevelAccomplished index)
        {
            if (DataStoreObject.FinishGameAccomplished) return;

            if (index.Index == 5)
            {
                DataStoreObject.FinishGameAccomplished = true;
                FireAchievement(texture);
            }
        }
    }
}