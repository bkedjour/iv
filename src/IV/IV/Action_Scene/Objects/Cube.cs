using BEPUphysics;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Action_Scene.Objects
{
    public class Cube : DrawableGameComponent
    {
        protected readonly Box entity;
        protected  Model model;
        public Box Entity{get{ return entity;}}

        protected readonly Matrix transform;
        protected readonly Camera camera;

        protected float initZ;

        public bool Fixed { get; set; }


        public Cube(Game game, Space space, Camera camera, Box entity) 
            : base(game)
        {
            this.entity = entity;
            this.camera = camera;
            transform = Matrix.CreateScale(entity.Width, entity.Height, entity.Length);
            space.Add(entity);
            initZ = entity.CenterPosition.Z;
            entity.Tag = this;
            Fixed = true;
            
        }
       public void SetInitPosition(float z)
       {
           initZ = z;
       }
        public virtual void LoadContent(ContentManager Content)
        {
            model = Content.Load<Model>("Models\\File_STD");
            EffectMaker.SetObjectEffect(GetType(),model,Content);
        }

        public override void Update(GameTime gameTime)
        {
            if (entity.Tag is Cube && ((Cube)entity.Tag).Fixed)
                entity.CenterPosition = new Vector3(entity.CenterPosition.X, entity.CenterPosition.Y, initZ);

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

                    effect.Parameters["World"].SetValue(entity.WorldTransform);
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