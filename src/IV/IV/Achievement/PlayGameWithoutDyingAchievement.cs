using Microsoft.Xna.Framework.Graphics;

namespace IV.Achievement
{
    public class PlayGameWithoutDyingAchievement : Achievement, ISubscriber<OnLevelAccomplished>, ISubscriber<OnPlayerDie>,
                                  ISubscriber<OnLevelStarted>
    {
        private bool isPlayerDied;
        private int currentLevel;

        private Texture2D texture;

        public PlayGameWithoutDyingAchievement()
        {
            EventAggregator.Instance.Subscribe(this);
        }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            texture = content.Load<Texture2D>("Textures\\Achievements\\1-Immortality");
        }

        public void OnEvent(OnLevelAccomplished level)
        {
            if ((currentLevel == level.Index && isPlayerDied) || DataStoreObject.PlayGameWithoutDying) return;

            DataStoreObject.PlayGameWithoutDying = true;
            FireAchievement(texture);
        }

        public void OnEvent(OnPlayerDie player)
        {
            isPlayerDied = true;
        }

        public void OnEvent(OnLevelStarted level)
        {
            currentLevel = level.Index;
            isPlayerDied = false;
        }
    }
}