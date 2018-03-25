using System;
using BEPUphysics;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Action_Scene.Objects
{
    class TurnDoor : DrawableGameComponent
    {
        private readonly Box entity;
        private readonly Camera camera;
        private Model model;
        public int ActivationBtnID { get; set; }
        private bool openRequeste;
        private float rotationValue = 360;
        private TimeSpan timer;
        private readonly bool isRight;

        public TurnDoor(Game game, Camera camera, Space space,Vector3 position, Vector3 dimension,bool isRight) 
            : base(game)
        {
            this.camera = camera;
            entity = new Box(position, dimension.X, dimension.Y, dimension.Z);
            space.Add(entity);
            this.isRight = isRight;
            entity.CenterOfMass = new Vector3(
                entity.CenterPosition.X + (isRight ? entity.HalfWidth : -entity.HalfWidth),
                entity.CenterPosition.Y, entity.CenterPosition.Z);
        }

        public void LoadContent(ContentManager content)
        {
            model = content.Load<Model>("Models\\Brick");
            EffectMaker.SetObjectEffect(typeof(Brick), model, content);
            
        }

        public void Open()
        {
            openRequeste = true;
        }

        public override void Update(GameTime gameTime)
        {
            if(openRequeste)
            {
                timer += gameTime.ElapsedGameTime;
                if(timer > TimeSpan.FromMilliseconds(1))
                {
                    timer -= TimeSpan.FromMilliseconds(1);
                    rotationValue += isRight ? .3f : -.3f;
                    if(isRight)
                    {
                        if (rotationValue > 450)
                        {
                            rotationValue = 450;
                            openRequeste = false;
                        }
                    }
                    else
                    {
                        if (rotationValue < 270)
                        {
                            rotationValue = 270;
                            openRequeste = false;
                        }
                    }

                }
            }

            entity.OrientationMatrix = Matrix.CreateRotationZ(MathHelper.ToRadians(rotationValue));
           
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            var tr = Matrix.CreateScale(1, .25f, .8f);
            
            foreach (var mesh in model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    var effect = (Effect)meshPart.Tag;
                    effect.CurrentTechnique = effect.Techniques["Technique1"];

                    effect.Parameters["World"].SetValue(tr*entity.WorldTransform*Matrix.CreateTranslation(0, -.125f, 0));
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
