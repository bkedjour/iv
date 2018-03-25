using System;
using IV.Menu_Scene.Extras;
using IV.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace IV.Menu_Scene
{
    enum TwoDSceneType{None,Audio,Command,Video,Chapter, Extras}
    class TwoDManager
    {
        private readonly AudioScene audioScene;
        private readonly CommandScene commandScene;
        private readonly VideoScene videoScene;
        private readonly ChapterSelect chapterScene;
        private readonly ExtrasScene extrasScene;
        public event EventHandler<MenuEventArgs> OnSelectLevel;
        public TwoDSceneType CurrentType{ get; private set;}

        private readonly SoundManager soundManager;

        public TwoDManager(ContentManager content, SoundManager soundManager)
        {
            audioScene = new AudioScene(content,soundManager);
            commandScene = new CommandScene(content);
            videoScene = new VideoScene(content,soundManager);
            chapterScene = new ChapterSelect(content,soundManager);
            extrasScene = new ExtrasScene(content, soundManager);
            chapterScene.OnSelectLevel += SelectingLevel;

            this.soundManager = soundManager;
        }

        private void SelectingLevel(object sender, MenuEventArgs e)
        {
            if (OnSelectLevel != null)
                OnSelectLevel(this, e);
        }

        public void SetType(TwoDSceneType type)
        {
            CurrentType = type;
            if(type == TwoDSceneType.Audio)
                audioScene.Reset();
            if (type == TwoDSceneType.Chapter)
            {
                chapterScene.FirstEnter = true;
            }
            if (type == TwoDSceneType.Extras)
            {
                extrasScene.RefreshAchievements();
            }
        }

        public void Update(GameTime gameTime,KeyboardState keyboardState, KeyboardState oldState)
        {
            if(keyboardState.IsKeyDown(Keys.Escape) && oldState.IsKeyUp(Keys.Escape))
            {
                CurrentType = TwoDSceneType.None;
                soundManager.PlaySound("echap_button");
            }

            switch (CurrentType)
            {
                case TwoDSceneType.Audio:
                    audioScene.Update(gameTime);
                    break;

                case TwoDSceneType.Video:
                    videoScene.Update(gameTime);
                    break;
                case TwoDSceneType.Chapter:
                    chapterScene.Update(gameTime);
                    break;
                case TwoDSceneType.Extras:
                    extrasScene.Update(gameTime);
                    break;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            switch (CurrentType)
            {
                case TwoDSceneType.Audio:
                    audioScene.Draw(spriteBatch);
                    break;
                case TwoDSceneType.Command:
                    commandScene.Draw(spriteBatch);
                    break;
                case TwoDSceneType.Video:
                    videoScene.Draw(spriteBatch);
                    break;
                case TwoDSceneType.Chapter:
                    chapterScene.Draw(spriteBatch);
                    break;
                case TwoDSceneType.Extras:
                    extrasScene.Draw(spriteBatch);
                    break;
            }
        }
    }
}
