using System;
using System.Collections.Generic;
using BEPUphysics;
using BEPUphysics.Entities;
using IV.Action_Scene.Effects;
using IV.Action_Scene.ParticleSystems;
using IV.Action_Scene.ParticleSystems.Core;
using IV.Action_Scene.Weapons;
using IVSaveLoadAssembly;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Action_Scene.Enemies
{
    class PlazmaGun : DrawableGameComponent
    {
        private Model model;
        private readonly Box entity;
        private readonly Space space;
        private readonly Camera camera;
        private readonly List<GameComponent> components;
        private readonly Player player;
        private Vector3 playerDistance;
        private readonly TimeSpan timeBetweenShoot;
        private TimeSpan timer;
        private readonly bool isRightDirection;
        private int Strength = 100;
        private bool wasZoomingOut;

        //Float Mouvement
        private float height;
        private TimeSpan mouvTimer;
        private bool heightUp;
        private float rotX;
        private bool rotPos;

        //Load Effect
        private Quad loadQuad;
        private BasicEffect loadBasicEffect;
        private QuadAnimation loadAnim;
        private QuadAnimationPlayer loadAnimPlayer;

        //Fire Effect
        private Quad fireQuad;
        private BasicEffect fireBasicEffect;
        private QuadAnimation fireAnim;
        private QuadAnimationPlayer fireAnimPlayer;
        private float fireWidth = 50;

        private EnemyBullet bullet;
        private bool shooting, loading;
        private TimeSpan timeToSharge;
        private readonly TimeSpan chargeTime = TimeSpan.FromSeconds(.7f);


        private bool destroyed;

        private ParticleSystem explosionSmokeParticle;
        private ParticleSystem explosionParticle;
        private TimeSpan timeToEnd;


        private readonly SoundManager soundManager;
        private bool playSound;

        public PlazmaGun(Game game, Space space, Camera camera, List<GameComponent> components, Vector3 position, Player player
            , bool isRightDirection) 
            : base(game)
        {
            this.space = space;
            this.camera = camera;
            this.components = components;
            this.isRightDirection = isRightDirection;
            entity = new Box(position, 6.891958f, 3.124004f, 2.664023f, 1000) {Tag = this};
            space.Add(entity);
            entity.EventManager.InitialCollisionDetected += collisionDetected;
            this.player = player;
            playerDistance = new Vector3(30, 4, 0);
            timer = timeBetweenShoot = TimeSpan.FromSeconds(3);
            //timeToSharge = TimeSpan.FromSeconds(2);
            Strength = 300;
            loadQuad =
                new Quad(
                    new Vector3(position.X + (isRightDirection ? entity.HalfWidth : -entity.HalfWidth), position.Y,
                                position.Z), Vector3.Backward,
                    Vector3.Up, 6, 6);
       
            fireQuad =
                new Quad(new Vector3(position.X + (isRightDirection ? entity.HalfWidth : -entity.HalfWidth), position.Y,
                                     position.Z), Vector3.Backward, Vector3.Up, fireWidth, 2);


            soundManager = (SoundManager) Game.Services.GetService(typeof (SoundManager));
        }

        private static void collisionDetected(Entity sender, Entity other, CollisionPair collisionpair)
        {
            if (other.CompoundBody != null && other.CompoundBody.Tag is Player)
                ((Player)other.CompoundBody.Tag).Die(DeathType.Electricity);
        }

        public void LoadContent(ContentManager content)
        {
            model = content.Load<Model>("Models\\Palasma_Cannon");
            EffectMaker.SetObjectEffect(GetType(), model, content);

            loadBasicEffect = new BasicEffect(GraphicsDevice);
            fireBasicEffect = new BasicEffect(GraphicsDevice);

            var loadTextures = new List<Texture2D>();
            for (int i = 0; i < 14; i++)
                loadTextures.Add(content.Load<Texture2D>(string.Format("Effects\\Plasma_Gun\\Load\\{0}_{1}", i,
                                                                       isRightDirection ? "R" : "L")));
            loadAnim = new QuadAnimation(loadTextures, .05f, true);
            loadAnimPlayer = new QuadAnimationPlayer();
            loadAnimPlayer.PlayAnimation(loadAnim);

            var fireTextures = new List<Texture2D>();
            for (int i = 0; i < 16; i++)
                fireTextures.Add(content.Load<Texture2D>(string.Format("Effects\\Plasma_Gun\\Fire\\{0}_{1}", i,
                                                                       isRightDirection ? "R" : "L")));
            fireAnim = new QuadAnimation(fireTextures, .05f, false);
            fireAnimPlayer = new QuadAnimationPlayer();
            fireAnimPlayer.PlayAnimation(fireAnim);


            explosionSmokeParticle = new ExplosionSmokeParticleSystem(Game, content) { DrawOrder = 200 };
            explosionSmokeParticle.Initialize();
            components.Add(explosionSmokeParticle);

            explosionParticle = new ExplosionParticleSystem(Game, content) { DrawOrder = 400 };
            explosionParticle.Initialize();
            components.Add(explosionParticle);

            Shoot(false);
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
            HandelFloatMouvement(gameTime);

            if (isRightDirection)
                entity.OrientationMatrix = Matrix.CreateRotationY(MathHelper.Pi);

            if (fireAnimPlayer.EndAnimation)
                shooting = false;

            if (Math.Abs(entity.CenterPosition.X + (isRightDirection ? 10 : -10) -
                                 player.Body.CenterPosition.X) < playerDistance.X
                        && Math.Abs(entity.CenterPosition.Y - player.Body.CenterPosition.Y) < playerDistance.Y)
                if ((isRightDirection && player.Body.CenterPosition.X > entity.CenterPosition.X)
                    || (!isRightDirection && player.Body.CenterPosition.X < entity.CenterPosition.X))
                    if (!loading && !shooting)
                        Shoot(true);



            loadBasicEffect.View = camera.ViewMatrix;
            loadBasicEffect.Projection = camera.ProjectionMatrix;
            fireBasicEffect.View = camera.ViewMatrix;
            fireBasicEffect.Projection = camera.ProjectionMatrix;

            if (loading)
            {
                timeToSharge += gameTime.ElapsedGameTime;
                if (timeToSharge >= chargeTime)
                {
                    shooting = true;
                    loading = false;
                    if (playSound)
                        soundManager.Play3DSound("plasma_shoot", entity.CenterPosition);

                    timeToSharge = chargeTime;
                    var posi = new Vector3(entity.CenterPosition.X + (isRightDirection
                                                                          ? entity.HalfWidth + 1
                                                                          : -entity.HalfWidth - 1),
                                           entity.CenterPosition.Y,
                                           entity.CenterPosition.Z);
                    bullet = new EnemyBullet(Game, space, camera, posi, 2.5f, 100, isRightDirection,
                                             components);
                    components.Add(bullet);
                    fireAnimPlayer.PlayAnimation(fireAnim);
                }
                else
                {
                    loadQuad =
                        new Quad(
                            new Vector3(
                                entity.CenterPosition.X + (isRightDirection ? entity.HalfWidth : -entity.HalfWidth),
                                entity.CenterPosition.Y,
                                entity.CenterPosition.Z), Vector3.Backward,
                            Vector3.Up, 6, 6);
                    
                    loadAnimPlayer.Update(gameTime);
                }
            }

            if (bullet != null && shooting)
            {
                if (isRightDirection)
                    fireWidth = bullet.Entity.CenterPosition.X - (entity.CenterPosition.X + entity.HalfWidth);
                else
                    fireWidth = (entity.CenterPosition.X - entity.HalfWidth) - bullet.Entity.CenterPosition.X;
                if (!bullet.Destroyed)
                    fireQuad =
                        new Quad(
                            new Vector3(
                                isRightDirection
                                    ? (entity.CenterPosition.X + entity.HalfWidth + fireWidth/2.0f)
                                    : bullet.Entity.CenterPosition.X + fireWidth/2.0f,
                                entity.CenterPosition.Y,
                                entity.CenterPosition.Z), Vector3.Backward, Vector3.Up, fireWidth, 3);
            }

            timer += gameTime.ElapsedGameTime;

            base.Update(gameTime);
        }

        void HandelFloatMouvement(GameTime gameTime)
        {
            if (Math.Abs(entity.CenterPosition.X - player.Body.CenterPosition.X) < playerDistance.X + 20
              && Math.Abs(entity.CenterPosition.Y - player.Body.CenterPosition.Y) < playerDistance.Y + 10)
            {
                camera.ZoomOut(50);
                wasZoomingOut = true;
            }
            else if (wasZoomingOut)
            {
                camera.ZoomIn();
                wasZoomingOut = false;
            }

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
                rotX += rotPos ? .07f : -.07f;

                if (rotX > 10)
                    rotPos = false;
                if (rotX < -10)
                    rotPos = true;
            }

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

        void Shoot(bool _playSound)
        {
            if (timer >= timeBetweenShoot)
            {
                timer = TimeSpan.Zero;
                loading = true;
                timeToSharge -= chargeTime;
                loadAnimPlayer.PlayAnimation(loadAnim);
                playSound = _playSound;
                if (playSound)
                    soundManager.Play3DSound("plasma_charge", entity.CenterPosition);
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

            var transform = Matrix.CreateRotationX(MathHelper.ToRadians(rotX)) *
                            Matrix.CreateTranslation(new Vector3(0, height + .5f, 0));
           
            foreach (var mesh in model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    var effect = (Effect)meshPart.Tag;
                    effect.CurrentTechnique = effect.Techniques["Technique1"];

                    effect.Parameters["World"].SetValue(transform*entity.WorldTransform);
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["LightPosition"].SetValue(EffectMaker.LightPosition);
                    effect.Parameters["CameraPosition"].SetValue(camera.Position);

                    meshPart.Effect = effect;

                }
                mesh.Draw();
            }

            if (loading)
                DrawQuad(loadBasicEffect, loadQuad, new Vector3(isRightDirection ? 3 : -3, height + .5f, 0),
                         loadAnimPlayer.Texture);

            if (shooting)
            {
                DrawQuad(fireBasicEffect, fireQuad, new Vector3(0, height + .5f, 0), fireAnimPlayer.Texture);
                fireAnimPlayer.Update(gameTime);
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
