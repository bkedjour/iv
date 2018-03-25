using System;
using System.Collections.Generic;
using IV.Achievement;
using IV.Action_Scene.Effects;
using IV.Action_Scene.ParticleSystems;
using IV.Action_Scene.ParticleSystems.Core;
using IV.Action_Scene.Weapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Action_Scene
{
    partial class Player
    {
        private PlayerBullet sero1;
        private PlayerBullet sero2;
        private int previosSeroIndex;

        //Sero Effect
        private Quad quad1;
        private Quad quad2;
        private BasicEffect basicEffect;
        private QuadAnimation animation;
        private QuadAnimationPlayer animPlayer;


        private Projectile projectile1;
        private Projectile projectile2;

        private SeroParticleSystem projectileTrailParticles;

        //Rockets
        private PlayerBullet missil1;
        private PlayerBullet missil2;

        private Projectile rocketProjectile1;
        private Projectile rocketProjectile2;

        private ProjectileTrailParticleSystem rocketProjectileTrailParticles;

        private ParticleSystem rocketExplosionSmokeParticle;
        private ParticleSystem rocketExplosionParticle;

        //Objects
        private Texture2D ceroInd;
        private Texture2D healthInd1;
        private Texture2D healthInd2;
        private Texture2D healthInd3;
        private Texture2D rocketInd;
        private SpriteFont font;

        void LoadTheWeapons(ContentManager content)
        {
            var seroTextures = new List<Texture2D>();
            for (int i = 0; i < 10; i++)
                seroTextures.Add(content.Load<Texture2D>(string.Format("Effects\\Player\\Sero\\{0}", i)));

            ceroInd = content.Load<Texture2D>("Textures\\Objects\\CERO_icon");
            healthInd1 = content.Load<Texture2D>("Textures\\Objects\\HP_bar_0");
            healthInd2 = content.Load<Texture2D>("Textures\\Objects\\HP_bar_1");
            healthInd3 = content.Load<Texture2D>("Textures\\Objects\\HP_bar_2");
            rocketInd = content.Load<Texture2D>("Textures\\Objects\\MISSILE_icon");
            font = content.Load<SpriteFont>("Fonts\\gameFont");

            animation = new QuadAnimation(seroTextures, .05f, true);
            animPlayer = new QuadAnimationPlayer();
            animPlayer.PlayAnimation(animation);

            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.EnableDefaultLighting();
            basicEffect.PreferPerPixelLighting = true;
            basicEffect.TextureEnabled = true;

            projectileTrailParticles = new SeroParticleSystem(Game, content) { DrawOrder = 300 };
            projectileTrailParticles.Initialize();
            gameComponents.Add(projectileTrailParticles);

            rocketProjectileTrailParticles = new ProjectileTrailParticleSystem(Game, content) { DrawOrder = 300 };
            rocketProjectileTrailParticles.Initialize();
            gameComponents.Add(rocketProjectileTrailParticles);

            rocketExplosionSmokeParticle = new RocketCollisionParticleSystem(Game, content) { DrawOrder = 200 };
            rocketExplosionSmokeParticle.Initialize();
            gameComponents.Add(rocketExplosionSmokeParticle);

            rocketExplosionParticle = new RocketExplosionParticleSystem(Game, content) { DrawOrder = 400 };
            rocketExplosionParticle.Initialize();
            gameComponents.Add(rocketExplosionParticle);

            sero1 = new PlayerBullet(space, 1, Vector3.Zero, false, 30,TimeSpan.FromSeconds(.6f));
            sero2 = new PlayerBullet(space, 1, Vector3.Zero, false, 30,TimeSpan.FromSeconds(.6f));

            missil1 = new PlayerBullet(space, 1, new Vector3(0,0,200), false, 45, TimeSpan.FromSeconds(1));
            missil1.OnCollide += rocketCollide;
            missil2 = new PlayerBullet(space, 1, new Vector3(0,0,200), false, 45, TimeSpan.FromSeconds(1));
        }

        private void rocketCollide(object sender, RocketEventArg e)
        {
            soundManager.Play3DSound("messil_explo",Body.CenterPosition);
            for (int i = 0; i < 3; i++)
            {
                rocketExplosionParticle.AddParticle(e.Position, new Vector3(.1f, .1f, .1f));
                rocketExplosionSmokeParticle.AddParticle(e.Position, new Vector3(.1f, .1f, .1f));
            }

        }

        void ShotSero(Vector3 position,bool isRightDirection)
        {
            if (sero1.IsDestroyed)
            {
                sero1.Reset(position, isRightDirection);
                previosSeroIndex = 1;
              
            }
            else if (sero2.IsDestroyed)
            {
                sero2.Reset(position, isRightDirection);
                previosSeroIndex = 2;
             
            }
            else
            {
                if (previosSeroIndex == 1)
                {
                    sero2.Reset(position, isRightDirection);
                    previosSeroIndex = 2;
                }
                else
                {
                    sero1.Reset(position, isRightDirection);
                    previosSeroIndex = 1;
                }
            }

            soundManager.Play3DSound("siro",Body.CenterPosition);

            EventAggregator.Instance.Publish(new OnCeroFired());
        }

        void LunchMissile(Vector3 position1,Vector3 position2,bool isRightDirection)
        {
            missil1.Reset(position1, isRightDirection);
            missil2.Reset(position2, isRightDirection);
        }

        void UpdateWeapons(GameTime gameTime)
        {
            UpdateRockets(gameTime);

            if(sero1.IsDestroyed && sero2.IsDestroyed)
                return;

            if(!sero1.IsDestroyed)
                sero1.Update(gameTime);
            if(!sero2.IsDestroyed)
                sero2.Update(gameTime);

            basicEffect.View = camera.ViewMatrix;
            basicEffect.Projection = camera.ProjectionMatrix;
            if(!sero1.IsDestroyed || !sero2.IsDestroyed)
                animPlayer.Update(gameTime);
            if (!sero1.PrepareToDestruction)
            {
                quad1 = new Quad(sero1.Position, Vector3.Backward, Vector3.Up, 1, 1);


                projectile1 = new Projectile(projectileTrailParticles,
                                             new Vector3(sero1.Position.X, sero1.Position.Y + .1f,
                                                         sero1.Position.Z));
                projectile1.Update(gameTime);
            }
            if (!sero2.PrepareToDestruction)
            {
                quad2 = new Quad(sero2.Position, Vector3.Backward, Vector3.Up, 1, 1);
                projectile2 = new Projectile(projectileTrailParticles,
                                             new Vector3(sero2.Position.X, sero2.Position.Y + .1f,
                                                         sero2.Position.Z));

                projectile2.Update(gameTime);
            }
        }

        void UpdateRockets(GameTime gameTime)
        {
            if (missil1.IsDestroyed && missil2.IsDestroyed) return;

            if (!missil1.PrepareToDestruction)
            {
                missil1.Update(gameTime);
                rocketProjectile1 = new Projectile(rocketProjectileTrailParticles,
                                                   new Vector3(missil1.Position.X, missil1.Position.Y + .1f,
                                                               missil1.Position.Z));
                rocketProjectile1.Update(gameTime);
            }
            if (!missil2.PrepareToDestruction)
            {
                missil2.Update(gameTime);

                rocketProjectile2 = new Projectile(rocketProjectileTrailParticles,
                                                   new Vector3(missil2.Position.X, missil2.Position.Y + .1f,
                                                               missil2.Position.Z));
                rocketProjectile2.Update(gameTime);
            }
        }

        

        void DrawWeapons()
        {
            if (!sero1.IsDestroyed || !sero2.IsDestroyed)
                projectileTrailParticles.SetCamera(camera.ViewMatrix, camera.ProjectionMatrix);
            if (!missil1.IsDestroyed || !missil2.IsDestroyed)
                rocketProjectileTrailParticles.SetCamera(camera.ViewMatrix, camera.ProjectionMatrix);

            if (!sero1.IsDestroyed && !sero1.PrepareToDestruction)
                DrawQuad(basicEffect, quad1, new Vector3(0, .2f, 0), animPlayer.Texture);
            if (!sero2.IsDestroyed && !sero2.PrepareToDestruction)
                DrawQuad(basicEffect, quad2, new Vector3(0, .2f, 0), animPlayer.Texture);

            if (!missil1.PrepareToDestruction)
                DrawRocket(missil1.Position, missil1.IsRightDirection);
            if (!missil2.PrepareToDestruction)
                DrawRocket(missil2.Position, missil2.IsRightDirection);

            if(missil1.PrepareToDestruction || missil2.PrepareToDestruction)
            {
                rocketExplosionParticle.SetCamera(camera.ViewMatrix,camera.ProjectionMatrix);
                rocketExplosionSmokeParticle.SetCamera(camera.ViewMatrix,camera.ProjectionMatrix);
            }
        }

        void DrawRocket(Vector3 position,bool isRightDirec)
        {
            foreach (var mesh in rocket.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();

                    effect.World = Matrix.CreateRotationY(isRightDirec ? 0 : MathHelper.Pi)*
                                   Matrix.CreateTranslation(new Vector3(position.X +
                                                                        (isRightDirec ? .8f : -.8f), position.Y, position.Z));
                    effect.View = camera.ViewMatrix;
                    effect.Projection = camera.ProjectionMatrix;
                }
                mesh.Draw();
            }
        }

       public void DrawObjects(SpriteBatch spriteBatch)
       {

           spriteBatch.Draw(ceroInd, Vector2.Zero, null, Color.White, 0, Vector2.Zero, .5f, SpriteEffects.None, 0);
           if (hasARocket)
               spriteBatch.Draw(rocketInd, new Vector2(ceroInd.Width/2.0f + 3, 0), null, Color.White, 0, Vector2.Zero,
                                .5f,
                                SpriteEffects.None, 0);
           spriteBatch.Draw(healthInd1, new Vector2(GameSettings.WindowWidth - healthInd1.Width - 3, 6), Color.White);
           spriteBatch.Draw(healthInd2, new Vector2(GameSettings.WindowWidth - healthInd2.Width - 3, 6),
                            new Rectangle(0, 0, (int) ((health*healthInd2.Width)/100), healthInd2.Height), Color.White,
                            0, Vector2.Zero, 1f, SpriteEffects.None, 0);
           spriteBatch.Draw(healthInd3, new Vector2(GameSettings.WindowWidth - healthInd3.Width - 3, 6), Color.White);
           spriteBatch.DrawString(font, "8", new Vector2(ceroInd.Width/2.0f - 10, ceroInd.Height/2.0f - 30),
                                  Color.Black, MathHelper.ToRadians(90), Vector2.Zero, 1, SpriteEffects.None, 0);
           if (hasARocket)
               spriteBatch.DrawString(font, rocketAmmo.ToString(),
                   new Vector2(ceroInd.Width -(rocketAmmo<10?30: 35), ceroInd.Height/2.0f - 40), Color.Black);

       }
    }
}
