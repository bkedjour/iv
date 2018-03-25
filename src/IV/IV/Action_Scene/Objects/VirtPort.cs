using System;
using System.Collections.Generic;
using BEPUphysics;
using BEPUphysics.Entities;
using IV.Action_Scene.ParticleSystems;
using IV.Action_Scene.ParticleSystems.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace IV.Action_Scene.Objects
{
    public class VirtPort : DrawableGameComponent
    {
        private Conveyor analConv;
        private Picker picker;
        private Elevator elevator;
        private Pucher sysPucher;
        private Pucher userPucher;
        private Conveyor sysConv;
        private Conveyor userConv;
        private ConvDoor intiviruScane;
        private readonly Vector3 gecPosition;

        private TimeSpan timeToScanne;
        private TimeSpan timeToGenerate = TimeSpan.FromSeconds(16);
        private readonly Space space;
        private readonly Camera camera;
        private readonly List<GameComponent> Components;
        private ContentManager content;
        
        private readonly Random rand = new Random();
        private Entity scanedEntity;

        ParticleSystem smokePlumeParticles;
        private Vector3 smokeOrigine;

        private KeyboardState oldState;

        public VirtPort(Game game, Space space, Camera camera, List<GameComponent> components,Vector3 GenPosition)
            : base(game)
        {
            this.space = space;
            Components = components;
            this.camera = camera;
            gecPosition = GenPosition;
        }

        public void SetPickerInfo( Vector3 PickerOrigin ,Box PickerHand)
        {
            picker = new Picker(Game, space, PickerHand, PickerOrigin, camera);
        }

        public void SetConveyorsInfo(Box AnalConv ,Box SysConv ,Box UserConv)
        {
            analConv = new Conveyor(Game, AnalConv, ConveyorDirecion.Right, space, 6,false);
            sysConv = new Conveyor(Game, SysConv, ConveyorDirecion.Right, space, 6,false);
            userConv = new Conveyor(Game, UserConv, ConveyorDirecion.Right, space, 6,false);
            
        }

        public void SetIntiVirusInfo( Box IntiVirusDoor,Box origin)
        {
            intiviruScane = new ConvDoor(Game, IntiVirusDoor, ConveyorDirecion.Right, space, 6, camera, origin);
            
        }

        public void SetElevPushersInfo(Box Elevator , Box SysPucher ,Box UserPucher)
        {
            sysPucher = new Pucher(Game, space, camera, SysPucher);
            userPucher = new Pucher(Game, space, camera, UserPucher);
            elevator = new Elevator(Game, space, camera, Elevator);
            smokeOrigine = elevator.Entity.CenterPosition;
        }

        public void LoadContent(ContentManager Content)
        {
            smokePlumeParticles = new SmokePlumeParticleSystem(Game, Content) { DrawOrder = 100 };
            smokePlumeParticles.Initialize();
            Components.Add(smokePlumeParticles);

            picker.LoadContent(Content);
            elevator.LoadContent(Content);
            content = Content;
            sysPucher.LoadContent(Content);
            userPucher.LoadContent(content);
            intiviruScane.LoadContent(content);

            analConv.Activate();
            sysConv.Activate();

            userConv.Activate();
            intiviruScane.Activate();

            Components.Add(picker);
            Components.Add(elevator);
            Components.Add(analConv);
            Components.Add(sysPucher);
            Components.Add(userPucher);
            Components.Add(sysConv);
            Components.Add(userConv);
            Components.Add(intiviruScane);

            picker.Activate();
        }

        public override void Update(GameTime gameTime)
        {
            GenerateFiles(gameTime);

            UpdateFire();

            if (!elevator.isActive)
            {
                scanedEntity = FireWallScanne();
                if (scanedEntity != null && scanedEntity.Tag is File)
                {
                    analConv.Disactivate();
                    timeToScanne += gameTime.ElapsedGameTime;
                    if (timeToScanne > TimeSpan.FromSeconds(2))
                    {
                        timeToScanne = TimeSpan.Zero;
                        switch (((File) scanedEntity.Tag).Type)
                        {
                            case FileType.System:
                                elevator.Move(sysConv.Entity.CenterPosition.Y);
                                break;
                            case FileType.User:
                                elevator.Move(userConv.Entity.CenterPosition.Y);
                                break;
                            case FileType.Unkown:
                                elevator.ThrowAnObject(true, scanedEntity, 40);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }else analConv.Activate();
            }
            else
            {
                if(elevator.RechedTheTop)
                    switch (((File)scanedEntity.Tag).Type)
                    {
                        case FileType.System:
                            sysPucher.Puch();
                            break;
                        case FileType.User:
                            userPucher.Puch();
                            break;
                        case FileType.Unkown:
                            userPucher.Puch();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
            }

            var fileToScane = IntiVirusScane();
            if (fileToScane != null && fileToScane.Tag is File && !((File)fileToScane.Tag).Scaned)
                intiviruScane.Scan((File) fileToScane.Tag);
            

            

            base.Update(gameTime);
        }
        
        void GenerateFiles(GameTime gameTime)
        {
            /*timeToGenerate += gameTime.ElapsedGameTime;
            if (timeToGenerate > TimeSpan.FromSeconds(16))
            {
                timeToGenerate = TimeSpan.Zero;
                var toAdd = new File(Game, space, camera,
                                     new Box(gecPosition, 3.5f, 3.5f, 3.5f, 100),
                                     rand.Next(2) == 0
                                         ? FileType.System
                                         : rand.Next(2) == 0 ? FileType.User : FileType.Unkown);
                toAdd.Fixed = false;
                toAdd.PlayerInside = false;
                toAdd.Initialize();
                toAdd.LoadContent(content);
                Components.Add(toAdd);
            }*/
            var currentKey = Keyboard.GetState();

            if (currentKey.IsKeyDown(Keys.NumPad1) && oldState.IsKeyUp(Keys.NumPad1))
            {
                var toAdd = new File(Game, space, camera,
                                     new Box(gecPosition, 3.5f, 3.5f, 3.5f, 100),
                                     FileType.Unkown) {Fixed = false, PlayerInside = false};
                toAdd.Initialize();
                toAdd.LoadContent(content);
                Components.Add(toAdd);
            }
            if (currentKey.IsKeyDown(Keys.NumPad2) && oldState.IsKeyUp(Keys.NumPad2))
            {
                var toAdd = new File(Game, space, camera,
                                     new Box(gecPosition, 3.5f, 3.5f, 3.5f, 100),
                                     FileType.User) { Fixed = false, PlayerInside = false };
                toAdd.Initialize();
                toAdd.LoadContent(content);
                Components.Add(toAdd);
            }
            if (currentKey.IsKeyDown(Keys.NumPad3) && oldState.IsKeyUp(Keys.NumPad3))
            {
                var toAdd = new File(Game, space, camera,
                                     new Box(gecPosition, 3.5f, 3.5f, 3.5f, 100),
                                     FileType.System) { Fixed = false, PlayerInside = false };
                toAdd.Initialize();
                toAdd.LoadContent(content);
                Components.Add(toAdd);
            }

            oldState = currentKey;
        }

        Entity IntiVirusScane()
        {
            for (var i = intiviruScane.Entity.CenterPosition.X /*- (intiviruScane.Entity.Width/3.0f)*/;
                 i < intiviruScane.Entity.CenterPosition.X + (intiviruScane.Entity.Width/2.0f);
                 i++)
            {
               
                var hitEntitie = new List<Entity>();
                var p = new Vector3(i, intiviruScane.Entity.CenterPosition.Y,
                                    intiviruScane.Entity.CenterPosition.Z);

                space.RayCast(p, Vector3.Up, 1f, false, hitEntitie, new List<Vector3>(),
                          new List<Vector3>(), new List<float>());
                foreach (var entity in hitEntitie.Where(entity => entity != intiviruScane.Entity))
                    return entity;

            }
            return null;
        }

        Entity FireWallScanne()
        {
            for (var i = elevator.Entity.CenterPosition.X - (elevator.Entity.Width / 3.0f) ;
                 i < elevator.Entity.CenterPosition.X + (elevator.Entity.Width / 2.0f);
                 i++)
            {
             
                var hitEntitie = new List<Entity>();
                var p = new Vector3(i, elevator.Entity.CenterPosition.Y,
                                    elevator.Entity.CenterPosition.Z);

                space.RayCast(p, Vector3.Up, 1f, false, hitEntitie, new List<Vector3>(),
                          new List<Vector3>(), new List<float>());
                foreach (var entity in hitEntitie.Where(entity => entity != elevator.Entity))
                    return entity;
                
            }
            return null;
        }


        void UpdateFire()
        {
            var fireOrigin = new Vector3(smokeOrigine.X + elevator.Entity.HalfWidth,
                                         smokeOrigine.Y - 5,
                                         smokeOrigine.Z - elevator.Entity.HalfLength);

            smokePlumeParticles.AddParticle(new Vector3(fireOrigin.X + rand.Next(35), fireOrigin.Y, fireOrigin.Z),
                                            Vector3.Zero);

            smokePlumeParticles.SetCamera(camera.ViewMatrix, camera.ProjectionMatrix);
        }
    }
}