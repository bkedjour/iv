using System;
using BEPUphysics;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace IV.Action_Scene.Objects
{
    public delegate void SliderDelegateIn(float velocity);
   
    class Slider : DrawableGameComponent
    {
        protected Model model;
        protected readonly Box entity;
        protected readonly Entity player;
        protected Vector3 velocity;
        protected bool activated = true;
        protected readonly Camera camera;
        public event SliderDelegateIn OnPlayerIsOn;

        protected TimeSpan timer;
        protected bool cameraInteraction;
        private KeyboardState oldState;


        public Slider(Game game, Space space, Vector3 position,Entity player, Vector3 velocity, Camera camera) 
            : base(game)
        {
            entity = new Box(position, 14.54198f, 2.282995f, 8.683978f);
            entity.EventManager.InitialCollisionDetected += TurnerCollision;
            space.Add(entity);
            this.player = player;
            this.camera = camera;
            this.velocity = velocity;
        }

        protected virtual void TurnerCollision(Entity sender, Entity other, CollisionPair collisionpair)
        {
            var turner = other.Tag as SliderTurner;
            if (turner == null) return;
            SetVelocity(turner.Velocity);
        }

        public virtual void LoadContent(ContentManager content)
        {
            model = content.Load<Model>("Models\\Brick");
            EffectMaker.SetObjectEffect(typeof(Brick),model,content);
        }

        void SetVelocity(Vector3 velo)
        {
            velocity = velo;
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState current = Keyboard.GetState();
            if (current.IsKeyDown(Keys.G) && oldState.IsKeyUp(Keys.G))
                activated = true;
            oldState = current;
            if (!activated)
            {
                base.Update(gameTime);
                return;
            }
            entity.LinearVelocity = velocity;
            bool found = false;
            for (var i = entity.CenterPosition.X - (entity.Width/2.0f);
                 i <= entity.CenterPosition.X + (entity.Width/2.0f);
                 i += 3)
            {
                Vector3 hit, normal;
                float t;
                if (player.RayTest(new Vector3(i, entity.CenterPosition.Y, entity.CenterPosition.Z), Vector3.Up, 6,
                                   false,
                                   out hit, out normal, out t))
                {
                    OnPlayerIsOn(velocity.X);
                    found = true;
                    camera.ZoomOut(80);
                    cameraInteraction = true;
                    break;
                }

            }
            if (cameraInteraction)
            {
                if (!found)
                {
                    timer += gameTime.ElapsedGameTime;
                    if (timer > TimeSpan.FromSeconds(2))
                    {
                        timer = TimeSpan.Zero;
                        camera.ZoomIn();
                        cameraInteraction = false;
                    }
                }
                else timer = TimeSpan.Zero;
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

                    effect.Parameters["World"].SetValue(Matrix.CreateScale(new Vector3(1, .5f, 1))*entity.WorldTransform);
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

    class AutoSlider : Slider
    {
        private bool isPlayerOn;
        private readonly Vector3 initVelo;
        private bool arrived;
        private TimeSpan timeToGoDown;
        private readonly Vector3 initPosition;
        private TimeSpan timeToStart;

        public AutoSlider(Game game, Space space, Vector3 position, Entity player, Vector3 velocity, Camera camera) 
            : base(game, space, position, player, velocity, camera)
        {
            OnPlayerIsOn += PlayerIsOn;
            initVelo = velocity;
            this.velocity = Vector3.Zero;
            initPosition = position;
        }

        protected override void TurnerCollision(Entity sender, Entity other, CollisionPair collisionpair)
        {
            if (other.Tag is SliderTurner)
                arrived = true;

            base.TurnerCollision(sender, other, collisionpair);
        }

        private void PlayerIsOn(float f)
        {
            isPlayerOn = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (isPlayerOn)
            {
                timeToStart += gameTime.ElapsedGameTime;
                if (timeToStart > TimeSpan.FromSeconds(1))
                {
                    velocity = arrived ? Vector3.Zero : initVelo;
                    isPlayerOn = false;
                    timeToGoDown = TimeSpan.Zero;
                }
            }
            else if (!IsDistanceZero(entity.CenterPosition, initPosition, .5f))
            {
                timeToGoDown += gameTime.ElapsedGameTime;
                if (timeToGoDown > TimeSpan.FromSeconds(3))
                {
                    velocity = -initVelo;
                    arrived = false;
                }
                else velocity = Vector3.Zero;
            }
            else
            {
                velocity = Vector3.Zero;
                timeToStart = TimeSpan.Zero;
            }
        }

       public static bool IsDistanceZero(Vector3 a,Vector3 b, float value)
        {
            var c = a - b;
            c = new Vector3(Math.Abs(c.X), Math.Abs(c.Y), Math.Abs(c.Z));
            return c.X <= value && c.Y <= value && c.Z <= value;
        }
    }

    class SliderTurner : GameComponent
    {
        private readonly Box entity;
        public Vector3 Velocity { get; private set; }
        public SliderTurner(Game game,Space space,Vector3 position,Vector3 dimension,Vector3 velocity) 
            : base(game)
        {
            entity = new Box(position, dimension.X,dimension.Y,dimension.Z) {Tag = this};
            foreach (var en in space.Entities)
                entity.CollisionRules.SpecificEntities.Add(en, CollisionRule.NoResponse);
            space.Add(entity);
            Velocity = velocity;
        }
    }



    //Level 1 Auto Slider

    class SmailSlider : AutoSlider
    {
        public SmailSlider(Game game, Space space, Vector3 position, Entity player, Vector3 velocity, Camera camera) 
            : base(game, space, position, player, velocity, camera)
        {

            entity.Width = 7.27099f;
            entity.Height = 1.1414975f;
        }

        public override void LoadContent(ContentManager content)
        {
            model = content.Load<Model>("Models\\Brick");
            EffectMaker.SeSmailtBrick(model, content, "Shaders\\uv_fx");
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (var mesh in model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    var effect = (Effect)meshPart.Tag;
                    effect.CurrentTechnique = effect.Techniques["Technique1"];

                    effect.Parameters["World"].SetValue(Matrix.CreateScale(new Vector3(.5f, .25f, 1))*
                                                        entity.WorldTransform);
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["LightPosition"].SetValue(EffectMaker.LightPosition);
                    effect.Parameters["CameraPosition"].SetValue(camera.Position);

                    meshPart.Effect = effect;

                }
                mesh.Draw();
            }

            //base.Draw(gameTime);
        }
    }


}
