using IV.Action_Scene.ParticleSystems.Core;
using Microsoft.Xna.Framework;

namespace IV.Action_Scene.ParticleSystems
{
    class Projectile
    {
        const float trailParticlesPerSecond = 200;
        const float gravity = 15;

        readonly ParticleEmitter trailEmitter;

        Vector3 position;
        Vector3 velocity;
        float age;

        public Projectile(ParticleSystem projectileTrailParticles, Vector3 position)
        {
            this.position = position;
            trailEmitter = new ParticleEmitter(projectileTrailParticles, trailParticlesPerSecond, position);
        }


        public bool Update(GameTime gameTime)
        {
            var elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            position += velocity * elapsedTime;
            velocity.Y -= elapsedTime * gravity;
            age += elapsedTime;

            trailEmitter.Update(gameTime, position);

            return true;
        }
    }
}
