using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Action_Scene.Effects
{
    class QuadAnimationPlayer
    {
        public QuadAnimation Animation
        {
            get { return animation; }
        }
        QuadAnimation animation;

        public int FrameIndex
        {
            get { return frameIndex; }
        }
        int frameIndex;

        public Texture2D Texture { get { return animation.Textures[FrameIndex]; } }

        private float time;

        public bool EndAnimation { get; private set; }

        public void PlayAnimation(QuadAnimation anim)
        {
            // If this animation is already running, do not restart it.
            //if (Animation == anim)
            //    return;

            // Start the new animation.
            animation = anim;
            frameIndex = 0;
            time = 0.0f;
            EndAnimation = false;
        }

        public void Update(GameTime gameTime)
        {
            if (Animation == null)
                throw new NotSupportedException("No animation is currently playing.");

            // Process passing time.
            time += (float) gameTime.ElapsedGameTime.TotalSeconds;
            while (time > Animation.FrameTime)
            {
                time -= Animation.FrameTime;

                // Advance the frame index; looping or clamping as appropriate.
                if (Animation.IsLooping)
                {
                    frameIndex = (frameIndex + 1)%Animation.FrameCount;
                }
                else
                {
                    frameIndex = Math.Min(frameIndex + 1, Animation.FrameCount - 1);
                    if(frameIndex == Animation.FrameCount - 1)
                        EndAnimation = true;
                }
            }
        }
    }
}