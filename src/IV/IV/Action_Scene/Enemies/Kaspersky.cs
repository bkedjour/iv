using System;
using System.Collections.Generic;
using BEPUphysics;
using BEPUphysics.Entities;
using IV.Action_Scene.Effects;
using IVSaveLoadAssembly;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Action_Scene.Enemies
{
    class Kaspersky : Enemy
    {
        //Fire Effect
        private Quad fireQuad;
        private BasicEffect fireBasicEffect;
        private QuadAnimation fireAnim_R, fireAnim_L;
        private QuadAnimationPlayer fireAnimPlayer;
        private float rayWidth;

        private readonly TimeSpan shootDuration = TimeSpan.FromSeconds(2.5f);
        private TimeSpan timer;
        private bool shooting;

        //Collision Effect
        private Quad collQuad;
        private BasicEffect collBasicEffect;
        private QuadAnimation collAnim;
        private QuadAnimationPlayer collAnimPlayer;
        private bool collision;

        //Egon Effect
        private List<Quad> egonQuads;
        private BasicEffect egonBasicEffect;
        private Texture2D egonTexture;

        private Cue cue;

        public Kaspersky(Game game, Space space, Camera camera, Vector3 position, Random rand, Player player,
            List<GameComponent> gameComponent) 
            : base(game, space, camera, position, EnemyType.Kaspersky, rand, player, gameComponent)
        {
            Strength = 100;
            MaxtimeToShoot = TimeSpan.FromSeconds(4);
            Body = new Box(position, 6f, 2, 1.8f, 10);
            Body.EventManager.InitialCollisionDetected += collisionDetected;
            space.Add(Body);
            Body.Tag = this;

            playerDistance = new Vector3(30, 4, 0);
        }

        public override void LoadContent(ContentManager Content)
        {
            model = Content.Load<Model>("Models\\AV_Laspersky");
            EffectMaker.SetObjectEffect(GetType(), model, Content);

            fireBasicEffect = new BasicEffect(GraphicsDevice);
            collBasicEffect = new BasicEffect(GraphicsDevice);
            egonBasicEffect = new BasicEffect(GraphicsDevice);

            var fireTextures_R = new List<Texture2D>();
            for (int i = 0; i < 10; i++)
                fireTextures_R.Add(Content.Load<Texture2D>(string.Format("Effects\\Laspersky\\Beam\\{0}_R", i)));
            fireAnim_R = new QuadAnimation(fireTextures_R, .05f, true);
            var fireTextures_L = new List<Texture2D>();
            for (int i = 0; i < 10; i++)
                fireTextures_L.Add(Content.Load<Texture2D>(string.Format("Effects\\Laspersky\\Beam\\{0}_L", i)));
            fireAnim_L = new QuadAnimation(fireTextures_L, .05f, true);
            fireAnimPlayer = new QuadAnimationPlayer();


            var collTextures = new List<Texture2D>();
            for (int i = 0; i < 5; i++)
                collTextures.Add(Content.Load<Texture2D>(string.Format("Effects\\Laspersky\\Col\\{0}", i)));
            collAnim = new QuadAnimation(collTextures, .05f, true);
            collAnimPlayer = new QuadAnimationPlayer();
            collAnimPlayer.PlayAnimation(collAnim);

            egonTexture = Content.Load<Texture2D>("Effects\\Laspersky\\Egon");
            egonQuads =new List<Quad>();

            base.LoadContent(Content);
        }

        protected override void Shoot()
        {
            shooting = true;
            fireAnimPlayer.PlayAnimation(isRightDirection ? fireAnim_R : fireAnim_L);
            cue = soundManager.Play3DSound("laspersky_shoot",Body.CenterPosition);
        }

        Entity GetMinimum(List<Entity> entities)
        {
            Entity toRet = entities[0];
            for (int i = 1; i < entities.Count; i++)
                if (isRightDirection)
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

        public override void Update(GameTime gameTime)
        {
            if(destroyed)
            {
                base.Update(gameTime);
                shooting = false;
                collision = false;
                soundManager.StopSound(cue);
                return;
            }

            base.Update(gameTime);


            fireBasicEffect.View = camera.ViewMatrix;
            fireBasicEffect.Projection = camera.ProjectionMatrix;
            collBasicEffect.View = camera.ViewMatrix;
            collBasicEffect.Projection = camera.ProjectionMatrix;
            egonBasicEffect.View = camera.ViewMatrix;
            egonBasicEffect.Projection = camera.ProjectionMatrix;


            if (shooting)
            {
                timer += gameTime.ElapsedGameTime;
                if (timer >= shootDuration)
                {
                    timer = TimeSpan.Zero;
                    shooting = false;
                    rayWidth = 0;
                    soundManager.StopSound(cue);
                }
            }

            if (isTurning)
            {
                shooting = false;
                rayWidth = 0;
                timer = TimeSpan.Zero;
                soundManager.StopSound(cue);
            }

            if (shooting)
            {
                var position =
                    new Vector3(
                        Body.CenterPosition.X + (isRightDirection ? Body.HalfWidth + .3f : -Body.HalfWidth - .3f),
                        Body.CenterPosition.Y + .8f,
                        Body.CenterPosition.Z);

                var hitEntitys = new List<Entity>();
                if (space.RayCast(position, isRightDirection ? Vector3.Right : Vector3.Left, rayWidth, true, hitEntitys,
                                  new List<Vector3>(),
                                  new List<Vector3>(), new List<float>()))
                {
                    collision = true;
                    var smallest = GetMinimum(hitEntitys);
                    if (smallest is Box)
                    {
                        rayWidth = isRightDirection
                                       ? (smallest.CenterPosition.X - ((Box) smallest).HalfWidth) -
                                         (Body.CenterPosition.X + Body.HalfWidth + .3f)
                                       : (Body.CenterPosition.X - Body.HalfWidth + .3f) -
                                         (smallest.CenterPosition.X + ((Box) smallest).HalfWidth);


                        collQuad = new Quad(
                            new Vector3(smallest.CenterPosition.X + (isRightDirection
                                                                         ? -((Box) smallest).HalfWidth - .8f
                                                                         : ((Box) smallest).HalfWidth) + .55f,
                                        Body.CenterPosition.Y + height + .8f,
                                        Body.CenterPosition.Z + .1f), Vector3.Backward, Vector3.Up, 3f, 3f);
                    }
                    else if (smallest.Tag is Player)
                    {
                        rayWidth = isRightDirection
                                       ? (smallest.CenterPosition.X -
                                          ((Box) ((Player) smallest.Tag).Body.SubBodies[0]).HalfWidth) -
                                         (Body.CenterPosition.X + Body.HalfWidth + .3f) + .75f
                                       : (Body.CenterPosition.X - Body.HalfWidth + .3f) -
                                         (smallest.CenterPosition.X +
                                          ((Box) ((Player) smallest.Tag).Body.SubBodies[0]).HalfWidth);


                        collQuad = new Quad(
                            new Vector3(smallest.CenterPosition.X + (isRightDirection
                                                                         ? -((Box)
                                                                             ((Player) smallest.Tag).Body.SubBodies[0]).
                                                                                HalfWidth + .8f
                                                                         : ((Box)
                                                                            ((Player) smallest.Tag).Body.SubBodies[0]).
                                                                               HalfWidth - .5f),
                                        Body.CenterPosition.Y + height + .8f,
                                        Body.CenterPosition.Z + .1f), Vector3.Backward, Vector3.Up, 3f,3f);

                        ((Player)smallest.Tag).Hurt(1);
                    }
                }
                else
                {
                    rayWidth += 1f;
                    collision = false;
                }


                fireQuad =
                    new Quad(
                        new Vector3(
                            Body.CenterPosition.X +
                            (isRightDirection
                                 ? (rayWidth + Body.HalfWidth)/2.0f - .5f
                                 : -(Body.HalfWidth + rayWidth)/2.0f + .5f),
                            Body.CenterPosition.Y + height + .8f,
                            Body.CenterPosition.Z), Vector3.Backward, Vector3.Up, rayWidth + Body.HalfWidth +1.5f,
                        1.5f);

                egonQuads = new List<Quad>();
                for (var i = 1; i < (int)(rayWidth / 1.5) + Body.HalfWidth/1.5f; i++)
                    egonQuads.Add(
                        new Quad(
                            new Vector3(Body.CenterPosition.X + (isRightDirection
                                                                     ? ( (1.5f*i) + 1.3f)
                                                                     : (-(1.5f*i) - 1.3f)),
                                        Body.CenterPosition.Y + height + .8f,
                                        Body.CenterPosition.Z + .05f),
                            Vector3.Backward, Vector3.Up, 1.5f, 1.5f));
            }


        }


        public override void Draw(GameTime gameTime)
        {
            if(destroyed && timeToEnd > TimeSpan.FromSeconds(.2f))
            {
                base.Draw(gameTime);
                return;
            }
            var transform = Matrix.CreateRotationX(MathHelper.ToRadians(rotX))*Matrix.CreateRotationY(MathHelper.Pi)*
                            Matrix.CreateTranslation(new Vector3(-1.7f, height + .5f, 0));

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

            if (shooting)
            {
                fireAnimPlayer.Update(gameTime);
                DrawQuad(fireBasicEffect, fireQuad, Vector3.Zero, fireAnimPlayer.Texture);
                foreach (var quad in egonQuads)
                    DrawQuad(egonBasicEffect, quad, Vector3.Zero, egonTexture);
            }
            if(shooting && collision)
            {
                collAnimPlayer.Update(gameTime);
                DrawQuad(collBasicEffect,collQuad,Vector3.Zero,collAnimPlayer.Texture);
            }

            base.Draw(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            soundManager.StopSound(cue);
            base.Dispose(disposing);
        }
    }
}
