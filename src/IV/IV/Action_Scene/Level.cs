using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using BEPUphysics;
using BEPUphysics.Entities;
using IV.Action_Scene.Enemies;
using IV.Action_Scene.Objects;
using IV.Action_Scene.Weapons;
using IVSaveLoadAssembly;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AutoDoor = IV.Action_Scene.Objects.AutoDoor;
using File = IV.Action_Scene.Objects.File;

namespace IV.Action_Scene
{
    public class Level : IDisposable 
    {
        private LevelInfo infos;
        public ContentManager Content{ get; private set;}
        private readonly List<GameComponent> Components;
        private readonly Game Game;

        private readonly Space space;
        private readonly Camera camera;
        private Player player;
        private Model IVModel;
        private Model levelModel;

        private CheckPoint checkPoint;
        private List<Vector3> checkPointsPositions;

        private int currentDebugTeleportIndex;
        private List<Vector3> debugTeleportations; 
       
        public bool PlayerDead { get { return player.IsDead; } }
        public bool LevelReached { get; set; }
        public bool ResetLevel { get; private set; }

        public bool CanShowMenu
        {
            get {return !camera.IsFocusMode; }
        }

        private readonly Random rand;
        private readonly int levelIndex;

        private readonly SoundManager soundManager;

        private ObjectivesManager objectivesManager;
        private List<ObjectiveInfo> objectivesPositions;

        public bool LevelLoaded;

        private bool isShowingCkeckPoint;
        private TimeSpan timeToShowCheckPoint;
        private readonly Texture2D checkPointTexture;
        private readonly Texture2D doubleJumpTexture;
        private TimeSpan doubleJumpTime;
        private bool isShowingDoubleJump;

        public Level(Game game, string path, List<GameComponent> components, Model ivModel, int levelIndex)
        {
            Content = new ContentManager(game.Services, "Content");
            LoadLevelInfo(path);
            soundManager = (SoundManager)game.Services.GetService(typeof (SoundManager));

            space = new Space();
            if (Environment.ProcessorCount > 1)
            {
                for (int i = 0; i < Environment.ProcessorCount; i++)
                {
                    space.ThreadManager.AddThread();
                }
                space.UseMultithreadedUpdate = true;
            }
            space.SimulationSettings.MotionUpdate.Gravity = new Vector3(0, -50f, 0);
            
            camera = new Camera(Vector3.Zero, 0, 0, 40, game.GraphicsDevice.Viewport.AspectRatio);
            Components = components;
            this.levelIndex = levelIndex;
            IVModel = ivModel;
            Game = game;
            rand = new Random();

            checkPointTexture = Content.Load<Texture2D>("Textures\\Objectives\\checkpoint");
            doubleJumpTexture = Content.Load<Texture2D>("Textures\\Objectives\\2xsaut_activ");
            if(levelIndex == 4)
                isShowingDoubleJump = true;

            levelModel = Content.Load<Model>(string.Format(@"Models/Level{0}", levelIndex));
            EffectMaker.SetLevelEffect(levelModel, Content, levelIndex);
            LoadContent();
            LevelLoaded = true;
            ResetLevel = false;
        }
        
        void LoadLevelInfo(string path)
        {
            var xml = new XmlSerializer(typeof (LevelInfo));
            var sr = new StreamReader(path);
            infos = (LevelInfo) xml.Deserialize(sr);
            sr.Close();
        }
        
        void LoadContent()
        {
            Content.Load<SpriteFont>("Fonts\\font");
            camera.LoadContent(Content);
           
            player = new Player(Game, space, camera, infos.IVPosition, Components);
            player.Initialize();
            player.LoadContent(Content, IVModel);
            Components.Add(player);
            if (levelIndex > 2)
                player.EnableRocket();
            if (levelIndex == 4)
                player.ActivateDoubleJump();

            camera.Position = infos.IVPosition;
            camera.ActivateChaseCameraMode(player.Body, new Vector3(0, 1, 0), false, 40, false);
            
            checkPointsPositions = new List<Vector3>();
            debugTeleportations = new List<Vector3>();

            foreach (var levelInfo in infos.Properties)
            {
                switch (levelInfo.Type)
                {
                    case ObjectType.Conveyor:
                        ConveyorDirecion convDirection;
                        switch (levelInfo.ConvDirection)
                        {
                            case ConveyorDirection.Left:
                                convDirection = ConveyorDirecion.Left;
                                break;
                            case ConveyorDirection.Right:
                                convDirection = ConveyorDirecion.Right;
                                break;
                            case ConveyorDirection.Backward:
                                convDirection = ConveyorDirecion.Backward;
                                break;
                            case ConveyorDirection.Forward:
                                convDirection = ConveyorDirecion.Forward;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        var conv = new Conveyor(Game, new Box(levelInfo.Position, levelInfo.Width, levelInfo.Height,
                                                              levelInfo.Length), convDirection, space, levelInfo.ConVelo,
                                                levelInfo.ConvIsFixed)
                                       {ActivationBtnID = levelInfo.ConActBtn ? levelInfo.ConBtnID : -1};
                        if (!levelInfo.ConActBtn)
                            conv.Activate();
                        Components.Add(conv);
                        break;
                    case ObjectType.Floor:
                        if (levelInfo.IsDynamic)
                        {
                            var toAdd = new Box(levelInfo.Position, levelInfo.Width, levelInfo.Height, levelInfo.Length,
                                                levelInfo.Mass) {Tag = levelInfo.Position};
                            var box = new Cube(Game, space, camera, toAdd) {Fixed = levelInfo.IsFixed};
                            box.LoadContent(Content);
                            Components.Add(box);
                        }
                        else
                            space.Add(new Box(levelInfo.Position, levelInfo.Width, levelInfo.Height, levelInfo.Length)
                                          {Tag = levelInfo.Rotate});
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            foreach (var button in infos.Buttons)
            {
                var btn = new ActivationButton(Game,
                                               new Box(button.Position, button.Dimention.X, button.Dimention.Y,
                                                       button.Dimention.Z), space, camera, player, button.ButtonID,
                                               button.IsReuse);
                if (button.FocusMode)
                    btn.ActiveFocusMode(new Vector3(button.CameraX, button.CameraY, button.CameraZ), button.CameraP,
                                        button.CameraYaw, button.Time);
                btn.LoadContent(Content);
                Components.Add(btn);
            }

            foreach (var brick in infos.WallBricks)
            {
                var br = new WallBrick(Game, space, camera,
                                       new Box(brick.Position, brick.Dimention.X, brick.Dimention.Y, brick.Dimention.Z,
                                               1f));
                br.LoadContent(Content);
                Components.Add(br);
            }


            if (infos.AutoDoor != null)
            {
                var doors = infos.AutoDoor.Doors.Select
                    (door => new Box(door.Position, door.Dimention.X, door.Dimention.Y, door.Dimention.Z)).ToList();

                var autoDoor = new AutoDoor(Game, camera, space,
                                            new Box(infos.AutoDoor.Button.Position, infos.AutoDoor.Button.Dimention.X,
                                                    infos.AutoDoor.Button.Dimention.Y, infos.AutoDoor.Button.Dimention.Z),
                                            doors, player, Components);
                autoDoor.LoadContent(Content);
            }

            foreach (var destroyer in infos.Destroyers)
            {
                var toAdd = new Box(destroyer.Position, destroyer.Dimention.X, destroyer.Dimention.Y,
                                    destroyer.Dimention.Z);
                toAdd.EventManager.InitialCollisionDetected += HandelDestroyerCollision;
                space.Add(toAdd);
            }

            foreach (var enemyInfo in infos.Enemies)
            {
                Enemy enemyToAdd = null;
                switch (enemyInfo.type)
                {
                    case EnemyType.Norton:
                        enemyToAdd = new Norton(Game, space, camera, enemyInfo.Position, rand, player,
                                                Components);
                        break;
                    case EnemyType.Kaspersky:
                        enemyToAdd = new Kaspersky(Game, space, camera, enemyInfo.Position, rand, player,
                                                   Components);
                        break;
                    case EnemyType.Virus:
                        enemyToAdd = new Virus(Game, space, camera, enemyInfo.Position, rand, player, Components);
                        break;
                }

                if (enemyToAdd == null) continue;

                enemyToAdd.Initialize();
                enemyToAdd.LoadContent(Content);
                Components.Add(enemyToAdd);
            }

            if (infos.LevelEnd != null)
            {
                var toAdd = new Box(infos.LevelEnd.Position, infos.LevelEnd.Dimention.X, infos.LevelEnd.Dimention.Y,
                                    infos.LevelEnd.Dimention.Z);
                toAdd.EventManager.InitialCollisionDetected += HandelLevelEndCollision;
                space.Add(toAdd);
            }
            foreach (var death in infos.Death)
            {
                var toAdd = new Box(death.Position, death.Dimention.X, death.Dimention.Y,
                                    death.Dimention.Z) {Tag = death.Cause};
                toAdd.EventManager.InitialCollisionDetected += HandelDeath;
                space.Add(toAdd);
            }

            if (infos.Ballance != null)
            {
                #region Declaration Of Ballance

                var ballance = new Ballance(Game, space, camera);
                ballance.SetPlatforme(new Box(infos.Ballance.platforme.Position,
                                              infos.Ballance.platforme.Dimention.X, infos.Ballance.platforme.Dimention.Y,
                                              infos.Ballance.platforme.Dimention.Z),
                                      new Box(infos.Ballance.platSupport.Position,
                                              infos.Ballance.platSupport.Dimention.X,
                                              infos.Ballance.platSupport.Dimention.Y,
                                              infos.Ballance.platSupport.Dimention.Z));
                ballance.SetCube(new Box(infos.Ballance.cubeSupport.Position,
                                         infos.Ballance.cubeSupport.Dimention.X,
                                         infos.Ballance.cubeSupport.Dimention.Y,
                                         infos.Ballance.cubeSupport.Dimention.Z),
                                 new Box(infos.Ballance.cubePlaforme.Position,
                                         infos.Ballance.cubePlaforme.Dimention.X,
                                         infos.Ballance.cubePlaforme.Dimention.Y,
                                         infos.Ballance.cubePlaforme.Dimention.Z),
                                 new Box(infos.Ballance.cubeRight.Position,
                                         infos.Ballance.cubeRight.Dimention.X,
                                         infos.Ballance.cubeRight.Dimention.Y,
                                         infos.Ballance.cubeRight.Dimention.Z),
                                 new Box(infos.Ballance.cubeLeft.Position,
                                         infos.Ballance.cubeLeft.Dimention.X,
                                         infos.Ballance.cubeLeft.Dimention.Y,
                                         infos.Ballance.cubeLeft.Dimention.Z));
                ballance.LoadContent(Content);
                Components.Add(ballance);

                #endregion
            }

            foreach (var machinGun in infos.MachinGuns)
            {
                var mg = new MachinGun(Game, space, camera, machinGun.Position, player, machinGun.Direction, Components)
                             {ActivationBtnID = machinGun.ButtonID};
                mg.Initialize();
                mg.LoadContent(Content);
                Components.Add(mg);
            }

            foreach (var plasmaGun in infos.PlasmaGuns)
            {
                var plas = new PlazmaGun(Game, space, camera, Components, plasmaGun.Position, player,
                                         plasmaGun.IsRightDirection);
                plas.Initialize();
                plas.LoadContent(Content);
                Components.Add(plas);
            }

            foreach (var point in infos.CheckPoints)
            {
                checkPointsPositions.Add(point.Position);
            }


            foreach (var teleportation in infos.DebugTeleportation)
            {
                debugTeleportations.Add(teleportation.Position);
            }


            if (infos.VirtualPort != null)
            {
                #region Declaration Of VirPort

                var virPort = new VirtPort(Game, space, camera, Components, infos.VirtualPort.GenPosition);

                virPort.SetPickerInfo(infos.VirtualPort.PickerOrigin.Position,
                                      new Box(infos.VirtualPort.PickerHand.Position,
                                              infos.VirtualPort.PickerHand.Dimention.X,
                                              infos.VirtualPort.PickerHand.Dimention.Y,
                                              infos.VirtualPort.PickerHand.Dimention.Z));
                virPort.SetConveyorsInfo(
                    new Box(infos.VirtualPort.AnalConv.Position,
                            infos.VirtualPort.AnalConv.Dimention.X,
                            infos.VirtualPort.AnalConv.Dimention.Y,
                            infos.VirtualPort.AnalConv.Dimention.Z),
                    new Box(infos.VirtualPort.SysConv.Position,
                            infos.VirtualPort.SysConv.Dimention.X,
                            infos.VirtualPort.SysConv.Dimention.Y,
                            infos.VirtualPort.SysConv.Dimention.Z),
                    new Box(infos.VirtualPort.UserConv.Position,
                            infos.VirtualPort.UserConv.Dimention.X,
                            infos.VirtualPort.UserConv.Dimention.Y,
                            infos.VirtualPort.UserConv.Dimention.Z));
                virPort.SetElevPushersInfo(
                    new Box(infos.VirtualPort.Elevator.Position,
                            infos.VirtualPort.Elevator.Dimention.X,
                            infos.VirtualPort.Elevator.Dimention.Y,
                            infos.VirtualPort.Elevator.Dimention.Z),
                    new Box(infos.VirtualPort.SysPucher.Position,
                            infos.VirtualPort.SysPucher.Dimention.X,
                            infos.VirtualPort.SysPucher.Dimention.Y,
                            infos.VirtualPort.SysPucher.Dimention.Z),
                    new Box(infos.VirtualPort.UserPucher.Position,
                            infos.VirtualPort.UserPucher.Dimention.X,
                            infos.VirtualPort.UserPucher.Dimention.Y,
                            infos.VirtualPort.UserPucher.Dimention.Z));
                virPort.SetIntiVirusInfo(
                    new Box(infos.VirtualPort.IntiVirusDoor.Position,
                            infos.VirtualPort.IntiVirusDoor.Dimention.X,
                            infos.VirtualPort.IntiVirusDoor.Dimention.Y,
                            infos.VirtualPort.IntiVirusDoor.Dimention.Z),
                    new Box(infos.VirtualPort.IntiVirusOrigin.Position,
                            infos.VirtualPort.IntiVirusOrigin.Dimention.X,
                            infos.VirtualPort.IntiVirusOrigin.Dimention.Y,
                            infos.VirtualPort.IntiVirusOrigin.Dimention.Z));

                #endregion

                virPort.LoadContent(Content);
                Components.Add(virPort);
            }

            if (infos.AntiVirus != null)
            {
                if (infos.AntiVirus.BigGearInfo != null && infos.AntiVirus.DoorInfo != null &&
                    infos.AntiVirus.LaserInfo != null
                    && infos.AntiVirus.SmallGearInfo_1 != null && infos.AntiVirus.SmallGearInfo_2 != null &&
                    infos.AntiVirus.BlockEntity != null)
                {
                    var antiVirusSys = new AntiVirusSys(Game, space, camera, Components, player.Body);
                    antiVirusSys.SetDoor(infos.AntiVirus.DoorInfo.Position, infos.AntiVirus.DoorInfo.Dimention);
                    antiVirusSys.SetGears(infos.AntiVirus.BigGearInfo.Position, infos.AntiVirus.SmallGearInfo_1.Position,
                                          infos.AntiVirus.SmallGearInfo_2.Position);
                    antiVirusSys.SetLaser(infos.AntiVirus.LaserInfo.Position);
                    antiVirusSys.SetBlocker(infos.AntiVirus.BlockEntity.Position, infos.AntiVirus.BlockEntity.Dimention);
                    antiVirusSys.LoadContent(Content);
                    Components.Add(antiVirusSys);
                }
            }

            foreach (var ammo in infos.RocketAmmos)
            {
                var amm = new RocketAmmo(Game, space, camera, ammo.Position, Components);
                amm.LoadContent(Content);
                Components.Add(amm);
            }

            foreach (var door in infos.TurningDoors)
            {
                var turnDoor = new TurnDoor(Game, camera, space, door.Position, door.Dimention, door.IsRight)
                                   {ActivationBtnID = door.ButtonID};
                turnDoor.LoadContent(Content);
                Components.Add(turnDoor);
            }

            foreach (var _slider in infos.Sliders)
            {
                var slider = new Slider(Game, space, _slider.Position,
                                        player.Body, _slider.Velocity, camera);
                slider.LoadContent(Content);
                slider.OnPlayerIsOn += player.OnSliding;
                Components.Add(slider);
            }

            foreach (var _slider in infos.AutoSliders)
            {
                AutoSlider slider;
                if(levelIndex == 1)
                {
                    slider = new SmailSlider(Game, space, _slider.Position,
                                             player.Body, _slider.Velocity, camera);
                }
                else slider = new AutoSlider(Game, space, _slider.Position,
                                            player.Body, _slider.Velocity, camera);
                slider.LoadContent(Content);
                slider.OnPlayerIsOn += player.OnSliding;
                Components.Add(slider);
            }

            foreach (var b in infos.BrickBlocker)
            {
                var blocker = new BrickBlocker(Game, space, camera, b.Position, b.Dimention, player);
                blocker.LoadContent(Content);
                Components.Add(blocker);
            }

            foreach (var b in infos.Bricks)
            {
                var brick = new Brick(Game, space, camera, new Box(b.Position, 14.54198f, 4.56599f,
                                                                   8.683978f, 800));
                brick.LoadContent(Content);
                Components.Add(brick);
            }

            if (infos.PipeLineCorner != null && infos.FireFileOrigin != null)
            {
                var tube = new PipeLineCorner(Game, space, camera, infos.PipeLineCorner.Position)
                               {
                                   ActivationBtnID = infos.PipeLineCorner.ButtonID
                               };
                tube.LoadContent(Content);
                Components.Add(tube);

                var firePosi = new Vector3(infos.PipeLineCorner.Position.X - 5,
                                           infos.PipeLineCorner.Position.Y - 3.9f, infos.PipeLineCorner.Position.Z - .5f);
                var fileCannon = new FilesCannon(Game, space, Content, Components, camera, Content,
                                                 infos.FireFileOrigin.Position, firePosi, player.Body);
                Components.Add(fileCannon);

                tube.OnPipeLineMoved += fileCannon.Fire;
                tube.OnPipeLineInitMove += fileCannon.Pause;
            }

            if (infos.GetWay != null && infos.GetWay.ElevatorInfo != null && infos.GetWay.FileOrigin != null
                && infos.GetWay.Pushers.Count > 0)
            {
                var getWay = new GetWay(Game, Components);
                getWay.SetElevatro(space, camera, infos.GetWay.ElevatorInfo.Position,
                                   infos.GetWay.ElevatorInfo.Dimention);
                getWay.SetFileGenerator(infos.GetWay.FileOrigin.Position, space, camera, Content);
                foreach (var pusher in infos.GetWay.Pushers)
                    getWay.SetPushers(space, camera, pusher.Position, pusher.Dimention, pusher.ID);
                getWay.LoadContent(Content);
                Components.Add(getWay);
            }

            foreach (var laser in infos.Lasers)
            {
                var toAdd = new LaserWall(Game, laser.Position, space, camera, laser.direction,Components);
                toAdd.Initialize();
                toAdd.LoadContent(Content);
                Components.Add(toAdd);
            }

            foreach (var turner in infos.SlidersTurners)
            {
                var toAdd = new SliderTurner(Game, space, turner.Position, turner.Dimention, turner.Velocity);
                Components.Add(toAdd);
            }

            foreach (var objective in infos.Objectives)
            {
                if(objectivesPositions == null)objectivesPositions = new List<ObjectiveInfo>();
                objectivesPositions.Add(new ObjectiveInfo {ID = objective.ID, Position = objective.Position});
            }

            //Button Activation
            foreach (var component in Components.Where(component => component is ActivationButton))
                foreach (var gameComponent in Components)
                {
                    if (gameComponent is Conveyor)
                        if (((Conveyor) gameComponent).ActivationBtnID == ((ActivationButton) component).ID)
                            ((ActivationButton) component).SetTarget(gameComponent);
                    if (gameComponent is MachinGun)
                        if (((MachinGun) gameComponent).ActivationBtnID == ((ActivationButton) component).ID)
                            ((ActivationButton) component).SetTarget(gameComponent);
                    if (gameComponent is TurnDoor)
                        if (((TurnDoor) gameComponent).ActivationBtnID == ((ActivationButton) component).ID)
                            ((ActivationButton) component).SetTarget(gameComponent);
                    if (gameComponent is PipeLineCorner)
                        if (((PipeLineCorner) gameComponent).ActivationBtnID == ((ActivationButton) component).ID)
                            ((ActivationButton) component).SetTarget(gameComponent);
                }

            infos = null;
        }

        void CheckForCheckPoint()
        {
            foreach (var position in checkPointsPositions)
            {
                if (Math.Abs(position.Y - player.Body.CenterPosition.Y) < 5 &&
                    Math.Abs(position.X - player.Body.CenterPosition.X) < 2)
                {
                    checkPointsPositions.Remove(position);
                    SaveCheckPoint();
                    isShowingCkeckPoint = true;
                    break;
                }
            }
        }

        public void SetObjectivesManager(ObjectivesManager obj)
        {
            objectivesManager = obj;
        }

        void CheckForObjectives()
        {
            foreach (var position in objectivesPositions)
            {
                if (Math.Abs(position.Position.Y - player.Body.CenterPosition.Y) < 10 &&
                    Math.Abs(position.Position.X - player.Body.CenterPosition.X) < 3)
                {
                    objectivesPositions.Remove(position);

                    if (position.ID == 0 && ObjectivesManager.Objective_0_Done) break;
                    if (position.ID == 1 && ObjectivesManager.Objective_1_Done) break;
                    if (position.ID == 3 && ObjectivesManager.Objective_3_Done) break;
                    if (position.ID == 4 && ObjectivesManager.Objective_4_Done) break;

                    objectivesManager.ShowObjective(position.ID);
                    break;
                }
            }
        }


        private void SaveCheckPoint()
        {
            checkPoint = new CheckPoint {PlayerPosition = player.Body.CenterPosition};
        }

        public void LoadCheckPoint()
        {
            if (checkPoint == null)
            {
                ResetLevel = true;
                return;
            }
            player.Initialize(checkPoint.PlayerPosition);
        }

        void UpdateCheckPointTexture(GameTime gameTime)
        {
            if(isShowingCkeckPoint)
            {
                timeToShowCheckPoint += gameTime.ElapsedGameTime;

                if(timeToShowCheckPoint > TimeSpan.FromSeconds(2))
                {
                    isShowingCkeckPoint = false;
                    timeToShowCheckPoint = TimeSpan.Zero;
                }
            }
        }

        void UpdateDoubleJump(GameTime gameTime)
        {
            if(isShowingDoubleJump)
            {
                doubleJumpTime += gameTime.ElapsedGameTime;

                if(doubleJumpTime > TimeSpan.FromSeconds(6))
                {
                    isShowingDoubleJump = false;
                    doubleJumpTime = TimeSpan.Zero;
                }
            }
        }

        private void HandelLevelEndCollision(Entity sender, Entity other, CollisionPair collisionpair)
        {
            if (other.CompoundBody != null && other.CompoundBody.Tag is Player)
                LevelReached = true;

            if (other.Tag is Cube)
            {
                Components.Remove((Cube)other.Tag);
                space.Remove(other);
            }
        }

        private void HandelDeath(Entity sender, Entity other, CollisionPair collisionpair)
        {
            if (other.CompoundBody != null && other.CompoundBody.Tag is Player)
                player.Die((DeathType) sender.Tag);
        }

        private void HandelDestroyerCollision(Entity sender, Entity other, CollisionPair collisionpair)
        {
            if (other.Tag is File)
            {
                if(((File)other.Tag).PlayerInside)
                {
                    if (((File) other.Tag).Type == FileType.System)
                        LevelReached = true;
                    else player.Die(DeathType.Crach);
                }

                Components.Remove((File)other.Tag);
                space.Remove(other);
            }
            else if (other.Tag is Cube)
            {
                Components.Remove((Cube) other.Tag);
                space.Remove(other);
            }
            else if (other.CompoundBody != null && other.CompoundBody.Tag is Player)
                player.Die(DeathType.Crach);
        }

        public void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();

            soundManager.SetListener(camera.Position);
            camera.Update(gameTime, keyboardState);

            CheckForCheckPoint();
            CheckForObjectives();
            UpdateCheckPointTexture(gameTime);
            UpdateDoubleJump(gameTime);
        }

        public void UpdateSpace(GameTime gameTime)
        {
            space.Update(gameTime);
        }

       
        public void NextDebugTeleport()
        {
            if (debugTeleportations == null || debugTeleportations.Count == 0) return;

            if (currentDebugTeleportIndex >= debugTeleportations.Count)
                currentDebugTeleportIndex = 0;

            player.Initialize(debugTeleportations[currentDebugTeleportIndex]);

            currentDebugTeleportIndex++;
        }

        public void Draw(GameTime gameTime)
        {
            //EffectMaker.LightPosition = camera.Position + camera.WorldMatrix.Forward * 5;
           foreach (var mesh in levelModel.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    var effect = (Effect)meshPart.Tag;
                    effect.CurrentTechnique = effect.Techniques["Technique1"];

                    effect.Parameters["World"].SetValue(Matrix.Identity);
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);

                    Vector3 lightPos;
                   
                    if (camera.IsFocusMode)
                        lightPos = camera.Position + camera.WorldMatrix.Forward * 15;
                    else if (player.IsInAFile)
                        lightPos = new Vector3(player.file.CenterPosition.X, player.file.CenterPosition.Y,
                                               player.file.CenterPosition.Z + 15);
                    else
                        lightPos = new Vector3(player.Body.CenterPosition.X, player.Body.CenterPosition.Y,
                                               player.Body.CenterPosition.Z + 15);
                    EffectMaker.LightPosition = lightPos;
                    effect.Parameters["LightPosition"].SetValue(lightPos);
                    effect.Parameters["CameraPosition"].SetValue(camera.Position);

                    meshPart.Effect = effect;

                }
                mesh.Draw();
            }

        }

        public void Dispose()
        {
            foreach (var component in Components)
            {
                component.Dispose();
            }
            foreach (var entity in space.Entities)
            {
                entity.Tag = null;
            }
            int i = space.Entities.Count - 1;
            while (space.Entities.Count > 0)
            {
                space.Remove(space.Entities[i]);
                i--;
            }
            space.Dispose();
            IVModel = null;
            checkPointsPositions.Clear();
            if (levelModel != null)
                foreach (var mesh in levelModel.Meshes)
                    foreach (var meshPart in mesh.MeshParts)
                        meshPart.Effect.Dispose();
            levelModel = null;
            
            Content.Unload();
        }
        
        public void Draw2DContent(SpriteBatch spriteBatch)
        {
            player.DrawObjects(spriteBatch);
            camera.Draw(spriteBatch);

            if(isShowingCkeckPoint)
                spriteBatch.Draw(checkPointTexture,
                                 new Vector2((GameSettings.WindowWidth - checkPointTexture.Width)/2.0f,
                                             GameSettings.WindowHeight - checkPointTexture.Height - 3),
                                 Color.White);

            if (isShowingDoubleJump)
                spriteBatch.Draw(doubleJumpTexture,
                                 new Vector2((GameSettings.WindowWidth - doubleJumpTexture.Width)/2.0f,
                                             (GameSettings.WindowHeight - doubleJumpTexture.Height)), Color.White);
        }

    }

    class  CheckPoint
    {
        public Vector3 PlayerPosition;

    }

    struct ObjectiveInfo
    {
        public Vector3 Position;
        public int ID;
    }
}