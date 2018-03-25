using System.ComponentModel;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Achievement
{
    public class EnemyKilledAchievement : Achievement, ISubscriber<OnLevelStarted>
    {
        private Texture2D texture;
        private int initNb;

        public EnemyKilledAchievement()
        {
            EventAggregator.Instance.Subscribe(this);
        }

        protected override void DataStoreObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "EnemyKilled" || DataStoreObject.ThirtyEnemiesKilledUnlocked) return;

            if (DataStoreObject.EnemyKilled - initNb == 30)
            {
                DataStoreObject.ThirtyEnemiesKilledUnlocked = true;
                FireAchievement(texture);
            }
        }

        public override void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("Textures\\Achievements\\7-The_Hunter");
        }

        public void OnEvent(OnLevelStarted level)
        {
            initNb = DataStoreObject.EnemyKilled;
        }
    }
}