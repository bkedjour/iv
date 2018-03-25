using System;
using System.Collections.Generic;
using BEPUphysics;
using BEPUphysics.Entities;
using IV.Achievement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using SkinnedModel;

namespace IV.Action_Scene.Objects
{
    public class Picker : DrawableGameComponent
    {
        private readonly Camera camera;
        private readonly Space space;
        private readonly Box picker;
        private Vector3 origin;
        private AnimationPlayer animationPlayer;
        private SkinningData skinningData;
        private Model animatedModel;
        private TimeSpan timeToThrowFile;
        private TimeSpan timeToThrowIV;
        private TimeSpan timeToPickUp;
        private bool isTimeToPick;
        private TimeSpan timeToRotateCube;
        private float rotation;
        private bool active;
        private TimeSpan timeToMove;
        private bool working;

        private Entity pickedEntity;

        public Picker(Game game,Space space,Box picker,Vector3 centerOfMass, Camera camera) 
            : base(game)
        {
            this.camera = camera;
            this.space = space;
            this.picker = picker;

            foreach (var entity in space.Entities)
                this.picker.CollisionRules.SpecificEntities.Add(entity, CollisionRule.NoPair);

            space.Add(picker);
            origin = centerOfMass;
        }

        public void LoadContent(ContentManager Content)
        {
            animatedModel = Content.Load<Model>("Models\\Robot_arm");

            skinningData = animatedModel.Tag as SkinningData;

            if (skinningData == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");
            animationPlayer = new AnimationPlayer(skinningData);
            animationPlayer.StartClip(skinningData.AnimationClips["Anim-1"]);
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
            animationPlayer.Update(gameTime.ElapsedGameTime, false, Matrix.Identity);

            if (active)
                Work(gameTime);

            base.Update(gameTime);
        }

        void Work(GameTime gameTime)
        {
            if(working)
            {
                if (pickedEntity != null && !isTimeToPick)
                {
                    var handIndex = skinningData.BoneIndices["Bone05"];
                    Matrix[] worldTransforms = animationPlayer.GetWorldTransforms();
                    pickedEntity.IsAffectedByGravity = false;
                    pickedEntity.CenterPosition =
                        new Vector3(worldTransforms[handIndex].Translation.X + origin.X,
                                    worldTransforms[handIndex].Translation.Y + origin.Y + ((pickedEntity.Tag is Player)?-1.5f:0),
                                    worldTransforms[handIndex].Translation.Z + origin.Z);

                    timeToRotateCube += gameTime.ElapsedGameTime;
                    if (timeToRotateCube > TimeSpan.FromMilliseconds(900))
                    {
                        rotation += 2.5f;
                        rotation = MathHelper.Clamp(rotation, 0, 180);
                        pickedEntity.OrientationMatrix = Matrix.CreateRotationY(MathHelper.ToRadians(rotation));
                    }
                    if (pickedEntity.Tag is Cube)
                    {
                        timeToThrowFile += gameTime.ElapsedGameTime;
                        if (timeToThrowFile > TimeSpan.FromMilliseconds(2500))
                        {
                            timeToThrowFile = TimeSpan.Zero;
                            timeToRotateCube = TimeSpan.Zero;

                            pickedEntity.IsAffectedByGravity = true;
                            ((Cube) pickedEntity.Tag).Fixed = true;
                            rotation = 0;
                            pickedEntity = null;
                        }
                    }else if(pickedEntity.Tag is Player)
                    {
                        timeToThrowIV += gameTime.ElapsedGameTime;
                        if (timeToThrowIV > TimeSpan.FromMilliseconds(900))
                        {
                            timeToThrowIV = TimeSpan.Zero;
                            timeToRotateCube = TimeSpan.Zero;

                            pickedEntity.LinearVelocity = new Vector3(-400, pickedEntity.LinearVelocity.Y, 0);
                            pickedEntity.IsAffectedByGravity = true;
                            rotation = 0;
                            ((Player) pickedEntity.Tag).Active = true;
                            pickedEntity = null;

                            EventAggregator.Instance.Publish(new OnRejected());
                        }
                    }
                }
                timeToMove += gameTime.ElapsedGameTime;
                if (timeToMove > animationPlayer.CurrentClip.Duration - TimeSpan.FromMilliseconds(1400))
                {
                    timeToMove = TimeSpan.Zero;
                    working = false;
                }
            }
            else
            {
                if (isTimeToPick)
                {
                    timeToPickUp += gameTime.ElapsedGameTime;
                    if(timeToPickUp > TimeSpan.FromMilliseconds((pickedEntity.Tag is File? 1600:850)))
                    {
                        timeToPickUp = TimeSpan.Zero;
                        isTimeToPick = false;
                        working = true;
                    }
                }
                else
                    PickUpObject();
            }
        }

        void PickUpObject()
        {
            var hitEntitie = new List<Entity>();

            space.RayCast(picker.CenterPosition, Vector3.Left, .5f, false, hitEntitie,
                          new List<Vector3>(),
                          new List<Vector3>(), new List<float>());

            foreach (var entity in hitEntitie.Where(entity => entity != picker))
            {
                if (entity.Tag is File)
                {
                    ((File) entity.Tag).SetInitPosition(entity.CenterPosition.Z);
                    ((File) entity.Tag).Fixed = false;
                    ((File) entity.Tag).CanPlayerGetInside = false;
                    pickedEntity = entity;
                    animationPlayer = new AnimationPlayer(skinningData);
                    animationPlayer.StartClip(skinningData.AnimationClips["Anim-1"]);
                    isTimeToPick = true;
                    break;
                }
                if (entity.Tag is Player && !((Player)entity.Tag).IsInAFile)
                {
                    ((Player) entity.Tag).Active = false;
                    pickedEntity = entity;
                    animationPlayer = new AnimationPlayer(skinningData);
                    animationPlayer.StartClip(skinningData.AnimationClips["Anim-2"]);
                    isTimeToPick = true;
                    break;
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            Matrix[] bones = animationPlayer.GetSkinTransforms();

            foreach (ModelMesh mesh in animatedModel.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    effect.SetBoneTransforms(bones);


                    effect.World = Matrix.CreateTranslation(origin);

                    effect.View = camera.ViewMatrix;
                    effect.Projection = camera.ProjectionMatrix;

                    effect.EnableDefaultLighting();

                    effect.SpecularColor = new Vector3(0.25f);
                    effect.SpecularPower = 16;
                }

                mesh.Draw();
            }
           
            base.Draw(gameTime);
        }
    }
}