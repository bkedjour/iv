using System;
using System.Collections.Generic;
using BEPUphysics;
using BEPUphysics.Entities;
using IV.Action_Scene.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Action_Scene.Objects
{
    public enum FileType{System, User, Unkown}
    
    public class File : Cube
    {
        public FileType Type{ get; private set;}
        public bool Scaned { get; set; }
        public bool PlayerInside { get; set; }
        public bool CanPlayerGetInside { get; set; }
        public bool IsPlayerInRight { get; set; }

        private Quad quad;
        private BasicEffect basicEffect;
        private QuadAnimation beginAnim, loopAnim, playerAnim_R, playerAnim_L;
        private QuadAnimationPlayer animationPlayer;
        private bool playingBegin, playingPlayer;
        private TimeSpan timeToTransform;

        public File(Game game, Space space, Camera camera, Box entity, FileType type) 
            : base(game, space, camera, entity)
        {
            Type = type;
            entity.Tag = this;
            CanPlayerGetInside = true;
        }
       
        public override void LoadContent(ContentManager Content)
        {
            switch (Type)
            {
                case FileType.System:
                    model = Content.Load<Model>("Models\\FileG");
                    break;
                case FileType.User:
                    model = Content.Load<Model>("Models\\FileB");
                    break;
                case FileType.Unkown:
                    model = Content.Load<Model>("Models\\FileR");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            EffectMaker.SetFile(model, Content, "Shaders\\uv_fx", Type);

            basicEffect = new BasicEffect(GraphicsDevice);

            var startTextures = new List<Texture2D>();
            for(int i = 0; i < 17; i++)
                startTextures.Add(Content.Load<Texture2D>("Effects\\File\\Anim\\Start\\" + i));
            beginAnim = new QuadAnimation(startTextures,.05f,false);

            var loopTextures = new List<Texture2D>();
            for (int i = 0; i < 13; i++)
                loopTextures.Add(Content.Load<Texture2D>("Effects\\File\\Anim\\Loop\\" + i));
            loopAnim = new QuadAnimation(loopTextures, .05f, true);

            var playerTextures = new List<Texture2D>();
            for (int i = 0; i < 15; i++)
                playerTextures.Add(Content.Load<Texture2D>("Effects\\File\\Player\\" + i+"_R"));
            playerAnim_R = new QuadAnimation(playerTextures, .05f, false);

            var playerTextures_L = new List<Texture2D>();
            for (int i = 0; i < 15; i++)
                playerTextures_L.Add(Content.Load<Texture2D>("Effects\\File\\Player\\" + i + "_L"));
            playerAnim_L = new QuadAnimation(playerTextures_L, .05f, false);

            animationPlayer = new QuadAnimationPlayer();
            playingPlayer = true;
        }

        public override void Update(GameTime gameTime)
        {
            if(!PlayerInside) return;
            if (playingPlayer && animationPlayer.Animation == null)
                animationPlayer.PlayAnimation(IsPlayerInRight ? playerAnim_R : playerAnim_L);

            timeToTransform += gameTime.ElapsedGameTime;

            basicEffect.View = camera.ViewMatrix;
            basicEffect.Projection = camera.ProjectionMatrix;

            if(playingPlayer && animationPlayer.FrameIndex == animationPlayer.Animation.FrameCount - 1)
            {
                playingPlayer = false;
                playingBegin = true;
                animationPlayer.PlayAnimation(beginAnim);
            }else if (playingBegin && animationPlayer.FrameIndex == animationPlayer.Animation.FrameCount - 1)
            {
                playingBegin = false;
                animationPlayer.PlayAnimation(loopAnim);
            }

            quad = new Quad(entity.CenterPosition, Vector3.Backward, Vector3.Up, 6, 6);
            
            if(timeToTransform > TimeSpan.FromSeconds(1))
                animationPlayer.Update(gameTime);
            
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (var mesh in model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    var effect = (Effect) meshPart.Tag;
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

            if (PlayerInside)
            {
                DrawQuad(basicEffect, quad, Vector3.Zero, animationPlayer.Texture);
            }
        }

        void DrawQuad(BasicEffect effect, Quad _quad, Vector3 position, Texture2D texture)
        {
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            effect.EnableDefaultLighting();
            effect.PreferPerPixelLighting = true;
            effect.TextureEnabled = true;

            effect.Texture = texture;
            effect.World = Matrix.CreateTranslation(position);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    _quad.Vertices, 0, 4,
                    _quad.Indexes, 0, 2);
            }

            GraphicsDevice.BlendState = BlendState.Opaque;
        }
    }
}