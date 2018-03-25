using System;
using System.Collections.Generic;
using IV.Action_Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace IV.Menu_Scene
{
    class PauseScene
    {
        private readonly Texture2D texture;
        private readonly Texture2D title;
        private readonly MenuSelector selector;
        private readonly List<Texture2D> levels;

        private KeyboardState oldState;

        private int selectedIndex;
        public event EventHandler OnGameExit;
        public event EventHandler OnResumeGame;
        public event EventHandler OnMainMenu;
        public event EventHandler OnResetLevel;
        public event EventHandler OnLoadCheckPoint;
       

        private readonly SoundManager soundManager;
        private readonly ObjectivesManager objectiveManager;

        private readonly CommandScene commands;
        private AudioScene audioScene;
        private bool isShowingCommands;
        private bool isShowingAudio;

        public PauseScene(ContentManager content, SoundManager soundManager, ObjectivesManager objectiveManager)
        {
            texture = content.Load<Texture2D>("Textures\\MENU\\Graphics\\menu_M_pause");
            title = content.Load<Texture2D>("Textures\\MENU\\Graphics\\Menu_pause");
            levels = new List<Texture2D>
                         {
                             content.Load<Texture2D>("Textures\\Objectives\\P1"),
                             content.Load<Texture2D>("Textures\\Objectives\\P2"),
                             content.Load<Texture2D>("Textures\\Objectives\\P3"),
                             content.Load<Texture2D>("Textures\\Objectives\\P4"),
                             content.Load<Texture2D>("Textures\\Objectives\\P5")
                         };

            selector = new MenuSelector(content, (1.5f * GameSettings.WindowWidth) / 100f,
                                        (19f * GameSettings.WindowHeight) / 100f,
                                        (33f * GameSettings.WindowHeight) / 100f,
                                        (41f * GameSettings.WindowHeight) / 100f,
                                        (49f * GameSettings.WindowHeight) / 100f,
                                        (57f * GameSettings.WindowHeight) / 100f,
                                        (65f * GameSettings.WindowHeight) / 100f,
                                        (84f * GameSettings.WindowHeight) / 100f);
            this.soundManager = soundManager;
            this.objectiveManager = objectiveManager;

            commands = new CommandScene(content);
            audioScene = new AudioScene(content, soundManager);
        }

        public void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();

            if((!isShowingAudio && !isShowingCommands)&&keyboardState.IsKeyDown(Keys.Up) && oldState.IsKeyUp(Keys.Up))
            {
                if (selector.MoveBack())
                {
                    selectedIndex--;
                    soundManager.PlaySound("chose_button");
                }

            }
            else if ((!isShowingAudio && !isShowingCommands) && keyboardState.IsKeyDown(Keys.Down) && oldState.IsKeyUp(Keys.Down))
            {
                if (selector.MoveNext())
                {
                    selectedIndex++;
                    soundManager.PlaySound("chose_button");
                }
            }else if(keyboardState.IsKeyDown(Keys.Enter) && oldState.IsKeyUp(Keys.Enter))
            {
                soundManager.PlaySound("selct_button");
                switch (selectedIndex)
                {
                    case 0:
                        if (OnResumeGame != null)
                            OnResumeGame(this, EventArgs.Empty);
                        break;
                    case 1:
                        if (OnMainMenu != null)
                            OnMainMenu(this, EventArgs.Empty);
                        break;
                    case 2:
                        if (OnResetLevel != null)
                            OnResetLevel(this, EventArgs.Empty);
                        break;
                    case 3:
                        if (OnLoadCheckPoint != null)
                            OnLoadCheckPoint(this, EventArgs.Empty);
                        break;
                    case 4:
                        isShowingCommands = !isShowingCommands;
                        break;
                    case 5:
                        isShowingAudio = !isShowingAudio;
                        break;
                    case 6:
                        if (OnGameExit != null)
                            OnGameExit(this, EventArgs.Empty);
                        break;
                }
            }

            oldState = keyboardState;
            selector.Update(gameTime);

            if(isShowingAudio)
                audioScene.Update(gameTime);
        }

        public void Draw(SpriteBatch sBatch,int levelID)
        {
            sBatch.Draw(texture, new Rectangle(0, 0, GameSettings.WindowWidth, GameSettings.WindowHeight),
                        new Rectangle(0, 0, texture.Width, texture.Height),
                        Color.White);
            sBatch.Draw(title,
                        new Rectangle(0, 0, GameSettings.WindowWidth*800/1600, GameSettings.WindowHeight*84/900),
                        Color.White);
            selector.Draw(sBatch);

            var posi = new Vector2((39f*GameSettings.WindowWidth)/100f,
                                   (21f*GameSettings.WindowHeight)/100f);
            sBatch.Draw(levels[levelID - 1],
                        new Rectangle((int) posi.X, (int) posi.Y, GameSettings.WindowWidth*900/1600,
                                      GameSettings.WindowHeight*100/900),
                        null, Color.White);


            var ySpacing = 10;
            foreach (var objective in objectiveManager.Objectives)
            {
                sBatch.Draw(objective,
                            new Rectangle((int) posi.X,
                                          (int) posi.Y + (int) ((ySpacing*GameSettings.WindowHeight)/100f),
                                          GameSettings.WindowWidth*900/1600,
                                          GameSettings.WindowHeight*(levelID == 5 ? 540 : 100)/900), null, Color.White);

                ySpacing += 10;
            }


            if (isShowingCommands)
                commands.Draw(sBatch);
            else if (isShowingAudio)
                audioScene.Draw(sBatch);
        }
    }
}
