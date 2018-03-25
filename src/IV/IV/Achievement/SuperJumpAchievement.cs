using System.ComponentModel;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Achievement
{
    public class SuperJumpAchievement : Achievement, ISubscriber<OnLevelStarted>
    {
        private Texture2D texture;
        private int initJump;

        public SuperJumpAchievement()
        {
            EventAggregator.Instance.Subscribe(this);
        }

        protected override void DataStoreObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "SuperJumpCount" || DataStoreObject.SuperJumpAccomplished) return;

            if (DataStoreObject.SuperJumpCount - initJump == 8)
            {
                DataStoreObject.SuperJumpAccomplished = true;
                FireAchievement(texture);
            }
        }

        public override void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("Textures\\Achievements\\5-The_SuperJumper");
        }

        public void OnEvent(OnLevelStarted level)
        {
            initJump = DataStoreObject.SuperJumpCount;
        }
    }
}