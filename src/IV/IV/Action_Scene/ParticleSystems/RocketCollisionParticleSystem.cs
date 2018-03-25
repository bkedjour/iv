using System;
using IV.Action_Scene.ParticleSystems.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;


namespace IV.Action_Scene.ParticleSystems
{
    class RocketCollisionParticleSystem : ParticleSystem
    {
        public RocketCollisionParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "ParticleSystems\\smoke";

            settings.MaxParticles = 800;

            settings.Duration = TimeSpan.FromSeconds(4);

            settings.MinHorizontalVelocity = -3;
            settings.MaxHorizontalVelocity = 3;

            settings.MinVerticalVelocity = -3;
            settings.MaxVerticalVelocity = 3;

            settings.Gravity = new Vector3(0, -1, 0);

            settings.EndVelocity = 1;

            settings.MinColor = Color.Black;
            settings.MaxColor = Color.Black;

            settings.MinRotateSpeed = 0;
            settings.MaxRotateSpeed = 0;

            settings.MinStartSize = 10;
            settings.MaxStartSize = 14;

            settings.MinEndSize = 16;
            settings.MaxEndSize = 23;
        }
    }
}
