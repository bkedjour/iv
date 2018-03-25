using System;
using IV.Action_Scene.ParticleSystems.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace IV.Action_Scene.ParticleSystems
{
    class SeroParticleSystem : ParticleSystem
    {
        public SeroParticleSystem(Game game, ContentManager content)
            : base(game, content)
        {
        }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "ParticleSystems\\sero_Particles";

            settings.MaxParticles = 1000;

            settings.Duration = TimeSpan.FromSeconds(.9f);

            settings.DurationRandomness = 1.5f;

            settings.EmitterVelocitySensitivity = 0.1f;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 1;

            settings.MinVerticalVelocity = 0;
            settings.MaxVerticalVelocity = 0;

            settings.MinColor = new Color(64, 96, 128, 255);
            settings.MaxColor = new Color(255, 255, 255, 128);

            settings.MinRotateSpeed = -4;
            settings.MaxRotateSpeed = 4;

            settings.MinStartSize = .8f;
            settings.MaxStartSize = 1.8f;

            settings.MinEndSize = 0;
            settings.MaxEndSize = 0;
        }
    }
}
