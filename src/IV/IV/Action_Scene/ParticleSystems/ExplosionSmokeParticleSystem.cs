using System;
using IV.Action_Scene.ParticleSystems.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace IV.Action_Scene.ParticleSystems
{
    class ExplosionSmokeParticleSystem : ParticleSystem
    {
        public ExplosionSmokeParticleSystem(Game game, ContentManager content) : base(game, content)
        {
        }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "ParticleSystems\\smoke";

            settings.MaxParticles = 400;

            settings.Duration = TimeSpan.FromSeconds(6);

            settings.MinHorizontalVelocity = -3;
            settings.MaxHorizontalVelocity = 3;

            settings.MinVerticalVelocity = -.1f;
            settings.MaxVerticalVelocity = 4;

            settings.Gravity = new Vector3(0, -0.7f, 0);

            settings.EndVelocity = 4;

            //settings.MinColor = Color.LightGray;
            settings.MinColor = Color.Black;
            settings.MaxColor = Color.White;

            settings.MinRotateSpeed = 0;
            settings.MaxRotateSpeed = 0;

            settings.MinStartSize = 4; 
            settings.MaxStartSize = 10;

            settings.MinEndSize = 4;
            settings.MaxEndSize = 16;
        }
    }
}
