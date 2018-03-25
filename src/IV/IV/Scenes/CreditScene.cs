using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace IV.Scenes
{
    class CreditScene : GameScene
    {
        private Video video;
        private VideoPlayer player;
        private Texture2D videoTexture;
        private readonly SpriteBatch spriteBatch;
        private KeyboardState oldState;
        public event EventHandler OnExit;

        private Texture2D loadingScreen;
        Thread loadingContentThread;

        public CreditScene(Game game) 
            : base(game)
        {
            spriteBatch = (SpriteBatch) Game.Services.GetService(typeof (SpriteBatch));
        }

        public void LoadContent(ContentManager content)
        {
            loadingScreen = content.Load<Texture2D>("Textures\\Loading");
            loadingContentThread = new Thread(new ThreadStart(
                                                     delegate
                                                     {
                                                         LoadVideo(content);
                                                         loadingContentThread = null;

                                                     }))
            {
                Name = "Next Thread",
                Priority = ThreadPriority.Normal
            };

            loadingContentThread.Start();  
        }

        void LoadVideo(ContentManager content)
        {
            video = content.Load<Video>("Credit\\credit");
            player = new VideoPlayer();
            player.Play(video);
        }

        public override void Update(GameTime gameTime)
        {
            if (loadingContentThread != null) return;

           /* if (player.State == MediaState.Stopped)
            {
                player.Play(video);
                player.IsLooped = false;
            }*/

            var keyState = Keyboard.GetState();
            if ((keyState.IsKeyDown(Keys.Escape) && oldState.IsKeyUp(Keys.Escape)) || player.State == MediaState.Stopped)
                if (OnExit != null)
                    OnExit(this, EventArgs.Empty);

            oldState = keyState;

            base.Update(gameTime);
        }

        public override void Hide()
        {
            player.Stop();
            base.Hide();
        }

        public override void Show()
        {
            if (player != null && video != null)
                player.Play(video);
            base.Show();
        }

        public override void Draw(GameTime gameTime)
        {
            if (loadingContentThread == null)
            {
                var blend = GraphicsDevice.BlendState;
                var depth = GraphicsDevice.DepthStencilState;
                var sampler = GraphicsDevice.SamplerStates[0];

                spriteBatch.Begin();

                if (player.State != MediaState.Stopped)
                    videoTexture = player.GetTexture();

                if (videoTexture != null)
                    spriteBatch.Draw(videoTexture,
                                     new Rectangle(0, 0, GameSettings.WindowWidth, GameSettings.WindowHeight),
                                     /*new Rectangle(150, 80, videoTexture.Width-300, videoTexture.Height-160)*/null,
                                     Color.White);


                spriteBatch.End();

                GraphicsDevice.BlendState = blend;
                GraphicsDevice.DepthStencilState = depth;
                GraphicsDevice.SamplerStates[0] = sampler;
            }
            else
            {
                spriteBatch.Begin();

                spriteBatch.Draw(loadingScreen, new Vector2(((GameSettings.WindowWidth - loadingScreen.Width) / 2.0f),
                                                       (GameSettings.WindowHeight - loadingScreen.Height) / 2.0f),
                            Color.White);

                spriteBatch.End();
            }

            base.Draw(gameTime);
        }
        
    }
}
