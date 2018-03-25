using System;
using System.Collections.Generic;
using BEPUphysics;
using BEPUphysics.Entities;
using IV.Action_Scene.Effects;
using IV.Action_Scene.ParticleSystems;
using IV.Action_Scene.ParticleSystems.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Action_Scene.Objects
{
    class BrickBlocker : DrawableGameComponent
    {
        private Model model;
        private readonly Camera camera;
        private readonly Box entity;
        private float rotateValue;
        private bool rotating;
        private readonly Player player;

        public BrickBlocker(Game game, Space space, Camera camera, Vector3 position,Vector3 dimension, Player player) 
            : base(game)
        {
            this.camera = camera;
            this.player = player;
            entity = new Box(position, dimension.X, dimension.Y, dimension.Z);
            entity.CenterOfMass = new Vector3(entity.CenterPosition.X + entity.HalfWidth,
                                              entity.CenterPosition.Y + entity.HalfHeight,
                                              entity.CenterPosition.Z);
            space.Add(entity);
            entity.Tag = this;
        }


        public void LoadContent(ContentManager content)
        {
            model = content.Load<Model>("Models\\Gru_Hand");
            EffectMaker.SetObjectEffect(GetType(), model, content);
        }

        public void Destroy()
        {
            rotating = true;

            camera.MakeFocusTo(new Vector3(483.045f, 43.39591f, 64.95387f), -.005000432f, -.7449997f, player,
                               TimeSpan.FromSeconds(3));
            player.Active = false;

            ObjectivesManager.Objective_4_Done = true;
        }

        public override void Update(GameTime gameTime)
        {
            if(rotating)
            {
                entity.AngularVelocity = new Vector3(0,0,.6f);
                rotateValue += .6f;
                if(rotateValue > 90)
                {
                    entity.AngularVelocity = Vector3.Zero;
                    rotating = false;
                }
            }
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (var mesh in model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    var effect = (Effect)meshPart.Tag;
                    effect.CurrentTechnique = effect.Techniques["Technique1"];

                    effect.Parameters["World"].SetValue(entity.WorldTransform *
                                   Matrix.CreateTranslation(rotateValue > 90 ? -.1f : .7f, /*.62*/1.2f, 0));
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

    class Brick : Cube
    {
        public Brick(Game game, Space space, Camera camera, Box entity) 
            : base(game, space, camera, entity)
        {
            entity.EventManager.InitialCollisionDetected += laseCollision;
            entity.Tag = this;
        }

        private static void laseCollision(Entity sender, Entity other, CollisionPair collisionpair)
        {
            var laser = other.Tag as LaserWall;
            if(laser == null) return;
            laser.Destroy();
        }

        public override void LoadContent(ContentManager Content)
        {
            model = Content.Load<Model>("Models\\Brick");
            EffectMaker.SetObjectEffect(GetType(), model, Content);
        }
    }

    class WallBrick : Cube
    {
        public WallBrick(Game game, Space space, Camera camera, Box entity) 
            : base(game, space, camera, entity)
        {
        }
        public override void LoadContent(ContentManager Content)
        {
            model = Content.Load<Model>("Models\\Brick");
            EffectMaker.SetObjectEffect(typeof (Brick), model, Content);
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (var mesh in model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    var effect = (Effect)meshPart.Tag;
                    effect.CurrentTechnique = effect.Techniques["Technique1"];

                    effect.Parameters["World"].SetValue(Matrix.CreateScale(.096f, .18f, .18f) * entity.WorldTransform);
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["LightPosition"].SetValue(EffectMaker.LightPosition);
                    effect.Parameters["CameraPosition"].SetValue(camera.Position);

                    meshPart.Effect = effect;

                }
                mesh.Draw();
            }
        }

    }

    class FilesCannon : DrawableGameComponent
    {
        private TimeSpan timer;
        private bool active;
        private float yValue;
        private float zValue = -1;
        readonly List<Texture2D> textures;
        private readonly Camera camera;
        private readonly Space space;
        private readonly Vector3 initPosition;
        private readonly Vector3 activePosition;
        private bool paused;

        private Box blockWall;
        private Box wallSupport;

        readonly ParticleSystem fireParticles;
        readonly ParticleSystem smokePlumeParticles;
        private readonly Random rand;
        private readonly Entity playerPosition;

        private Quad quad;
        private readonly BasicEffect basicEffect;
        private readonly QuadAnimation animation;
        private readonly QuadAnimationPlayer animPlayer;

        private Projectile projectile;

        private readonly FileFireParticle projectileTrailParticles;

        readonly Sphere kapow;

        public FilesCannon(Game game,Space space,ContentManager Content,List<GameComponent>components,Camera camera
            , ContentManager content, Vector3 initPosition, Vector3 activePosition, Entity playerPosition)
            :base(game)
        {
            textures = new List<Texture2D>();
            for (int i = 0; i < 10; i++)
                textures.Add(Content.Load<Texture2D>(string.Format("Effects\\Player\\Sero\\{0}", i)));
            this.playerPosition = playerPosition;
            this.activePosition = activePosition;
            this.initPosition = initPosition;
            this.space = space;
            this.camera = camera;

            blockWall = new Box(new Vector3(499.2231f, -39.6427f, 5.277952f), 0.65f, 9.89f, 9.680001f);
            space.Add(blockWall);
            wallSupport = new Box(new Vector3(501.8081f, -39.53713f, 5.353421f), 1, 10.24f, 10.77201f);
            space.Add(wallSupport);



            fireParticles = new FireParticleSystem(Game, Content) {DrawOrder = 500};
            fireParticles.Initialize();
            components.Add(fireParticles);
            smokePlumeParticles = new SmokePlumeParticleSystem(Game, Content) {DrawOrder = 100};
            smokePlumeParticles.Initialize();
            components.Add(smokePlumeParticles);
            rand = new Random();


            animation = new QuadAnimation(textures, .05f, true);
            animPlayer = new QuadAnimationPlayer();
            animPlayer.PlayAnimation(animation);

            projectileTrailParticles = new FileFireParticle(Game, content) { DrawOrder = 300 };
            projectileTrailParticles.Initialize();
            components.Add(projectileTrailParticles);

            Initialize();
            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.EnableDefaultLighting();
            basicEffect.PreferPerPixelLighting = true;
            basicEffect.TextureEnabled = true;

            kapow = new Sphere(Vector3.Zero, 1.4f, 10)
            {
                CollisionMargin = .1f,
                AngularVelocity = Vector3.Zero,
                LinearVelocity = Vector3.Zero
            };
            space.Add(kapow);
            kapow.EventManager.InitialCollisionDetected += wallCollision;
        }

        public void Fire(bool up)
        {
            active = up;
            paused = false;
        }

        public void Pause(bool pause)
        {
            paused = pause;
        }

        public override void Update(GameTime gameTime)
        {
            if(paused)
            {
                base.Update(gameTime);
                return;
            }
            
            timer += gameTime.ElapsedGameTime;
            if(timer > TimeSpan.FromSeconds(2))
            {
                timer = TimeSpan.Zero;
                
                kapow.CenterPosition = active ? activePosition : initPosition;
                kapow.LinearVelocity = new Vector3(180, yValue++, zValue);
                zValue += 2;
                
                yValue = MathHelper.Clamp(yValue, 0, 10);
                if (zValue > 10)
                    zValue = -10;
            }

            if (active && blockWall != null)
            {
                space.Remove(blockWall);
                space.Remove(wallSupport);
                wallSupport = null;
                blockWall = null;
            }

            if (AutoSlider.IsDistanceZero(playerPosition.CenterPosition, initPosition, 100))
                UpdateFire();

            UpdateFile(gameTime);

            base.Update(gameTime);
        }

        void UpdateFile(GameTime gameTime)
        {
            basicEffect.View = camera.ViewMatrix;
            basicEffect.Projection = camera.ProjectionMatrix;

            animPlayer.Update(gameTime);
            quad = new Quad(kapow.CenterPosition, Vector3.Backward, Vector3.Up, 3f, 3f);
            projectile = new Projectile(projectileTrailParticles,
                                            new Vector3(kapow.CenterPosition.X, kapow.CenterPosition.Y + .1f,
                                                        kapow.CenterPosition.Z));

            projectile.Update(gameTime);
        }

        public override void  Draw(GameTime gameTime)
        {
            projectileTrailParticles.SetCamera(camera.ViewMatrix, camera.ProjectionMatrix);
            DrawQuad(basicEffect, quad, Vector3.Zero, animPlayer.Texture);

            base.Draw(gameTime);
        }

        void UpdateFire()
        {
            const int fireParticlesPerFrame = 30;

            for (int i = 0; i < fireParticlesPerFrame; i++)
                fireParticles.AddParticle(
                    new Vector3(initPosition.X + rand.Next(3, 42), initPosition.Y - 2.7f, initPosition.Z),
                    Vector3.Zero);

            smokePlumeParticles.AddParticle(
                new Vector3(initPosition.X + rand.Next(3, 42), initPosition.Y - 2, initPosition.Z), Vector3.Zero);


            fireParticles.SetCamera(camera.ViewMatrix, camera.ProjectionMatrix);
            smokePlumeParticles.SetCamera(camera.ViewMatrix, camera.ProjectionMatrix);
        }

        void DrawQuad(BasicEffect effect, Quad _quad, Vector3 position, Texture2D texture)
        {
            if(_quad.Vertices == null) return;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                effect.Texture = texture;
                effect.World = Matrix.CreateTranslation(position);

                GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    _quad.Vertices, 0, 4,
                    _quad.Indexes, 0, 2);
            }

            GraphicsDevice.BlendState = BlendState.Opaque;

        }

        static void wallCollision(Entity sender, Entity other, CollisionPair collisionPair)
        {
            if (other.Tag is bool)
                other.Tag = true;

            if (other.CompoundBody != null && other.CompoundBody.Tag is Player)
                ((Player)other.CompoundBody.Tag).Hurt(200);
        }
    }

    
    internal delegate void PipeLineMoved(bool up);
    internal delegate void PipeLineInitMove(bool up);

    class PipeLineCorner : DrawableGameComponent
    {
        private readonly Box entity;
        private readonly Camera camera;
        private Model model;

        private readonly float initY;
        private bool opened;
        private bool openRequest;
        private bool closeRequest;

        public int ActivationBtnID { get; set; }

        public event PipeLineMoved OnPipeLineMoved;
        public event PipeLineInitMove OnPipeLineInitMove;

        public PipeLineCorner(Game game, Space space, Camera camera, Vector3 position) 
            : base(game)
        {
            entity = new Box(position, 6.460005f, 12.47999f, 4f);
            space.Add(entity);
            this.camera = camera;
            initY = position.Y;
        }

        public void Open()
        {
            if (!opened)
            {
                openRequest = true;
                OnPipeLineInitMove(true);
            }
        }

        public void Close()
        {
            if(opened)
            {
                closeRequest = true;
                OnPipeLineInitMove(false);
            }
        }

        public void LoadContent(ContentManager content)
        {
            model = content.Load<Model>("Models\\Tube_Part");
            EffectMaker.SetObjectEffect(GetType(), model, content);
        }

        public override void Update(GameTime gameTime)
        {
            if(openRequest)
            {
                entity.LinearVelocity = new Vector3(0,1f,0);
                if(entity.CenterPosition.Y - initY > 6f)
                {
                    entity.LinearVelocity = Vector3.Zero;
                    openRequest = false;
                    opened = true;
                    if(OnPipeLineMoved != null)
                        OnPipeLineMoved(true);
                }
            }else if(closeRequest)
            {
                entity.LinearVelocity = new Vector3(0, -1, 0);
                if(entity.CenterPosition.Y <= initY)
                {
                    entity.CenterPosition = new Vector3(entity.CenterPosition.X, initY, entity.CenterPosition.Z);
                    closeRequest = false;
                    entity.LinearVelocity = Vector3.Zero;
                    opened = false;
                    if (OnPipeLineMoved != null)
                        OnPipeLineMoved(false);
                }
            }
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (var mesh in model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    var effect = (Effect)meshPart.Tag;
                    effect.CurrentTechnique = effect.Techniques["Technique1"];

                    effect.Parameters["World"].SetValue(entity.WorldTransform*Matrix.CreateTranslation(-2, -4, 0));
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
