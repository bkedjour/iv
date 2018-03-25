using System;
using System.Collections.Generic;
using System.Linq;
using BEPUphysics;
using BEPUphysics.Entities;
using IV.Achievement;
using IV.Action_Scene.Effects;
using IV.Action_Scene.Objects;
using IVSaveLoadAssembly;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SkinnedModel;
using AnimationPlayer = SkinnedModel.AnimationPlayer;

namespace IV.Action_Scene
{
    public partial class Player : DrawableGameComponent
    {
        private Model IVModel;
        private AnimationPlayer animationPlayer;
        private SkinningData skinningData;
        Matrix[] boneTransforms;

        private readonly SoundManager soundManager;

        private bool isAnimationLoop;

        private Space space;
        public CompoundBody Body { get; private set; }

        private Camera camera;

        private float moveSpeed;
        private bool isDirectionRight;
        private Matrix orientationMatrix = Matrix.Identity;
        
        public float turnValue;
        
        private Vector3 initPosition;

        public Entity file { get; private set; }

        #region Flags
        private bool isOnGround;
        public bool isFalling;
        private bool isJumping;
        private bool headerOnGround;
        private bool backOnGround;
        private bool headerCollision;
        private bool cornerPos;
        private bool canMove;
        private bool isNearCube;
        private bool pushing;       
        private bool isTerminatingPush;
        private bool isPressingButton;
        private bool isReleasingButton;
        private bool isTurning;
        private bool isLanding;
        private bool isPlayingOtherIdle;
        public bool IsInAFile{ get; private set;}
        public bool IsInsideFile
        {
            get
            {
                return (file != null)
                    ? ((File)file.Tag).PlayerInside
                    : false;
            }
        }
        private bool doubleJump;
        private bool isDoubleJumpActivated;
        public bool isCrouch;
        public bool tryToEndCrouch;
        public bool IsDead { get; private set; }
        public bool Active{ get; set;}

        private bool canJump;
        #endregion

        #region Timers
        private TimeSpan timeToPressButton;
        private TimeSpan timeToReleaseButton;
        private TimeSpan timeToPuchCube;
        private TimeSpan timeToTerminatePush;
        private TimeSpan turnTime;
        private TimeSpan timeToLand;
        private TimeSpan timeToChangeIdle;
        private TimeSpan timeToEnterFile;
        private TimeSpan timeToEndCrouch;
        private TimeSpan timeToPrepareHit;
        private bool isTimeToPrepareHit;
        private TimeSpan timeToHit;
        private bool isTimeToHit;
        private bool pushingBack;
        private TimeSpan timeToShoot = TimeSpan.FromSeconds(1);

        private TimeSpan totalIdle;
        #endregion

        private KeyboardState oldState;
        private readonly List<GameComponent> gameComponents;

        private Entity hitedEntity;
        private bool hiting;

        private float health = 100;
        private TimeSpan timeToHealth;
        private TimeSpan timeToRecharge;
        private bool charging;

        //Rocket Luncher
        private Model rocketLauncher;
        private Model rocket;
        private bool RocketEnabled;
        private TimeSpan timeToReloadRocket;
        private Quad rocketQuad;
        private BasicEffect rocketBasicEffect;
        private QuadAnimation rocketAnimation_R, rocketAnimation_L;
        private QuadAnimationPlayer rocketAnimationPlayer;
        private bool rocketReloading;
        private bool hasARocket;
        private int rocketAmmo;

        private bool isOnASlider;
        private float slideVelocity;

        protected override void Dispose(bool disposing)
        {
            rocketLauncher = null;
            rocket = null;
            rocketBasicEffect.Dispose();
            rocketAnimationPlayer = null;
            rocketAnimation_L.Dispose();
            rocketAnimation_R.Dispose();
            rocketAnimation_L = null;
            rocketAnimation_R = null;
            hitedEntity = null;
            file = null;
            camera = null;
            Body = null;
            space = null;
            skinningData = null;
            animationPlayer = null;
            IVModel = null;
            GC.SuppressFinalize(this);
        }

        public Player(Game game,Space space, Camera camera, Vector3 position, List<GameComponent> gameComponents) 
            : base(game)
        {
            this.space = space;
            this.gameComponents = gameComponents;
            this.camera = camera;
            initPosition = position;
            isDirectionRight = true;
            isDoubleJumpActivated = false;

            Body = new CompoundBody();
            Body.AddBody(new Box(new Vector3(0f, 0f, 0f), 4f, 2f, 1.8f, 10));
            Body.AddBody(new Box(new Vector3(-.75f, 1.25f, 0), 2.5f, 2.5f, 1.8f, .3f) { IsAffectedByGravity = false });
            
            space.Add(Body);

            Body.TeleportTo(position);
            Body.Tag = this;
            Active = true;

            soundManager = (SoundManager) Game.Services.GetService(typeof (SoundManager));
        }
        
        public void Initialize(Vector3 position)
        {
            if (position == Vector3.Zero) position = initPosition;
            Body.CenterPosition = (new Vector3(position.X, position.Y + 1, initPosition.Z));
            IsDead = false;
            health = 100;
            timeToHealth = TimeSpan.Zero;
            timeToRecharge = TimeSpan.Zero;
            charging = false;

            if (IsInAFile)
            {
                ((File) file.Tag).PlayerInside = false;
                file = null;
                IsInAFile = false;
                Body.LinearVelocity = Vector3.Zero;
                timeToEnterFile = TimeSpan.Zero;
            }

            camera.ActivateChaseCameraMode(Body, new Vector3(0, 1, 0), false, camera.InitialDistanceToTarget, true);
        }

        public  void LoadContent(ContentManager content,Model model)
        {
            IVModel = model;

            skinningData = IVModel.Tag as SkinningData;

            if (skinningData == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            boneTransforms = new Matrix[skinningData.BindPose.Count];

            animationPlayer = new AnimationPlayer(skinningData);

            
            rocketLauncher = content.Load<Model>("Models\\ROCKET_LAUNCHER");
            rocket = content.Load<Model>("Models\\Rockette");

            rocketBasicEffect = new BasicEffect(GraphicsDevice);

            var rocketTextures_R = new List<Texture2D>();
            for(int i = 0; i< 15; i++)
                rocketTextures_R.Add(content.Load<Texture2D>(string.Format("Effects\\Player\\Rocket\\{0}_R", i)));
            rocketAnimation_R = new QuadAnimation(rocketTextures_R, .08f, false);

            var rocketTextures_L = new List<Texture2D>();
            for (int i = 0; i < 15; i++)
                rocketTextures_L.Add(content.Load<Texture2D>(string.Format("Effects\\Player\\Rocket\\{0}_L", i)));
            rocketAnimation_L = new QuadAnimation(rocketTextures_L, .08f, false);

            rocketAnimationPlayer = new QuadAnimationPlayer();

            LoadTheWeapons(content);
        }

        public override void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();

            if (!IsDead)
                if (IsInAFile) InsideFileLogic(gameTime);
                else HandelMovement(gameTime, keyboardState);


            UpdateWeapons(gameTime);
            
            oldState = keyboardState;


            base.Update(gameTime);
        }


        void HandelMovement(GameTime gameTime,KeyboardState keyboardState)
        {
            //it was 20
            moveSpeed = 30f;

            if (keyboardState.IsKeyDown(Keys.LeftControl) && isOnGround)
                moveSpeed = 40;

            var mouvement = new Vector3(Body.LinearVelocity.X*(isOnASlider ? 0 : .4f), 0, 0);
            if (keyboardState.IsKeyDown(Keys.Right) && Active)
            {
                if (!isDirectionRight && !pushingBack)
                {
                    isDirectionRight = true;
                    isTurning = true;

                    orientationMatrix *= Matrix.CreateRotationY(MathHelper.Pi);

                }
                if (canMove)
                {
                    mouvement.X = pushingBack ? moveSpeed/2 : moveSpeed;
                    if (isOnGround && !isLanding && !isNearCube && !isCrouch)
                        PlayAnimation("Walk", true);

                    if(pushingBack) hitedEntity.LinearVelocity = mouvement;
                }
                timeToChangeIdle = TimeSpan.Zero;
                totalIdle = TimeSpan.Zero;

                CancelHit();
            }
            else if (keyboardState.IsKeyDown(Keys.Left) && Active)
            {
                if (isDirectionRight && !pushingBack)
                {
                    isDirectionRight = false;
                    isTurning = true;

                    orientationMatrix *= Matrix.CreateRotationY(MathHelper.Pi);
                }
                if (canMove)
                {
                    mouvement.X = -(pushingBack ? moveSpeed/2 : moveSpeed);
                    if (isOnGround && !isLanding && !isNearCube && !isCrouch)
                        PlayAnimation("Walk", true);

                    if (pushingBack) hitedEntity.LinearVelocity = mouvement;
                }
                timeToChangeIdle = TimeSpan.Zero;
                totalIdle = TimeSpan.Zero;

                CancelHit();
            }
            else
            {
                if ((isOnGround && !isLanding && !isNearCube && !isTerminatingPush && !isPressingButton &&
                     !isReleasingButton && !isCrouch) ||
                    (!isOnGround && !cornerPos && !isJumping && !isFalling))
                {
                    if (!isPlayingOtherIdle)
                        timeToChangeIdle += gameTime.ElapsedGameTime;
                    if (timeToChangeIdle >= TimeSpan.FromSeconds(5) && !isPlayingOtherIdle)
                    {
                        var rnd = new Random();
                        PlayAnimation(rnd.Next(2) == 0 ? "Idle2" : "Idle3", true);
                        isPlayingOtherIdle = true;
                        timeToChangeIdle = animationPlayer.CurrentClip.Duration;
                    }
                    else
                    {
                        if (isPlayingOtherIdle)
                        {
                            timeToChangeIdle -= TimeSpan.FromMilliseconds(10);
                            if (timeToChangeIdle <= TimeSpan.Zero)
                                isPlayingOtherIdle = false;
                        }
                        else
                            PlayAnimation("Idle1", false);
                    }

                    totalIdle += gameTime.ElapsedGameTime;
                    if (totalIdle >= TimeSpan.FromSeconds(30))
                    {
                        EventAggregator.Instance.Publish(new OnIdle());
                    }
                }
            }

            

            #region Turning

            if (isTurning)
            {
                turnTime += gameTime.ElapsedGameTime;
                if (turnTime >= TimeSpan.FromMilliseconds(1))
                {
                    turnTime = TimeSpan.Zero;
                    turnValue += isDirectionRight ? 10 : -10;
                    if ((!isDirectionRight && turnValue <= -180) || (isDirectionRight && turnValue >= 0))
                        isTurning = false;
                    turnValue = MathHelper.Clamp(turnValue, -180, 0);
                    orientationMatrix = Matrix.CreateRotationY(MathHelper.ToRadians(turnValue));
                }
            }

            #endregion

            Body.OrientationQuaternion = Quaternion.CreateFromRotationMatrix(orientationMatrix);

            mouvement.Y = Body.LinearVelocity.Y;
            Body.LinearVelocity = mouvement + (isOnASlider ? new Vector3(slideVelocity, 0, 0) : Vector3.Zero);

            if(isOnASlider) isOnASlider = false;

            if (Active)
            {
                var fixedPosi = Body.CenterPosition;
                fixedPosi.Z = initPosition.Z;
                Body.CenterPosition = fixedPosi;
            }
            HandelRays();

            Body.IsAffectedByGravity = headerOnGround == backOnGround;
            isOnGround = headerOnGround && backOnGround;
            canJump = headerOnGround || backOnGround;

            if (isOnGround && isFalling)
            {
                PlayAnimation("jump_crash", false);
                timeToLand = animationPlayer.CurrentClip.Duration;
                if (Body.LinearVelocity.Y < -70)
                {
                    camera.Shake(0.5f, .7f);
                    soundManager.Play3DSound("fall_down",Body.CenterPosition);

                    EventAggregator.Instance.Publish(new OnPlayerFallDown());
                }

            }

            if (timeToLand >= TimeSpan.Zero)
                timeToLand -= TimeSpan.FromMilliseconds(11.5f);

            isLanding = timeToLand >= TimeSpan.Zero;
            isFalling = !headerOnGround && !backOnGround && Body.LinearVelocity.Y < 0;
            isJumping = !headerOnGround && !backOnGround && Body.LinearVelocity.Y > 0;

            canMove = !headerCollision;

            if (cornerPos)
                PlayAnimation("Corner_Pos", false);


            if (keyboardState.IsKeyDown(Keys.Down) && oldState.IsKeyUp(Keys.Down) && Active
                && !tryToEndCrouch && !isCrouch && isOnGround)
            {
                isCrouch = true;
                Body.SubBodies[1].CollisionRules.SpecificEntities.Clear();

                foreach (var entity in space.Entities)
                    Body.SubBodies[1].CollisionRules.SpecificEntities.Add(entity, CollisionRule.NoPair);
                PlayAnimation("Crowl", false);

                CancelHit();
            }

            if (keyboardState.IsKeyUp(Keys.Down) && oldState.IsKeyDown(Keys.Down) && Active && isCrouch && isOnGround)
            {
                tryToEndCrouch = true;

                CancelHit();
            }

            if (keyboardState.IsKeyDown(Keys.Up) && oldState.IsKeyUp(Keys.Up) && Active && !tryToEndCrouch)
                if (isDoubleJumpActivated && isJumping && doubleJump)
                {
                    isCrouch = false;
                    Jump(40);
                    PlayAnimation("DoubleJump", false);
                    doubleJump = false;

                    CancelHit();
                }
                else
                {
                    if ( /*isOnGround*/canJump && !cornerPos)
                    {
                        Jump(30);
                        PlayAnimation("Jump", false);
                        isOnGround = false;
                        isJumping = true;
                        doubleJump = true;

                        CancelHit();
                        EventAggregator.Instance.Publish(new OnPlayerJump{IsNormalJump = true});
                    }
                    else if (cornerPos)
                    {
                        Jump(35);
                        PlayAnimation("Corner_jumpPos", false);
                        doubleJump = true;

                        CancelHit();
                        EventAggregator.Instance.Publish(new OnPlayerJump { IsNormalJump = false });
                    }
                }

            if (isFalling)
                PlayAnimation("Jump_Fall", false);

            if (keyboardState.IsKeyDown(Keys.LeftShift) && oldState.IsKeyUp(Keys.LeftShift) && Active)
            {
                if (!headerOnGround && backOnGround && !cornerPos)
                    Body.CenterPosition = PutMeInEdge();
            }

            if (!IsInAFile)
                HandelOtheAnimations(gameTime);
            if (tryToEndCrouch)
                HandelEndCrouch();

            float mouthRotation = 0;
            timeToShoot += gameTime.ElapsedGameTime;
            if (keyboardState.IsKeyDown(Keys.Space) && !isTurning && Active)
            {
                mouthRotation = -1;
                if (timeToShoot > TimeSpan.FromMilliseconds(550))
                {
                    timeToShoot = TimeSpan.Zero;
                    var bulletPosi = new Vector3(Body.CenterPosition.X + (isDirectionRight ? 1f : -1f),
                                                 Body.CenterPosition.Y + (isFalling ? .8f : isCrouch ? -.6f : -.3f),
                                                 Body.CenterPosition.Z);
                    ShotSero(bulletPosi, isDirectionRight);
                }
            }
            Matrix mouthTransform = Matrix.CreateRotationY(mouthRotation);

            animationPlayer.UpdateBoneTransforms(gameTime.ElapsedGameTime, isAnimationLoop);

            animationPlayer.GetBoneTransforms().CopyTo(boneTransforms, 0);

            int mouthIndex = skinningData.BoneIndices["Mouth"];
            boneTransforms[mouthIndex] = mouthTransform*boneTransforms[mouthIndex];

            animationPlayer.UpdateWorldTransforms(Matrix.Identity, boneTransforms);
            animationPlayer.UpdateSkinTransforms();

            if (keyboardState.IsKeyDown(Keys.X) && oldState.IsKeyUp(Keys.X))
                camera.ZoomOut(60);
            if (keyboardState.IsKeyUp(Keys.X) && oldState.IsKeyDown(Keys.X))
                camera.ZoomIn();

            if (hasARocket && rocketAmmo > 0)
                HandelRocket(gameTime, keyboardState);
            
        }

        private void CancelHit()
        {
            isTimeToPrepareHit = false;
            isTimeToHit = false;
            timeToPrepareHit = TimeSpan.Zero;
            timeToHit = TimeSpan.Zero;
            hiting = false;
        }

        public void OnSliding(float velocity)
        {
            isOnASlider = true;
            slideVelocity = velocity;
        }
       
        void HandelRocket(GameTime gameTime, KeyboardState keyboardState)
        {
            var coreIndex = skinningData.BoneIndices["CORE_dummy"];
            Matrix[] worldTransforms = animationPlayer.GetWorldTransforms();
            if (!RocketEnabled && !rocketReloading)
            {
                timeToReloadRocket += gameTime.ElapsedGameTime;
                if (timeToReloadRocket > TimeSpan.FromSeconds(.5f))
                {
                    rocketReloading = true;
                    rocketAnimationPlayer.PlayAnimation(isDirectionRight ? rocketAnimation_R : rocketAnimation_L);
                }
            }

            if (rocketReloading)
            {
                rocketBasicEffect.View = camera.ViewMatrix;
                rocketBasicEffect.Projection = camera.ProjectionMatrix;

                rocketAnimationPlayer.Update(gameTime);

                rocketQuad = new Quad(new Vector3(
                                          Body.SubBodies[0].CenterPosition.X +
                                          (isDirectionRight
                                               ? worldTransforms[coreIndex].Translation.X - .98f
                                               : -worldTransforms[coreIndex].Translation.X + .98f),
                                          Body.SubBodies[0].CenterPosition.Y +
                                          worldTransforms[coreIndex].Translation.Y -
                                          .91f,
                                          Body.SubBodies[0].CenterPosition.Z +
                                          worldTransforms[coreIndex].Translation.Z +
                                          .55f), Vector3.Backward, Vector3.Up, 1.15f, 1.15f/2.0f);

                if (rocketAnimationPlayer.FrameIndex == rocketAnimationPlayer.Animation.FrameCount - 1)
                {
                    RocketEnabled = true;
                    rocketReloading = false;
                }
            }



            if (keyboardState.IsKeyDown(Keys.W) && oldState.IsKeyUp(Keys.W) && !isTurning && Active && RocketEnabled)
            {
                RocketEnabled = false;
                timeToReloadRocket = TimeSpan.Zero;
                soundManager.Play3DSound("messil_launch",Body.CenterPosition);

                var missilPosition1 = new Vector3(
                    Body.SubBodies[0].CenterPosition.X +
                    (isDirectionRight
                         ? worldTransforms[coreIndex].Translation.X - .8f
                         : -worldTransforms[coreIndex].Translation.X + .8f),
                    Body.SubBodies[0].CenterPosition.Y + worldTransforms[coreIndex].Translation.Y -
                    1.042f,
                    Body.SubBodies[0].CenterPosition.Z + worldTransforms[coreIndex].Translation.Z +
                    .55f);

                var missilPosition2 = new Vector3(
                    Body.SubBodies[0].CenterPosition.X +
                    (isDirectionRight
                         ? worldTransforms[coreIndex].Translation.X - .8f
                         : -worldTransforms[coreIndex].Translation.X + .8f),
                    Body.SubBodies[0].CenterPosition.Y + worldTransforms[coreIndex].Translation.Y -
                    1.042f,
                    Body.SubBodies[0].CenterPosition.Z + worldTransforms[coreIndex].Translation.Z -
                    .55f);

                LunchMissile(missilPosition1, missilPosition2, isDirectionRight);
                rocketAmmo -= 2;
            }
        }

        public void EnableRocket()
        {
            hasARocket = true;
            rocketAmmo = 12;
        }

        void HandelOtheAnimations(GameTime gameTime)
        {
            if (isNearCube && isOnGround && !hiting)
            {
                if (Math.Abs(Body.LinearVelocity.X) > 4 && Math.Abs(Body.LinearVelocity.X) < 10 && timeToPuchCube <= TimeSpan.Zero)
                {
                    PlayAnimation("Push_InAction", true);
                    pushing = true;
                }
                else if (!pushing)
                {
                    PlayAnimation("Push_PreAction", false);
                    if (timeToPuchCube <= TimeSpan.Zero) timeToPuchCube = TimeSpan.FromMilliseconds(100);
                }

            }
            else if (pushing && !isNearCube)
            {
                isTerminatingPush = true;
                pushing = false;
                PlayAnimation("Push_AterAction", false);
            }

            if (isTerminatingPush)
            {
                timeToTerminatePush += gameTime.ElapsedGameTime;
                if (timeToTerminatePush >= animationPlayer.CurrentClip.Duration)
                {
                    timeToTerminatePush = TimeSpan.Zero;
                    isTerminatingPush = false;
                }
            }
            if (timeToPuchCube >= TimeSpan.Zero)
                timeToPuchCube -= TimeSpan.FromMilliseconds(10);

            if (isPressingButton)
            {
                timeToPressButton += gameTime.ElapsedGameTime;
                if (timeToPressButton >= animationPlayer.CurrentClip.Duration)
                {
                    timeToPressButton = TimeSpan.Zero;
                    isPressingButton = false;
                }
            }

            if (isReleasingButton)
            {
                timeToReleaseButton += gameTime.ElapsedGameTime;
                if (timeToReleaseButton >= animationPlayer.CurrentClip.Duration)
                {
                    timeToReleaseButton = TimeSpan.Zero;
                    isReleasingButton = false;
                }
            }

            if (timeToEndCrouch >= TimeSpan.Zero)
            {
                timeToEndCrouch -= TimeSpan.FromMilliseconds(25);
                if (timeToEndCrouch <= TimeSpan.Zero) isCrouch = false;
            }

            if (isTimeToHit)
            {
                timeToHit += gameTime.ElapsedGameTime;
                if (timeToHit >= animationPlayer.CurrentClip.Duration-TimeSpan.FromMilliseconds(300))
                {
                    timeToHit = TimeSpan.Zero;
                    isTimeToHit = false;
                    if (!(hitedEntity.Tag is Brick))
                    {
                        hitedEntity.LinearVelocity = new Vector3(isDirectionRight ? 40 : -40, 0, 0);
                        soundManager.Play3DSound("push_violance",Body.CenterPosition);
                    }
                    hiting = false;
                }
            }
            if(isTimeToPrepareHit)
            {
                timeToPrepareHit += gameTime.ElapsedGameTime;
                if (timeToPrepareHit >= animationPlayer.CurrentClip.Duration)
                {
                    timeToPrepareHit = TimeSpan.Zero;
                    isTimeToPrepareHit = false;
                    PlayAnimation("Hite", false);
                    isTimeToHit = true;
                }
            }

            if(timeToHealth > TimeSpan.Zero)
            {
                timeToHealth -= gameTime.ElapsedGameTime;
                if(timeToHealth <= TimeSpan.Zero)
                {
                    timeToHealth = TimeSpan.Zero;
                    charging = true;
                }
            }
            if(charging)
            {
                timeToRecharge += gameTime.ElapsedGameTime;
                if (timeToRecharge > TimeSpan.FromMilliseconds(100))
                {
                    timeToRecharge = TimeSpan.Zero;
                    health++;
                    if (health >= 100)
                    {
                        health = 100;
                        timeToRecharge = TimeSpan.Zero;
                        charging = false;
                    }
                }
            }
        }

        Vector3  PutMeInEdge()
        {
            var nextPos = Body.CenterPosition;
            if(isDirectionRight)
            {
                List<Entity> hitEntitie;
                int rayCount = 0;
                int Bug = 0;
                while (rayCount == 0)

                {
                    hitEntitie = new List<Entity>();
                    var ray = new Vector3(nextPos.X + .3f, nextPos.Y - 1f, nextPos.Z);
                    space.RayCast(ray, Vector3.Down, .2f, true, hitEntitie, new List<Vector3>(),
                          new List<Vector3>(), new List<float>());
                    rayCount = hitEntitie.OfType<Box>().Count();
                    nextPos.X -= .1f;
                    if(Bug++ > 100) break;
                }
            }
            else
            {
                List<Entity> hitEntitie;
                int rayCount = 0;
                int Bug = 0;
                while (rayCount == 0)
                {
                    hitEntitie = new List<Entity>();
                    var ray = new Vector3(nextPos.X - .3f, nextPos.Y - 1f, nextPos.Z);
                    space.RayCast(ray, Vector3.Down, .2f, true, hitEntitie, new List<Vector3>(),
                          new List<Vector3>(), new List<float>());
                    rayCount = hitEntitie.OfType<Box>().Count();
                    nextPos.X += .1f;
                    if (Bug++ > 700) break;
                }

            }
            if(Math.Abs(Body.CenterPosition.X - nextPos.X) > 10)
                nextPos = Body.CenterPosition;
            return nextPos;
        }

        void PlayAnimation(string anim, bool loop)
        {
            AnimationClip clip = skinningData.AnimationClips[anim];

            animationPlayer.StartClip(clip);
            isAnimationLoop = loop;

        }
        
        
        public void PresseButton()
        {
            isPressingButton = true;
            PlayAnimation("Button_Bar_down", false);
        }

        public void ReleaseButton()
        {
            isReleasingButton = true;
            PlayAnimation("Button_Bar_Up", false);
        }

        void InsideFileLogic(GameTime gameTime)
        {
            animationPlayer.Update(gameTime.ElapsedGameTime, isAnimationLoop, Matrix.Identity);

            timeToEnterFile += gameTime.ElapsedGameTime;
            if (!((File) file.Tag).PlayerInside)
            {
                if (timeToEnterFile >= animationPlayer.CurrentClip.Duration - TimeSpan.FromMilliseconds(50))
                {
                    camera.ActivateChaseCameraMode(file, new Vector3(0, 1, 0), false, camera.CurrentDistanceToTarget,false); 
                    camera.ZoomOut(60);
                    ((File) file.Tag).PlayerInside = true;
                    ((File) file.Tag).IsPlayerInRight = isDirectionRight;

                }
            }
            else
                Body.TeleportTo(new Vector3(0, 0, 200));
               
        }

        

        void HandelEndCrouch()
        {
            var hitEntitie = new List<Entity>();
            var posi1 = new Vector3(Body.SubBodies[0].CenterPosition.X + 1.9f,
                                        Body.SubBodies[0].CenterPosition.Y + 1f,
                                        Body.SubBodies[0].CenterPosition.Z);
            space.RayCast(posi1, Vector3.Up, .5f, true, hitEntitie, new List<Vector3>(),
                          new List<Vector3>(), new List<float>());
            var posi1Count = hitEntitie.OfType<Box>().Count();
            hitEntitie = new List<Entity>();
            var posi2 = new Vector3(Body.SubBodies[0].CenterPosition.X - 1.9f,
                                        Body.SubBodies[0].CenterPosition.Y + 1f,
                                        Body.SubBodies[0].CenterPosition.Z);
            space.RayCast(posi2, Vector3.Up, .5f, true, hitEntitie, new List<Vector3>(),
                          new List<Vector3>(), new List<float>());
            var posi2Count = hitEntitie.OfType<Box>().Count();
            if (posi1Count + posi2Count == 0)
            {
                Body.SubBodies[1].CollisionRules.SpecificEntities.Clear();
                PlayAnimation("Crowl_end", false);
                timeToEndCrouch = animationPlayer.CurrentClip.Duration;
                tryToEndCrouch = false;
            }
        }

        void HandelRays()
        {
           var hitEntitie = new List<Entity>();

            #region Header Foot
            var headerPos1 = new Vector3(Body.SubBodies[0].CenterPosition.X + (isDirectionRight ? 1.9f : -1.9f),
                                        Body.SubBodies[0].CenterPosition.Y - 1f,
                                        Body.SubBodies[0].CenterPosition.Z);
           
            var headerPos3 = new Vector3(Body.SubBodies[0].CenterPosition.X + (isDirectionRight ? .35f : -.3f),
                                         Body.SubBodies[0].CenterPosition.Y - 1f,
                                         Body.SubBodies[0].CenterPosition.Z);
            var headerPos4 = new Vector3(Body.SubBodies[0].CenterPosition.X + (isDirectionRight ? .7f : -.7f),
                                        Body.SubBodies[0].CenterPosition.Y - 1f,
                                        Body.SubBodies[0].CenterPosition.Z);
            space.RayCast(headerPos1, Vector3.Down, .2f, true, hitEntitie, new List<Vector3>(),
                          new List<Vector3>(), new List<float>());
            var headerPos1Count = hitEntitie.OfType<Box>().Count(entity => !(entity.Tag is SliderTurner));
          
            hitEntitie = new List<Entity>();
            space.RayCast(headerPos3, Vector3.Down, .2f, true, hitEntitie, new List<Vector3>(),
                          new List<Vector3>(), new List<float>());
            var headerPos3Count = hitEntitie.OfType<Box>().Count(entity => !(entity.Tag is SliderTurner));
      
            hitEntitie = new List<Entity>();
            space.RayCast(headerPos4, Vector3.Down, .2f, true, hitEntitie, new List<Vector3>(),
                          new List<Vector3>(), new List<float>());
            var headerPos4Count = hitEntitie.OfType<Box>().Count(entity => !(entity.Tag is SliderTurner));

            headerOnGround = headerPos1Count + headerPos4Count > 0;
            cornerPos = headerPos4Count == 0 && headerPos3Count > 0;
            #endregion

            #region Back Foot
            var backPos1 = new Vector3(Body.SubBodies[0].CenterPosition.X + (isDirectionRight ? -1.8f : 1.7f),
                                      Body.SubBodies[0].CenterPosition.Y - 1f,
                                      Body.SubBodies[0].CenterPosition.Z);
            var backPos2 = new Vector3(Body.SubBodies[0].CenterPosition.X + (isDirectionRight ? -1 : .9f),
                                      Body.SubBodies[0].CenterPosition.Y - 1f,
                                      Body.SubBodies[0].CenterPosition.Z);
            var backPos3 = new Vector3(Body.SubBodies[0].CenterPosition.X + (isDirectionRight ? -.4f : .3f),
                                      Body.SubBodies[0].CenterPosition.Y - 1f,
                                      Body.SubBodies[0].CenterPosition.Z);
           
            hitEntitie = new List<Entity>();
            space.RayCast(backPos1, Vector3.Down, .2f, true, hitEntitie, new List<Vector3>(),
                          new List<Vector3>(), new List<float>());
            var backPos1Count = hitEntitie.OfType<Box>().Count(entity =>  !(entity.Tag is SliderTurner));
         
            hitEntitie = new List<Entity>();
            space.RayCast(backPos2, Vector3.Down, .2f, true, hitEntitie, new List<Vector3>(),
                          new List<Vector3>(), new List<float>());
            var backPos2Count = hitEntitie.OfType<Box>().Count(entity => !(entity.Tag is SliderTurner));
       
            hitEntitie = new List<Entity>();
            space.RayCast(backPos3, Vector3.Down, .2f, true, hitEntitie, new List<Vector3>(),
                          new List<Vector3>(), new List<float>());
            var backPos3Count = hitEntitie.OfType<Box>().Count(entity => !(entity.Tag is SliderTurner));
            backOnGround = backPos1Count + backPos2Count + backPos3Count > 0;
            #endregion

            #region Header
          
            hitEntitie = new List<Entity>();
            var direction = isDirectionRight ? Vector3.Right : Vector3.Left;
            var hPos1 = new Vector3(Body.SubBodies[0].CenterPosition.X,
                                        Body.SubBodies[0].CenterPosition.Y + .9f,
                                        Body.SubBodies[0].CenterPosition.Z);
            var hPos2 = Body.SubBodies[0].CenterPosition;
            var hPos3 = new Vector3(Body.SubBodies[0].CenterPosition.X,
                                        Body.SubBodies[0].CenterPosition.Y - .9f,
                                        Body.SubBodies[0].CenterPosition.Z);
            space.RayCast(hPos1,direction , 2.5f, true, hitEntitie, new List<Vector3>(),
                          new List<Vector3>(), new List<float>());
            var hPos1Count = hitEntitie.OfType<Box>().Count(entity => !entity.IsDynamic && !(entity.Tag is SliderTurner));
            isNearCube = false;
            foreach (var entity in hitEntitie.Where(entity => entity is Box && entity.IsDynamic))
                if (isOnGround)
                    isNearCube = true;
            
            hitEntitie = new List<Entity>();
            space.RayCast(hPos2, direction, 2.5f, true, hitEntitie, new List<Vector3>(),
                          new List<Vector3>(), new List<float>());
            var hPos2Count = hitEntitie.OfType<Box>().Count(entity => !entity.IsDynamic && !(entity.Tag is SliderTurner));
           
            hitEntitie = new List<Entity>();
            space.RayCast(hPos3, direction, 2.5f, true, hitEntitie, new List<Vector3>(),
                          new List<Vector3>(), new List<float>());

            if (hitEntitie.Count(entity => entity is Box) == 0)
                pushingBack = false;
            var hPos3Count = 0;
            foreach (var entity in hitEntitie.Where(entity => entity is Box))
            {
                if(entity.Tag is SliderTurner) continue;
                if (!entity.IsDynamic)
                    hPos3Count++;
                else if (entity.IsDynamic && !isOnGround)
                    hPos3Count++;
             
                if (entity.Tag is File && ((File)entity.Tag).CanPlayerGetInside)
                {
                    if (Keyboard.GetState().IsKeyDown(Keys.Enter) && oldState.IsKeyUp(Keys.Enter))
                        if (!IsInAFile && Active)
                        {
                            file = entity;
                            IsInAFile = true;
                            PlayAnimation("Enter_file", false);
                        }

                }else if(entity.Tag is Cube && !(entity.Tag is File))
                {
                    if (Keyboard.GetState().IsKeyDown(Keys.Enter) && oldState.IsKeyUp(Keys.Enter))
                    {
                        PlayAnimation("Push_AterAction", false);
                        hitedEntity = entity;
                        hiting = true;
                        isTimeToPrepareHit = true;
                    }else if(Keyboard.GetState().IsKeyDown(Keys.C) && entity.Mass < 300)
                    {
                        hitedEntity = entity;
                        pushingBack = true;
                    }else
                    {
                        pushingBack = false;
                    }
                }
            }
            headerCollision = hPos1Count + hPos2Count + hPos3Count > 0;

            #endregion
        }

       void Jump(float max)
       {
           var tmp = Body.LinearVelocity;
           tmp.Y = max;
           Body.LinearVelocity = tmp;
       }


        public void ActivateDoubleJump()
        {
            isDoubleJumpActivated = true;
        }

        public void Die(DeathType deathType)
        {
            switch (deathType)
            {
                case DeathType.Electricity:
                    IsDead = true;
                    break;
                case DeathType.Falling:
                    IsDead = true;
                    break;
                case DeathType.Crach:
                    IsDead = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("deathType");
            }

            EventAggregator.Instance.Publish(new OnPlayerDie());
        }

        public void Hurt(float strength)
        {
            health -= strength;
            timeToHealth = TimeSpan.FromSeconds(3);
            timeToRecharge = TimeSpan.Zero;
            charging = false;
            if(health <= 0)
                Die(DeathType.Falling);
        }

        public override void Draw(GameTime gameTime)
        {
           if((file != null && !((File)file.Tag).PlayerInside) || file == null)
            {
                //DrawSkelton();
                DrawSkinedIV();
            }

            DrawWeapons();
            base.Draw(gameTime);

        }

        void DrawSkinedIV()
        {
            Matrix[] bones = animationPlayer.GetSkinTransforms();

            foreach (ModelMesh mesh in IVModel.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    effect.SetBoneTransforms(bones);
                

                    effect.World = Body.SubBodies[0].WorldTransform*Matrix.CreateTranslation(0, -1f, 0);

                    effect.View = camera.ViewMatrix;
                    effect.Projection = camera.ProjectionMatrix;

                    effect.EnableDefaultLighting();

                    effect.SpecularColor = new Vector3(0.25f);
                    effect.SpecularPower = 16;
                }

                mesh.Draw();
            }

            if(hasARocket)
                DrawRocket();
        }

        public void EatRocketAmmo(int value)
        {
            rocketAmmo += value;
            soundManager.Play3DSound("rockette_ammo",Body.CenterPosition);
        }

        void DrawRocket()
        {
            var coreIndex = skinningData.BoneIndices["CORE_dummy"];
            Matrix[] worldTransforms = animationPlayer.GetWorldTransforms();

            var rocketLauncherWorld = Body.SubBodies[0].OrientationMatrix*
                                      Matrix.CreateTranslation(
                                          new Vector3(
                                              Body.SubBodies[0].CenterPosition.X +
                                              (isDirectionRight
                                                   ? worldTransforms[coreIndex].Translation.X - 1.1f
                                                   : -worldTransforms[coreIndex].Translation.X + 1.1f),
                                              Body.SubBodies[0].CenterPosition.Y +
                                              worldTransforms[coreIndex].Translation.Y -
                                              1.2f,
                                              Body.SubBodies[0].CenterPosition.Z +
                                              worldTransforms[coreIndex].Translation.Z));
            DrawModel(rocketLauncher,rocketLauncherWorld);
            if (RocketEnabled && rocketAmmo > 0)
            {
                var rocket1World = Body.SubBodies[0].OrientationMatrix*
                                   Matrix.CreateTranslation(
                                       new Vector3(
                                           Body.SubBodies[0].CenterPosition.X +
                                           (isDirectionRight
                                                ? worldTransforms[coreIndex].Translation.X - .8f
                                                : -worldTransforms[coreIndex].Translation.X + .8f),
                                           Body.SubBodies[0].CenterPosition.Y + worldTransforms[coreIndex].Translation.Y -
                                           1.042f,
                                           Body.SubBodies[0].CenterPosition.Z + worldTransforms[coreIndex].Translation.Z +
                                           .55f));
                DrawModel(rocket, rocket1World);

                var rocket2World = Body.SubBodies[0].OrientationMatrix*
                                   Matrix.CreateTranslation(
                                       new Vector3(
                                           Body.SubBodies[0].CenterPosition.X +
                                           (isDirectionRight
                                                ? worldTransforms[coreIndex].Translation.X - .8f
                                                : -worldTransforms[coreIndex].Translation.X + .8f),
                                           Body.SubBodies[0].CenterPosition.Y + worldTransforms[coreIndex].Translation.Y -
                                           1.042f,
                                           Body.SubBodies[0].CenterPosition.Z + worldTransforms[coreIndex].Translation.Z -
                                           .55f));
                DrawModel(rocket, rocket2World);
            }

            if (rocketReloading)
                DrawQuad(rocketBasicEffect, rocketQuad, Vector3.Zero, rocketAnimationPlayer.Texture);
        }

        void DrawModel(Model model,Matrix world)
        {
            foreach (var mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    effect.World = world;
                    effect.View = camera.ViewMatrix;
                    effect.Projection = camera.ProjectionMatrix;
                    effect.SpecularColor = new Vector3(0.25f);
                    effect.SpecularPower = 16;
                }
                mesh.Draw();
            }
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