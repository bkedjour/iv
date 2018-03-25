using System;
using System.Collections.Generic;
using System.Linq;
using BEPUphysics;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace IV.Action_Scene.Objects
{
    public class AutoDoor : DrawableGameComponent
    {
        private Model buttonModel;
        private readonly Box button;
        private readonly List<Door> doors;
        private readonly Space space;
        private readonly Camera camera;
        private readonly Player player;
        private bool openRequest;
        private TimeSpan timeToBeginOpening;
        private TimeSpan timeToMoveCamera;
        private TimeSpan timeToPresse;
        private TimeSpan timer = TimeSpan.FromSeconds(1);
        private int index = -1;
        private bool buttonPressed;
        bool doorsOpened;
        private readonly List<GameComponent> components;
        private Vector3 initPosition;
        private bool presseOnce;
        private readonly SoundManager soundManager;

        public AutoDoor(Game game, Camera camera, Space space, Box button, List<Box> doors, Player player,
            List<GameComponent> components)
            : base(game)
        {
            this.camera = camera;
            this.player = player;
            this.button = button;
            this.space = space;
            space.Add(button);
            this.components = components;
            this.doors = new List<Door>();
            foreach (var door in doors)
                this.doors.Add(new Door(Game, space, camera, door));
            initPosition = button.CenterPosition;
            soundManager = (SoundManager)Game.Services.GetService(typeof(SoundManager));
        }

        public void LoadContent(ContentManager content)
        {
            buttonModel = content.Load<Model>("Models\\Button");
            EffectMaker.SetObjectEffect(typeof (ActivationButton), buttonModel, content);
            var doorModel = content.Load<Model>("Models\\PORT_lvl1");
            EffectMaker.SetPort_lvl1(doorModel, content, "Shaders\\uv_fx");
            foreach (var door in doors)
            {
                door.LoadContent(doorModel);
                components.Add(door);
            }
            components.Add(this);

        }
        public override void Update(GameTime gameTime)
        {
           var hitEntitie = new List<Entity>();

           space.RayCast(button.CenterPosition, Vector3.Left, 3f, false, hitEntitie, new List<Vector3>(),
                         new List<Vector3>(), new List<float>());
           space.RayCast(button.CenterPosition, Vector3.Right, 3f, false, hitEntitie, new List<Vector3>(),
                         new List<Vector3>(), new List<float>());
          
           foreach (var entity in hitEntitie.Where(entity => entity == player.Body))
           {
               if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !presseOnce)
               {
                   presseOnce = true;
                   timeToBeginOpening = TimeSpan.FromSeconds(4);
                   timeToMoveCamera = TimeSpan.FromSeconds(1);
                   player.PresseButton();
                   player.Active = false;
                   timeToPresse = TimeSpan.FromSeconds(.2f);


                   ObjectivesManager.Objective_0_Done = true;
               }
           }
            if(timeToPresse > TimeSpan.Zero)
            {
                timeToPresse -= gameTime.ElapsedGameTime;
                if (timeToPresse <= TimeSpan.Zero)
                {
                    buttonPressed = true;
                    soundManager.Play3DSound("Button_activation", button.CenterPosition);
                }
            }
            if(timeToBeginOpening > TimeSpan.Zero)
            {
                timeToBeginOpening -= gameTime.ElapsedGameTime;
                if (timeToBeginOpening <= TimeSpan.Zero)
                    openRequest = true;
            }
            if(timeToMoveCamera > TimeSpan.Zero)
            {
                timeToMoveCamera -= gameTime.ElapsedGameTime;
                if (timeToMoveCamera <= TimeSpan.Zero)
                    camera.MakeFocusTo(new Vector3(263.0042f, -29.73733f, 43.60814f), .8300006f, .3660007f, player);
            }

            if(openRequest && !doorsOpened)
            {
                timer += gameTime.ElapsedGameTime;
                if(timer >= TimeSpan.FromSeconds(1f))
                {
                    timer -= TimeSpan.FromSeconds(1f);
                    if (++index >= doors.Count)
                        doorsOpened = true;
                    else if (!doors[index].Opened)
                        doors[index].Open(index == 3);
                }
            }
            if(buttonPressed)
            {
                if (button.CenterPosition.Y > initPosition.Y - 1.09f)
                    button.CenterPosition = new Vector3(button.CenterPosition.X, button.CenterPosition.Y - .0745f,
                                                        button.CenterPosition.Z);
            }

            base.Update(gameTime);
        }

      
        public override void Draw(GameTime gameTime)
        {
            foreach (var mesh in buttonModel.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    var effect = (Effect) meshPart.Tag;
                    effect.CurrentTechnique = effect.Techniques["Technique1"];

                    effect.Parameters["World"].SetValue(button.WorldTransform);
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

        protected override void Dispose(bool disposing)
        {
            if(disposing) doors.Clear();
            base.Dispose(disposing);
        }
    }

    class Door : DrawableGameComponent
    {
        private Model model;
        private readonly Box door;
        private readonly Camera camera;
        private TimeSpan timer;
        private readonly Vector3 initPosi;
        private bool openRequest;
        public bool Opened { get; private set; }
        private bool fullOpen;
        public Vector3 Position { get { return door.CenterPosition; } }
        private readonly SoundManager soundManager;

        public Door(Game game, Space space, Camera camera, Box door) : base(game)
        {
            this.door = door;
            space.Add(door);
            this.camera = camera;
            initPosi = door.CenterPosition;
            soundManager = (SoundManager) Game.Services.GetService(typeof (SoundManager));
        }
       
        public void LoadContent(Model _model)
        {
            model = _model;
        }
        public void Open(bool isFullOpen)
        {
            openRequest = true;
            fullOpen = isFullOpen;
        }
        public override void Update(GameTime gameTime)
        {
            if(openRequest && !Opened)
            {
                timer += gameTime.ElapsedGameTime;
                if (timer > TimeSpan.FromMilliseconds(10))
                {
                    timer -= TimeSpan.FromMilliseconds(10);
                    door.CenterPosition = new Vector3(door.CenterPosition.X, door.CenterPosition.Y + .1f,
                                                      door.CenterPosition.Z);
                }
                if (door.CenterPosition.Y - (door.Height / 2.0f) > initPosi.Y  + (fullOpen? (door.Height / 2.0f):0))
                {
                    Opened = true;
                    camera.Shake(.5f, .5f);
                    soundManager.Play3DSound("door_slam", door.CenterPosition);
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

                    effect.Parameters["World"].SetValue(door.WorldTransform);
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