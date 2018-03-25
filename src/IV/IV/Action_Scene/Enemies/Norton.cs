using System;
using System.Collections.Generic;
using BEPUphysics;
using BEPUphysics.Entities;
using IV.Action_Scene.Effects;
using IV.Action_Scene.ParticleSystems;
using IV.Action_Scene.Weapons;
using IVSaveLoadAssembly;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Action_Scene.Enemies
{
    class Norton : Enemy
    {
        //Load Effect 1
        private Quad loadQuad1;
        private BasicEffect loadBasicEffect1;
        private QuadAnimation loadAnim_R, loadAnim_L;
        private QuadAnimationPlayer loadAnimPlayer1;

        //Load Effect 2
        private Quad loadQuad2;
        private BasicEffect loadBasicEffect2;
        private QuadAnimationPlayer loadAnimPlayer2;

        //Fire Effect
        private Quad fireQuad1;
        private BasicEffect fireBasicEffect1;
        private QuadAnimation fireAnim_R, fireAnim_L;
        private QuadAnimationPlayer fireAnimPlayer1;

        //Fire Effect 2
        private Quad fireQuad2;
        private BasicEffect fireBasicEffect2;
        private QuadAnimationPlayer fireAnimPlayer2;

        private EnemyBullet bullet1,bullet2;
        private bool shooting1, loading1, shooting2, loading2;
        private TimeSpan timeToSharge1, timeToSharge2;
        private readonly TimeSpan chargeTime = TimeSpan.FromSeconds(.7f);
        private TimeSpan timeToSecondShoot;
        private readonly TimeSpan timeBetweenShoots = TimeSpan.FromMilliseconds(300);
        private bool once = true;


        //Particle Effect
        private Projectile projectile1;
        private Projectile projectile2;

        private ShortonParicleSystem projectileTrailParticles;


        public Norton(Game game, Space space, Camera camera, Vector3 position, Random rand, Player player, 
            List<GameComponent> gameComponent) 
            : base(game, space, camera, position, EnemyType.Norton, rand, player, gameComponent)
        {
            Strength = 200;
            MaxtimeToShoot = timeToShoot = TimeSpan.FromSeconds(2);
            Body = new Box(position, 6f, 2, 1.8f, 10) {Tag = this};
            Body.EventManager.InitialCollisionDetected += collisionDetected;
            space.Add(Body);

            playerDistance = new Vector3(30, 4, 0);
        }

        public override void LoadContent(ContentManager Content)
        {
            model = Content.Load<Model>("Models\\AV_Shorton");
            EffectMaker.SetObjectEffect(GetType(), model, Content);

            loadBasicEffect1 = new BasicEffect(GraphicsDevice);
            loadBasicEffect2 = new BasicEffect(GraphicsDevice);
            fireBasicEffect1 = new BasicEffect(GraphicsDevice);
            fireBasicEffect2 = new BasicEffect(GraphicsDevice);

            var loadTextures_R = new List<Texture2D>();
            for (int i = 0; i < 13; i++)
                loadTextures_R.Add(Content.Load<Texture2D>(string.Format("Effects\\Shorton\\Load\\{0}_R", i)));

            var loadTextures_L = new List<Texture2D>();
            for (int i = 0; i < 13; i++)
                loadTextures_L.Add(Content.Load<Texture2D>(string.Format("Effects\\Shorton\\Load\\{0}_L", i)));

            loadAnim_R = new QuadAnimation(loadTextures_R, .09f, true);
            loadAnim_L = new QuadAnimation(loadTextures_L, .09f, true);
            loadAnimPlayer1 = new QuadAnimationPlayer();
            loadAnimPlayer2 = new QuadAnimationPlayer();
            

            var fireTextures_R = new List<Texture2D>();
            for (int i = 0; i < 15; i++)
                fireTextures_R.Add(Content.Load<Texture2D>(string.Format("Effects\\Shorton\\Fire\\{0}_R", i)));
            var fireTextures_L = new List<Texture2D>();
            for (int i = 0; i < 15; i++)
                fireTextures_L.Add(Content.Load<Texture2D>(string.Format("Effects\\Shorton\\Fire\\{0}_L", i)));

            fireAnim_R = new QuadAnimation(fireTextures_R, .05f, true);
            fireAnim_L = new QuadAnimation(fireTextures_L, .05f, true);
            fireAnimPlayer1 = new QuadAnimationPlayer();
            fireAnimPlayer2 = new QuadAnimationPlayer();

            projectileTrailParticles = new ShortonParicleSystem(Game, Content) { DrawOrder = 300 };
            projectileTrailParticles.Initialize();
            gameComponent.Add(projectileTrailParticles);
            base.LoadContent(Content);
        }

        protected override void Shoot()
        {
            loading1 = true;
            timeToSharge1 -= chargeTime;
            loadAnimPlayer1.PlayAnimation(isRightDirection ? loadAnim_R : loadAnim_L);
            once = false;
            timeToSecondShoot = TimeSpan.Zero;
        }

        public override void Update(GameTime gameTime)
        {
            if (destroyed)
            {
                base.Update(gameTime);
                shooting1 = shooting2 = false;
                loading1 = loading2 = false;
                return;
            }

            base.Update(gameTime);

            loadBasicEffect1.View = camera.ViewMatrix;
            loadBasicEffect1.Projection = camera.ProjectionMatrix;
            loadBasicEffect2.View = camera.ViewMatrix;
            loadBasicEffect2.Projection = camera.ProjectionMatrix;
            fireBasicEffect1.View = camera.ViewMatrix;
            fireBasicEffect1.Projection = camera.ProjectionMatrix;
            fireBasicEffect2.View = camera.ViewMatrix;
            fireBasicEffect2.Projection = camera.ProjectionMatrix;

            if (isTurning)
                shooting1 = shooting2 = false;

            if (loading1)
            {
                timeToSharge1 += gameTime.ElapsedGameTime;
                if (timeToSharge1 >= chargeTime)
                {
                    shooting1 = true;
                    loading1 = false;
                    soundManager.Play3DSound("shorton_shoot", Body.CenterPosition);
                    timeToSharge1 = chargeTime;
                    var posi =
                        new Vector3(
                            Body.CenterPosition.X + (isRightDirection ? Body.HalfWidth + 2.5f : -Body.HalfWidth - 2.5f),
                            Body.CenterPosition.Y + height + .5f,
                            Body.CenterPosition.Z - .4f);
                    bullet1 = new EnemyBullet(Game, space, camera, posi, 1.5f, 40, isRightDirection, gameComponent);
                    gameComponent.Add(bullet1);
                    fireAnimPlayer1.PlayAnimation(isRightDirection ? fireAnim_R : fireAnim_L);
                }
                else
                {
                    loadQuad1 =
                        new Quad(
                            new Vector3(Body.CenterPosition.X + (isRightDirection ? Body.HalfWidth : -Body.HalfWidth),
                                        Body.CenterPosition.Y,
                                        Body.CenterPosition.Z), Vector3.Backward, Vector3.Up, 3, 3);

                    loadAnimPlayer1.Update(gameTime);
                }
            }

            if (bullet1 != null && shooting1)
            {
                fireQuad1 = new Quad(
                    new Vector3(bullet1.Entity.CenterPosition.X + (isRightDirection ? - 2.5f : 2.5f),
                                bullet1.Entity.CenterPosition.Y,
                                bullet1.Entity.CenterPosition.Z - 1), Vector3.Backward, Vector3.Up, 6, 3);
                fireAnimPlayer1.Update(gameTime);


                projectile1 = new Projectile(projectileTrailParticles,
                                             new Vector3(bullet1.Entity.CenterPosition.X - 1,
                                                         bullet1.Entity.CenterPosition.Y,
                                                         bullet1.Entity.CenterPosition.Z));

                projectile1.Update(gameTime);


                if (bullet1.Destroyed)
                    shooting1 = false;
            }

            timeToSecondShoot += gameTime.ElapsedGameTime;
            if (!once && timeToSecondShoot >= timeBetweenShoots)
            {
                once = true;
                timeToSecondShoot -= timeBetweenShoots;
                loading2 = true;
                timeToSharge2 -= chargeTime;
                loadAnimPlayer2.PlayAnimation(isRightDirection ? loadAnim_R : loadAnim_L);
            }
            if (loading2)
            {
                timeToSharge2 += gameTime.ElapsedGameTime;
                if (timeToSharge2 >= chargeTime)
                {
                    shooting2 = true;
                    loading2 = false;
                    soundManager.Play3DSound("shorton_shoot", Body.CenterPosition);
                    timeToSharge2 = chargeTime;
                    var posi =
                        new Vector3(
                            Body.CenterPosition.X + (isRightDirection ? Body.HalfWidth + 2.5f : -Body.HalfWidth - 2.5f),
                            Body.CenterPosition.Y + height + .5f,
                            Body.CenterPosition.Z + .4f);
                    bullet2 = new EnemyBullet(Game, space, camera, posi, 1.5f, 40, isRightDirection, gameComponent);
                    gameComponent.Add(bullet2);
                    fireAnimPlayer2.PlayAnimation(isRightDirection ? fireAnim_R : fireAnim_L);
                }
                else
                {
                    loadQuad2 =
                        new Quad(
                            new Vector3(Body.CenterPosition.X + (isRightDirection ? Body.HalfWidth : -Body.HalfWidth),
                                        Body.CenterPosition.Y,
                                        Body.CenterPosition.Z), Vector3.Backward, Vector3.Up, 3, 3);

                    loadAnimPlayer2.Update(gameTime);
                }
            }
            if (bullet2 != null && shooting2)
            {
                fireQuad2 = new Quad(
                    new Vector3(bullet2.Entity.CenterPosition.X + (isRightDirection ? -2.5f : 2.5f),
                                bullet2.Entity.CenterPosition.Y,
                                bullet2.Entity.CenterPosition.Z + .85f), Vector3.Backward, Vector3.Up, 6, 3);
                fireAnimPlayer2.Update(gameTime);

                projectile2 = new Projectile(projectileTrailParticles,
                                             new Vector3(bullet2.Entity.CenterPosition.X - 1,
                                                         bullet2.Entity.CenterPosition.Y,
                                                         bullet2.Entity.CenterPosition.Z));

                projectile2.Update(gameTime);

                if (bullet2.Destroyed)
                    shooting2 = false;
            }

        }

        public override void Draw(GameTime gameTime)
        {
            projectileTrailParticles.SetCamera(camera.ViewMatrix, camera.ProjectionMatrix);

            if (destroyed && timeToEnd > TimeSpan.FromSeconds(.2f))
            {
                base.Draw(gameTime);
                return;
            }

            Matrix transform = Matrix.CreateRotationX(MathHelper.ToRadians(rotX)) * Matrix.CreateRotationY(MathHelper.Pi) *
                            Matrix.CreateTranslation(new Vector3(-.9f, height - .5f, 0));
           
            foreach (var mesh in model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    var effect = (Effect)meshPart.Tag;
                    effect.CurrentTechnique = effect.Techniques["Technique1"];

                    effect.Parameters["World"].SetValue(transform * Body.WorldTransform);
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["LightPosition"].SetValue(EffectMaker.LightPosition);
                    effect.Parameters["CameraPosition"].SetValue(camera.Position);

                    meshPart.Effect = effect;

                }
                mesh.Draw();
            }

            if (loading1)
                DrawQuad(loadBasicEffect1, loadQuad1, new Vector3(isRightDirection ? .8f : -.8f, height + .3f, -1.5f),
                         loadAnimPlayer1.Texture);
            if (shooting1)
                DrawQuad(fireBasicEffect1, fireQuad1, Vector3.Zero, fireAnimPlayer1.Texture);
            if (loading2)
                DrawQuad(loadBasicEffect2, loadQuad2, new Vector3(isRightDirection ? .8f : -.8f, height + .3f, 1.25f),
                         loadAnimPlayer2.Texture);
            if (shooting2)
                DrawQuad(fireBasicEffect2, fireQuad2, Vector3.Zero, fireAnimPlayer2.Texture);



            base.Draw(gameTime);
        }
    }
}
