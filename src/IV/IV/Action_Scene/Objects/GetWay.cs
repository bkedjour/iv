using System;
using System.Collections.Generic;
using System.Linq;
using BEPUphysics;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Action_Scene.Objects
{
    class GetWay : GameComponent
    {
        private GetWayElevator elevator;
        private GetWayFileGenerator fileGenerator;
        private readonly List<GetWayPucher> pushers;

        private readonly List<GameComponent> components;

        public GetWay(Game game, List<GameComponent> components) : base(game)
        {
            pushers = new List<GetWayPucher>();
            this.components = components;
        }

        public void SetElevatro(Space space,Camera camera,Vector3 position,Vector3 dimension)
        {
            elevator = new GetWayElevator(Game, space, camera, position, dimension);
        }

        public void SetFileGenerator(Vector3 position, Space space, Camera camera, ContentManager content)
        {
            fileGenerator = new GetWayFileGenerator(Game, components, position, space, camera,content);
        }

        public void SetPushers(Space space, Camera camera, Vector3 position, Vector3 dimension, int id)
        {
            pushers.Add(new GetWayPucher(Game, space, camera, position, dimension, id));
        }

        public void LoadContent(ContentManager content)
        {
            elevator.LoadContent(content);
            foreach (var pucher in pushers)
            {
                pucher.LoadContent(content);
                components.Add(pucher);

                elevator.OnBeginMoving += pucher.GetID;
                pucher.OnSetTarget += elevator.GetTargetPosition;
                elevator.OnReachTarget += pucher.Move;
            }
            components.Add(elevator);
            elevator.OnWaitingFiles += fileGenerator.Generate;
            fileGenerator.OnGenerating += elevator.WaitTheFile;


            fileGenerator.Generate(this,EventArgs.Empty);
        }
    }


    internal delegate void ElevatorPusher(int target);
    class GetWayElevator : DrawableGameComponent
    {
        private Model model;
        private readonly Box entity;
        private readonly Box line;
        private readonly Camera camera;
        private readonly Space space;
        private readonly Vector3 initPosition;

        public event ElevatorPusher OnBeginMoving;
        public event ElevatorPusher OnReachTarget;
        public event EventHandler OnWaitingFiles;

        private readonly Random rand;
        private Vector3 targetPosition;
        private int targetID;

        private bool isWaitingFile;
        private bool isMoving;
        private bool isMovingBack;
        private bool isReady;

        private TimeSpan timeToBack;
        private TimeSpan timeToWait;
        private TimeSpan timeToMove;

        public GetWayElevator(Game game, Space space, Camera camera,Vector3 position, Vector3 dimension) 
            : base(game)
        {
            this.space = space;
            this.camera = camera;
            entity = new Box(position, dimension.X, dimension.Y, dimension.Z);
            space.Add(entity);
            line = new Box(new Vector3(position.X, position.Y - 43.339835f, position.Z), 4.595998f, 86.67967f, 4.692007f);
            space.Add(line);
            rand = new Random();
            initPosition = position;
        }

        public void LoadContent(ContentManager content)
        {
            model = content.Load<Model>("Models\\GetWayElev");
            EffectMaker.SetObjectEffect(GetType(), model, content);
        }

        public void WaitTheFile(object sender, EventArgs eventArgs)
        {
            isWaitingFile = true;
        }

        void DitectAFile()
        {
            foreach (var e in space.DynamicEntities.Where(e => e.Tag is Cube))
            {
                for (var i = entity.CenterPosition.X - (entity.Width/2.0f);
                     i <= entity.CenterPosition.X + (entity.Width/2.0f);
                     i += 3)
                {
                    Vector3 hit, normal;
                    float t;
                    if (e.RayTest(new Vector3(i, entity.CenterPosition.Y, entity.CenterPosition.Z), Vector3.Up, 2,
                                       false,
                                       out hit, out normal, out t))
                    {
                        isWaitingFile = false;
                        Move();
                        return;
                    }

                }
            }
        }

        void Move()
        {
            targetID = rand.Next(6);
            if (OnBeginMoving != null)
                OnBeginMoving(targetID);
        }

        public void GetTargetPosition(Vector3 position)
        {
            targetPosition = position;
            isReady = true;
            timeToWait = TimeSpan.Zero;
        }

        static bool IsDistanceZero(Vector3 a, Vector3 b)
        {
            return Math.Abs(a.Y - b.Y) < .3f;
        }

        public override void Update(GameTime gameTime)
        {
            if(isReady)
            {
                timeToMove += gameTime.ElapsedGameTime;
                if (timeToMove > TimeSpan.FromSeconds(2))
                {
                    entity.LinearVelocity = new Vector3(0, 10, 0);
                    isMoving = true;
                    isReady = false;
                    timeToMove = TimeSpan.Zero;
                }
            }
            if (isMoving && IsDistanceZero(entity.CenterPosition, targetPosition))
            {
                if (entity.LinearVelocity != Vector3.Zero &&OnReachTarget != null)
                    OnReachTarget(targetID);
                entity.LinearVelocity = Vector3.Zero;
                timeToBack += gameTime.ElapsedGameTime;
                if (timeToBack > TimeSpan.FromSeconds(2))
                {
                    isMovingBack = true;
                    isMoving = false;
                    timeToBack = TimeSpan.Zero;
                    entity.LinearVelocity = new Vector3(0, -10, 0);
                }
            }

            if(isMovingBack && IsDistanceZero(entity.CenterPosition,initPosition))
            {
                isMovingBack = false;
                entity.LinearVelocity = Vector3.Zero;
                OnWaitingFiles(this, EventArgs.Empty);
            }

            if(isWaitingFile)
            {
                timeToWait += gameTime.ElapsedGameTime;
                if (timeToWait > TimeSpan.FromSeconds(10))
                {
                    isWaitingFile = false;
                    timeToWait = TimeSpan.Zero;
                    OnWaitingFiles(this, EventArgs.Empty);
                }
                else
                    DitectAFile();
            }

            line.LinearVelocity = entity.LinearVelocity;
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

                    effect.Parameters["World"].SetValue(entity.WorldTransform*Matrix.CreateTranslation(-.2f, 0, -.2f));
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

    class GetWayFileGenerator : GameComponent
    {
        private readonly List<GameComponent> components;
        private readonly Vector3 source;
        private readonly Space space;
        private readonly Camera camera;
        public event EventHandler OnGenerating;
        private readonly ContentManager content;

        public GetWayFileGenerator(Game game,List<GameComponent> components, Vector3 source, Space space, Camera camera, 
            ContentManager content) 
            : base(game)
        {
            this.components = components;
            this.content = content;
            this.camera = camera;
            this.space = space;
            this.source = source;
        }

        public void Generate(object sender, EventArgs eventArgs)
        {
            if (OnGenerating != null)
                OnGenerating(this, EventArgs.Empty);

            var toAdd = new Cube(Game, space, camera, new Box(source, 3.5f, 3.5f, 3.5f, 240));
            toAdd.LoadContent(content);
            components.Add(toAdd);
        }
   }

    internal delegate void SetTarget(Vector3 position);
    class GetWayPucher : DrawableGameComponent
    {
        private Model model;
        private readonly Vector3 initPosition;
        private readonly Camera camera;
        private readonly int id;
        private readonly Box entity;
        public event SetTarget OnSetTarget;

        private bool isMoving;
        private bool isMovingBack;

        public GetWayPucher(Game game, Space space, Camera camera, Vector3 initPosition,Vector3 dimension, int id) 
            : base(game)
        {
            this.id = id;
            this.initPosition = initPosition;
            this.camera = camera;
            entity = new Box(initPosition, dimension.X, dimension.Y, dimension.Z);
            space.Add(entity);
        }

        public void LoadContent(ContentManager content)
        {
            model = content.Load<Model>("Models\\GetWayPusher");
            EffectMaker.SetObjectEffect(GetType(), model, content);
        }


        public void GetID(int _id)
        {
            if(id != _id) return;
            if (OnSetTarget != null)
                OnSetTarget(new Vector3(initPosition.X, initPosition.Y - entity.HalfHeight, initPosition.Z));
        }

        public void Move(int _id)
        {
            if(id != _id) return;
            isMoving = true;
            isMovingBack = false;
            entity.LinearVelocity = new Vector3(10, 0, 0);
        }

        public override void Update(GameTime gameTime)
        {
            if(isMoving)
            {
                if(entity.CenterPosition.X >= initPosition.X + entity.HalfWidth + entity.HalfWidth/2)
                {
                    isMovingBack = true;
                    isMoving = false;
                    entity.LinearVelocity = new Vector3(-10,0,0);
                }

            }
            if(isMovingBack)
            {
                if(Math.Abs(entity.CenterPosition.X - initPosition.X) < .2f)
                {
                    isMovingBack = false;
                    entity.LinearVelocity = Vector3.Zero;
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

                    effect.Parameters["World"].SetValue(entity.WorldTransform*
                                                        Matrix.CreateTranslation(entity.HalfWidth - .05f, 1.25f, .05f));
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
