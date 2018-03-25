using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IV.Action_Scene.ParticleSystems.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace IV.Action_Scene.ParticleSystems
{
    class TestParticleEffect : DrawableGameComponent
    {
        readonly ParticleSystem fireParticles;
        readonly Random random = new Random();
        private readonly List<GameComponent> Components;
        private readonly Camera camera;


        public TestParticleEffect(Game game,ContentManager content, List<GameComponent> components, Camera camera) 
            : base(game)
        {
            fireParticles = new FireParticleSystem(Game, content) {DrawOrder = 500};
            Components = components;
            Components.Add(fireParticles);
            fireParticles.Initialize();
            this.camera = camera;
        }

        public override void Update(GameTime gameTime)
        {
            UpdateFire();
            base.Update(gameTime);
        }

        /// <summary>
        /// Helper for updating the fire effect.
        /// </summary>
        void UpdateFire()
        {
            const int fireParticlesPerFrame = 20;

            // Create a number of fire particles, randomly positioned around a circle.
            for (int i = 0; i < fireParticlesPerFrame; i++)
            {
                fireParticles.AddParticle(RandomPoint(), Vector3.Zero);
            }

            // Create one smoke particle per frmae, too.
            //smokePlumeParticles.AddParticle(RandomPointOnCircle(), Vector3.Zero);
        }

        private Vector3 RandomPoint()
        {
            return new Vector3(-random.Next(20), 0.3928192f, -0.1466081f);
        }
        public override void Draw(GameTime gameTime)
        {
            fireParticles.SetCamera(camera.ViewMatrix, camera.ProjectionMatrix);

            base.Draw(gameTime);
        }
    }
}
