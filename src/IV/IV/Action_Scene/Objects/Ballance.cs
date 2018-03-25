using System;
using System.Collections.Generic;
using BEPUphysics;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Action_Scene.Objects
{
    class Ballance : DrawableGameComponent
    {
        private Box platforme;
        private Box platSupport;
        private CompoundBody theCube;
        private Box detector;
        private Model cubeModel, platformeModel;
        private readonly Space space;
        private readonly Camera camera;

        private float maxValue;
        private const float moveValue = .09f;
        private  float moveStep ;
        private int moveCall;
        private bool moving;
        private TimeSpan timer;

        private readonly List<Entity> Boxes;

        public Ballance(Game game, Space space, Camera camera) 
            : base(game)
        {
            this.space = space;
            this.camera = camera;
            Boxes = new List<Entity>();
        }

        public void LoadContent(ContentManager content)
        {
            cubeModel = content.Load<Model>("Models\\BalanceG");
            EffectMaker.SetBalanceG(cubeModel, content, "Shaders\\uv_fx");
            platformeModel = content.Load<Model>("Models\\BalanceD");
            EffectMaker.SetBalanceD(platformeModel, content, "Shaders\\uv_fx");
        }

        public void SetPlatforme(Box _platforme,Box _platSupport)
        {
            platforme = _platforme;
            platSupport = _platSupport;
            space.Add(platforme);
            space.Add(_platSupport);
            maxValue = platSupport.CenterPosition.Y + platSupport.Height + 4;
        }

        public void SetCube(Box cubeSupport,Box cubePlatforme,Box cubeRight,Box cubeLeft)
        {
            theCube = new CompoundBody(false);
            theCube.AddBody(cubeSupport);
            theCube.AddBody(cubePlatforme);
            theCube.AddBody(cubeRight);
            theCube.AddBody(cubeLeft);
            space.Add(theCube);

            detector =
                new Box(new Vector3(cubePlatforme.CenterPosition.X, cubeRight.CenterPosition.Y + cubeRight.Height/2,
                                    cubePlatforme.CenterPosition.Z), cubePlatforme.Width, .5f, cubePlatforme.Length);
            detector.EventManager.InitialCollisionDetected += CubeDetection;
            foreach (var entity in space.Entities.Where(entity => entity.Tag is Cube))
                detector.CollisionRules.SpecificEntities.Add(entity, CollisionRule.NoResponse);

            theCube.AddBody(detector);
        }

        private void CubeDetection(Entity sender, Entity other, CollisionPair collisionpair)
        {
            bool found = false;
            foreach (var entity1 in Boxes.Where(entity => entity == other))
                found = true;
            
            if (!found && other.Tag is Cube)
            {
                moveCall++;
                Boxes.Add(other);

                ObjectivesManager.Objective_1_Done = true;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if ((moving || moveCall > 0) && platforme.CenterPosition.Y < maxValue)
                Move(gameTime);

            base.Update(gameTime);
        }
        void Move(GameTime gameTime)
        {
            moving = true;
            timer += gameTime.ElapsedGameTime;
            if (timer >= TimeSpan.FromMilliseconds(5))
            {
                timer = TimeSpan.Zero;
                moveStep += moveValue;
                platforme.CenterPosition = new Vector3(platforme.CenterPosition.X,
                                                       platforme.CenterPosition.Y + moveValue,
                                                       platforme.CenterPosition.Z);
                platSupport.CenterPosition = new Vector3(platSupport.CenterPosition.X,
                                                         platSupport.CenterPosition.Y + moveValue,
                                                         platSupport.CenterPosition.Z);
                theCube.TeleportTo(new Vector3(theCube.CenterPosition.X,
                                               theCube.CenterPosition.Y - moveValue/2,
                                               theCube.CenterPosition.Z));
                if (moveStep >= 2 || platforme.CenterPosition.Y >= maxValue)
                {
                    moveStep = 0;
                    moveCall--;
                    moving = false;
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (var mesh in cubeModel.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    var effect = (Effect)meshPart.Tag;
                    effect.CurrentTechnique = effect.Techniques["Technique1"];

                    effect.Parameters["World"].SetValue(theCube.WorldTransform*
                                                        Matrix.CreateTranslation(new Vector3(-1.59f, -13f, -.2f)));
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["LightPosition"].SetValue(EffectMaker.LightPosition);
                    effect.Parameters["CameraPosition"].SetValue(camera.Position);

                    meshPart.Effect = effect;

                }
                mesh.Draw();
            }
            
            foreach (var mesh in platformeModel.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    var effect = (Effect)meshPart.Tag;
                    effect.CurrentTechnique = effect.Techniques["Technique1"];

                    effect.Parameters["World"].SetValue(platforme.WorldTransform*
                                                        Matrix.CreateTranslation(new Vector3(0, -13.3f, -.2f)));
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
