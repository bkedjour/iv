using Microsoft.Xna.Framework.Graphics;

namespace IV.Achievement
{
    public class PlayGameWithoutKillingAchievement : Achievement, ISubscriber<OnLevelAccomplished>, ISubscriber<OnLevelStarted>
    {
        private Texture2D texture;

        private int currentLevel;
        private int initKill;

        public PlayGameWithoutKillingAchievement()
        {
            EventAggregator.Instance.Subscribe(this);
        }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            texture = content.Load<Texture2D>("Textures\\Achievements\\2-Peace_n_love");
        }

        public void OnEvent(OnLevelAccomplished level)
        {
            if ((level.Index == currentLevel && initKill != DataStoreObject.EnemyKilled) ||
                (DataStoreObject.PlayGameWithoutKilling)) return;

            DataStoreObject.PlayGameWithoutKilling = true;
            FireAchievement(texture);
        }

        public void OnEvent(OnLevelStarted level)
        {
            currentLevel = level.Index;
            initKill = DataStoreObject.EnemyKilled;
        }
    }
}