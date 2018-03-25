using System;
using IV.Action_Scene.ParticleSystems.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace IV.Action_Scene.ParticleSystems
{
    class SmokePlumeParticleSystem : ParticleSystem
    {
        public SmokePlumeParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            const float test = 5;

            settings.TextureName = "ParticleSystems\\smoke";

            settings.MaxParticles = 600;

            settings.Duration = TimeSpan.FromSeconds(5);

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 15 / test;

            settings.MinVerticalVelocity = 10 / test;
            settings.MaxVerticalVelocity = 20 / test;

            // Create a wind effect by tilting the gravity vector sideways.
            settings.Gravity = new Vector3(-20/ test, -5/ test, 0);

            settings.EndVelocity = 0.75f;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 4 / test; 
            settings.MaxStartSize = 7 / test;

            settings.MinEndSize = 35 / test;
            settings.MaxEndSize = 140 / test;
        }
    }
}
