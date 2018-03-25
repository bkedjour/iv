using System;
using IV.Action_Scene.ParticleSystems.Core;
using IV.Action_Scene.Weapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Action_Scene.ParticleSystems
{
    /// <summary>
    /// Custom particle system for creating the fiery part of the explosions.
    /// </summary>
    class RocketExplosionParticleSystem : ParticleSystem
    {
        public RocketExplosionParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "ParticleSystems\\explosion";
            settings.MaxParticles = 1000;

            settings.Duration = TimeSpan.FromSeconds(2);
            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = -0.1f;
            settings.MaxHorizontalVelocity = .1f;

            settings.MinVerticalVelocity = -0.1f;
            settings.MaxVerticalVelocity = 0.1f;

            settings.EndVelocity = 1 ;

            settings.MinColor = Color.DarkGray;
            settings.MaxColor = Color.Firebrick;

            settings.MinRotateSpeed = -2;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 5;
            settings.MaxStartSize = 12;

            settings.MinEndSize = 1;
            settings.MaxEndSize = 2;

            // Use additive blending.
            settings.BlendState = BlendState.Additive;
        }
    }
}
