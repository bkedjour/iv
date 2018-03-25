using Microsoft.Xna.Framework.Graphics;

namespace IV.Achievement
{
    public class KillAllEnemiesAcheivement : Achievement, ISubscriber<OnZeroEnemy>
    {
        private Texture2D texture;

        public KillAllEnemiesAcheivement()
        {
            EventAggregator.Instance.Subscribe(this);
        }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            texture = content.Load<Texture2D>("Textures\\Achievements\\9-The_Hitman");
        }

        public void OnEvent(OnZeroEnemy e)
        {
            if (DataStoreObject.ZeroEnemiesAccomplished) return;

            DataStoreObject.ZeroEnemiesAccomplished = true;
            FireAchievement(texture);
        }
    }
}