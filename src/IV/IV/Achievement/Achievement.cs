using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Achievement
{
    public abstract class Achievement
    {
        public AchivementDSO DataStoreObject
        {
            get { return _dataStoreObject; }
            set
            {
                _dataStoreObject = value;
                _dataStoreObject.PropertyChanged += DataStoreObject_PropertyChanged;
            }
        }

        private TimeSpan displayTimer;
        private bool isShowingAchievement;

        private readonly Queue<Texture2D> achievementTexutres = new Queue<Texture2D>();
        private Texture2D achievmentTexture;

        private int alpha;
        private TimeSpan fadeOutTimer;
        private AchivementDSO _dataStoreObject;

        public virtual void LoadContent(ContentManager content)
        {
            
        }

        public virtual void Update(GameTime gameTime)
        {
            if (!isShowingAchievement)
            {
                if (achievementTexutres.Count > 0)
                {
                    achievmentTexture = achievementTexutres.Dequeue();
                    isShowingAchievement = true;
                    alpha = 255;
                    displayTimer = TimeSpan.Zero;
                }

                return;
            }

            displayTimer += gameTime.ElapsedGameTime;
            if (displayTimer >= TimeSpan.FromSeconds(5))
            {
                fadeOutTimer += gameTime.ElapsedGameTime;
                if (fadeOutTimer > TimeSpan.FromMilliseconds(5))
                {
                    fadeOutTimer = TimeSpan.Zero;
                    alpha-=10;

                    if (alpha <= 0)
                    {
                        displayTimer = TimeSpan.Zero;
                        isShowingAchievement = false;
                    }
                }
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (!isShowingAchievement) return;

            spriteBatch.Draw(achievmentTexture, new Vector2((34.4f*GameSettings.WindowWidth)/100f,
                                                            (int) ((1* GameSettings.WindowHeight)/100f)),
                             Color.White*(alpha/255f));
        }

        protected virtual void FireAchievement(Texture2D texture)
        {
            achievementTexutres.Enqueue(texture);
        }

        protected virtual void DataStoreObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            
        }
    }
}