using System;
using System.Collections.Generic;
using System.Linq;
using BEPUphysics;
using BEPUphysics.Entities;
using IV.Action_Scene.Enemies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace IV.Action_Scene.Objects
{
    class ActivationButton : DrawableGameComponent 
    {
        private Model model;
        private readonly Box button;
        private readonly Space space;
        private readonly Camera camera;
        private readonly Player player;
        public int ID { get; private set; }
        private Vector3 initPosition;
        private List<object> target;

       
        private TimeSpan timeToActivate;
        private TimeSpan timeToPresse;
        private bool pressRequest;
        private bool pressWorking;
        private bool isPressed;

        private readonly bool isReusable;
        private TimeSpan timeToRelease;
        private TimeSpan timeToBeginRelease;
        private bool releaseRequest;
        private bool releaseWorking;

        //Focus Mode
        private TimeSpan timeToMoveCamera;
        private bool isFocusMode;
        private Vector3 cameraPosition;
        private float cameraPitch;
        private float cameraYaw;
        private TimeSpan focusTime;

        private KeyboardState oldState;
        private readonly SoundManager soundManager;

        public ActivationButton(Game game,Box button, Space space, Camera camera,Player player, int id, bool isReusable) 
            : base(game)
        {
            this.space = space;
            this.isReusable = isReusable;
            ID = id;
            this.button = button;
            this.camera = camera;
            this.button = button;
            space.Add(button);
            this.player = player;
            initPosition = button.CenterPosition;
            soundManager = (SoundManager) Game.Services.GetService(typeof (SoundManager));
        }

        public void ActiveFocusMode(Vector3 _cameraPosition,float _cameraPitch,float _cameraYaw,float time)
        {
            cameraPosition = _cameraPosition;
            cameraYaw = _cameraYaw;
            cameraPitch = _cameraPitch;
            isFocusMode = true;
            focusTime = TimeSpan.FromSeconds(time);
        }

        public void SetTarget(object _target)
        {
            if(target == null) target = new List<object>();
            target.Add(_target);
        }

        public void LoadContent(ContentManager content)
        {
            model = content.Load<Model>("Models\\Button");
            EffectMaker.SetObjectEffect(GetType(), model, content);
        }

        public override void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();
            
            var hitEntitie = new List<Entity>();

            space.RayCast(button.CenterPosition, Vector3.Left, 3f, false, hitEntitie, new List<Vector3>(),
                          new List<Vector3>(), new List<float>());
            space.RayCast(button.CenterPosition, Vector3.Right, 3f, false, hitEntitie, new List<Vector3>(),
                          new List<Vector3>(), new List<float>());
            foreach (var entity in hitEntitie.Where(entity => entity == player.Body))
            {
                if (!player.Active) break;
                if (keyboardState.IsKeyDown(Keys.Enter) && oldState.IsKeyUp(Keys.Enter) && !isPressed && !pressRequest)
                {
                    pressRequest = true;
                    player.PresseButton();
                    timeToActivate = TimeSpan.FromSeconds(.5f);
                    timeToPresse = TimeSpan.FromSeconds(.2f);
                    if (isFocusMode)
                    {
                        timeToActivate = TimeSpan.FromSeconds(3);
                        timeToMoveCamera = TimeSpan.FromSeconds(1);
                        player.Active = false;
                    }
                }
                else if (isReusable && keyboardState.IsKeyDown(Keys.Enter) && oldState.IsKeyUp(Keys.Enter) && isPressed &&
                         !releaseRequest)
                {
                    player.ReleaseButton();
                    timeToRelease = TimeSpan.FromSeconds(.5f);
                    timeToBeginRelease = TimeSpan.FromSeconds(.2f);

                    releaseRequest = true;
                }
            }

            if (timeToPresse > TimeSpan.Zero)
            {
                timeToPresse -= gameTime.ElapsedGameTime;
                if (timeToPresse <= TimeSpan.Zero)
                {
                    pressWorking = true;
                    soundManager.Play3DSound("Button_activation", button.CenterPosition);
                }
            }
            if(timeToBeginRelease > TimeSpan.Zero)
            {
                timeToBeginRelease -= gameTime.ElapsedGameTime;
                if(timeToBeginRelease <= TimeSpan.Zero)
                    releaseWorking = true;
            }

            if (pressWorking)
            {
                if (button.CenterPosition.Y > initPosition.Y - 1.09f)
                    button.CenterPosition = new Vector3(button.CenterPosition.X, button.CenterPosition.Y - .0745f,
                                                        button.CenterPosition.Z);
                else
                {
                    pressWorking = false;
                    pressRequest = false;
                    isPressed = true;
                }
            }

            if(releaseWorking)
            {
                if (button.CenterPosition.Y < initPosition.Y)
                    button.CenterPosition = new Vector3(button.CenterPosition.X, button.CenterPosition.Y + .0745f,
                                                        button.CenterPosition.Z);
                else
                {
                    releaseWorking = false;
                    releaseRequest = false;
                    isPressed = false;
                }
            }

            if (timeToActivate > TimeSpan.Zero)
            {
                timeToActivate -= gameTime.ElapsedGameTime;
                if (timeToActivate <= TimeSpan.Zero)
                    Activate();
            }

            if(timeToRelease > TimeSpan.Zero)
            {
                timeToRelease -= gameTime.ElapsedGameTime;
                if(timeToRelease <= TimeSpan.Zero)
                    Disativate();
            }

            if(isFocusMode)
                if (timeToMoveCamera > TimeSpan.Zero)
                {
                    timeToMoveCamera -= gameTime.ElapsedGameTime;
                    if (timeToMoveCamera <= TimeSpan.Zero)
                        camera.MakeFocusTo(cameraPosition, cameraYaw, cameraPitch, player,focusTime);
                }

            oldState = keyboardState;

            base.Update(gameTime);
        }

        void Activate()
        {
            if (target == null) return;

            foreach (object obj in target)
            {
                if (obj is Conveyor)
                    ((Conveyor) obj).Activate();
                else if(obj is MachinGun)
                    ((MachinGun) obj).Disactivate();
                else if(obj is TurnDoor)
                    ((TurnDoor) obj).Open();
                else if(obj is PipeLineCorner)
                    ((PipeLineCorner) obj).Open();
            } 
           
        }

        void Disativate()
        {
            if (target == null) return;

            foreach (object obj in target)
            {
                if (obj is Conveyor)
                    ((Conveyor)obj).Disactivate();
                else if (obj is MachinGun)
                    ((MachinGun)obj).Activate();
                else if(obj is PipeLineCorner)
                    ((PipeLineCorner)obj).Close();
            }

        }

        public override void Draw(GameTime gameTime)
        {
            foreach (var mesh in model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    var effect = (Effect) meshPart.Tag;
                    effect.CurrentTechnique = effect.Techniques["Technique1"];

                    effect.Parameters["World"].SetValue(ID < 0
                                                            ? Matrix.CreateRotationZ(MathHelper.Pi)*
                                                              button.WorldTransform
                                                            : button.WorldTransform);
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
