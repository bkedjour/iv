using System;
using BEPUphysics;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Action_Scene.Objects
{
    public class Elevator : DrawableGameComponent
    {
        private readonly Box entity;
        private const float velocity = 10;
        private readonly Camera camera;
        private Vector3 initPosition;
        private bool upDirection;
        private float maxDistance;
        private bool active;
        private TimeSpan timer;
        public Box Entity{get{ return entity;}}
        public bool RechedTheTop { get; private set; }

        private Model model;

        public Elevator(Game game, Space space, Camera camera, Box entity) 
            : base(game)
        {
            this.entity = entity;
            this.camera = camera;
            space.Add(entity);
            initPosition = entity.CenterPosition;
        }

        public override void Update(GameTime gameTime)
        {
            if(active)
            {
                timer += gameTime.ElapsedGameTime;
                if (upDirection)
                {
                    if (timer > TimeSpan.FromMilliseconds(300))
                    {
                        timer -= TimeSpan.FromMilliseconds(300);
                        entity.LinearVelocity = new Vector3(0,velocity,0);
                    }
                    if(entity.CenterPosition.Y >= maxDistance)
                    {
                        upDirection = false;
                        RechedTheTop = true;
                        entity.LinearVelocity = Vector3.Zero;
                    }
                }
                else
                {
                    if(timer > TimeSpan.FromSeconds(1.5))
                    {
                        timer -= TimeSpan.FromMilliseconds(300);
                        RechedTheTop = false;
                        entity.LinearVelocity = new Vector3(0,-velocity,0);
                    }
                    if(entity.CenterPosition.Y <= initPosition.Y)
                    {
                        active = false;
                        entity.LinearVelocity = Vector3.Zero;
                    }

                }
            }
            base.Update(gameTime);
        }
        public void ThrowAnObject(bool rightDirection, Entity toThrow, float speed)
        {
            toThrow.LinearVelocity = new Vector3(rightDirection ? speed : -speed, 0, 0);
        }
        public void Move(float Distance)
        {
            if(active) return;
            active = true;
            maxDistance = Distance;
            upDirection = true;
            timer = TimeSpan.Zero;
        }

        public bool isActive{get{ return active;}}

        public void LoadContent(ContentManager Content)
        {
            model = Content.Load<Model>("Models\\Elevator");
            EffectMaker.SetObjectEffect(GetType(), model, Content);
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (var mesh in model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    var effect = (Effect)meshPart.Tag;
                    effect.CurrentTechnique = effect.Techniques["Technique1"];

                    effect.Parameters["World"].SetValue(Matrix.CreateTranslation(0, 0, -entity.Length/2)*
                                                        entity.WorldTransform);
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["LightPosition"].SetValue(EffectMaker.LightPosition);
                    effect.Parameters["CameraPosition"].SetValue(camera.Position);

                    meshPart.Effect = effect;

                }
                mesh.Draw();
            }


            base.Draw(gameTime);
        }
    }

    class Pucher : DrawableGameComponent
    {
        private readonly Box entity;
        private Model model;
        private const float velocity = 10;
        private readonly Camera camera;
        private Vector3 initPosition;
        private bool rightDirection;
        private readonly float maxDistance;
        private bool active;
        private TimeSpan timer;

        public bool isActive{get{ return active;}}

        public Pucher(Game game, Space space, Camera camera, Box entity) 
            : base(game)
        {
            this.camera = camera;
            this.entity = entity;
            space.Add(entity);
            initPosition = entity.CenterPosition;

            maxDistance = entity.CenterPosition.X + entity.Width/1.5f;
        }
        public void LoadContent(ContentManager Content)
        {
            model = Content.Load<Model>("Models\\VirPortPusher");
            EffectMaker.SetObjectEffect(GetType(), model, Content);
        }

        public override void Update(GameTime gameTime)
        {
            if (active)
            {
                timer += gameTime.ElapsedGameTime;
                if (rightDirection)
                {
                    if (timer > TimeSpan.FromMilliseconds(300))
                    {
                        timer -= TimeSpan.FromMilliseconds(300);
                        entity.LinearVelocity = new Vector3(velocity, 0, 0);
                    }
                    if (entity.CenterPosition.X >= maxDistance)
                    {
                        rightDirection = false;
                        entity.LinearVelocity = Vector3.Zero;
                    }
                }
                else
                {
                    if (timer > TimeSpan.FromMilliseconds(300))
                    {
                        timer -= TimeSpan.FromMilliseconds(300);
                        entity.LinearVelocity = new Vector3(-velocity, 0, 0);
                    }
                    if (entity.CenterPosition.X <= initPosition.X)
                    {
                        active = false;
                        entity.LinearVelocity = Vector3.Zero;
                    }

                }
            }
            base.Update(gameTime);
        }

        public void Puch()
        {
            if (active) return;
            active = true;
            rightDirection = true;
            timer = TimeSpan.Zero;
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (var mesh in model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    var effect = (Effect)meshPart.Tag;
                    effect.CurrentTechnique = effect.Techniques["Technique1"];

                    effect.Parameters["World"].SetValue(entity.WorldTransform*
                                                        Matrix.CreateTranslation(entity.Width/2, 0, 0));
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["LightPosition"].SetValue(EffectMaker.LightPosition);
                    effect.Parameters["CameraPosition"].SetValue(camera.Position);

                    meshPart.Effect = effect;

                }
                mesh.Draw();
            }
            base.Draw(gameTime);
        }
    }
}