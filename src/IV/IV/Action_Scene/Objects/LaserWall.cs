using System;
using System.Collections.Generic;
using System.Linq;
using BEPUphysics;
using BEPUphysics.Entities;
using IV.Action_Scene.Effects;
using IV.Action_Scene.ParticleSystems;
using IV.Action_Scene.ParticleSystems.Core;
using IVSaveLoadAssembly;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Action_Scene.Objects
{
    class LaserWall : DrawableGameComponent
    {
        private Model model;
        private Model destrModel;
        private readonly Box myEntity;
        private Box laser;
        private readonly Space space;
        private readonly Camera camera;
        private readonly LaserDirection direction;
        private bool active;
        public bool Destroyed { get; private set; }

        //For The Effect
        readonly Quad quad;
        BasicEffect basicEffect;
        private Texture2D laserTexture;

        private readonly Quad sourceQuad;
        private BasicEffect sourceBasicEffect;
        private QuadAnimation laserSource;
        private QuadAnimationPlayer laserSourcePlayer;

        private readonly Quad endQuad;
        private BasicEffect endBasicEffect;
        private QuadAnimation laserEnd;
        private QuadAnimationPlayer laserEndPlayer;

        //Explosion Effect
        private ParticleSystem explosionSmokeParticle;
        private ParticleSystem explosionParticle;
        private readonly List<GameComponent> gameComponents;
        private SoundManager soundManager;

        public LaserWall(Game game,Vector3 position, Space space, Camera camera, LaserDirection direction, List<GameComponent> gameComponents) 
            : base(game)
        {
            this.space = space;
            this.gameComponents = gameComponents;
            this.direction = direction;
            this.camera = camera;
            myEntity = new Box(position, 2.115994f, 3.042036f, 9.190003f) {Tag = this};
            space.Add(myEntity);
            CreateTheLaser();
            laser.Tag = "Move";
            foreach (var entity in space.Entities)
                laser.CollisionRules.SpecificEntities.Add(entity, CollisionRule.NoResponse);

            laser.EventManager.InitialCollisionDetected += HandelPlayerDetection;
            active = true;

            quad = new Quad(Vector3.Zero, Vector3.Backward, Vector3.Up, direction == LaserDirection.Down
                                                                            ? 1/3.0f
                                                                            : laser.Width,
                            direction == LaserDirection.Down
                                ? laser.Height
                                : 1/3.0f);

            sourceQuad = new Quad(Vector3.Zero, Vector3.Backward, Vector3.Up, direction == LaserDirection.Down
                                                                                  ? .8f
                                                                                  : /*laser.Width*/8,
                                  direction == LaserDirection.Down ? /*laser.Height*/8 : .8f);
            endQuad = new Quad(Vector3.Zero, Vector3.Backward, Vector3.Up, direction == LaserDirection.Down
                                                                               ? 4f
                                                                               : 2,
                               direction == LaserDirection.Down ? 2 : 4);

            soundManager = (SoundManager) Game.Services.GetService(typeof (SoundManager));
        }

        private static void HandelPlayerDetection(Entity sender, Entity other, CollisionPair collisionpair)
        {
            if(other.CompoundBody != null && other.CompoundBody.Tag is Player)
            {
                ((Player) other.CompoundBody.Tag).Die(DeathType.Electricity);
            }
        }

        public void Activate()
        {
            if (active) return;
            active = true;
            space.Add(laser);
        }

        public void Disactivate()
        {
            active = false;
            space.Remove(laser);
        }

        public void Destroy()
        {
            if(Destroyed) return;
            Destroyed = true;
            if (active)
                space.Remove(laser);

            for (int i = 0; i < 30; i++)
            {
                var posi = new Vector3(myEntity.CenterPosition.X + (i % 2 == 0 ? .5f : -.5f),
                                       myEntity.CenterPosition.Y + (i % 2 == 2 ? .5f : 1.5f),
                                       myEntity.CenterPosition.Z);
                explosionParticle.AddParticle(posi, new Vector3(.1f, .1f, .1f));
            }
            for (int i = 0; i < 7; i++)
            {
                var posi = new Vector3(myEntity.CenterPosition.X + (i % 2 == 0 ? .5f : -.5f),
                                       myEntity.CenterPosition.Y + (i % 2 == 0 ? .5f : -.5f),
                                       myEntity.CenterPosition.Z);
                explosionSmokeParticle.AddParticle(posi, new Vector3(.1f, .1f, .1f));
            }
            soundManager.Play3DSound("enemie_Explosion", myEntity.CenterPosition);

            ObjectivesManager.Objective_3_Done = true;

        }

        public void LoadContent(ContentManager content)
        {
            model = content.Load<Model>("Models\\Laser_Wall");
            EffectMaker.SetLaserWall(model, content, "Shaders\\uv_fx");
            destrModel = content.Load<Model>("Models\\Laser_Wall_destro");
            EffectMaker.SetLaserWallDestro(destrModel, content, "Shaders\\uv_fx");

            string name = direction == LaserDirection.Down ? "Effects\\Laser\\LaserV" : "Effects\\Laser\\LaserH";
            laserTexture = content.Load<Texture2D>(name);

            basicEffect = new BasicEffect(GraphicsDevice);
            sourceBasicEffect = new BasicEffect(GraphicsDevice);
            endBasicEffect = new BasicEffect(GraphicsDevice);
           
            var sourceTextures = new List<Texture2D>();
            for (int i = 0; i < 7; i++)
                sourceTextures.Add(
                    content.Load<Texture2D>(string.Format("Effects\\Laser\\Source\\{0}_{1}", i,
                                                          direction == LaserDirection.Down
                                                              ? "V"
                                                              : "H")));

            laserSource = new QuadAnimation(sourceTextures, .1f, true);
            laserSourcePlayer = new QuadAnimationPlayer();
            laserSourcePlayer.PlayAnimation(laserSource);

            var endTextures = new List<Texture2D>();
            for (int i = 0; i < 5; i++)
                endTextures.Add(content.Load<Texture2D>(string.Format("Effects\\Laser\\End\\{0}_{1}", i,
                                                          direction == LaserDirection.Down
                                                              ? "V"
                                                              : "H")));

            laserEnd = new QuadAnimation(endTextures, .075f, true);
            laserEndPlayer = new QuadAnimationPlayer();
            laserEndPlayer.PlayAnimation(laserEnd);


            explosionSmokeParticle = new ExplosionSmokeParticleSystem(Game, content) { DrawOrder = 200 };
            explosionSmokeParticle.Initialize();
            gameComponents.Add(explosionSmokeParticle);

            explosionParticle = new ExplosionParticleSystem(Game, content) { DrawOrder = 400 };
            explosionParticle.Initialize();
            gameComponents.Add(explosionParticle);
        }

    
        void CreateTheLaser()
        {
            for (int i = 1; i < 300; i++)
            {
                var hitEntities = new List<Entity>();
                Vector3 rayDirection;
                switch (direction)
                {
                    case LaserDirection.Down:
                        rayDirection = Vector3.Down;
                        break;
                    case LaserDirection.Left:
                        rayDirection = Vector3.Left;
                        break;
                    case LaserDirection.Right:
                        rayDirection = Vector3.Right;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                space.RayCast(myEntity.CenterPosition, rayDirection, i, true, hitEntities, new List<Vector3>(),
                              new List<Vector3>(), new List<float>());
                if(hitEntities.Count(entity => entity != myEntity) > 0)
                {
                    var position = new Vector3
                                       {
                                           X = direction == LaserDirection.Left
                                                   ? myEntity.CenterPosition.X - i/2.0f
                                                   : (direction == LaserDirection.Right
                                                          ? myEntity.CenterPosition.X + i/2.0f
                                                          : myEntity.CenterPosition.X),
                                           Y = (direction == LaserDirection.Left || direction == LaserDirection.Right)
                                                   ? myEntity.CenterPosition.Y
                                                   : myEntity.CenterPosition.Y - i/2.0f,
                                           Z = myEntity.CenterPosition.Z
                                       };
                    laser = new Box(position, (direction == LaserDirection.Left ||
                                               direction == LaserDirection.Right)
                                                  ? i
                                                  : .5f, direction == LaserDirection.Down ? i : .5f, myEntity.Length);
                    space.Add(laser);
                    return;
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            switch (direction)
            {
                case LaserDirection.Down:
                    myEntity.OrientationMatrix = Matrix.CreateRotationZ(MathHelper.Pi);
                    break;
                case LaserDirection.Left:
                    myEntity.OrientationMatrix = Matrix.CreateRotationZ(MathHelper.PiOver2);
                    break;
                case LaserDirection.Right:
                    myEntity.OrientationMatrix = Matrix.CreateRotationZ(-MathHelper.PiOver2);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            UpdateEffectView(basicEffect);
            UpdateEffectView(sourceBasicEffect);
            UpdateEffectView(endBasicEffect);
       
            base.Update(gameTime);
        }

        void UpdateEffectView(BasicEffect effect)
        {
            effect.View = camera.ViewMatrix;
            effect.Projection = camera.ProjectionMatrix;
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (var mesh in Destroyed ? destrModel.Meshes : model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    var effect = (Effect)meshPart.Tag;
                    effect.CurrentTechnique = effect.Techniques["Technique1"];

                    effect.Parameters["World"].SetValue(Matrix.CreateTranslation(new Vector3(0, -1.4f, 0))*
                                                        myEntity.WorldTransform);
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["LightPosition"].SetValue(EffectMaker.LightPosition);
                    effect.Parameters["CameraPosition"].SetValue(camera.Position);

                    meshPart.Effect = effect;

                }
                mesh.Draw();
            }
         
            if (active && !Destroyed)
            {
                // Lasers
                DrawQuad(basicEffect, quad, new Vector3(laser.CenterPosition.X, laser.CenterPosition.Y,
                                                        laser.CenterPosition.Z + -3.5f), laserTexture);
                DrawQuad(basicEffect, quad, new Vector3(laser.CenterPosition.X, laser.CenterPosition.Y,
                                                        laser.CenterPosition.Z + -1.25f), laserTexture);
                DrawQuad(basicEffect, quad, new Vector3(laser.CenterPosition.X, laser.CenterPosition.Y,
                                                        laser.CenterPosition.Z + 1f), laserTexture);
                DrawQuad(basicEffect, quad, new Vector3(laser.CenterPosition.X, laser.CenterPosition.Y,
                                                        laser.CenterPosition.Z + 3.25f), laserTexture);


                if (direction == LaserDirection.Right)
                {
                    base.Draw(gameTime);
                    return;
                }

                //Source
                DrawQuad(sourceBasicEffect, sourceQuad,
                         new Vector3(
                             laser.CenterPosition.X + (direction == LaserDirection.Down ? 0 : laser.HalfWidth - 1),
                             laser.CenterPosition.Y + (direction == LaserDirection.Down ? laser.HalfHeight - 1.25f : 0),
                             laser.CenterPosition.Z - 3.6f), laserSourcePlayer.Texture);
                DrawQuad(sourceBasicEffect, sourceQuad,
                         new Vector3(
                             laser.CenterPosition.X + (direction == LaserDirection.Down ? 0 : laser.HalfWidth - 1),
                             laser.CenterPosition.Y + (direction == LaserDirection.Down ? laser.HalfHeight - 1.25f : 0),
                             laser.CenterPosition.Z - 1.26f), laserSourcePlayer.Texture);
                DrawQuad(sourceBasicEffect, sourceQuad,
                         new Vector3(
                             laser.CenterPosition.X + (direction == LaserDirection.Down ? 0 : laser.HalfWidth - 1),
                             laser.CenterPosition.Y + (direction == LaserDirection.Down ? laser.HalfHeight - 1.25f : 0),
                             laser.CenterPosition.Z + 1.1f), laserSourcePlayer.Texture);
                DrawQuad(sourceBasicEffect, sourceQuad,
                         new Vector3(
                             laser.CenterPosition.X + (direction == LaserDirection.Down ? 0 : laser.HalfWidth - 1),
                             laser.CenterPosition.Y + (direction == LaserDirection.Down ? laser.HalfHeight - 1.25f : 0),
                             laser.CenterPosition.Z + 3.26f), laserSourcePlayer.Texture);


                //End
                DrawQuad(endBasicEffect, endQuad,
                         new Vector3(
                             laser.CenterPosition.X - (direction == LaserDirection.Left ? laser.HalfWidth - 1f : 0)
                             ,
                             laser.CenterPosition.Y -
                             (direction == LaserDirection.Down
                                  ? (laser.HalfHeight - (laser.HalfHeight > 6 ? 1.2f : 1.5f))
                                  : 0),
                             laser.CenterPosition.Z - 3.4f), laserEndPlayer.Texture);
                DrawQuad(endBasicEffect, endQuad,
                         new Vector3(
                             laser.CenterPosition.X - (direction == LaserDirection.Left ? laser.HalfWidth - 1f : 0)
                             ,
                             laser.CenterPosition.Y -
                             (direction == LaserDirection.Down
                                  ? (laser.HalfHeight - (laser.HalfHeight > /*12*/6 ? 1.2f : 1.5f))
                                  : 0),
                             laser.CenterPosition.Z - 1.24f), laserEndPlayer.Texture);
                DrawQuad(endBasicEffect, endQuad,
                         new Vector3(
                             laser.CenterPosition.X - (direction == LaserDirection.Left ? laser.HalfWidth - 1f : 0)
                             ,
                             laser.CenterPosition.Y -
                             (direction == LaserDirection.Down
                                  ? (laser.HalfHeight - (laser.HalfHeight > 6 ? 1.2f : 1.5f))
                                  : 0),
                             laser.CenterPosition.Z + 1.3f), laserEndPlayer.Texture);
                DrawQuad(endBasicEffect, endQuad,
                         new Vector3(
                             laser.CenterPosition.X - (direction == LaserDirection.Left ? laser.HalfWidth - 1f : 0),
                             laser.CenterPosition.Y -
                             (direction == LaserDirection.Down
                                  ? (laser.HalfHeight - (laser.HalfHeight > 6 ? 1.2f : 1.5f))
                                  : 0),
                             laser.CenterPosition.Z + 3.28f), laserEndPlayer.Texture);

            }

            laserSourcePlayer.Update(gameTime);
            laserEndPlayer.Update(gameTime);


            if(Destroyed)
            {
                explosionSmokeParticle.SetCamera(camera.ViewMatrix, camera.ProjectionMatrix);
                explosionParticle.SetCamera(camera.ViewMatrix, camera.ProjectionMatrix);
            }
            base.Draw(gameTime);
        }

        void DrawQuad(BasicEffect effect,Quad _quad,Vector3 position, Texture2D texture)
        {
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            effect.EnableDefaultLighting();
            effect.PreferPerPixelLighting = true;
            effect.TextureEnabled = true;
            effect.Texture = texture;
            effect.World = Matrix.CreateTranslation(position);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    _quad.Vertices, 0, 4,
                    _quad.Indexes, 0, 2);
            }

            GraphicsDevice.BlendState = BlendState.Opaque;

        }
    }
}
