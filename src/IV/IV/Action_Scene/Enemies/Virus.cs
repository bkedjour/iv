using System;
using System.Collections.Generic;
using BEPUphysics;
using BEPUphysics.Entities;
using IVSaveLoadAssembly;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Action_Scene.Enemies
{
    class Virus : Enemy
    {
        public Virus(Game game, Space space, Camera camera, Vector3 position, Random rand, Player player, 
            List<GameComponent> gameComponent) 
            : base(game, space, camera, position, EnemyType.Virus, rand, player, gameComponent)
        {
            Strength = 50;
            Body = new Box(position, 4.5f, 1.1f, 1.8f, 10);
            Body.EventManager.InitialCollisionDetected += collisionDetected;
            space.Add(Body);
            Body.Tag = this;

            playerDistance = new Vector3(15, 1, 0);

        }

        public override void LoadContent(ContentManager Content)
        {
            model = Content.Load<Model>("Models\\VIRUS");
            EffectMaker.SetObjectEffect(GetType(), model, Content);
            base.LoadContent(Content);
        }

        protected override void Shoot()
        {
        }

        public override void Draw(GameTime gameTime)
        {
            if (destroyed && timeToEnd > TimeSpan.FromSeconds(.2f))
            {
                base.Draw(gameTime);
                return;
            }

            var transform = Matrix.CreateRotationY(MathHelper.Pi)*Matrix.CreateTranslation(new Vector3(0f, -.6f, 0));

            foreach (var mesh in model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    var effect = (Effect)meshPart.Tag;
                    effect.CurrentTechnique = effect.Techniques["Technique1"];

                    effect.Parameters["World"].SetValue(transform*Body.WorldTransform);
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
