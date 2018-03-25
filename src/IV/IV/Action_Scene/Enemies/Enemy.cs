using System;
using System.Collections.Generic;
using System.Linq;
using BEPUphysics;
using BEPUphysics.Entities;
using IV.Achievement;
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
    public abstract class Enemy : DrawableGameComponent
    {
      protected Model model;
      protected Box Body;
      protected readonly Space space;
      protected readonly Camera camera;
      protected bool isRightDirection;
      private float waitTime;
      private const float MaxWaitTime = 2.5f;
      private const float speed = 10f;
      private readonly Random rand;
      protected float initZ;
      private readonly Player player;
      protected Vector3 playerDistance;
      protected readonly List<GameComponent> gameComponent;
      protected bool isTurning;
      private float turnValue = 180;
      protected TimeSpan timeToShoot;
      protected TimeSpan MaxtimeToShoot;
      private bool canShot;
      private readonly EnemyType type;
      public float Strength { get; set; }

       //Float Mouvement
      protected float height;
      private TimeSpan mouvTimer;
      private bool heightUp;
      protected float rotX;
      private bool rotPos;
      private bool wasZoomingOut;

      protected bool destroyed;
      protected TimeSpan timeToEnd;

      private ParticleSystem explosionSmokeParticle;
      private ParticleSystem explosionParticle;

      protected readonly SoundManager soundManager;

      protected Enemy(Game game, Space space, Camera camera, Vector3 position,EnemyType type, Random rand, Player player, 
          List<GameComponent> gameComponent) 
            : base(game)
      {
          this.space = space;
          this.gameComponent = gameComponent;
          this.player = player;
          this.rand = rand;
          this.camera = camera;
          this.type = type;
          initZ = position.Z;

          soundManager = (SoundManager)Game.Services.GetService(typeof (SoundManager));
          //floatSound = soundManager.Play3DSound("levitation", Vector3.Zero);
          
          
      }

      protected static void collisionDetected(Entity sender, Entity other, CollisionPair collisionpair)
      {
          if (other.CompoundBody != null && other.CompoundBody.Tag is Player)
              ((Player) other.CompoundBody.Tag).Die(DeathType.Electricity);
      }

      public virtual void LoadContent(ContentManager Content)
      {
          explosionSmokeParticle = new ExplosionSmokeParticleSystem(Game, Content) {DrawOrder = 200};
          explosionSmokeParticle.Initialize();
          gameComponent.Add(explosionSmokeParticle);

          explosionParticle = new ExplosionParticleSystem(Game, Content) {DrawOrder = 400};
          explosionParticle.Initialize();
          gameComponent.Add(explosionParticle);
      }

      public override void Update(GameTime gameTime)
      {
          if (destroyed)
          {
              timeToEnd += gameTime.ElapsedGameTime;
              if (timeToEnd > TimeSpan.FromSeconds(5))
              {
                  gameComponent.Remove(this);

                  if (!gameComponent.OfType<Enemy>().Any())
                      EventAggregator.Instance.Publish(new OnZeroEnemy());
              }
              return;
          }

          HandelPlayerReaction();

          if (type != EnemyType.Virus)
              HandelFloatMouvement(gameTime);

          if (waitTime > 0)
          {
              // Wait for some amount of time.
              waitTime = Math.Max(0.0f, waitTime - (float) gameTime.ElapsedGameTime.TotalSeconds);
              if (waitTime <= 0.0f)
              {
                  isRightDirection = !isRightDirection;
                  if (HandelCollision())
                      isRightDirection = !isRightDirection;
                  isTurning = true;
              }
          }
          else
          {
              // If we are about to run into a wall or off a cliff, start waiting.
              if (HandelCollision())
                  waitTime = MaxWaitTime;
              else
              {
                  if (rand.Next(100) == 5 && !isTurning)
                  {
                      waitTime = MaxWaitTime;
                  }
                  else if (!isTurning)
                      Body.LinearVelocity = new Vector3(isRightDirection ? speed : -speed, 0, 0);
              }

              HandelEdges();

          }

          var posi = Body.CenterPosition;
          posi.Z = initZ;
          Body.CenterPosition = posi;

          var orientationMatrix = Matrix.CreateRotationY(MathHelper.ToRadians(turnValue));
          Body.OrientationQuaternion = Quaternion.CreateFromRotationMatrix(orientationMatrix);

          if (isTurning)
          {
              turnValue += isRightDirection ? -5 : 5;
              turnValue = MathHelper.Clamp(turnValue, 0, 180);
              if (turnValue <= 0 || turnValue >= 180)
                  isTurning = false;
          }
          timeToShoot += gameTime.ElapsedGameTime;
          canShot = timeToShoot >= MaxtimeToShoot;

          if (Strength <= 0)
              Die();
          base.Update(gameTime);
      }

      bool HandelCollision()
      {
          var hitEntitie = new List<Entity>();
          var direction = isRightDirection ? Vector3.Right : Vector3.Left;
          var position = new Vector3(Body.CenterPosition.X, Body.CenterPosition.Y, Body.CenterPosition.Z);
          space.RayCast(position, direction, 2.5f, true, hitEntitie, new List<Vector3>(),
                        new List<Vector3>(), new List<float>());
          return hitEntitie.Count(entity => entity != Body && entity is Box) > 0;
      }

      void HandelEdges()
      {
          var hitEntities = new List<Entity>();
          var position = new Vector3(Body.CenterPosition.X + (isRightDirection ? Body.Width/2 : -Body.Width/2),
                                     Body.CenterPosition.Y, Body.CenterPosition.Z);
          space.RayCast(position, Vector3.Down, Body.Height/2 + .5f, false, hitEntities, new List<Vector3>(),
                        new List<Vector3>(), new List<float>());
          if (hitEntities.Count(entity => entity != Body) == 0)
          {
              waitTime = MaxWaitTime;
              Body.LinearVelocity = Vector3.Zero;
          }
      }

      void HandelPlayerReaction()
      {
          if (Math.Abs(Body.CenterPosition.X - player.Body.CenterPosition.X) < playerDistance.X + 20
              && Math.Abs(Body.CenterPosition.Y - player.Body.CenterPosition.Y) < playerDistance.Y + 10)
          {
              camera.ZoomOut(50);
              wasZoomingOut = true;
          }
          else if (wasZoomingOut)
          {
              camera.ZoomIn();
              wasZoomingOut = false;
          }

          if (Math.Abs(Body.CenterPosition.X - player.Body.CenterPosition.X) < playerDistance.X
              && Math.Abs(Body.CenterPosition.Y - player.Body.CenterPosition.Y) < playerDistance.Y)
          {
              if (isRightDirection != Body.CenterPosition.X < player.Body.CenterPosition.X)
              {
                  isTurning = true;
                  timeToShoot = MaxtimeToShoot;
                  isRightDirection = Body.CenterPosition.X < player.Body.CenterPosition.X;
              }
              waitTime = MaxWaitTime;
              if (!isTurning && canShot)
              {
                  Shoot();
                  timeToShoot = TimeSpan.Zero;
              }

          }
      }

      protected abstract void Shoot();

      void HandelFloatMouvement(GameTime gameTime)
      {
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

      protected virtual void Die()
      {
          camera.ZoomIn();
          space.Remove(Body);
          destroyed = true;
          for (int i = 0; i < 30; i++)
          {
              var posi = new Vector3(Body.CenterPosition.X + (i%2 == 0 ? .5f : -.5f),
                                     Body.CenterPosition.Y + (i%2 == 2 ? .5f : 1.5f),
                                     Body.CenterPosition.Z);
              explosionParticle.AddParticle(posi, new Vector3(.1f, .1f, .1f));
          }
          for (int i = 0; i < 7; i++)
          {
              var posi = new Vector3(Body.CenterPosition.X + (i%2 == 0 ? .5f : -.5f),
                                     Body.CenterPosition.Y + (i%2 == 0 ? .5f : -.5f),
                                     Body.CenterPosition.Z);
              explosionSmokeParticle.AddParticle(posi, new Vector3(.1f, .1f, .1f));
          }
          soundManager.Play3DSound("enemie_Explosion",Body.CenterPosition);

          EventAggregator.Instance.Publish(new OnEnemyKilled {Enemy = this});
      }

      public override void Draw(GameTime gameTime)
      {
          if (destroyed)
          {
              explosionSmokeParticle.SetCamera(camera.ViewMatrix, camera.ProjectionMatrix);
              explosionParticle.SetCamera(camera.ViewMatrix, camera.ProjectionMatrix);
          }

          base.Draw(gameTime);
      }

      protected  void DrawQuad(BasicEffect effect, Quad _quad, Vector3 position, Texture2D texture)
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