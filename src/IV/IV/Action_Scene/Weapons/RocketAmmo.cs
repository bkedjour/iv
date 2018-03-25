using System;
using System.Collections.Generic;
using BEPUphysics;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Action_Scene.Weapons
{
    class RocketAmmo : DrawableGameComponent
    {
        private Model model;
        private Box entity;
        private readonly Camera camera;
        private readonly List<GameComponent> components;
        private readonly Space space;

        //Float Mouvement
        protected float height;
        private TimeSpan mouvTimer;
        private bool heightUp;
        protected float rotY;

        public RocketAmmo(Game game,Space space,Camera camera,Vector3 position, List<GameComponent> components) : base(game)
        {
            this.camera = camera;
            this.components = components;
            entity = new Box(position,2,2,2,100) {Tag = this};
            entity.EventManager.InitialCollisionDetected += OnCollide;
            space.Add(entity);
            this.space = space;
        }

        private void OnCollide(Entity sender, Entity other, CollisionPair collisionpair)
        {
            if (entity != null && other.CompoundBody != null && other.CompoundBody.Tag is Player)
            {
                ((Player) other.CompoundBody.Tag).EatRocketAmmo(12);
                space.Remove(entity);
                components.Remove(this);
                entity = null;
            }
        }

        public void LoadContent(ContentManager content)
        {
            model = content.Load<Model>("Models\\Rocket_Ammo");
            EffectMaker.SetObjectEffect(GetType(), model, content);
        }

        public override void Update(GameTime gameTime)
        {
            HandelFloatMouvement(gameTime);
            base.Update(gameTime);
        }

        void HandelFloatMouvement(GameTime gameTime)
        {
            mouvTimer += gameTime.ElapsedGameTime;
            if (mouvTimer >= TimeSpan.FromMilliseconds(10))
            {
                mouvTimer -= TimeSpan.FromMilliseconds(10);
                height += heightUp ? .007f : -.007f;

                if (height > .3f)
                    heightUp = false;
                if (height < -.3f)
                    heightUp = true;

                //Rotation
                rotY += .7f;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            var transform = Matrix.CreateRotationY(MathHelper.ToRadians(rotY))*
                            Matrix.CreateTranslation(new Vector3(0, height - .5f, 0));
            /* foreach (var mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = Matrix.CreateScale(.6f)*transform*entity.WorldTransform;
                    effect.View = camera.ViewMatrix;
                    effect.Projection = camera.ProjectionMatrix;
                }
                mesh.Draw();
            }*/

            foreach (var mesh in model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    var effect = (Effect) meshPart.Tag;
                    effect.CurrentTechnique = effect.Techniques["Technique1"];

                    effect.Parameters["World"].SetValue(Matrix.CreateScale(.6f)*transform*entity.WorldTransform);
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
