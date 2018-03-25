using System;
using System.Collections.Generic;
using BEPUphysics;
using BEPUphysics.Entities;
using IV.Action_Scene.Enemies;
using IVSaveLoadAssembly;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Action_Scene.Objects
{
    class AntiVirusSys : DrawableGameComponent
    {
        private LaserWall laser;
        private TurnDoor door;
        private SmallGear gear1, gear2;
        private Box blocker;
        private Gear bigGear;
        private readonly Space space;
        private readonly Camera camera;
        private readonly List<GameComponent> _components;
        private readonly Entity player;
        private bool zoomOnce;

        private Model kasper, norton;
        private bool isKasper;
        private TimeSpan timeToSwitchEnemy;
        private Box enemyGene;
        private Vector3 enemyGenPosi;

        private readonly SoundManager soundManager;
        private Cue alarmSound;

        public AntiVirusSys(Game game, Space space, Camera camera, List<GameComponent> components,Entity player) 
            : base(game)
        {
            this.space = space;
            this.camera = camera;
            this.player = player;
            _components = components;

            soundManager = (SoundManager) Game.Services.GetService(typeof (SoundManager));
        }

        public void LoadContent(ContentManager Content)
        {
            laser.Initialize();
            door.Initialize();
            laser.LoadContent(Content);
            laser.Disactivate();
            door.LoadContent(Content);
            bigGear.LoadContent(Content);
            gear1.LoadContent(Content);
            gear2.LoadContent(Content);
            _components.Add(laser);
            _components.Add(door);
            _components.Add(bigGear);
            _components.Add(gear1);
            _components.Add(gear2);

            kasper = Content.Load<Model>("Models\\AV_Laspersky");
            EffectMaker.SetObjectEffect(typeof (Kaspersky), kasper, Content);
            norton = Content.Load<Model>("Models\\AV_Shorton");
            EffectMaker.SetObjectEffect(typeof (Norton), norton, Content);
            enemyGenPosi = new Vector3(263.84f, 110f, 4.328447f);
            enemyGene = new Box(enemyGenPosi, 1, 1, 1) {LinearVelocity = new Vector3(10, 0, 0)};
            space.Add(enemyGene);
        }

        public void SetLaser(Vector3 position)
        {
            laser = new LaserWall(Game, position, space, camera, LaserDirection.Down,_components);
        }

        public void SetDoor(Vector3 position,Vector3 dimension)
        {
            door = new TurnDoor(Game, camera, space, position, dimension, false);
        }

        public void  SetGears(Vector3 bigPosition,Vector3 smallPos1,Vector3 smallPos2)
        {
            gear1 = new SmallGear(Game, camera, space, smallPos1);
            gear2 = new SmallGear(Game, camera, space, smallPos2);
            bigGear = new Gear(Game, space, camera, bigPosition);
            gear1.Start(5);
            gear2.Start(5);
            bigGear.Start(3);
        }

        public void SetBlocker(Vector3 position,Vector3 dimension)
        {
            blocker = new Box(position, dimension.X, dimension.Y, dimension.Z);
            blocker.EventManager.InitialCollisionDetected += OnCollide;
            space.Add(blocker);
        }

        
        private void OnCollide(Entity sender, Entity other, CollisionPair collisionpair)
        {
            if (!(other.Tag is Cube)) return;
            bigGear.Stop();
            gear1.Stop();
            gear2.Stop();

            laser.Activate();
            door.Open();
            if (alarmSound == null)
                alarmSound = soundManager.Play3DSound("alarm_sound", Vector3.Zero);
        }

        protected override void Dispose(bool disposing)
        {
            soundManager.StopSound(alarmSound);
            base.Dispose(disposing);
        }

        public override void Update(GameTime gameTime)
        {

            if (Math.Abs(blocker.CenterPosition.X - player.CenterPosition.X) < 100 &&
                Math.Abs(blocker.CenterPosition.Y - player.CenterPosition.Y) < 25)
            {
                camera.ZoomOut(50);
                zoomOnce = true;
            }
            else if (zoomOnce)
            {
                zoomOnce = false;
                camera.ZoomIn();
            }
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            timeToSwitchEnemy += gameTime.ElapsedGameTime;
            if(timeToSwitchEnemy > TimeSpan.FromSeconds(3))
            {
                timeToSwitchEnemy = TimeSpan.Zero;
                var rand = new Random();
                if(rand.Next(2) == 0)
                    isKasper = !isKasper;
                enemyGene.CenterPosition = enemyGenPosi;
            }

            foreach (var mesh in isKasper ? kasper.Meshes : norton.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    var effect = (Effect)meshPart.Tag;
                    effect.CurrentTechnique = effect.Techniques["Technique1"];

                    effect.Parameters["World"].SetValue(Matrix.CreateRotationY(MathHelper.Pi)*enemyGene.WorldTransform*
                                                        (isKasper
                                                             ? Matrix.Identity
                                                             : Matrix.CreateTranslation(new Vector3(0, -1, 0))));
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


    class Gear : DrawableGameComponent
    {
        private readonly CompoundBody entity;
        private Model model;
        private readonly Camera camera;

        public Gear(Game game, Space space, Camera camera,Vector3 position) : base(game)
        {
            this.camera = camera;

            entity = new CompoundBody(false);

            #region Declaration
            entity.AddBody(new Box(new Vector3(-7.7645f, 1.859033f, -4.147278f), 3.777994f, 4.274022f, 7.616015f)
                               {
                                   OrientationMatrix = Matrix.CreateRotationZ(MathHelper.ToRadians(-17))
                               });
            entity.AddBody(new Box(new Vector3(-2.102266f, 7.188753f, -4.147278f), 4.335984f, 4.274022f, 7.616015f)
                               {
                                   OrientationMatrix = Matrix.CreateRotationZ(MathHelper.ToRadians(17))
                               });
            entity.AddBody(new Box(new Vector3(5.381957f, 5.354339f, -4.147278f), 3.777994f, 4.274022f, 7.616015f)
                               {
                                   OrientationMatrix = Matrix.CreateRotationZ(MathHelper.ToRadians(45))
                               });
            entity.AddBody(new Box(new Vector3(7.24728f, -2.071441f, -4.147278f), 3.777994f, 4.274022f, 7.616015f)
                               {
                                   OrientationMatrix = Matrix.CreateRotationZ(MathHelper.ToRadians(-17))
                               });
            entity.AddBody(new Box(new Vector3(1.734293f, -7.557037f, -4.147278f), 4.335984f, 4.274022f, 7.616015f)
                               {
                                   OrientationMatrix = Matrix.CreateRotationZ(MathHelper.ToRadians(17))
                               });
            entity.AddBody(new Box(new Vector3(-5.704686f, -5.632113f, -4.147278f), 3.777994f, 4.274022f, 7.616015f)
                               {
                                   OrientationMatrix = Matrix.CreateRotationZ(MathHelper.ToRadians(45))
                               });
            //Circle
            entity.AddBody(new Box(new Vector3(-5.146469f, 4.7829f, -4.147278f), 1.770001f, 4.219999f, 7.616015f)
                               {
                                   OrientationMatrix = Matrix.CreateRotationZ(MathHelper.ToRadians(-45))
                               });
            entity.AddBody(new Box(new Vector3(1.644553f, 6.3764f, -4.147278f), 4.079999f, 2.190001f, 7.616015f)
                               {
                                   OrientationMatrix = Matrix.CreateRotationZ(MathHelper.ToRadians(-15))
                               });
            entity.AddBody(new Box(new Vector3(6.013341f, 1.418633f, -4.147278f), 2.96f, 4.079999f, 7.616015f)
                               {
                                   OrientationMatrix = Matrix.CreateRotationZ(MathHelper.ToRadians(15))
                               });
            entity.AddBody(new Box(new Vector3(4.915377f, -5.027769f, -4.147278f), 1.770001f, 4.219999f, 7.616015f)
                               {
                                   OrientationMatrix = Matrix.CreateRotationZ(MathHelper.ToRadians(-45))
                               });
            entity.AddBody(new Box(new Vector3(-2.02284f, -6.740571f, -4.147278f), 4.079999f, 2.190001f, 7.616015f)
                               {
                                   OrientationMatrix = Matrix.CreateRotationZ(MathHelper.ToRadians(-15))
                               });
            entity.AddBody(new Box(new Vector3(-6.397552f, -2.037199f, -4.147278f), 2.96f, 4.079999f, 7.616015f)
                               {
                                   OrientationMatrix = Matrix.CreateRotationZ(MathHelper.ToRadians(15))
                               });
            #endregion

            space.Add(entity);
            entity.TeleportTo(position);
        }

        public void LoadContent(ContentManager content)
        {
            model = content.Load<Model>("Models\\Big_Gear");
            EffectMaker.SetObjectEffect(GetType(), model, content);
        }

       public void Stop()
       {
           entity.AngularVelocity = Vector3.Zero;
       }

        public void Start(float velocity)
        {
            entity.AngularVelocity = new Vector3(0, 0, velocity);
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
                                                        Matrix.CreateTranslation(new Vector3(0, 0, 3)));
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

    class SmallGear : DrawableGameComponent
    {
        private Model model;
        private readonly Camera camera;
        private readonly Box entity;

        public SmallGear(Game game, Camera camera,Space space,Vector3 position) : base(game)
        {
            this.camera = camera;
            entity = new Box(position, 1, 1, 1);
            space.Add(entity);
        }

        public void LoadContent(ContentManager content)
        {
            model = content.Load<Model>("Models\\SML_Gear");
            EffectMaker.SetObjectEffect(GetType(), model, content);
        }

        public void Stop()
        {
            entity.AngularVelocity = Vector3.Zero;
        }

        public void Start(float velocity)
        {
            entity.AngularVelocity = new Vector3(0, 0, velocity);
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (var mesh in model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    var effect = (Effect)meshPart.Tag;
                    effect.CurrentTechnique = effect.Techniques["Technique1"];

                    effect.Parameters["World"].SetValue(entity.WorldTransform);
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
