using System.ComponentModel;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Achievement
{
    public class RejectedAchievement : Achievement
    {
        private Texture2D texture;

        protected override void DataStoreObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "RejectedCounter" || DataStoreObject.Rejected3TimesAccomplished) return;

            if (DataStoreObject.RejectedCounter == 3)
            {
                DataStoreObject.Rejected3TimesAccomplished = true;
                FireAchievement(texture);
            }
        }

        public override void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("Textures\\Achievements\\11-The_Rejected");
        }

    }
}