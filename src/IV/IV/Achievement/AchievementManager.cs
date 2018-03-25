using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

/*
 *- ( Immortality )Play game without daying DONNE
*- ( Peace & love )Play without killing any enemy
*- ( The Jumper ) Jump 100 times in one level
*- ( skyJump )Super falldown
*- ( The SuperJumper ) 8 times super jump in one level
*- ( The Zombie ) Die 42 time
*- ( The Hunter ) Kill 30 enemy in one level
*- ( The hollow killer ) 100 times cero in one level
*- ( The Hitman ) kill all enemy in one level
*- ( Statue )idle for 30 seconds
*- ( The Rejected ) be rejected3 times by bra
*- ( The Winner)Finish the Game*/
namespace IV.Achievement
{
    public class AchievementManager
    {
        private static AchievementManager _manager;

        public static AchievementManager Manager { get { return _manager ?? (_manager = new AchievementManager()); } }

        private readonly List<Achievement> achievements;

        private readonly string folder;

        private readonly AchivementDSO dataStoreObject;

        public AchivementDSO AchievementsStatus { get { return dataStoreObject; } }

        private AchievementManager()
        {
            folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            folder = Path.Combine(folder, "ITG");
            folder = Path.Combine(folder, "IV");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            folder = Path.Combine(folder, "Achievements.xml");

            dataStoreObject = Load();

            achievements = new List<Achievement> {new Counter {DataStoreObject = dataStoreObject}};
            CreateLockedAchievements();
        }

        public void LoadContent(ContentManager content)
        {
            foreach (var achievement in achievements)
            {
                achievement.LoadContent(content);
            }
        }

        private void CreateLockedAchievements()
        {
            if (!dataStoreObject.PlayGameWithoutDying)
                achievements.Add(new PlayGameWithoutDyingAchievement {DataStoreObject = dataStoreObject});
            if (!dataStoreObject.PlayGameWithoutKilling)
                achievements.Add(new PlayGameWithoutKillingAchievement {DataStoreObject = dataStoreObject});
            if (!dataStoreObject.Jump100Accomplished)
                achievements.Add(new JumpAcheivement {DataStoreObject = dataStoreObject});
            if(!dataStoreObject.FallDownAccomplished)
                achievements.Add(new FallDownAchievement{DataStoreObject = dataStoreObject});
            if(!dataStoreObject.SuperJumpAccomplished)
                achievements.Add(new SuperJumpAchievement{DataStoreObject = dataStoreObject});
            if(!dataStoreObject.Die42TimeAccomplished)
                achievements.Add(new Die42Achievement{DataStoreObject = dataStoreObject});
            if (!dataStoreObject.ThirtyEnemiesKilledUnlocked)
                achievements.Add(new EnemyKilledAchievement { DataStoreObject = dataStoreObject });
            if(!dataStoreObject.Cero100Accomplished)
                achievements.Add(new Cero100Achievement{DataStoreObject = dataStoreObject});
            if(!dataStoreObject.ZeroEnemiesAccomplished)
                achievements.Add(new KillAllEnemiesAcheivement{DataStoreObject = dataStoreObject});
            if(!dataStoreObject.IdleFor30SecAccomplished)
                achievements.Add(new IdleFor30SecAchievement{DataStoreObject = dataStoreObject});
            if (!dataStoreObject.Rejected3TimesAccomplished)
                achievements.Add(new RejectedAchievement {DataStoreObject = dataStoreObject});
            if (!dataStoreObject.FinishGameAccomplished)
                achievements.Add(new Level5AccomplishedAchievement { DataStoreObject = dataStoreObject });
        }

        public void Update(GameTime gameTime)
        {
            foreach (var achievement in achievements)
            {
                achievement.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var achievement in achievements)
            {
                achievement.Draw(spriteBatch);
            }
        }

        public void Save()
        {
            using (Stream stream = File.Create(folder))
            {
                var ser = new XmlSerializer(typeof(AchivementDSO));
                ser.Serialize(stream, dataStoreObject);
            }

        }

        public AchivementDSO Load()
        {
            if (!File.Exists(folder))
                return new AchivementDSO();

            using (Stream stream = File.OpenRead(folder))
            {
                try
                {
                    var ser = new XmlSerializer(typeof(AchivementDSO));
                    return (AchivementDSO)ser.Deserialize(stream);

                }
                catch (InvalidOperationException)
                {
                    stream.Close();
                    File.Delete(folder);
                    return new AchivementDSO();
                }

            }
        }
    }
}