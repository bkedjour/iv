using System;
using IV.Action_Scene.Enemies;
using IV.Action_Scene.Objects;
using IV.Action_Scene.Weapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Action_Scene
{
    class EffectMaker
    {
        public static Vector3 LightPosition;
        public static void SetLevelEffect(Model model, ContentManager content, int index)
        {
            switch (index)
            {
                case 1:
                    Level1(model, content);
                    break;
                case 2:
                    Level2(model, content);
                    break;
                case 3:
                    Level3(model, content);
                    break;
                case 4:
                    Level4(model, content);
                    break;
                case 5:
                    Level5(model, content);
                    break;
            }
        }

        public static void SetObjectEffect(Type obj, Model model, ContentManager content)
        {
            const string effectPath = "Shaders\\uv_fx";
            if (obj == typeof(Norton))
                SetShorton(model, content, effectPath);
            else if (obj == typeof(Kaspersky))
                SetLaspersky(model, content, effectPath);
            else if (obj == typeof(Gear))
                SetBig_Gear(model, content, effectPath);
            else if (obj == typeof(Brick))
                SetBrick(model, content, effectPath);
            else if (obj == typeof(ActivationButton))
                SetButton(model, content, effectPath);
            else if (obj == typeof(Elevator))
                SetElevator(model, content, effectPath);
           else if (obj == typeof(Cube))
                SetFile_STD(model, content, effectPath);
            else if (obj == typeof(GetWayElevator))
                SetLevel5Elev(model, content, effectPath);
            else if (obj == typeof(GetWayPucher))
                SetLevel5Pusher(model, content, effectPath);
            else if (obj == typeof(BrickBlocker))
                SetGru_Hand(model, content, effectPath);
            else if (obj == typeof(Player))
                SetIV(model, content, effectPath);
            else if (obj == typeof(PlazmaGun))
                SetPlasmaCannon(model, content, effectPath);
            else if (obj == typeof(Pucher))
                SetvirPortPusher(model, content, effectPath);
            else if (obj == typeof(Virus))
                SetVirus(model, content, effectPath);
            else if (obj == typeof(Picker))
                SetRobot_arm(model, content, effectPath);
            else if (obj == typeof(ConvDoor))
                SetScan_Port(model, content, effectPath);
            else if (obj == typeof(SmallGear))
                SetSML_Gear(model, content, effectPath);
            else if (obj == typeof(PipeLineCorner))
                SetTube_Part(model, content, effectPath);
            else if(obj == typeof(RocketAmmo))
                SetRocketAmmo(model, content, effectPath);
        }

        static void Level1(Model model, ContentManager content)
        {
            var effect = content.Load<Effect>("Shaders\\uv_fx");
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures//BIG_Brick_texture");
                            break;
                        case 2:
                            texture = content.Load<Texture2D>("Textures//Server_uv");
                            break;
                        case 3:
                            texture = content.Load<Texture2D>("Textures//metal_texture");
                            break;
                        case 4:
                            texture = content.Load<Texture2D>("Textures//Brick_texture");
                            break;
                        case 5:
                            texture = content.Load<Texture2D>("Textures//green_metal");
                            break;
                        case 6:
                            texture = content.Load<Texture2D>("Textures//Button_Core_UV");
                            break;
                        case 7:
                            texture = content.Load<Texture2D>("Textures//Super_white_metal");
                            break;
                        case 8:
                            texture = content.Load<Texture2D>("Textures//LVL_border");
                            break;
                        case 9:
                            texture = content.Load<Texture2D>("Textures//Back_ground_color");
                            break;
                        case 10:
                            texture = content.Load<Texture2D>("Textures//Diod_uv");
                            break;
                        case 11:
                            texture = content.Load<Texture2D>("Textures//Elect_death");
                            break;
                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }

        static void Level2(Model model, ContentManager content)
        {
            var effect = content.Load<Effect>("Shaders\\uv_fx");
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures//Brick_texture");
                            break;
                        case 2:
                            texture = content.Load<Texture2D>("Textures//BIG_Brick_texture");
                            break;
                        case 3:
                            texture = content.Load<Texture2D>("Textures//Convoyer");
                            break;
                        case 4:
                            texture = content.Load<Texture2D>("Textures//Brick_texture");
                            break;
                        case 5:
                            texture = content.Load<Texture2D>("Textures//cool_metal_texture_by_skipgo");
                            break;
                        case 6:
                            texture = content.Load<Texture2D>("Textures//Button_Core_UV");
                            break;
                        case 7:
                            texture = content.Load<Texture2D>("Textures//Convoyer_wheel");
                            break;
                        case 8:
                            texture = content.Load<Texture2D>("Textures//Super_white_metal");
                            break;
                        case 9:
                            texture = content.Load<Texture2D>("Textures//LVL_border");
                            break;
                        case 10:
                            texture = content.Load<Texture2D>("Textures//Back_ground_color");
                            break;
                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }

        static void Level3(Model model, ContentManager content)
        {
            var effect = content.Load<Effect>("Shaders\\uv_fx");
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures//Brick_texture_2");
                            break;
                        case 2:
                            texture = content.Load<Texture2D>("Textures//Blue_not");
                            break;
                        case 3:
                            texture = content.Load<Texture2D>("Textures//YELLOW");
                            break;
                        case 4:
                            texture = content.Load<Texture2D>("Textures//Brick_texture");
                            break;
                        case 5:
                            texture = content.Load<Texture2D>("Textures//Super_white_metal");
                            break;
                        case 6:
                            texture = content.Load<Texture2D>("Textures//Button_Core_UV");
                            break;
                        case 7:
                            texture = content.Load<Texture2D>("Textures//Multiplexer_UV");
                            break;
                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }

        static void Level4(Model model, ContentManager content)
        {
            var effect = content.Load<Effect>("Shaders\\uv_fx");
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures//Brick_texture_2");
                            break;
                        case 2:
                            texture = content.Load<Texture2D>("Textures//Anti-Machin_UV");
                            break;
                        case 3:
                            texture = content.Load<Texture2D>("Textures//Super_white_metal");
                            break;
                        case 4:
                            texture = content.Load<Texture2D>("Textures//LVL_border");
                            break;
                        case 5:
                            texture = content.Load<Texture2D>("Textures//metal_texture");
                            break;
                        case 6:
                            texture = content.Load<Texture2D>("Textures//Button_Core_UV");
                            break;
                        case 7:
                            texture = content.Load<Texture2D>("Textures//BG_lvl4_UV");
                            break;
                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }
        static void Level5(Model model, ContentManager content)
        {
            var effect = content.Load<Effect>("Shaders\\uv_fx");
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures//Brick_texture_2");
                            break;
                        case 2:
                            texture = content.Load<Texture2D>("Textures//Blue_not");
                            break;
                        case 3:
                            texture = content.Load<Texture2D>("Textures//metal_texture");
                            break;
                        case 4:
                            texture = content.Load<Texture2D>("Textures//Convoyer_wheel");
                            break;
                        case 5:
                            texture = content.Load<Texture2D>("Textures//Convoyer");
                            break;
                        case 6:
                            texture = content.Load<Texture2D>("Textures//Bg_ch2_lvl3");
                            break;
                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }

        static void SetShorton(Model model, ContentManager content, string effectPath)
        {
            var effect = content.Load<Effect>(effectPath);
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures\\Shorton_UV_core");
                            break;
                        case 2:
                            texture = content.Load<Texture2D>("Textures\\Shorton_UV_suport");
                            break;
                        case 3:
                            texture = content.Load<Texture2D>("Textures\\Shorton_UV_Cannon");
                            break;
                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }
        static void SetLaspersky(Model model, ContentManager content, string effectPath)
        {
            var effect = content.Load<Effect>(effectPath);
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures\\UV_laspersky");
                            break;

                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }
        public static void SetBalanceD(Model model, ContentManager content, string effectPath)
        {
            var effect = content.Load<Effect>(effectPath);
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures\\Super_white_metal");
                            break;
                        case 2:
                            texture = content.Load<Texture2D>("Textures\\metal_texture");
                            break;

                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }
        public static void SetBalanceG(Model model, ContentManager content, string effectPath)
        {
            var effect = content.Load<Effect>(effectPath);
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures\\Super_white_metal");
                            break;
                        case 2:
                            texture = content.Load<Texture2D>("Textures\\metal_texture");
                            break;

                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }
        static void SetBig_Gear(Model model, ContentManager content, string effectPath)
        {
            var effect = content.Load<Effect>(effectPath);
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures\\Big_Gear_UV");
                            break;

                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }

        public static void SeSmailtBrick(Model model, ContentManager content, string effectPath)
        {
            var effect = content.Load<Effect>(effectPath);
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures\\plasiX");
                            break;

                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }

        static void SetBrick(Model model, ContentManager content, string effectPath)
        {
            var effect = content.Load<Effect>(effectPath);
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures\\Brick_texture_2");
                            break;

                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }
        static void SetButton(Model model, ContentManager content, string effectPath)
        {
            var effect = content.Load<Effect>(effectPath);
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures\\File_RED");
                            break;
                        case 2:
                            texture = content.Load<Texture2D>("Textures\\white_metal");
                            break;
                        case 3:
                            texture = content.Load<Texture2D>("Textures\\metal_texture");
                            break;
                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }
        static void SetElevator(Model model, ContentManager content, string effectPath)
        {
            var effect = content.Load<Effect>(effectPath);
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures\\Brick_texture");
                            break;
                        case 2:
                            texture = content.Load<Texture2D>("Textures\\Super_white_metal");
                            break;

                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }
        public static void SetFile(Model model, ContentManager content, string effectPath,FileType type)
        {
            var effect = content.Load<Effect>(effectPath);
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures\\File_WHITE");
                            break;
                        case 2:
                            {
                                switch (type)
                                {
                                    case FileType.System:
                                        texture = content.Load<Texture2D>("Textures\\File_GREEN");
                                        break;
                                    case FileType.User:
                                        texture = content.Load<Texture2D>("Textures\\File_BLUE");
                                        break;
                                    case FileType.Unkown:
                                        texture = content.Load<Texture2D>("Textures\\File_RED");
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException("type");
                                }
                            }
                            break;

                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }
        static void SetFile_STD(Model model, ContentManager content, string effectPath)
        {
            var effect = content.Load<Effect>(effectPath);
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures\\File_WHITE");
                            break;
                        case 2:
                            texture = content.Load<Texture2D>("Textures\\metal_texture");
                            break;

                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }
        static void SetLevel5Elev(Model model, ContentManager content, string effectPath)
        {
            var effect = content.Load<Effect>(effectPath);
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures\\metal_texture");
                            break;
                        case 2:
                            texture = content.Load<Texture2D>("Textures\\Super_white_metal");
                            break;

                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }
        static void SetLevel5Pusher(Model model, ContentManager content, string effectPath)
        {
            var effect = content.Load<Effect>(effectPath);
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures\\Super_white_metal");
                            break;

                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }
        static void SetGru_Hand(Model model, ContentManager content, string effectPath)
        {
            var effect = content.Load<Effect>(effectPath);
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures\\Brick_texture_2_gru");
                            break;

                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }
        static void SetIV(Model model, ContentManager content, string effectPath)
        {
            var effect = content.Load<Effect>(effectPath);
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures\\CORE_UV");
                            break;
                        case 2:
                            texture = content.Load<Texture2D>("Textures\\Mouth");
                            break;
                        case 3:
                            texture = content.Load<Texture2D>("Textures\\metal_texture");
                            break;
                        case 4:
                            texture = content.Load<Texture2D>("Textures\\white_metal");
                            break;
                        case 5:
                            texture = content.Load<Texture2D>("Textures\\green_metal");
                            break;
                        case 6:
                            texture = content.Load<Texture2D>("Textures\\plasiX");
                            break;
                        case 7:
                            texture = content.Load<Texture2D>("Textures\\Wheel_G_r");
                            break;

                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }
        public static void SetLaserWall(Model model, ContentManager content, string effectPath)
        {
            var effect = content.Load<Effect>(effectPath);
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures\\Laser_Wall_uv");
                            break;

                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }
        public static void SetLaserWallDestro(Model model, ContentManager content, string effectPath)
        {
            var effect = content.Load<Effect>(effectPath);
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures\\Laser_Wall_dest_uv");
                            break;

                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }
        public static void SetMachinGun(Model model, ContentManager content, string effectPath)
        {
            var effect = content.Load<Effect>(effectPath);
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures\\MG_core");
                            break;
                        case 2:
                            texture = content.Load<Texture2D>("Textures\\MG_canon");
                            break;

                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }
        public static void SetMG_Support(Model model, ContentManager content, string effectPath)
        {
            var effect = content.Load<Effect>(effectPath);
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures\\metal_texture");
                            break;

                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }
        static void SetPlasmaCannon(Model model, ContentManager content, string effectPath)
        {
            var effect = content.Load<Effect>(effectPath);
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures\\Plasma_cannon_core");
                            break;
                        case 2:
                            texture = content.Load<Texture2D>("Textures\\plazma");
                            break;
                        case 3:
                            texture = content.Load<Texture2D>("Textures\\Super_white_metal");
                            break;

                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }

        public static void SetPort_lvl1(Model model, ContentManager content, string effectPath)
        {
            var effect = content.Load<Effect>(effectPath);
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures\\PORT_lvl1");
                            break;
                        case 2:
                            texture = content.Load<Texture2D>("Textures\\green_metal");
                            break;

                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }
        static void SetRobot_arm(Model model, ContentManager content, string effectPath)
        {
            var effect = content.Load<Effect>(effectPath);
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures\\BLACK");
                            break;
                        case 2:
                            texture = content.Load<Texture2D>("Textures\\YELLOW");
                            break;
                        case 3:
                            texture = content.Load<Texture2D>("Textures\\BOLT_ARM");
                            break;
                        case 4:
                            texture = content.Load<Texture2D>("Textures\\HAND_arm");
                            break;

                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }
        static void SetScan_Port(Model model, ContentManager content, string effectPath)
        {
            var effect = content.Load<Effect>(effectPath);
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures\\Brick_texture");
                            break;
                        case 2:
                            texture = content.Load<Texture2D>("Textures\\Convoyer_wheel");
                            break;
                        case 3:
                            texture = content.Load<Texture2D>("Textures\\Convoyer");
                            break;

                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }
        static void SetSML_Gear(Model model, ContentManager content, string effectPath)
        {
            var effect = content.Load<Effect>(effectPath);
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures\\SML_Gear_UV");
                            break;

                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }
        static void SetTube_Part(Model model, ContentManager content, string effectPath)
        {
            var effect = content.Load<Effect>(effectPath);
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures\\Super_white_metal");
                            break;
                        case 2:
                            texture = content.Load<Texture2D>("Textures\\Brick_texture");
                            break;

                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }
        static void SetvirPortPusher(Model model, ContentManager content, string effectPath)
        {
            var effect = content.Load<Effect>(effectPath);
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures\\Brick_texture");
                            break;
                        case 2:
                            texture = content.Load<Texture2D>("Textures\\Super_white_metal");
                            break;

                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }
        static void SetVirus(Model model, ContentManager content, string effectPath)
        {
            var effect = content.Load<Effect>(effectPath);
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures\\VIRUS_uv");
                            break;

                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }
        static void SetRocketAmmo(Model model, ContentManager content, string effectPath)
        {
            var effect = content.Load<Effect>(effectPath);
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures\\ROCKET_BOX_UV");
                            break;
                        case 2:
                            texture = content.Load<Texture2D>("Textures\\Rockette_uv");
                            break;

                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }

        public static void SetMenuEffect(Model model, ContentManager content)
        {
            var effect = content.Load<Effect>("Shaders\\menu_fx");
            Texture2D texture = null;
            foreach (var mesh in model.Meshes)
            {
                var id = 0;
                foreach (var meshPart in mesh.MeshParts)
                {
                    id++;
                    switch (id)
                    {
                        case 1:
                            texture = content.Load<Texture2D>("Textures\\metal_texture");
                            break;
                        case 2:
                            texture = content.Load<Texture2D>("Textures\\CORE_UV");
                            break;
                        case 3:
                            texture = content.Load<Texture2D>("Textures\\Mouth");
                            break;
                        case 4:
                            texture = content.Load<Texture2D>("Textures\\metal_texture");
                            break;
                        case 5:
                            texture = content.Load<Texture2D>("Textures\\white_metal");
                            break;
                        case 6:
                            texture = content.Load<Texture2D>("Textures\\green_metal");
                            break;
                        case 7:
                            texture = content.Load<Texture2D>("Textures\\plasiX");
                            break;
                        case 8:
                            texture = content.Load<Texture2D>("Textures\\Wheel_G_r");
                            break;
                        case 9:
                            texture = content.Load<Texture2D>("Textures\\UV_laspersky");
                            break;
                        case 10:
                            texture = content.Load<Texture2D>("Textures\\Shorton_UV_core");
                            break;
                        case 11:
                            texture = content.Load<Texture2D>("Textures\\Shorton_UV_suport");
                            break;
                        case 12:
                            texture = content.Load<Texture2D>("Textures\\Shorton_UV_Cannon");
                            break;
                        case 13:
                            texture = content.Load<Texture2D>("Textures\\green_metal");
                            break;
                        case 14:
                            texture = content.Load<Texture2D>("Textures\\LVL_border");
                            break;
                        case 15://compagne
                            texture = content.Load<Texture2D>("Textures\\MENU\\no_selected");
                            break;
                        case 16://Extras
                            texture = content.Load<Texture2D>("Textures\\MENU\\no_selected");
                            break;
                        case 17://Options
                            texture = content.Load<Texture2D>("Textures\\MENU\\no_selected");
                            break;
                        case 18://quitter
                            texture = content.Load<Texture2D>("Textures\\MENU\\no_selected");
                            break;
                        case 19://continuer
                            texture = content.Load<Texture2D>("Textures\\MENU\\no_selected");
                            break;
                        case 20://nouvelle partie
                            texture = content.Load<Texture2D>("Textures\\MENU\\no_selected");
                            break;
                        case 21://selection chapitre
                            texture = content.Load<Texture2D>("Textures\\MENU\\no_selected");
                            break;
                        case 22://commands
                            texture = content.Load<Texture2D>("Textures\\MENU\\no_selected");
                            break;
                        case 23://video
                            texture = content.Load<Texture2D>("Textures\\MENU\\no_selected");
                            break;
                        case 24://audio
                            texture = content.Load<Texture2D>("Textures\\MENU\\no_selected");
                            break;
                        case 25://generique
                            texture = content.Load<Texture2D>("Textures\\MENU\\no_selected");
                            break;


                    }
                    meshPart.Tag = effect.Clone();
                    if (texture == null) throw new NullReferenceException("Ya James le Tex raho NULL");
                    ((Effect)meshPart.Tag).Parameters["tex"].SetValue(texture);
                }
            }
        }
    }
}
