using System;
using IV.Action_Scene.ParticleSystems.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Action_Scene.ParticleSystems
{
    /// <summary>
    /// Custom particle system for creating a flame effect.
    /// </summary>
    class FireParticleSystem : ParticleSystem
    {
        public FireParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            float test = 10;
            settings.TextureName = "ParticleSystems\\fire";

            settings.MaxParticles = 2400;

            settings.Duration = TimeSpan.FromSeconds(1);

            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 15 / test;

            settings.MinVerticalVelocity = -10 / test;
            settings.MaxVerticalVelocity = 10 / test;

            // Set gravity upside down, so the flames will 'fall' upward.
            settings.Gravity = new Vector3(0, 15/ test, 0);

            settings.MinColor = new Color(255, 255, 255, 10);
            settings.MaxColor = new Color(255, 255, 255, 40);

            settings.MinStartSize = 5 / test;
            settings.MaxStartSize = 10/test;

            settings.MinEndSize = 10 / test; 
            settings.MaxEndSize = 40 / test;

            // Use additive blending.
            settings.BlendState = BlendState.Additive;
        }
    }
}
