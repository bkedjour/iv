using System;
using System.Threading;
using IV.Action_Scene;
using IV.Menu_Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace IV.Scenes
{
    enum SceneType{Main,Compagne,Option,Exit}
    public class MenuScene : GameScene
    {
        private SpriteBatch sBatch;

        Matrix view;
        Matrix proj;
        double time;
        private float timeToTurn;
        private bool activeTurn;
        private bool isExiting;
        private static float aspectRatio;
        const float FOV = MathHelper.PiOver4;
        const float nearClip = 5.0f;
        const float farClip = 1500.0f;
        private Model model;
        private Curve3D cameraCurvePosition;
        private Curve3D cameraCurveLookat;
        private Vector3 cameraPosition;
        private Vector3 cameraLookat;

        private Texture2D denied;
        private Texture2D selected;
        private Texture2D notSelected;
        private int selectedIndex;
        private SceneType sceneType;

        private KeyboardState oldState;

        private TwoDManager twoDManager;
        private Texture2D loadingScreen;
        Thread loadingContentThread;

        private readonly SoundManager soundManager;
        private Song backMusic;
        private bool isShowing;
        public event EventHandler<MenuEventArgs> OnGameStart;
        public event EventHandler OnCreditClicked;
        // attinuation DJM mod
        private float attpower = 2.1f;
        private bool attpawerbool = true;
        //-----------
        //djmpost
        private RenderTarget2D PP_guisanteBlure;
        private Texture2D original_menu_output;
        private Effect pp_blure;
        //----
        public MenuScene(Game game) 
            : base(game)
        {
            sceneType = SceneType.Main;
            cameraCurvePosition = new Curve3D(CurveLoopType.Linear);
            cameraCurveLookat = new Curve3D(CurveLoopType.Linear);

            soundManager = (SoundManager) Game.Services.GetService(typeof (SoundManager));
        }

        void IncreaseIndex()
        {
            soundManager.PlaySound("chose_button");
            selectedIndex++;
            switch (sceneType)
            {
                case SceneType.Main:
                    if (selectedIndex > 18)
                        selectedIndex = 15;
                    break;
                case SceneType.Compagne:
                    if (selectedIndex > 21)
                        selectedIndex = 19;
                    break;
                case SceneType.Option:
                    if (selectedIndex > 25)
                        selectedIndex = 22;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        void DecreaseIndex()
        {
            soundManager.PlaySound("chose_button");
            selectedIndex--;
            switch (sceneType)
            {
                case SceneType.Main:
                    if (selectedIndex < 15)
                        selectedIndex = 18;
                    break;
                case SceneType.Compagne:
                    if (selectedIndex < 19)
                        selectedIndex = 21;
                    break;
                case SceneType.Option:
                    if (selectedIndex < 22)
                        selectedIndex = 25;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void ResetCurves(CurveLoopType type)
        {
            cameraCurvePosition = new Curve3D(type);
            cameraCurveLookat = new Curve3D(type);
            time = 0;
        }

        void SelectScene()
        {
            soundManager.PlaySound("selct_button");
            if (selectedIndex == 15)
            {
                sceneType = SceneType.Compagne;
                selectedIndex = 19;
                ResetCurves(CurveLoopType.Linear);
                var _time = 0;
                AddPoint(new Vector3(-253.5485f, 1.462287f, 92.37619f), new Vector3(-145.6624f, 56.93191f, -32.89682f), _time);
                _time += 1000;
                AddPoint(new Vector3(-130.7379f, 144.2281f, 289.9554f), Vector3.Zero, _time);
                _time += 1000;
                AddPoint(new Vector3(24.88234f, 57.17424f, 273.5295f), new Vector3(105f, -60f, 0f), _time);
                timeToTurn = 2000;
                activeTurn = true;
            }else if (selectedIndex == 16)
            {
                twoDManager.SetType(TwoDSceneType.Extras);                
            }
            else if (selectedIndex == 17)
            {
                sceneType = SceneType.Option;
                selectedIndex = 22;
                ResetCurves(CurveLoopType.Linear);
                var _time = 0;
                AddPoint(new Vector3(-253.5485f, 1.462287f, 92.37619f), new Vector3(-145.6624f, 56.93191f, -32.89682f), _time);
                _time += 1000;
                AddPoint(new Vector3(-339.1484f, 225.2347f, -234.1863f), Vector3.Zero, _time);
                _time += 1000;
                AddPoint(new Vector3(-94.55411f, 13.07233f, -287.2042f), new Vector3(-260f, 40f, 0f), _time);
                timeToTurn = 2000;
                activeTurn = true;
            }
            else if (selectedIndex == 18)
            {
                ResetCurves(CurveLoopType.Linear);
                var _time = 0;
                AddPoint(new Vector3(-253.5485f, 1.462287f, 92.37619f), new Vector3(-145.6624f, 56.93191f, -32.89682f),
                          _time);
                _time += 800;
                AddPoint(new Vector3(1.827225f, 394.9366f, 0.6908872f), new Vector3(-20, 56.93191f, -30), _time);
                _time += 800;
                AddPoint(new Vector3(0.1802747f, 820.5233f, 7.19148f), new Vector3(-20, -500, 30), _time);
                timeToTurn = 1600;
                isExiting = true;
            }
            else if (selectedIndex == 22)
                twoDManager.SetType(TwoDSceneType.Command);
            else if (selectedIndex == 23)
                twoDManager.SetType(TwoDSceneType.Video);
            else if (selectedIndex == 24)
                twoDManager.SetType(TwoDSceneType.Audio);
            else if (selectedIndex == 21)
                twoDManager.SetType(TwoDSceneType.Chapter);
            else if (selectedIndex == 20)
            {
                if (OnGameStart != null)
                {
                    OnGameStart(this, new MenuEventArgs { IsNewGame = true, LevelIndex = 0 });
                    GameSettings.LevelIndex = 0;
                }
            }
            else if (selectedIndex == 19 && GameSettings.LevelIndex > 0)
            {
                if (OnGameStart != null)
                    OnGameStart(this, new MenuEventArgs { IsNewGame = false, LevelIndex = GameSettings.LevelIndex });
            }
            else if (selectedIndex == 25)
                if (OnCreditClicked != null)
                    OnCreditClicked(this, EventArgs.Empty);
        }

        void Turning()
        {
            activeTurn = false;
            if(sceneType == SceneType.Main)
            {
                ResetCurves(CurveLoopType.Oscillate);
                AddPoint(new Vector3(-253.5485f, 1.462287f, 92.37619f), new Vector3(-140.6624f, 50.93191f, -32.89682f),
                     0);
                AddPoint(new Vector3(-253.5485f, 1.462287f, 92.37619f), new Vector3(-135.6624f, 55.93191f, -30.89682f),
                     4000);
                AddPoint(new Vector3(-253.5485f, 1.462287f, 92.37619f), new Vector3(-150.6624f, 62.93191f, -32.89682f),
                     8000);
            }else if(sceneType == SceneType.Compagne)
            {
                ResetCurves(CurveLoopType.Oscillate);
                AddPoint(new Vector3(24.88234f, 57.17424f, 273.5295f), new Vector3(100f, -65f, 0f), 0);
                AddPoint(new Vector3(24.88234f, 57.17424f, 273.5295f), new Vector3(95f, -60f, 2f), 4000);
                AddPoint(new Vector3(24.88234f, 57.17424f, 273.5295f), new Vector3(80f, -55f, 0f), 8000);
            }else if(sceneType == SceneType.Option)
            {
                ResetCurves(CurveLoopType.Oscillate);
                AddPoint(new Vector3(-94.55411f, 13.07233f, -287.2042f), new Vector3(-260+5f, 40-5f, 0f), 0);
                AddPoint(new Vector3(-94.55411f, 13.07233f, -287.2042f), new Vector3(-260f, 40f, 2f), 4000);
                AddPoint(new Vector3(-94.55411f, 13.07233f, -287.2042f), new Vector3(-260-15f, 40+7f, 0f), 4000);
                
            }
        }

        void BackToMenu()
        {
            soundManager.PlaySound("echap_button");
            if(sceneType == SceneType.Compagne)
            {
                ResetCurves(CurveLoopType.Linear);
                var _time = 0;
                AddPoint(new Vector3(24.88234f, 57.17424f, 273.5295f), new Vector3(105f, -60f, 0f), _time);
                _time += 1000;
                AddPoint(new Vector3(-130.7379f, 144.2281f, 289.9554f), Vector3.Zero, _time);
                _time += 1000;
                AddPoint(new Vector3(-253.5485f, 1.462287f, 92.37619f), new Vector3(-145.6624f, 56.93191f, -32.89682f), _time);
                timeToTurn = 2000;
                activeTurn = true;
            }
            else
            {
                ResetCurves(CurveLoopType.Linear);
                var _time = 0;
                AddPoint(new Vector3(-94.55411f, 13.07233f, -287.2042f), new Vector3(-260f, 40f, 0f), _time);
                _time += 1000;
                AddPoint(new Vector3(-339.1484f, 225.2347f, -234.1863f), Vector3.Zero, _time);
                _time += 1000;
                 AddPoint(new Vector3(-253.5485f, 1.462287f, 92.37619f), new Vector3(-145.6624f, 56.93191f, -32.89682f), _time);
                 timeToTurn = 2000;
                 activeTurn = true;
            }

            sceneType = SceneType.Main;
            selectedIndex = 15;
        }

        public void LoadContent(ContentManager Content)
        {
            sBatch = (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));

            loadingScreen = Content.Load<Texture2D>("Textures\\Loading");

            loadingContentThread = new Thread(new ThreadStart(
                                                  delegate
                                                      {
                                                          Load(Content);
                                                          loadingContentThread = null;

                                                      }))
                                       {
                                           Name = "Next Thread",
                                           Priority = ThreadPriority.Normal
                                       };

            loadingContentThread.Start();
            
        }

        void Load(ContentManager Content)
        {
            selected = Content.Load<Texture2D>("Textures\\MENU\\selected");
            notSelected = Content.Load<Texture2D>("Textures\\MENU\\no_selected");
            denied = Content.Load<Texture2D>("Textures\\File_RED");

            model = Content.Load<Model>("Models\\level0");
            EffectMaker.SetMenuEffect(model, Content);

            aspectRatio = GraphicsDevice.Viewport.AspectRatio;
            selectedIndex = 15;

            twoDManager = new TwoDManager(Content,soundManager);
            twoDManager.OnSelectLevel += SelectingLevel;
            InitCurve();

            //postprocess ----
            pp_blure = Content.Load<Effect>("Shaders\\menu_blur");
            PP_guisanteBlure = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                                                  GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color,
                                                  DepthFormat.Depth24);
            //------------------

            backMusic = Content.Load<Song>("Audio\\Music\\DYNAMITE");
            MediaPlayer.Play(backMusic);
        }

        private void SelectingLevel(object sender, MenuEventArgs e)
        {
            OnGameStart(this, e);
        }

        void AddPoint(Vector3 position,Vector3 lockAt,float _time)
        {
            cameraCurvePosition.AddPoint(position, _time);
            cameraCurveLookat.AddPoint(lockAt, _time);
            cameraCurvePosition.SetTangents();
            cameraCurveLookat.SetTangents();
        }

        private void InitCurve()
        {
            float _time = 0;
            AddPoint(new Vector3(0.1802747f, 820.5233f, 7.19148f), new Vector3(-20, -500, 30), _time);
            _time += 2000;
            AddPoint(new Vector3(1.827225f, 394.9366f, 0.6908872f), new Vector3(-20, 56.93191f, -30), _time);
            _time += 2200;
            AddPoint(new Vector3(-253.5485f, 1.462287f, 92.37619f), new Vector3(-145.6624f, 56.93191f, -32.89682f),
                     _time);
            timeToTurn = 4200;
            activeTurn = true;
        }

        public override void Update(GameTime gameTime)
        {
            if(loadingContentThread != null) return;

            var currentState = Keyboard.GetState();


            UpdateCameraCurve(gameTime);

            

            if (twoDManager.CurrentType == TwoDSceneType.None && !activeTurn)
            {
                if (currentState.IsKeyDown(Keys.Up) && oldState.IsKeyUp(Keys.Up))
                    DecreaseIndex();
                else if (currentState.IsKeyDown(Keys.Down) && oldState.IsKeyUp(Keys.Down))
                    IncreaseIndex();
                else if (currentState.IsKeyDown(Keys.Enter) && oldState.IsKeyUp(Keys.Enter))
                    SelectScene();
                else if (currentState.IsKeyDown(Keys.Escape) && oldState.IsKeyUp(Keys.Escape) &&
                         sceneType != SceneType.Main)
                    BackToMenu();
            }

            twoDManager.Update(gameTime, currentState, oldState);
            oldState = currentState;

            if(isShowing && MediaPlayer.State == MediaState.Stopped)
                MediaPlayer.Play(backMusic);


            base.Update(gameTime);
        }

        void UpdateCameraCurve(GameTime gameTime)
        {
            // Calculate the camera's current position.
            cameraPosition =
                cameraCurvePosition.GetPointOnCurve((float) time);
            cameraLookat =
                cameraCurveLookat.GetPointOnCurve((float) time);

            // Set up the view matrix and projection matrix.
            view = Matrix.CreateLookAt(cameraPosition, cameraLookat,
                                       new Vector3(0.0f, 1.0f, 0.0f));

            proj = Matrix.CreatePerspectiveFieldOfView(FOV, aspectRatio,
                                                       nearClip, farClip);

            time += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (isExiting && time >= timeToTurn)
                Game.Exit();
            if (activeTurn && time >= timeToTurn)
                Turning();
        }

        public override void Draw(GameTime gameTime)
        {
            if (loadingContentThread == null)
            {
                GraphicsDevice.SetRenderTarget(PP_guisanteBlure);

                GraphicsDevice.BlendState = BlendState.Opaque;
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
                GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;
                GraphicsDevice.Clear(Color.Black);

                foreach (var mesh in model.Meshes)
                {
                    var id = 0;
                    foreach (var meshPart in mesh.MeshParts)
                    {
                        id++;
                        var effect = (Effect) meshPart.Tag;
                        effect.CurrentTechnique = effect.Techniques["Technique1"];

                        effect.Parameters["World"].SetValue(Matrix.Identity);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(proj);

                        Vector3 lightPos = cameraPosition;
                        lightPos.Y = lightPos.Y > 150 ? 150 : cameraPosition.Y;
                        if (attpower < 2 || attpower > 3)
                            attpawerbool = !attpawerbool;

                        if (attpawerbool)
                        {
                            attpower += 0.0001f;
                        }
                        else
                        {
                            attpower -= 0.0001f;
                        }

                        effect.Parameters["LightPosition"].SetValue(new Vector3(lightPos.X, lightPos.Y, lightPos.Z));
                        effect.Parameters["attpower"].SetValue(attpower);
                        effect.Parameters["CameraPosition"].SetValue(cameraPosition);
                        effect.Parameters["LightAttenuation"].SetValue(1000);

                        if (id == selectedIndex)
                        {
                            if (id == 19 && GameSettings.LevelIndex == 0) effect.Parameters["tex"].SetValue(denied);
                            else
                                effect.Parameters["tex"].SetValue(/*id == 16 ? denied :*/ selected);
                        }
                        else if (id > 14)
                            effect.Parameters["tex"].SetValue(notSelected);

                        meshPart.Effect = effect;
                    }
                    mesh.Draw();
                }
                GraphicsDevice.SetRenderTarget(null);
                GraphicsDevice.Clear(Color.Black);
                original_menu_output = PP_guisanteBlure;

                sBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                pp_blure.CurrentTechnique.Passes[0].Apply();
                pp_blure.Parameters["textureToRender"].SetValue(original_menu_output);
                sBatch.Draw(original_menu_output, Vector2.Zero, Color.White);
                sBatch.End();

                sBatch.Begin();
                twoDManager.Draw(sBatch);
                sBatch.End();
            }
            else
            {
                sBatch.Begin();

                sBatch.Draw(loadingScreen, new Vector2(((GameSettings.WindowWidth - loadingScreen.Width) / 2.0f),
                                                       (GameSettings.WindowHeight - loadingScreen.Height) / 2.0f),
                            Color.White);

                sBatch.End();
            }
            base.Draw(gameTime);
        }

        public override void Show()
        {
            MediaPlayer.Stop();
            isShowing = true;
            base.Show();
        }

        public override void Hide()
        {
            MediaPlayer.Stop();
            isShowing = false;
            base.Hide();
        }
    }

    public class MenuEventArgs : EventArgs
    {
        public int LevelIndex { get; set; }
        public bool IsNewGame { get; set; }
        public bool IsSelecting { get; set; }
    }
}