using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Action_Scene
{
    public class EntityModel : DrawableGameComponent
    {
        readonly Entity entity;
        readonly Model model;

        public Matrix Transform;
        private readonly Camera camera;
        private readonly Texture2D texture;


        public EntityModel(Entity entity, Model model, Matrix transform, Game game, Camera camera, Texture2D[] textures, int rnd)
            : base(game)
        {
            this.entity = entity;
            this.model = model;
            Transform = transform;
            this.camera = camera;
            texture = textures[rnd];
        }
        public EntityModel(Game game, Entity entity, Model model, Matrix transform, Camera camera, Texture2D texture)
            : base(game)
        {
            this.entity = entity;
            this.model = model;
            Transform = transform;
            this.camera = camera;
            this.texture = texture;
            Game.Components.Add(this);
        }
        public void Delete()
        {
            Game.Components.Remove(this);
        }
        public override void Draw(GameTime gameTime)
        {
            var worldMatrix = Transform * entity.WorldTransform;
            if (entity.Tag is float && ((float)entity.Tag != 0))
                entity.OrientationQuaternion *= Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationZ(
                    MathHelper.ToRadians((float) entity.Tag)));
            if (entity.Tag is float) entity.Tag = 0.0f;

            foreach (var mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.TextureEnabled = true;
                    effect.Texture = texture;

                    effect.World = worldMatrix;
                    effect.View = camera.ViewMatrix;
                    effect.Projection = camera.ProjectionMatrix;
                }
                mesh.Draw();
            }
            base.Draw(gameTime);
        }
    }
}