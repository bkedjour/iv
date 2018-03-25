using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Action_Scene.Effects
{
    class QuadAnimation
    {

        public List<Texture2D> Textures { get; private set; }

        public float FrameTime { get; private set; }

        public bool IsLooping { get; private set; }


        public int FrameCount
        {
            get { return Textures.Count; }
        }

        public QuadAnimation(List<Texture2D> textures, float frameTime, bool isLooping)
        {
            Textures = textures;
            FrameTime = frameTime;
            IsLooping = isLooping;
        }

        public void Dispose()
        {
            foreach (var texture in Textures)
            {
                texture.Dispose();
            }
            Textures.Clear();
        }
    }
}
