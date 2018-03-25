using System.ComponentModel;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Achievement
{
    public class Cero100Achievement : Achievement, ISubscriber<OnLevelStarted>
    {
        private Texture2D texture;
        private int initCero;

        public Cero100Achievement()
        {
            EventAggregator.Instance.Subscribe(this);
        }

        protected override void DataStoreObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "CeroCount" || DataStoreObject.Cero100Accomplished) return;

            if (DataStoreObject.CeroCount - initCero == 100)
            {
                DataStoreObject.Cero100Accomplished = true;
                FireAchievement(texture);
            }
        }

        public override void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("Textures\\Achievements\\8-The_hollow_killer");
        }

        public void OnEvent(OnLevelStarted level)
        {
            initCero = DataStoreObject.CeroCount;
        }
    }
}