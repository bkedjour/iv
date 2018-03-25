using System;
using IV.Action_Scene.ParticleSystems.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Action_Scene.ParticleSystems
{
    class ExplosionParticleSystem : ParticleSystem
    {
        public ExplosionParticleSystem(Game game, ContentManager content) : base(game, content)
        {
        }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "ParticleSystems\\explosion";

            settings.MaxParticles = 100;

            settings.Duration = TimeSpan.FromSeconds(1);
            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = -5;
            settings.MaxHorizontalVelocity = 5;

            settings.MinVerticalVelocity = -1;
            settings.MaxVerticalVelocity = 1;

            settings.EndVelocity = 4f;

            settings.MinColor = Color.DarkGray;
            settings.MaxColor = Color.Gray;

            settings.MinRotateSpeed = -2;
            settings.MaxRotateSpeed = 3;

            settings.MinStartSize = 1;
            settings.MaxStartSize = 6;

            settings.MinEndSize = 1;
            settings.MaxEndSize = 16;

            // Use additive blending.
            settings.BlendState = BlendState.Additive;
        }
    }
}
