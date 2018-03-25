using System;
using System.Collections.Generic;
using BEPUphysics;
using BEPUphysics.Entities;
using IV.Action_Scene.Enemies;
using IV.Action_Scene.Objects;
using Microsoft.Xna.Framework;

namespace IV.Action_Scene.Weapons
{
    class PlayerBullet 
    {
        public Vector3 Position { get; private set; }
        private readonly float speed;
        private readonly Space space;
        public bool IsDestroyed { get; private set; }
        public bool IsRightDirection{ get; private set;}
        private readonly int strength;
        public bool IsShowed { get; set; }
        public bool PrepareToDestruction{ get; private set;}
        private TimeSpan timeToDestruction;
        private readonly TimeSpan destructionTime;

        public event EventHandler<RocketEventArg> OnCollide;

        public PlayerBullet(Space space, float speed, Vector3 position, bool isRightDirection, int strength, TimeSpan destructionTime)
        {
            this.space = space;
            this.destructionTime = destructionTime;
            this.strength = strength;
            IsRightDirection = isRightDirection;
            this.speed = speed;
            Position = position;
            IsDestroyed = true;
        }

        public void Reset(Vector3 position, bool isRightDire)
        {
            Position = position;
            IsRightDirection = isRightDire;
            IsDestroyed = false;
            PrepareToDestruction = false;
            timeToDestruction = TimeSpan.Zero;
        }

        public void Update(GameTime gameTime)
        {
            if(PrepareToDestruction)
            {
                timeToDestruction += gameTime.ElapsedGameTime;
                if(timeToDestruction > destructionTime)
                    IsDestroyed = true;
                return;
            }
            var hitEntitys = new List<Entity>();
            space.RayCast(Position, IsRightDirection ? Vector3.Right : Vector3.Left, .25f, false, hitEntitys,
                          new List<Vector3>(), new List<Vector3>(), new List<float>());
            foreach (var hitEntity in hitEntitys)
                HandelCollision(hitEntity);

            Position = new Vector3(Position.X + (IsRightDirection ? speed : -speed), Position.Y, Position.Z);

        }

        protected  void HandelCollision(Entity other)
        {
            if(IsDestroyed) return;
            if ((other.Tag is Player)||(other.Tag is File))
                return;
            if (other.Tag is Enemy)
                ((Enemy)other.Tag).Strength -= strength;
            else if (other.Tag is PlazmaGun)
                ((PlazmaGun)other.Tag).Hurt(strength);
            else if (other.Tag is MachinGun)
                ((MachinGun)other.Tag).Hurt(strength);
            else if (other.Tag is LaserWall)
                ((LaserWall)other.Tag).Destroy();
            else if (other.Tag is BrickBlocker)
                ((BrickBlocker)other.Tag).Destroy();
            else if (other.IsDynamic && !(other.Tag is Brick) && !(other.Tag is RocketAmmo))
                other.LinearVelocity = new Vector3(IsRightDirection ? strength : -strength, 0, 0);

            if (other.Tag is string && ((string) other.Tag) == "Move")
                return;

            PrepareToDestruction = true;
            if (OnCollide != null)
                OnCollide(this, new RocketEventArg {Position = Position});

        }
       
    }

    class RocketEventArg : EventArgs
    {
        public Vector3 Position { get; set; }
    }
}
