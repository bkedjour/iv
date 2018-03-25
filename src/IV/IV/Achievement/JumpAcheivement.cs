using System.ComponentModel;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Achievement
{
    public class JumpAcheivement : Achievement, ISubscriber<OnLevelStarted>
    {
        private Texture2D texture;
        private int initJump;

        public JumpAcheivement()
        {
            EventAggregator.Instance.Subscribe(this);
        }

        protected override void DataStoreObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "JumpCount" || DataStoreObject.Jump100Accomplished) return;

            if (DataStoreObject.JumpCount - initJump == 100)
            {
                DataStoreObject.Jump100Accomplished = true;
                FireAchievement(texture);
            }
        }

        public override void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("Textures\\Achievements\\3-The_Jumper");
        }

        public void OnEvent(OnLevelStarted level)
        {
            initJump = DataStoreObject.JumpCount;
        }
    }
}