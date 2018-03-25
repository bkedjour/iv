using System.Collections.Generic;
using BEPUphysics;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;

namespace IV.Action_Scene.Weapons
{
    class EnemyBullet : Bullet
    {
        public bool Destroyed{ get; private set;}

        public EnemyBullet(Game game, Space space, Camera camera, Vector3 position, float speed, int strength, 
            bool isRightDirection, List<GameComponent> gameComponents) 
            : base(game, space, camera, position, speed, strength, isRightDirection, gameComponents)
        {
        }

        protected override void HandelCollision(Entity other)
        {
            if (other.Tag is Player)
                ((Player)other.Tag).Hurt(strength);

            gameComponents.Remove(this);
            Destroyed = true;
        }

        
    }
}
