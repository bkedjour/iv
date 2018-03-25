using System.Collections.Generic;
using BEPUphysics;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;

namespace IV.Action_Scene.Weapons
{
    abstract class Bullet : DrawableGameComponent
    {
        public Box Entity { get; protected set; }
        protected  float speed;
        protected int strength;
        protected Space space;
        protected readonly Camera camera;
        protected readonly bool isRightDirection;
        protected List<GameComponent> gameComponents;

       protected Bullet(Game game,Space space,Camera camera,Vector3 position, float speed, int strength,bool isRightDirection
           , List<GameComponent> gameComponents) : base(game)
       {
           this.speed = speed;
           this.gameComponents = gameComponents;
           this.strength = strength;
           Entity = new Box(position, .5f, .5f, .5f);
           this.space = space;
           this.camera = camera;
           this.isRightDirection = isRightDirection;
       }

        protected abstract void HandelCollision(Entity other);
      
        public override void Update(GameTime gameTime)
        {
            var hitEntitys = new List<Entity>();
            space.RayCast(Entity.CenterPosition, isRightDirection ? Vector3.Right : Vector3.Left, /*.25f*/.5f, false, hitEntitys,
                          new List<Vector3>(), new List<Vector3>(), new List<float>());
            foreach (var hitEntity in hitEntitys)
                HandelCollision(hitEntity);

            if (Entity == null) return;
            
            var newPos = Entity.CenterPosition;
            newPos.X += isRightDirection ? speed : -speed;
            Entity.CenterPosition = newPos;

            base.Update(gameTime);
        }
    }
}
