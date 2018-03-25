using System.Collections.Generic;
using IV.Achievement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace IV.Menu_Scene.Extras
{
    public class AcheivementsView
    {
        private readonly Texture2D texture;
        private Vector2 position;
        private SpriteFont font;

        private readonly List<AchievementCell> achievementCells;

        private KeyboardState oldState;
        public bool FirstEnter { get; set; }

        private int horizontalIndex;
        private int verticleIndex;

        public bool isFocused { get; set; }

        private readonly MenuSelector leftSelector;
        private readonly MenuSelector rightSelector;
        private bool isShowRight;

        #region Achievements

        private readonly AchievementCell immortality;
        private readonly AchievementCell peace;
        private readonly AchievementCell jumper;
        private readonly AchievementCell skyJump;
        private readonly AchievementCell superJump;
        private readonly AchievementCell zombie;
        private readonly AchievementCell hunter;
        private readonly AchievementCell killer;
        private readonly AchievementCell hitman;
        private readonly AchievementCell statue;
        private readonly AchievementCell rejected;
        private readonly AchievementCell winner;

        #endregion


        public AcheivementsView(ContentManager content, Vector2 position)
        {
            this.position = position;
            texture = content.Load<Texture2D>("Textures\\MENU\\Graphics\\Extras\\Achievements");
            font = content.Load<SpriteFont>("Fonts\\gameFont");

            var cellsOrigin = new Vector2(position.X + (int) ((5*GameSettings.WindowWidth)/100f),
                                          position.Y + (int) ((2.2f*GameSettings.WindowHeight)/100f));

            achievementCells = new List<AchievementCell>();

            #region Achievements

            var lockedAcheivement = content.Load<Texture2D>("Textures\\Achievements\\Lock");

            var descriptionPosition = new Vector2((41.4f*GameSettings.WindowWidth)/100f,
                                                  cellsOrigin.Y + (int) ((57.5f*GameSettings.WindowHeight)/100f));

            immortality = new AchievementCell(content.Load<Texture2D>("Textures\\Achievements\\1-Immortality"),
                                                  Vector2.Zero, "Play game without dying", descriptionPosition,
                                                  font, AchievementManager.Manager.AchievementsStatus.PlayGameWithoutDying, lockedAcheivement);
            peace = new AchievementCell(content.Load<Texture2D>("Textures\\Achievements\\2-Peace_n_love"),
                                            Vector2.Zero, "Play game without killing any enemy", descriptionPosition,
                                            font, AchievementManager.Manager.AchievementsStatus.PlayGameWithoutKilling, lockedAcheivement);
            jumper = new AchievementCell(content.Load<Texture2D>("Textures\\Achievements\\3-The_Jumper"),
                                             Vector2.Zero, "Jump 100 times in one level", descriptionPosition,
                                             font, AchievementManager.Manager.AchievementsStatus.Jump100Accomplished, lockedAcheivement);
            skyJump = new AchievementCell(content.Load<Texture2D>("Textures\\Achievements\\4-skyJump"),
                                              Vector2.Zero, "Super fall", descriptionPosition,
                                              font, AchievementManager.Manager.AchievementsStatus.FallDownAccomplished, lockedAcheivement);
            superJump = new AchievementCell(content.Load<Texture2D>("Textures\\Achievements\\5-The_SuperJumper"),
                                                Vector2.Zero, "8 times super jump in one level", descriptionPosition,
                                                font, AchievementManager.Manager.AchievementsStatus.SuperJumpAccomplished, lockedAcheivement);
            zombie = new AchievementCell(content.Load<Texture2D>("Textures\\Achievements\\6-The_Zombie"),
                                             Vector2.Zero, "Die 42 times", descriptionPosition,
                                             font, AchievementManager.Manager.AchievementsStatus.Die42TimeAccomplished, lockedAcheivement);
            hunter = new AchievementCell(content.Load<Texture2D>("Textures\\Achievements\\7-The_Hunter"),
                                             Vector2.Zero, "Kill 30 enemies in one level", descriptionPosition,
                                             font, AchievementManager.Manager.AchievementsStatus.ThirtyEnemiesKilledUnlocked, lockedAcheivement);
            killer = new AchievementCell(content.Load<Texture2D>("Textures\\Achievements\\8-The_hollow_killer"),
                                             Vector2.Zero, "100 times cero in one level", descriptionPosition,
                                             font, AchievementManager.Manager.AchievementsStatus.Cero100Accomplished, lockedAcheivement);
            hitman = new AchievementCell(content.Load<Texture2D>("Textures\\Achievements\\9-The_Hitman"),
                                             Vector2.Zero, "kill all the enemies in one level", descriptionPosition,
                                             font, AchievementManager.Manager.AchievementsStatus.ZeroEnemiesAccomplished, lockedAcheivement);
            statue = new AchievementCell(content.Load<Texture2D>("Textures\\Achievements\\10-Statue"),
                                             Vector2.Zero, "Idle for 30 seconds", descriptionPosition,
                                             font, AchievementManager.Manager.AchievementsStatus.IdleFor30SecAccomplished, lockedAcheivement);
            rejected = new AchievementCell(content.Load<Texture2D>("Textures\\Achievements\\11-The_Rejected"),
                                               Vector2.Zero, "Be rejected 3 times by file analyzer", descriptionPosition,
                                               font, AchievementManager.Manager.AchievementsStatus.Rejected3TimesAccomplished, lockedAcheivement);
            winner = new AchievementCell(content.Load<Texture2D>("Textures\\Achievements\\12-The_Winner"),
                                             Vector2.Zero, "Finish the Game", descriptionPosition,
                                             font, AchievementManager.Manager.AchievementsStatus.FinishGameAccomplished, lockedAcheivement);

            achievementCells.Add(immortality);
            achievementCells.Add(peace);
            achievementCells.Add(jumper);
            achievementCells.Add(skyJump);
            achievementCells.Add(superJump);
            achievementCells.Add(zombie);
            achievementCells.Add(hunter);
            achievementCells.Add(killer);
            achievementCells.Add(hitman);
            achievementCells.Add(statue);
            achievementCells.Add(rejected);
            achievementCells.Add(winner);

            #endregion

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    achievementCells[i*2 + j].Position = new Vector2(
                        cellsOrigin.X + (int) ((4*(j*7.05f)*GameSettings.WindowWidth)/100f),
                        cellsOrigin.Y + (int) ((5*(i*1.8f)*GameSettings.WindowHeight)/100f));
                }
            }

            leftSelector = new MenuSelector(content, (39.8f*GameSettings.WindowWidth)/100f,
                                            cellsOrigin.Y + ((0*GameSettings.WindowHeight)/100f),
                                            cellsOrigin.Y + ((9f*GameSettings.WindowHeight)/100f),
                                            cellsOrigin.Y + ((18f*GameSettings.WindowHeight)/100f),
                                            cellsOrigin.Y + ((27f*GameSettings.WindowHeight)/100f),
                                            cellsOrigin.Y + ((36f*GameSettings.WindowHeight)/100f),
                                            cellsOrigin.Y + ((45f*GameSettings.WindowHeight)/100f));
            rightSelector = new MenuSelector(content, (68*GameSettings.WindowWidth)/100f,
                                             cellsOrigin.Y + ((0*GameSettings.WindowHeight)/100f),
                                             cellsOrigin.Y + ((9f*GameSettings.WindowHeight)/100f),
                                             cellsOrigin.Y + ((18f*GameSettings.WindowHeight)/100f),
                                             cellsOrigin.Y + ((27f*GameSettings.WindowHeight)/100f),
                                             cellsOrigin.Y + ((36f*GameSettings.WindowHeight)/100f),
                                             cellsOrigin.Y + ((45f*GameSettings.WindowHeight)/100f));

        }

        public void Refresh()
        {
            immortality.SetUnlocked(AchievementManager.Manager.AchievementsStatus.PlayGameWithoutDying);
            peace.SetUnlocked(AchievementManager.Manager.AchievementsStatus.PlayGameWithoutKilling);
            jumper.SetUnlocked(AchievementManager.Manager.AchievementsStatus.Jump100Accomplished);
            skyJump.SetUnlocked(AchievementManager.Manager.AchievementsStatus.FallDownAccomplished);
            superJump.SetUnlocked(AchievementManager.Manager.AchievementsStatus.SuperJumpAccomplished);
            zombie.SetUnlocked(AchievementManager.Manager.AchievementsStatus.Die42TimeAccomplished);
            hunter.SetUnlocked(AchievementManager.Manager.AchievementsStatus.ThirtyEnemiesKilledUnlocked);
            killer.SetUnlocked(AchievementManager.Manager.AchievementsStatus.Cero100Accomplished);
            hitman.SetUnlocked(AchievementManager.Manager.AchievementsStatus.ZeroEnemiesAccomplished);
            statue.SetUnlocked(AchievementManager.Manager.AchievementsStatus.IdleFor30SecAccomplished);
            rejected.SetUnlocked(AchievementManager.Manager.AchievementsStatus.Rejected3TimesAccomplished);
            winner.SetUnlocked(AchievementManager.Manager.AchievementsStatus.FinishGameAccomplished);
        }

        public void Update(GameTime gameTime)
        {
            foreach (var achievementCell in achievementCells)
            {
                achievementCell.Update(gameTime);
                achievementCell.IsSelected = false;
            }

            if (!isFocused) return;

            var currentState = Keyboard.GetState();

            if (currentState.IsKeyDown(Keys.Left) && oldState.IsKeyUp(Keys.Left))
            {
                horizontalIndex--;
                if (horizontalIndex < 0)
                {
                    horizontalIndex = 0;
                    isFocused = false;
                }
                isShowRight = false;

            }else if (currentState.IsKeyDown(Keys.Right) && oldState.IsKeyUp(Keys.Right))
            {
                if (FirstEnter)
                {
                    FirstEnter = false;
                }
                else
                {
                    horizontalIndex++;
                    if (horizontalIndex > 1)
                    {
                        if (verticleIndex < 5)
                        {
                            horizontalIndex = 0;
                            isShowRight = false;
                            verticleIndex++;
                            leftSelector.MoveNext();
                            rightSelector.MoveNext();
                        }
                        else
                        {
                            horizontalIndex = 1;
                            isShowRight = true;
                        }
                    }
                    else isShowRight = true;
                }
            }

            if (currentState.IsKeyDown(Keys.Up) && oldState.IsKeyUp(Keys.Up))
            {
                verticleIndex--;
                leftSelector.MoveBack();
                rightSelector.MoveBack();
                if (verticleIndex < 0)
                    verticleIndex = 0;
            }
            else if (currentState.IsKeyDown(Keys.Down) && oldState.IsKeyUp(Keys.Down))
            {
                verticleIndex++;
                leftSelector.MoveNext();
                rightSelector.MoveNext();
                if (verticleIndex > 5)
                    verticleIndex = 5;
            }

            achievementCells[verticleIndex*2 + horizontalIndex].IsSelected = true;

            leftSelector.Update(gameTime);
            rightSelector.Update(gameTime);

            oldState = currentState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture,
                           new Rectangle((int)position.X,
                                         (int)position.Y,
                                         GameSettings.WindowWidth * texture.Width / 1600,
                                         GameSettings.WindowHeight * texture.Height / 900), null, Color.White);
            foreach (var achievementCell in achievementCells)
            {
                achievementCell.Draw(spriteBatch);
            }

            if (isFocused)
            {
                if (!isShowRight)
                    leftSelector.Draw(spriteBatch);
                else
                    rightSelector.Draw(spriteBatch);
            }
        }
    }
}