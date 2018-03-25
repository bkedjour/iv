using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Action_Scene
{
    public class ObjectivesManager
    {
        private readonly List<Texture2D> textures;
    
        private TimeSpan timer;
        private int currentID;
        public List<Texture2D> Objectives { get; private set; }

        //

        public static bool Objective_0_Done;
        public static bool Objective_1_Done;
        public static bool Objective_3_Done;
        public static bool Objective_4_Done;

        //

        public ObjectivesManager(ContentManager content)
        {
            currentID = -1;
            textures = new List<Texture2D>
                           {
                               content.Load<Texture2D>("Textures\\Objectives\\0"),
                               content.Load<Texture2D>("Textures\\Objectives\\1"),
                               content.Load<Texture2D>("Textures\\Objectives\\2"),
                               content.Load<Texture2D>("Textures\\Objectives\\3"),
                               content.Load<Texture2D>("Textures\\Objectives\\4"),
                               content.Load<Texture2D>("Textures\\Objectives\\5"),
                               content.Load<Texture2D>("Textures\\Objectives\\6")
                           };
            Objectives = new List<Texture2D>();
        }

        public void Update(GameTime gameTime)
        {
            if(currentID == -1) return;
            timer += gameTime.ElapsedGameTime;
            if(timer > TimeSpan.FromSeconds(6))
            {
                timer = TimeSpan.Zero;
                currentID = -1;
            }
        }

        public void ShowObjective(int id)
        {
            currentID = id;
            Objectives.Add(textures[currentID]);
        }

        public void Reset()
        {
            Objectives.Clear();
        }

        public void Draw(SpriteBatch sBatch)
        {
            if(currentID == -1) return;
            sBatch.Draw(textures[currentID], new Vector2((GameSettings.WindowWidth - textures[currentID].Width)/2.0f,
                                                         currentID == 6
                                                             ? (GameSettings.WindowHeight - textures[currentID].Height)/
                                                               2.0f
                                                             : (GameSettings.WindowHeight - textures[currentID].Height)),
                        Color.White);

        }
    }
}
