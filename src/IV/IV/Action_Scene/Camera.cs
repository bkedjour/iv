using System;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace IV.Action_Scene
{
    public class Camera
    {
        public Vector3 Position { get; set; }

        private float yaw;
        private float pitch;

        private float xVar, yVar;
        private bool xVarUp;
        private bool yVarUp = true;

        private TimeSpan varTimer;

        public float Yaw
        {
            get { return yaw; }
            set { yaw = MathHelper.WrapAngle(value); }
        }

        public float Pitch
        {
            get { return pitch; }
            set { pitch = MathHelper.Clamp(value, -MathHelper.PiOver2,MathHelper.PiOver2); }
        }

       
        public float Speed { get; set; }

        public Matrix WorldMatrix { get; set; }
        public Matrix ViewMatrix { get; protected set; }
        public Matrix ProjectionMatrix { get; set; }
        private bool starting;

        public bool UseMovementControls = true;
        private bool canZoom = true;


        public float aspectRatio { get; private set; }

        #region Focus Mode

        public bool IsFocusMode { get; private set; }
        private Vector3 focTargetPosition;
        private float focYaw, oldYaw;
        private float focPitch, oldPitch;
        private Vector3 focOldPosition;
        private bool focReterning;
        private object focus;
        private TimeSpan focusTime;
        private bool focIsModFree;

        private float returnAmount;
        private float focusAmount;

        private Texture2D skipTexture;

        #endregion

        #region Shase Mode

        private Entity entityToChase;

        private Vector3 offsetFromChaseTarget;
        private bool transformOffset;
        private float distanceToTarget;
        private float maxDistanceToTarget;
        public bool isChaseCameraMode{ get; private set;}
        public float InitialDistanceToTarget{ get; private set;}
        private bool isZoomingOut;
        private bool isZoomingIn;
        private float distanceToZoom;
        private float zoomTimer;
        public float CurrentDistanceToTarget{get{ return distanceToTarget;}}

        public float Distance { get { return distanceToTarget; } set { distanceToTarget = value; } }

        public void ActivateChaseCameraMode(Entity target, Vector3 offset, bool transform, float distance, bool isStarting)
        {
            entityToChase = target;
            offsetFromChaseTarget = offset;
            transformOffset = transform;
            if (isStarting)
            {
                starting = true;
                distanceToTarget = -5;
            }
            else
                distanceToTarget = distance;


            isChaseCameraMode = true;
            InitialDistanceToTarget = distance;
            maxDistanceToTarget = distance;

            UseMovementControls = false;
            canZoom = true;
        }

        public void DeactivateChaseCameraMode()
        {
            isChaseCameraMode = false;
        }
        #endregion

        public void ZoomOut(float distance)
        {
            if(!canZoom) return;
            if (distanceToTarget < distance) isZoomingOut = true;
            distanceToZoom = distance;
            if (!starting) canZoom = false;
        }
       

        public void ZoomIn()
        {
            if (distanceToTarget > InitialDistanceToTarget)
                isZoomingIn = true;
        }

        

        #region Shake

        private static readonly Random random = new Random();

        private bool shaking;

        private float shakeMagnitude;
        private float shakeDuration;
        private float shakeTimer;

        private Vector3 shakeOffset;

        public Vector3 Target = Vector3.Zero;

        public void Shake(float magnitude, float duration)
        {
            // We're now shaking
            shaking = true;

            // Store our magnitude and duration
            shakeMagnitude = magnitude;
            shakeDuration = duration;

            // Reset our timer
            shakeTimer = 0f;
        }

        #endregion

        public void MakeFocusTo(Vector3 targetPosition, float _yaw, float _pitch,object _focus,bool isReturning,float amount)
        {
            focTargetPosition = targetPosition;
            focYaw = _yaw;
            focPitch = _pitch;
            IsFocusMode = true;
            isChaseCameraMode = false;
            focOldPosition = Position;
            focus = _focus;
            focusTime = TimeSpan.Zero;
            returnAmount = amount;
            focReterning = isReturning;

            oldPitch = pitch;
            oldYaw = yaw;
        }
        public void MakeFocusTo(Vector3 targetPosition, float _yaw, float _pitch,object _focus,bool isReturning,float amount,
            bool oldModeShase)
        {
            focTargetPosition = targetPosition;
            focYaw = _yaw;
            focPitch = _pitch;
            IsFocusMode = true;
            isChaseCameraMode = false;
            focOldPosition = Position;
            focus = _focus;
            focusTime = TimeSpan.Zero;
            returnAmount = amount;
            focReterning = isReturning;
            focIsModFree = oldModeShase;
            oldPitch = pitch;
            oldYaw = yaw;
        }
        public void MakeFocusTo(Vector3 targetPosition, float _yaw, float _pitch, object _focus)
        {
            focTargetPosition = targetPosition;
            focYaw = _yaw;
            focPitch = _pitch;
            IsFocusMode = true;
            isChaseCameraMode = false;
            focOldPosition = Position;
            focus = _focus;
            focusTime = TimeSpan.Zero;
            focusAmount = .01f;
            returnAmount = .5f;

            oldPitch = pitch;
            oldYaw = yaw;
        }

        public void MakeFocusTo(Vector3 targetPosition, float _yaw, float _pitch, object _focus,TimeSpan time)
        {
            focTargetPosition = targetPosition;
            focYaw = _yaw;
            focPitch = _pitch;
            IsFocusMode = true;
            isChaseCameraMode = false;
            focOldPosition = Position;
            focus = _focus;
            focusTime = time;
            focusAmount = .01f;
            returnAmount = .5f;

            oldPitch = pitch;
            oldYaw = yaw;
        }

        public Camera(Vector3 position, float yaw, float pitch, float speed, float aspectRation)
        {
            Position = position;
            Yaw = yaw;
            Pitch = pitch;
            Speed = speed;
            starting = true;
            aspectRatio = aspectRation;
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                                   aspectRation, 1, 2000);

        }



        public void Update(GameTime gameTime, KeyboardState keyboardState)
        {
            varTimer += gameTime.ElapsedGameTime;
            if(varTimer > TimeSpan.FromMilliseconds(10))
            {
                varTimer = TimeSpan.Zero;
                const float val = .2f;
                xVar = xVarUp ? val : -val;
                yVar = yVarUp ? val : -val;

            }

            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            Yaw += (UseMovementControls?200-Mouse.GetState().X:xVar) * dt * .12f;
            Pitch += (UseMovementControls ?200- Mouse.GetState().Y : yVar) * dt * .12f;
            if(UseMovementControls)
                Mouse.SetPosition(200, 200);
            if (Yaw > .2f || Yaw < -.2f) xVarUp = !xVarUp;
            if (Pitch > 0f || Pitch < -.3f) yVarUp = !yVarUp;
         
            WorldMatrix = Matrix.CreateFromAxisAngle(Vector3.Right, Pitch) * Matrix.CreateFromAxisAngle(Vector3.Up, Yaw);
           
            if (isChaseCameraMode)
            {
                var offset = transformOffset
                                     ? Vector3.Transform(offsetFromChaseTarget, entityToChase.OrientationMatrix)
                                     : offsetFromChaseTarget;
                var lookAt = entityToChase.CenterPosition + offset +(shaking ? shakeOffset : Vector3.Zero);
                var backwards = WorldMatrix.Backward;

                Position = lookAt + (distanceToTarget)*backwards;
                if (starting && !isZoomingOut)
                    ZoomOut(maxDistanceToTarget);
              
            }
            else if (UseMovementControls)
            {
                var distance = Speed * dt;

                if (keyboardState.IsKeyDown(Keys.E))
                    MoveForward(distance);
                if (keyboardState.IsKeyDown(Keys.D))
                    MoveForward(-distance);
                if (keyboardState.IsKeyDown(Keys.F))
                    MoveRight(distance);
                if (keyboardState.IsKeyDown(Keys.S))
                    MoveRight(-distance);
                if (keyboardState.IsKeyDown(Keys.Z))
                    MoveUp(-distance);
                if (keyboardState.IsKeyDown(Keys.A))
                    MoveUp(distance);
            }
            zoomTimer += dt;
            if (isZoomingOut)
            {
                if (zoomTimer > .001f)
                {
                    zoomTimer = 0;
                    distanceToTarget += 1f;
                    if (distanceToTarget >= distanceToZoom)
                    {
                        isZoomingOut = false;
                        if (starting) starting = false;
                    }
                }
            }
            else if (isZoomingIn)
            {
                if (zoomTimer > .001f)
                {
                    zoomTimer = 0;
                    distanceToTarget -= 1f;
                    if (distanceToTarget <= InitialDistanceToTarget)
                    {
                        isZoomingIn = false;
                        canZoom = true;
                    }
                }
            }else if(IsFocusMode)
            {
                if(Keyboard.GetState().IsKeyDown(Keys.Enter))
                {
                    IsFocusMode = false;
                    isChaseCameraMode = true;
                    if (focus is Player)
                        ((Player)focus).Active = true;
                    focus = null;
                    Pitch = Yaw = 0;

                    focTargetPosition = Vector3.Zero;
                    focYaw = focPitch = 0;
                    focReterning = false;
                    focusTime = TimeSpan.Zero;
                }
                if (focusTime > TimeSpan.Zero)
                    focusTime -= gameTime.ElapsedGameTime;

                if (focReterning)
                {
                    float distance = Vector3.Distance(Position, focTargetPosition);
                    if (distance > 0)
                        Position = Vector3.Lerp(Position, focTargetPosition, /*.5f*/returnAmount / distance) + (shaking ? shakeOffset : Vector3.Zero);

                    yaw += (focYaw - yaw) * .05f;
                    pitch += (focPitch - pitch) * .05f;
                }
                else
                {
                    Position += (focTargetPosition - Position)*/*.01f*/focusAmount + (shaking ? shakeOffset : Vector3.Zero);
                    yaw += (focYaw - yaw)*.03f;
                    pitch += (focPitch - pitch)*.03f;
                }
                if (focusTime <= TimeSpan.Zero && (Math.Abs(Position.Y - focTargetPosition.Y) < .4f))
                {
                    if (focReterning)
                    {
                        IsFocusMode = false;
                        if(focIsModFree) UseMovementControls = true;
                        else
                            isChaseCameraMode = true;
                        if (focus is Player)
                            ((Player) focus).Active = true;
                        focus = null;
                        //Pitch = Yaw = 0;

                        focTargetPosition = Vector3.Zero;
                        //focYaw = focPitch = 0;
                        focReterning = false;
                        focusTime = TimeSpan.Zero;
                    }
                    else focReterning = true;
                    focYaw = oldYaw; focPitch = oldPitch;
                    focTargetPosition = focOldPosition;
                }
            }
            if (shaking)
            {
                shakeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (shakeTimer >= shakeDuration)
                {
                    shaking = false;
                    shakeTimer = shakeDuration;
                }

                float progress = shakeTimer / shakeDuration;


                float magnitude = shakeMagnitude * (1f - (progress * progress));

                shakeOffset = new Vector3(NextFloat(), NextFloat(), NextFloat()) * magnitude;
            }
            WorldMatrix *= Matrix.CreateTranslation(Position);

            ViewMatrix = Matrix.Invert(WorldMatrix);
        }
        private static float NextFloat()
        {
            return (float)random.NextDouble() * 2f - 1f;
        }
        void MoveForward(float distance)
        {
            Position += WorldMatrix.Forward * distance;
        }

        void MoveRight(float distance)
        {
            Position += WorldMatrix.Right * distance;
        }

        void MoveUp(float distance)
        {
            Position += new Vector3(0, distance, 0);
        }

        public void Draw(SpriteBatch sBatch)
        {
            if (IsFocusMode)
                sBatch.Draw(skipTexture, new Vector2(GameSettings.WindowWidth - skipTexture.Width - 3,
                                                     GameSettings.WindowHeight - skipTexture.Height - 3), Color.White);
        }
        public void LoadContent(ContentManager content)
        {
            skipTexture = content.Load<Texture2D>("Textures\\MENU\\Graphics\\passer");
        }
    }
}