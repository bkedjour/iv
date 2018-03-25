using System;
using System.Collections.Generic;
using BEPUphysics;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace IV.Action_Scene.Objects
{
    public enum ConveyorDirecion{Left,Right,Forward,Backward}
    public class Conveyor : DrawableGameComponent
    {
        protected readonly Box entity;
        protected readonly float velocity;
        protected readonly ConveyorDirecion direction;
        protected bool active;
        private readonly Space space;
        public Box Entity { get { return entity; } }
        public object Tag { get; set; }
        private readonly bool fixTheEntity;
        public int ActivationBtnID { get; set; }
        private KeyboardState oldState;

        public Conveyor(Game game, Box _entity, ConveyorDirecion direction, Space space, float velocity,bool fixTheEntity)
            : base(game)
        {
            this.direction = direction;
            this.velocity = velocity;
            this.space = space;
            entity = _entity;
            space.Add(entity);
            this.fixTheEntity = fixTheEntity;
        }

        public void Activate()
        {
            active = true;
        }

        public void Disactivate()
        {
            active = false;
        }

        public override void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();
            if(active)
                Move(keyboardState);
            oldState = keyboardState;
            base.Update(gameTime);
        }

        void Move(KeyboardState keyboardState)
        {
            if (direction == ConveyorDirecion.Left || direction == ConveyorDirecion.Right)
                for (var i = entity.CenterPosition.X - (entity.Width/2.0f);
                     i <= entity.CenterPosition.X + (entity.Width/2.0f);
                     i++)
                {
                    var hitEntitie = new List<Entity>();
                    var p = new Vector3(i, entity.CenterPosition.Y, entity.CenterPosition.Z);

                    space.RayCast(p, Vector3.Up, 1f, false, hitEntitie, new List<Vector3>(),
                                  new List<Vector3>(), new List<float>());


                    foreach (var hit in hitEntitie.Where(ent => ent != entity))
                    {
                        if (hit.Tag is Player && ((Player)hit.Tag).Active && !((Player)hit.Tag).IsInsideFile)
                        {
                            var opv = hit.LinearVelocity;
                            opv.X = direction == ConveyorDirecion.Left ? -velocity : velocity;
                            if (!((Player)hit.Tag).IsInAFile)
                            {
                                if (keyboardState.IsKeyDown(Keys.Up) && oldState.IsKeyUp(Keys.Up))
                                    opv.Y = 27;
                                if (keyboardState.IsKeyDown(Keys.Left))
                                    opv.X = direction == ConveyorDirecion.Left ? -20 - velocity : -20 + velocity;
                                if (keyboardState.IsKeyDown(Keys.Right))
                                    opv.X = direction == ConveyorDirecion.Left ? 20 - velocity : 20 + velocity;
                            }
                            hit.LinearVelocity = opv;
                        }
                        else if(!(hit.Tag is Player))
                        {
                            var ov = hit.LinearVelocity;
                            ov.X = direction == ConveyorDirecion.Left ? -velocity : velocity;
                            hit.LinearVelocity = ov;
                            if (fixTheEntity)
                            {
                                var op = hit.CenterPosition;
                                op.Z = entity.CenterPosition.Z;
                                hit.CenterPosition = op;
                            }
                        }
                    }
                }
            else
                for (var i = entity.CenterPosition.Z - (entity.Length/2.0f);
                     i < entity.CenterPosition.Z + (entity.Length/2.0f);
                     i++)
                {
                    var hitEntitie = new List<Entity>();
                    var p = new Vector3(entity.CenterPosition.X, entity.CenterPosition.Y, i);

                    space.RayCast(p, Vector3.Up, 1f, false, hitEntitie, new List<Vector3>(),
                                  new List<Vector3>(), new List<float>());


                    foreach (var hit in hitEntitie.Where(ent => ent != entity))
                    {
                        var ov = hit.LinearVelocity;
                        ov.Z = direction == ConveyorDirecion.Forward ? velocity : -velocity;
                        hit.LinearVelocity = ov;
                        if (fixTheEntity)
                        {
                            var op = hit.CenterPosition;
                            op.X = entity.CenterPosition.X;
                            hit.CenterPosition = op;
                        }
                        hit.OrientationMatrix = Matrix.CreateRotationY(0);

                    }
                }
        }

    }

    class ConvDoor : Conveyor
    {
        private bool open;
        private readonly Vector3 initPos;
        private TimeSpan timeToClose;
        private TimeSpan timeToScane;
        private bool scaning;
        private File file;
        private readonly Camera camera;
        private Model model;
        private readonly Entity origin;
        private TimeSpan timeToRotate;
        private float rotation;

        public ConvDoor(Game game, Box _entity, ConveyorDirecion direction, Space space, float velocity, Camera camera, 
            Entity origin) 
            : base(game, _entity, direction, space, velocity,false)
        {
            initPos = _entity.CenterPosition;
            this.camera = camera;
            this.origin = origin;
        }

        public void Scan(File _file)
        {
            if(scaning) return;
            file = _file;
            scaning = true;
            Disactivate();
        }
       public void LoadContent(ContentManager content)
       {
           model = content.Load<Model>("Models\\Scan_Port");
           EffectMaker.SetObjectEffect(GetType(), model, content);
       }
        public override void Update(GameTime gameTime)
        {
            if (scaning)
            {
                timeToScane += gameTime.ElapsedGameTime;
                if (timeToScane > TimeSpan.FromSeconds(2))
                {
                    timeToScane -= TimeSpan.FromSeconds(2);
                    if (file.PlayerInside)
                    {
                        entity.CenterPosition = new Vector3(0, 0, 100);
                        open = true;
                    }
                    else Activate();
                    scaning = false;
                    file.Scaned = true;
                }
            }
            if (open)
            {
                timeToRotate += gameTime.ElapsedGameTime;
                if(timeToRotate > TimeSpan.FromMilliseconds(1))
                {
                    timeToRotate = TimeSpan.Zero;
                    rotation += 3;
                    rotation = MathHelper.Clamp(rotation, 0, 90);
                }

                timeToClose += gameTime.ElapsedGameTime;
                
                if (timeToClose > TimeSpan.FromSeconds(3))
                {
                    timeToClose = TimeSpan.Zero;
                    open = false;
                    Activate();
                    entity.CenterPosition = initPos;
                }
            }
            else
            {
                if(rotation > 0)
                {
                    timeToRotate += gameTime.ElapsedGameTime;
                    if (timeToRotate > TimeSpan.FromMilliseconds(1))
                    {
                        timeToRotate = TimeSpan.Zero;
                        rotation -= 3;
                        rotation = MathHelper.Clamp(rotation, 0, 90);
                    }
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

                    effect.Parameters["World"].SetValue(Matrix.CreateRotationZ(MathHelper.ToRadians(rotation))*
                                                        origin.WorldTransform);
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