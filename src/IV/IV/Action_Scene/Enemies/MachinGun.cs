using System;
using System.Collections.Generic;
using BEPUphysics;
using BEPUphysics.Entities;
using IV.Action_Scene.Effects;
using IV.Action_Scene.ParticleSystems;
using IV.Action_Scene.ParticleSystems.Core;
using IVSaveLoadAssembly;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Action_Scene.Enemies
{
    class MachinGun : DrawableGameComponent
    {
        private Model supportModel;
        private Model gunModel;
        private readonly Box entity;
        private readonly Space space;
        private readonly Camera camera;
        private readonly Player player;
        private Vector3 playerDistance;
        private readonly float initZ;
        private readonly MachiGunDirection direction;
        private int Strength = 100;
        private bool wasZoomingOut;

        private Matrix gunRotation;
        private bool active;
        private float disValue, disXValue;
        private TimeSpan timeToDisactivate;

        public int ActivationBtnID { get; set; }

        //Fire Effect
        private Quad sourceQuad;
        private BasicEffect sourceBasicEffect;
        private QuadAnimation sourceAnim;
        private QuadAnimationPlayer sourceAnimPlayer;

        //Fire Effect
        private Quad fireQuad;
        private BasicEffect fireBasicEffect;
        private QuadAnimation fireAnim_R, fireAnim_L;
        private QuadAnimationPlayer fireAnimPlayer;
        private float rayWidth = 60;
        private bool shooting;


        private bool destroyed;

        private ParticleSystem explosionSmokeParticle;
        private ParticleSystem explosionParticle;
        private TimeSpan timeToEnd;

        private readonly List<GameComponent> components;


        private readonly SoundManager soundManager;
        private Cue fireSound;
        private bool playingSound;

        public MachinGun(Game game, Space space, Camera camera,Vector3 position, Player player
            , MachiGunDirection direction, List<GameComponent> components) 
            : base(game)
        {
            this.space = space;
            this.components = components;
            this.direction = direction;
            this.player = player;
            this.camera = camera;
            entity = new Box(position, 3, 3, 1.8f, 1000) {Tag = this};
            space.Add(entity);
            playerDistance = new Vector3(30, 4, 0);
            TimeSpan.FromSeconds(2);
            initZ = position.Z;
            active = true;

            soundManager = (SoundManager) Game.Services.GetService(typeof (SoundManager));
        }

        public void LoadContent(ContentManager content)
        {
            gunModel = content.Load<Model>("Models\\Machin_Gun");
            EffectMaker.SetMachinGun(gunModel, content, "Shaders\\uv_fx");
            supportModel = content.Load<Model>("Models\\MG_support");
            EffectMaker.SetMG_Support(supportModel, content, "Shaders\\uv_fx");

            fireBasicEffect = new BasicEffect(GraphicsDevice);
            sourceBasicEffect = new BasicEffect(GraphicsDevice);

            var fireTextures = new List<Texture2D>();
            for (int i = 0; i < 2; i++)
                fireTextures.Add(content.Load<Texture2D>(string.Format("Effects\\Machin_Gun\\Fire\\{0}_{1}", i,
                                                                       direction == MachiGunDirection.Right ? "R" : "L")));
            sourceAnim = new QuadAnimation(fireTextures, .05f, true);
            sourceAnimPlayer = new QuadAnimationPlayer();
            sourceAnimPlayer.PlayAnimation(sourceAnim);

            var fireTextures_R = new List<Texture2D>();
            for (int i = 0; i < 5; i++)
                fireTextures_R.Add(content.Load<Texture2D>(string.Format("Effects\\Machin_Gun\\Bullets\\{0}_R", i)));
            fireAnim_R = new QuadAnimation(fireTextures_R, .02f, true);
            var fireTextures_L = new List<Texture2D>();
            for (int i = 0; i < 5; i++)
                fireTextures_L.Add(content.Load<Texture2D>(string.Format("Effects\\Machin_Gun\\Bullets\\{0}_L", i)));
            fireAnim_L = new QuadAnimation(fireTextures_L, .02f, true);
            fireAnimPlayer = new QuadAnimationPlayer();

            fireAnimPlayer.PlayAnimation(direction == MachiGunDirection.Right ? fireAnim_R : fireAnim_L);


            explosionSmokeParticle = new ExplosionSmokeParticleSystem(Game, content) { DrawOrder = 200 };
            explosionSmokeParticle.Initialize();
            components.Add(explosionSmokeParticle);

            explosionParticle = new ExplosionParticleSystem(Game, content) { DrawOrder = 400 };
            explosionParticle.Initialize();
            components.Add(explosionParticle);
        }

        public void Disactivate()
        {
            active = false;
        }

        public void Activate()
        {
            active = true;
        }

        public override void Update(GameTime gameTime)
        {
            if (destroyed)
            {
                timeToEnd += gameTime.ElapsedGameTime;
                if (timeToEnd > TimeSpan.FromSeconds(5))
                    components.Remove(this);
                base.Update(gameTime);
                return;
            }
            if (Math.Abs(entity.CenterPosition.X - player.Body.CenterPosition.X) < playerDistance.X + 20
              && Math.Abs(entity.CenterPosition.Y - player.Body.CenterPosition.Y) < playerDistance.Y + 10)
            {
                camera.ZoomOut(60);
                wasZoomingOut = true;
            }
            else if (wasZoomingOut)
            {
                camera.ZoomIn();
                wasZoomingOut = false;
            }

            if (!active)
            {
                timeToDisactivate += gameTime.ElapsedGameTime;
                if (timeToDisactivate >= TimeSpan.FromMilliseconds(10) && disValue < 20)
                {
                    timeToDisactivate -= TimeSpan.FromMilliseconds(10);
                    disValue = MathHelper.Clamp(disValue + .3f, 0, 20);
                    disXValue += .001f;

                }
                gunRotation = Matrix.CreateRotationZ(MathHelper.ToRadians(disValue))*
                              Matrix.CreateTranslation(new Vector3(disXValue, 0, 0));
            }
            else
            {
                if (direction == MachiGunDirection.Face)
                {
                    entity.OrientationMatrix = Matrix.CreateRotationY(MathHelper.PiOver2);
                    gunRotation = RotateToFace(entity.CenterPosition, player.Body.CenterPosition, Vector3.UnitY)*
                                  Matrix.CreateRotationY(MathHelper.Pi);
                }
                else
                {
                    entity.OrientationMatrix =
                        Matrix.CreateRotationY(direction == MachiGunDirection.Right
                                                   ? MathHelper.Pi
                                                   : 0);
                    gunRotation = Matrix.Identity;

                    if (
                        Math.Abs(entity.CenterPosition.X + (direction == MachiGunDirection.Right ? 10 : -10)-
                                 player.Body.CenterPosition.X) < playerDistance.X
                        && Math.Abs(entity.CenterPosition.Y - player.Body.CenterPosition.Y) < playerDistance.Y)
                    {
                        if ((direction == MachiGunDirection.Right &&
                             player.Body.CenterPosition.X > entity.CenterPosition.X)
                            ||
                            (direction == MachiGunDirection.Left &&
                             player.Body.CenterPosition.X < entity.CenterPosition.X))
                            Shoot();
                        
                    }else shooting = false;
                }
            }

            entity.CenterPosition = new Vector3(entity.CenterPosition.X,entity.CenterPosition.Y,initZ);

            if (shooting)
            {
                sourceBasicEffect.View = camera.ViewMatrix;
                sourceBasicEffect.Projection = camera.ProjectionMatrix;
                fireBasicEffect.View = camera.ViewMatrix;
                fireBasicEffect.Projection = camera.ProjectionMatrix;

                sourceQuad = new Quad(new Vector3(
                                          entity.CenterPosition.X +
                                          (direction == MachiGunDirection.Right
                                               ? entity.Width + 1.3f
                                               : -entity.Width - 1.3f),
                                          entity.CenterPosition.Y /*+ .34f*/,
                                          entity.CenterPosition.Z), Vector3.Backward,
                                      Vector3.Up, 5, 5);

                sourceAnimPlayer.Update(gameTime);
                var position =
                    new Vector3(
                        entity.CenterPosition.X +
                        (direction == MachiGunDirection.Right ? entity.HalfWidth + .3f : -entity.HalfWidth - .3f),
                        entity.CenterPosition.Y + .8f,
                        entity.CenterPosition.Z);

                var hitEntitys = new List<Entity>();
                if (space.RayCast(position, direction == MachiGunDirection.Right ? Vector3.Right : Vector3.Left,
                                  rayWidth, true, hitEntitys,
                                  new List<Vector3>(),
                                  new List<Vector3>(), new List<float>()))
                {
                    var smallest = GetMinimum(hitEntitys);
                    if (smallest is Box)
                    {
                        rayWidth = direction == MachiGunDirection.Right
                                       ? (smallest.CenterPosition.X - ((Box) smallest).HalfWidth) -
                                         (entity.CenterPosition.X + entity.HalfWidth + .3f)
                                       : (entity.CenterPosition.X - entity.HalfWidth + .3f) -
                                         (smallest.CenterPosition.X + ((Box) smallest).HalfWidth);
                    }
                    else if (smallest.Tag is Player)
                    {
                        rayWidth = direction == MachiGunDirection.Right
                                       ? (smallest.CenterPosition.X -
                                          ((Box) ((Player) smallest.Tag).Body.SubBodies[0]).HalfWidth) -
                                         (entity.CenterPosition.X + entity.HalfWidth + .3f) + .75f
                                       : (entity.CenterPosition.X - entity.HalfWidth + .3f) -
                                         (smallest.CenterPosition.X +
                                          ((Box) ((Player) smallest.Tag).Body.SubBodies[0]).HalfWidth);

                        ((Player)smallest.Tag).Hurt(1.5f);
                    }

                }
                else
                {
                    rayWidth += 4f;
                }

                fireQuad =
                    new Quad(
                        new Vector3(
                            entity.CenterPosition.X +
                            (direction == MachiGunDirection.Right
                                 ? (rayWidth + entity.HalfWidth)/2.0f - .5f
                                 : -(entity.HalfWidth + rayWidth)/2.0f + .5f),
                            entity.CenterPosition.Y + 0f,
                            entity.CenterPosition.Z), Vector3.Backward, Vector3.Up, rayWidth + entity.HalfWidth + 1.5f,
                        .4f);
                fireAnimPlayer.Update(gameTime);

            }else
            {
                soundManager.StopSound(fireSound);
                playingSound = false;
            }
            base.Update(gameTime);
        }

        Entity GetMinimum(List<Entity> entities)
        {
            Entity toRet = entities[0];
            for (int i = 1; i < entities.Count; i++)
                if (direction == MachiGunDirection.Right)
                {
                    if (toRet.CenterPosition.X > entities[i].CenterPosition.X)
                        toRet = entities[i];
                }
                else
                {
                    if (toRet.CenterPosition.X < entities[i].CenterPosition.X)
                        toRet = entities[i];
                }
            return toRet;
        }
        // O is your object's position
        // P is the position of the object to face
        // U is the nominal "up" vector (typically Vector3.Y)
       static Matrix RotateToFace(Vector3 O, Vector3 P, Vector3 U)
        {
            Vector3 D = (O - P);
            Vector3 Right = Vector3.Cross(U, D);
            Vector3.Normalize(ref Right, out Right);
            Vector3 Backwards = Vector3.Cross(Right, U);
            Vector3.Normalize(ref Backwards, out Backwards);
            Vector3 Up = Vector3.Cross(Backwards, Right);
            var rot = new Matrix(Right.X, Right.Y, Right.Z, 0, Up.X, Up.Y, Up.Z, 0, Backwards.X, Backwards.Y, Backwards.Z, 0, 0, 0, 0, 1);
            return rot;
        }

       public void Hurt(int amount)
       {
           Strength -= amount;

           if (Strength <= 0)
               Die();
       }
       void Die()
       {
           camera.ZoomIn();
           soundManager.StopSound(fireSound);
           space.Remove(entity);
           destroyed = true;
           for (int i = 0; i < 30; i++)
           {
               var posi = new Vector3(entity.CenterPosition.X + (i % 2 == 0 ? .5f : -.5f),
                                      entity.CenterPosition.Y + (i % 2 == 2 ? .5f : 1.5f),
                                      entity.CenterPosition.Z);
               explosionParticle.AddParticle(posi, new Vector3(.1f, .1f, .1f));
           }
           for (int i = 0; i < 10; i++)
           {
               var posi = new Vector3(entity.CenterPosition.X + (i % 2 == 0 ? .5f : -.5f),
                                      entity.CenterPosition.Y + (i % 2 == 0 ? .5f : -.5f),
                                      entity.CenterPosition.Z);
               explosionSmokeParticle.AddParticle(posi, new Vector3(.1f, .1f, .1f));
           }
           soundManager.Play3DSound("enemie_Explosion",entity.CenterPosition);
       }

       protected override void Dispose(bool disposing)
       {
           soundManager.StopSound(fireSound);
           base.Dispose(disposing);
       }

        void Shoot()
        {
            shooting = true;
            if(!playingSound)
            {
                fireSound = soundManager.Play3DSound("mashine_gun_shoot", entity.CenterPosition);
                playingSound = true;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            if (destroyed)
            {
                explosionSmokeParticle.SetCamera(camera.ViewMatrix, camera.ProjectionMatrix);
                explosionParticle.SetCamera(camera.ViewMatrix, camera.ProjectionMatrix);
                if (timeToEnd > TimeSpan.FromSeconds(.2f))
                {
                    base.Draw(gameTime);
                    return;
                }
            }
            var transform = Matrix.CreateTranslation(new Vector3(-1.7f, /*.9f*/.5f - .4f, -.65f));

            foreach (var mesh in gunModel.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    var effect = (Effect) meshPart.Tag;
                    effect.CurrentTechnique = effect.Techniques["Technique1"];

                    effect.Parameters["World"].SetValue(transform*gunRotation*entity.WorldTransform);
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["LightPosition"].SetValue(EffectMaker.LightPosition);
                    effect.Parameters["CameraPosition"].SetValue(camera.Position);

                    meshPart.Effect = effect;

                }
                mesh.Draw();
            }
            var suppTransform = Matrix.CreateTranslation(new Vector3(0, .4f - .4f, 0));

            foreach (var mesh in supportModel.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    var effect = (Effect) meshPart.Tag;
                    effect.CurrentTechnique = effect.Techniques["Technique1"];

                    effect.Parameters["World"].SetValue(suppTransform*entity.WorldTransform);
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["LightPosition"].SetValue(EffectMaker.LightPosition);
                    effect.Parameters["CameraPosition"].SetValue(camera.Position);

                    meshPart.Effect = effect;

                }
                mesh.Draw();
            }

            if (shooting)
            {
                DrawQuad(sourceBasicEffect, sourceQuad, Vector3.Zero, sourceAnimPlayer.Texture);
                DrawQuad(fireBasicEffect, fireQuad, Vector3.Zero, fireAnimPlayer.Texture);
            }
            base.Draw(gameTime);
        }

        void DrawQuad(BasicEffect effect, Quad _quad, Vector3 position, Texture2D texture)
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
