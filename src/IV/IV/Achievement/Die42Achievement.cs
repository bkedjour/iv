using System.ComponentModel;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Achievement
{
    public class Die42Achievement : Achievement
    {
        private Texture2D texture;

        protected override void DataStoreObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "PlayerDeath" || DataStoreObject.Die42TimeAccomplished) return;

            if (DataStoreObject.PlayerDeath >= 42)
            {
                DataStoreObject.Die42TimeAccomplished = true;
                FireAchievement(texture);
            }
        }

        public override void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("Textures\\Achievements\\6-The_Zombie");
        }

    }
}