using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using CleanupDataTypes;

namespace Cleanup
{

    public class Cleanup : Microsoft.Xna.Framework.Game
    {
        public enum Types
        { /* THIS ENUMERATION IS NOT TO BE USED. IT IS JUST HERE AS A REFERENCE (FOR NOW). */
            BulldozerYellow = 0,
            BulldozerRed = 1,
            BulldozerBlue = 2,
            BulldozerGreen = 3,
            BulldozerPurple = 4,
            BulldozerBlack = 5,

            BagYellow = 6,
            BagRed = 7,
            BagBlue = 8,
            BagGreen = 9,
            BagPurple = 10,
            BagBlack = 11,

            TrashYellow = 12,
            TrashRed = 13,
            TrashBlue = 14,
            TrashGreen = 15,
            TrashPurple = 16,
            TrashBlack = 17,

            Wall = 18,
            RecyclingBin = 19,
            TranslucentBox = 20,
            Fire = 21,
            ShallowWater = 22,
            DeepWater = 23
        };

        public static Cleanup Instance;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public static KeyboardState oldKeyboard;
        public static KeyboardState currentKeyboard;
        public static MouseState oldMouse;
        public static MouseState currentMouse;

        public enum GameState
        {
            TitleScreen = 1,
            StageSelect = 2,
            InstructionScreen = 3,
            MainGame = 4,
            PauseScreen = 5,
            WinnerScreen = 6
        };

        GameState gameState;
        int menuChoice;
        int numPlayers;

        List<string> stages;

        CleanupDataTypes.StageData theStage;
        public List<Entity> entityList;

        Vector2 center;
        Rectangle tileBackgroundBounds;

        Camera2d cam;

        List<SnakePlayer> players;
        int winner;

        int garbageAtEnd;

        Vector2 oldMinXY;
        Vector2 oldMaxXY;

        Texture2D titleScreenImage;
        Texture2D stageSelectScreenImage;
        Texture2D instructionScreenImage;

        public List<Texture2D> textureList;

        Texture2D backgroundTile;

        public TimeSpan matchTimeRemaining;
        public TimeSpan matchStartsIn;

        SpriteFont spritefont;

        Song backgroundMusic;
        bool songStart;

        public Cleanup()
        {
            Instance = this;
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = 800;
            graphics.PreferredBackBufferWidth = 800;
            //graphics.PreferredBackBufferHeight = 1200;
            //graphics.PreferredBackBufferWidth = 1920;
            //graphics.IsFullScreen = true;
            Content.RootDirectory = "Content";
            Window.Title = "Cleanup!";
        }

        void LoadAllTextures()
        {
            textureList = new List<Texture2D>();

            textureList.Add(Content.Load<Texture2D>("bulldozers/bulldozer_yellow"));    /* 0 */
            textureList.Add(Content.Load<Texture2D>("bulldozers/bulldozer_red"));
            textureList.Add(Content.Load<Texture2D>("bulldozers/bulldozer_blue"));
            textureList.Add(Content.Load<Texture2D>("bulldozers/bulldozer_green"));
            textureList.Add(Content.Load<Texture2D>("bulldozers/bulldozer_purple"));
            textureList.Add(Content.Load<Texture2D>("bulldozers/bulldozer_black"));

            textureList.Add(Content.Load<Texture2D>("bags/bag_yellow"));                /* 6 */
            textureList.Add(Content.Load<Texture2D>("bags/bag_red"));
            textureList.Add(Content.Load<Texture2D>("bags/bag_blue"));
            textureList.Add(Content.Load<Texture2D>("bags/bag_green"));
            textureList.Add(Content.Load<Texture2D>("bags/bag_purple"));
            textureList.Add(Content.Load<Texture2D>("bags/bag_black"));

            textureList.Add(Content.Load<Texture2D>("trash/trash_yellow"));             /* 12 */
            textureList.Add(Content.Load<Texture2D>("trash/trash_red"));
            textureList.Add(Content.Load<Texture2D>("trash/trash_blue"));
            textureList.Add(Content.Load<Texture2D>("trash/trash_green"));
            textureList.Add(Content.Load<Texture2D>("trash/trash_purple"));
            textureList.Add(Content.Load<Texture2D>("trash/trash_black"));

            textureList.Add(Content.Load<Texture2D>("grayblock_64"));                    /* 18 */
            textureList.Add(Content.Load<Texture2D>("recycling_bin"));
            textureList.Add(Content.Load<Texture2D>("translucent_box"));
            textureList.Add(Content.Load<Texture2D>("fire"));
            textureList.Add(Content.Load<Texture2D>("water_shallow"));                  /* 22 */
            textureList.Add(Content.Load<Texture2D>("water_deep"));                     /* 23 */
        }

        protected override void Initialize()
        {
            oldKeyboard = Keyboard.GetState();
            oldMouse = Mouse.GetState();

            titleScreenImage = Content.Load<Texture2D>("screens/title_screen");
            stageSelectScreenImage = Content.Load<Texture2D>("screens/stage_select_screen");
            instructionScreenImage = Content.Load<Texture2D>("screens/instruction_screen");

            stages = new List<string>();
            stages.Add("Grassy");
            stages.Add("Gravelly");

            LoadAllTextures();
            spritefont = Content.Load<SpriteFont>("courier_new");

            backgroundMusic = Content.Load<Song>("music/Final Stance");
            MediaPlayer.IsRepeating = true;

            gameState = GameState.TitleScreen;

            base.Initialize();
        }

        void InitializeMainGame(int numPlayers, string stageName)
        {
            theStage = Content.Load<CleanupDataTypes.StageData>("stages/" + stageName);
            backgroundTile = Content.Load<Texture2D>(theStage.tileTextureName);
            backgroundMusic = Content.Load<Song>("music/" + theStage.music);
            songStart = false;

            cam = new Camera2d();
            cam.Pos = theStage.cameraStartPosition;
            tileBackgroundBounds = new Rectangle(0, 0, 20000, 20000);

            entityList = new List<Entity>();

            /* top wall */
            entityList.Add(new Entity((int)Types.Wall, new Vector2(theStage.bounds.Width / 2, 0)));
            entityList[0].bounds = new Rectangle(theStage.bounds.X - 100, theStage.bounds.Y - 100, theStage.bounds.Width + 2 * 100, 100);
            /* left wall */
            entityList.Add(new Entity((int)Types.Wall, new Vector2(0, theStage.bounds.Height / 2)));
            entityList[1].bounds = new Rectangle(theStage.bounds.X - 100, theStage.bounds.Y, 100, theStage.bounds.Height);
            /* right wall */
            entityList.Add(new Entity((int)Types.Wall, new Vector2(0, 0)));
            entityList[2].bounds = new Rectangle(theStage.bounds.X + theStage.bounds.Width, theStage.bounds.Y, 100, theStage.bounds.Height);
            /* bottom wall */
            entityList.Add(new Entity((int)Types.Wall, new Vector2(0, 0)));
            entityList[3].bounds = new Rectangle(theStage.bounds.X - 100, theStage.bounds.Y + theStage.bounds.Height, theStage.bounds.Width + 2 * 100, 100);

            foreach (StageEntity entity in theStage.entities)
            {
                entityList.Add(new Entity(entity.type, entity.position));
            }


            players = new List<SnakePlayer>(2);
            players.Add(new SnakePlayer(numPlayers - 1, theStage.player1StartDirection, Keys.Left, Keys.Right, Keys.Down, Keys.Up, theStage.player1StartPosition));
            if (numPlayers >= 2)
            {
                players.Add(new SnakePlayer(2, theStage.player2StartDirection, Keys.A, Keys.D, Keys.S, Keys.W, theStage.player2StartPosition));
            }

            oldMinXY = new Vector2(theStage.bounds.Width / 2, theStage.bounds.Height / 2);
            oldMaxXY = new Vector2(theStage.bounds.Width / 2, theStage.bounds.Height / 2);

            matchTimeRemaining = new TimeSpan(0, theStage.minutes, 0);
            matchStartsIn = new TimeSpan(0, 0, 6);
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            currentKeyboard = Keyboard.GetState();
            currentMouse = Mouse.GetState();
            
            // Allows the game to exit
            if (currentKeyboard.IsKeyDown(Keys.Escape))
                this.Exit();


            if (!songStart)
            {
                MediaPlayer.Stop();
                MediaPlayer.Play(backgroundMusic);
                songStart = true;
            }

            if (gameState == GameState.TitleScreen)
            {
                UpdateTitleScreen(gameTime);
            }
            else if (gameState == GameState.StageSelect)
            {
                UpdateStageSelectScreen(gameTime);
            }
            else if (gameState == GameState.InstructionScreen)
            {
                UpdateInstructionScreen(gameTime);
            }
            else if (gameState == GameState.PauseScreen)
            {
                UpdatePauseScreen(gameTime);
            }
            else if (gameState == GameState.MainGame)
            {
                UpdateMainGame(gameTime);
            }
            else if (gameState == GameState.WinnerScreen)
            {
                UpdateWinnerScreen(gameTime);
            }

            oldKeyboard = currentKeyboard;
            oldMouse = currentMouse;

            base.Update(gameTime);
        }

        void UpdateTitleScreen(GameTime gameTime)
        {
            if (currentKeyboard.IsKeyDown(Keys.Down) && oldKeyboard.IsKeyUp(Keys.Down))
            {
                menuChoice = (menuChoice + 1) % 4;
            }
            if (currentKeyboard.IsKeyDown(Keys.Up) && oldKeyboard.IsKeyUp(Keys.Up))
            {
                menuChoice = (menuChoice + 3) % 4;
            }

            if ((currentKeyboard.IsKeyDown(Keys.Enter) && oldKeyboard.IsKeyUp(Keys.Enter)) || (currentKeyboard.IsKeyDown(Keys.Space) && oldKeyboard.IsKeyUp(Keys.Space)))
            {
                if (menuChoice == 0)
                {
                    numPlayers = 1;
                    menuChoice = 0;
                    gameState = GameState.StageSelect;
                }
                else if (menuChoice == 1)
                {
                    numPlayers = 2;
                    menuChoice = 0;
                    gameState = GameState.StageSelect;
                }
                else if (menuChoice == 2)
                {
                    gameState = GameState.InstructionScreen;
                }
                else if (menuChoice == 3)
                {
                    this.Exit();
                }
            }
        }

        void UpdateStageSelectScreen(GameTime gameTime)
        {
            if (currentKeyboard.IsKeyDown(Keys.Down) && oldKeyboard.IsKeyUp(Keys.Down))
            {
                menuChoice = (menuChoice + 1) % stages.Count();
            }
            if (currentKeyboard.IsKeyDown(Keys.Up) && oldKeyboard.IsKeyUp(Keys.Up))
            {
                menuChoice = (menuChoice + (stages.Count() - 1)) % stages.Count();
            }
            if ((currentKeyboard.IsKeyDown(Keys.Enter) && oldKeyboard.IsKeyUp(Keys.Enter)) || (currentKeyboard.IsKeyDown(Keys.Space) && oldKeyboard.IsKeyUp(Keys.Space)))
            {
                InitializeMainGame(numPlayers, stages[menuChoice]);
                gameState = GameState.MainGame;
            }
        }

        void UpdateInstructionScreen(GameTime gameTime)
        {
            if (currentKeyboard.IsKeyDown(Keys.Enter) && oldKeyboard.IsKeyUp(Keys.Enter))
            {
                gameState = GameState.TitleScreen;
            }
            if (currentKeyboard.IsKeyDown(Keys.Space) && oldKeyboard.IsKeyUp(Keys.Space))
            {
                gameState = GameState.TitleScreen;
            }
        }

        void UpdateMainGame(GameTime gameTime)
        {
            center = Vector2.Zero;
            Vector2 minXY, maxXY;
            minXY = maxXY = players[0].body[0].position;
            int totalpieces = 0;
            foreach (SnakePlayer player in players)
            {
                foreach (SnakeBody segment in player.body)
                { // camera movement isn't as jagged with this one
                    center += segment.position;
                    totalpieces++;
                    if (segment.position.X > maxXY.X) maxXY.X = segment.position.X;
                    if (segment.position.Y > maxXY.Y) maxXY.Y = segment.position.Y;
                    if (segment.position.X < minXY.X) minXY.X = segment.position.X;
                    if (segment.position.Y < minXY.Y) minXY.Y = segment.position.Y;
                }
                /*for (int i = 0; i < player.bodyTypes.Count(); i++)
                { // much less computationally expensive, obviously (actually that doesn't really matter; the other one is still fast enough)
                    if (player.body.Count() > (i * Constants.BODIES_BETWEEN_BAGS))
                    {
                        center += player.body[i * Constants.BODIES_BETWEEN_BAGS].position;
                        totalpieces++;
                        if (player.body[i * Constants.BODIES_BETWEEN_BAGS].position.X > maxXY.X) maxXY.X = player.body[i * Constants.BODIES_BETWEEN_BAGS].position.X;
                        if (player.body[i * Constants.BODIES_BETWEEN_BAGS].position.Y > maxXY.Y) maxXY.Y = player.body[i * Constants.BODIES_BETWEEN_BAGS].position.Y;
                        if (player.body[i * Constants.BODIES_BETWEEN_BAGS].position.X < minXY.X) minXY.X = player.body[i * Constants.BODIES_BETWEEN_BAGS].position.X;
                        if (player.body[i * Constants.BODIES_BETWEEN_BAGS].position.Y < minXY.Y) minXY.Y = player.body[i * Constants.BODIES_BETWEEN_BAGS].position.Y;
                    }
                }*/
            }
            center /= totalpieces;
            // limits the camera centering speed. nice effect.
            if (cam.Pos.X > center.X) { cam.Pos += new Vector2(-Constants.DEFAULT_SPEED, 0.0f); }
            if (cam.Pos.Y > center.Y) { cam.Pos += new Vector2(0.0f, -Constants.DEFAULT_SPEED); }
            if (cam.Pos.X < center.X) { cam.Pos += new Vector2(Constants.DEFAULT_SPEED, 0.0f); }
            if (cam.Pos.Y < center.Y) { cam.Pos += new Vector2(0.0f, Constants.DEFAULT_SPEED); }

            /*if ((minXY.X < oldMinXY.X) || (maxXY.X > oldMaxXY.X) || (minXY.Y < oldMinXY.Y) || (maxXY.Y > oldMaxXY.Y))
            {
                cam.Zoom -= 0.005f;
            }
            if ((minXY.X > oldMinXY.X) || (maxXY.X < oldMaxXY.X) || (minXY.Y > oldMinXY.Y) || (maxXY.Y < oldMaxXY.Y))
            {
                cam.Zoom += 0.005f;
            }
            cam.Zoom = MathHelper.Clamp(cam.Zoom, 0.5f, 1.0f);*/

            float possibleNextZoom = MathHelper.Clamp(300 / Math.Max(maxXY.X - minXY.X, maxXY.Y - minXY.Y), theStage.maxZoomOut, 1.0f);
            cam.Zoom = Math.Min(cam.Zoom, possibleNextZoom);

            //cam.Zoom = MathHelper.Clamp(300 / Math.Max(maxXY.X - minXY.X, maxXY.Y - minXY.Y), 0.1f, 1.0f);

            oldMinXY = minXY;
            oldMaxXY = maxXY;

            if (matchStartsIn.TotalSeconds > 1)
            {
                MediaPlayer.Pause();
                matchStartsIn -= gameTime.ElapsedGameTime;
                return;
            }
            MediaPlayer.Resume();

            matchTimeRemaining -= gameTime.ElapsedGameTime;

            if (currentKeyboard.IsKeyDown(Keys.P) && oldKeyboard.IsKeyUp(Keys.P))
            {
                MediaPlayer.Pause();
                gameState = GameState.PauseScreen;
                return;
            }

            if (numPlayers == 1)
            {
                cam.Pos = center;
            }

            // spawn new trash
            Random rand = new Random();
            Vector2 trashSpawnPosition;
            double trashSpawnTypeRandomizer;
            int trashSpawnType;
            Entity theNewTrash;
            if ((rand.Next(150) == 1) /*&& (entityList.Count() <= 30)*/)
            {
                trashSpawnPosition = new Vector2(rand.Next(theStage.bounds.X, theStage.bounds.X + theStage.bounds.Width), rand.Next(theStage.bounds.Y, theStage.bounds.Y + theStage.bounds.Height));
                trashSpawnTypeRandomizer = rand.NextDouble();
                if (trashSpawnTypeRandomizer <= theStage.trashSpawnRateYellow)
                {
                    trashSpawnType = (int)Types.TrashYellow;
                }
                else
                {
                    trashSpawnTypeRandomizer -= theStage.trashSpawnRateYellow;
                    if (trashSpawnTypeRandomizer <= theStage.trashSpawnRateRed)
                    {
                        trashSpawnType = (int)Types.TrashRed;
                    }
                    else
                    {
                        trashSpawnTypeRandomizer -= theStage.trashSpawnRateRed;
                        if (trashSpawnTypeRandomizer <= theStage.trashSpawnRateBlue)
                        {
                            trashSpawnType = (int)Types.TrashBlue;
                        }
                        else
                        {
                            trashSpawnTypeRandomizer -= theStage.trashSpawnRateBlue;
                            if (trashSpawnTypeRandomizer <= theStage.trashSpawnRateGreen)
                            {
                                trashSpawnType = (int)Types.TrashGreen;
                            }
                            else
                            {
                                trashSpawnTypeRandomizer -= theStage.trashSpawnRateGreen;
                                if (trashSpawnTypeRandomizer <= theStage.trashSpawnRatePurple)
                                {
                                    trashSpawnType = (int)Types.TrashPurple;
                                }
                                else
                                {
                                    trashSpawnType = (int)Types.TrashBlack;
                                }
                            }
                        }
                    }
                }
                theNewTrash = new Entity(trashSpawnType, trashSpawnPosition);
                entityList.Add(theNewTrash);
            }

            foreach (SnakePlayer player in players)
            {

                if (currentKeyboard.IsKeyDown(player.leftKey) && currentKeyboard.IsKeyUp(player.rightKey))
                {
                    player.direction -= 0.10f;
                    player.direction %= 2.0f * (float)Math.PI;
                }
                if (currentKeyboard.IsKeyDown(player.rightKey) && currentKeyboard.IsKeyUp(player.leftKey))
                {
                    player.direction += 0.10f;
                    player.direction %= 2.0f * (float)Math.PI;
                }
                if (currentKeyboard.IsKeyDown(player.scrollKey) && oldKeyboard.IsKeyUp(player.scrollKey))
                {
                    int startedWith = player.selectedPowerup;
                    do
                    {
                        player.selectedPowerup = (player.selectedPowerup + 1) % (player.bodyTypes.Count());
                    } while ((player.selectedPowerup != startedWith) && (player.bodyTypes[player.selectedPowerup] == (int)Types.BagBlack));
                }
                if (currentKeyboard.IsKeyDown(player.activateKey) && oldKeyboard.IsKeyUp(player.activateKey))
                {
                    player.activatePowerup();
                }

                if (currentKeyboard.IsKeyDown(Keys.F) && oldKeyboard.IsKeyUp(Keys.F))
                {
                    player.fireActive = !player.fireActive;
                    player.powerupRemainingTime = new TimeSpan(0, 1, 0);
                }


                if (player.fireActive)
                {
                    /*for (int i = 0; i < entityList.Count(); i++)
                    {
                        if (player.fireRectangle.Intersects(entityList[i].bounds))
                        {
                            if (((int)Types.BagYellow <= entityList[i].type) && (entityList[i].type <= (int)Types.BagBlack))
                            {
                                entityList.RemoveAt(i);
                            }
                        }
                    }
                    foreach (SnakePlayer otherPlayer in players)
                    {
                        if (!otherPlayer.Equals(player))
                        {
                            if (player.fireRectangle.Intersects(otherPlayer.body[0].bounds))
                            {
                                otherPlayer.diesThisRound = true;
                            }
                        }
                    }*/
                    for (int i = 1; i < 5; i++)
                    {
                        Rectangle testingThisSpace = new Rectangle(
                            (int)(player.body[0].position.X + i * Constants.BODIES_BETWEEN_BAGS * Constants.DEFAULT_SPEED * (float)Math.Cos((double)player.direction)),
                            (int)(player.body[0].position.Y + i * Constants.BODIES_BETWEEN_BAGS * Constants.DEFAULT_SPEED * (float)Math.Sin((double)player.direction)),
                            (int)1,
                            (int)1);
                        for (int j = 0; j < entityList.Count(); j++)
                        {
                            if (testingThisSpace.Intersects(entityList[j].bounds))
                            {
                                if (((int)Types.BagYellow <= entityList[j].type) && (entityList[j].type <= (int)Types.BagBlack))
                                {
                                    entityList.RemoveAt(j);
                                }
                            }
                        }
                        foreach (SnakePlayer otherPlayer in players)
                        {
                            if (!otherPlayer.Equals(player))
                            {
                                if (testingThisSpace.Intersects(otherPlayer.body[0].bounds))
                                {
                                    otherPlayer.diesThisRound = true;
                                }
                            }
                        }
                    }
                }

                if (player.powerupRemainingTime != null)
                {
                    player.powerupRemainingTime -= gameTime.ElapsedGameTime;
                    if (player.powerupRemainingTime.TotalSeconds <= 1)
                    {
                        player.speed = Constants.DEFAULT_SPEED;
                        player.fireActive = false;
                        //player.bodiesBetweenBags = Constants.BODIES_BETWEEN_BAGS;
                        //player.powerupRemainingTime = null;
                    }
                }

                //if (currentKeyboard.IsKeyDown(Keys.Space))//
                //{//
                player.body.Insert(0, new SnakeBody(new Vector2(player.body[0].position.X + player.speed * (float)Math.Cos((double)player.direction), player.body[0].position.Y + player.speed * (float)Math.Sin((double)player.direction)), 0));
                if (player.body.Count() > Constants.BODIES_BETWEEN_BAGS * player.bodyTypes.Count() + 1)
                {
                    player.body.RemoveAt(player.body.Count() - 1);
                }
                //}//
                /*player.fireRectangle = new RotatedRectangle(new Rectangle(
                    (int)(player.body[0].position.X + Constants.BODIES_BETWEEN_BAGS * Constants.DEFAULT_SPEED * Math.Cos((double)player.direction)),
                    (int)(player.body[0].position.Y + Constants.BODIES_BETWEEN_BAGS * Constants.DEFAULT_SPEED * Math.Sin((double)player.direction)),
                    textureList[(int)Cleanup.Types.Fire].Width,
                    textureList[(int)Cleanup.Types.Fire].Height),
                        player.direction,
                        player.body[0].origin);*/

                for (int i = 0; i < entityList.Count(); i++)
                {
                    if (player.body[0].bounds.Intersects(entityList[i].bounds))
                        player.CollidedWith(entityList[i]);
                }
                for (int i = 0; i < numPlayers; i++)
                {
                    for (int j = (players[i] == player ? 25 : 0); j < players[i].body.Count(); j++)
                    {
                        if (player.body[0].bounds.Intersects(players[i].body[j].bounds))
                        {
                            player.CollidedWith(new Entity(1, players[i].body[j].position));
                        }
                    }
                }

                //if (currentKeyboard.IsKeyDown(Keys.G) && oldKeyboard.IsKeyUp(Keys.G))
                //{
                //    player.bodyTypes.Add((int)Types.BagBlack);
                //}

            }

            foreach (SnakePlayer player in players)
            {
                if (player.diesThisRound)
                {
                    player.respawn();
                    //cam.Pos = center;
                }
            }

            if (matchTimeRemaining.TotalSeconds <= 0)
            {
                /*for (int i = 0; i < numPlayers; i++)
                {
                    if (players[i].score > players[winner].score)
                    {
                        winner = i;
                    }
                }*/
                if (numPlayers > 1)
                {
                    winner = (players[0].score > players[1].score) ? 1 : 2;
                    winner = (players[0].score == players[1].score) ? 3 : winner;
                }
                garbageAtEnd = entityList.Count(entity => (((int)Types.BagYellow <= entity.type) && (entity.type <= (int)Types.TrashBlack)));
                
                backgroundMusic = Content.Load<Song>("music/Rest A While");
                songStart = false;
                MediaPlayer.IsRepeating = false;
                gameState = GameState.WinnerScreen;
            }
        }

        void UpdatePauseScreen(GameTime gameTime)
        {
            if (currentKeyboard.IsKeyDown(Keys.P) && oldKeyboard.IsKeyUp(Keys.P))
            {
                MediaPlayer.Resume();
                gameState = GameState.MainGame;
            }
        }

        void UpdateWinnerScreen(GameTime gameTime)
        {
            if (currentKeyboard.IsKeyDown(Keys.Enter) && oldKeyboard.IsKeyUp(Keys.Enter))
            {
                this.Exit();
            }
            if (currentKeyboard.IsKeyDown(Keys.Space) && oldKeyboard.IsKeyUp(Keys.Space))
            {
                this.Exit();
            }
        }


        protected override void Draw(GameTime gameTime)
        {
            if (gameState == GameState.TitleScreen)
            {
                DrawTitleScreen(gameTime);
            }
            else if (gameState == GameState.StageSelect)
            {
                DrawStageSelectScreen(gameTime);
            }
            else if (gameState == GameState.InstructionScreen)
            {
                DrawInstructionScreen(gameTime);
            }
            else if (gameState == GameState.MainGame)
            {
                DrawMainGame(gameTime);
            }
            else if (gameState == GameState.PauseScreen)
            {
                DrawMainGame(gameTime);
            }
            else if (gameState == GameState.WinnerScreen)
            {
                DrawWinnerScreen(gameTime);
            }

            base.Draw(gameTime);
        }

        void DrawTitleScreen(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Green);
            Viewport viewport = graphics.GraphicsDevice.Viewport;

            spriteBatch.Begin();

            spriteBatch.Draw(titleScreenImage, new Rectangle(0, 0, viewport.Width, viewport.Height), Color.White);

            spriteBatch.DrawString(spritefont, "1-player game", new Vector2(100, 200), (menuChoice == 0) ? Color.Yellow : Color.White);
            spriteBatch.DrawString(spritefont, "2-player game", new Vector2(100, 250), (menuChoice == 1) ? Color.Yellow : Color.White);
            spriteBatch.DrawString(spritefont, "How To Play", new Vector2(100, 300), (menuChoice == 2) ? Color.Yellow : Color.White);
            spriteBatch.DrawString(spritefont, "Quit", new Vector2(100, 350), (menuChoice == 3) ? Color.Yellow : Color.White);
            spriteBatch.DrawString(spritefont, ">", new Vector2(80, menuChoice * 50 + 200), Color.Yellow);

            spriteBatch.End();
        }

        void DrawStageSelectScreen(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Green);
            Viewport viewport = graphics.GraphicsDevice.Viewport;

            spriteBatch.Begin();

            spriteBatch.Draw(stageSelectScreenImage, new Rectangle(0, 0, viewport.Width, viewport.Height), Color.White);

            spriteBatch.DrawString(spritefont, "Choose your level", new Vector2(325, 100), Color.White);

            for (int i = 0; i < stages.Count(); i++)
            {
                spriteBatch.DrawString(spritefont, stages[i], new Vector2(100, 200 + 25 * i), (menuChoice == i) ? Color.Yellow : Color.White);
            }

            spriteBatch.End();
        }

        void DrawInstructionScreen(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Green);
            Viewport viewport = graphics.GraphicsDevice.Viewport;

            spriteBatch.Begin();

            spriteBatch.Draw(instructionScreenImage, new Rectangle(0, 0, viewport.Width, viewport.Height), Color.White);

            spriteBatch.Draw(textureList[(int)Types.TrashGreen], new Vector2(20 * GraphicsDevice.Viewport.Width / 800, 450 * GraphicsDevice.Viewport.Height / 800), Color.White);
            spriteBatch.Draw(textureList[(int)Types.TrashRed], new Vector2(20 * GraphicsDevice.Viewport.Width / 800, 550 * GraphicsDevice.Viewport.Height / 800), Color.White);
            spriteBatch.Draw(textureList[(int)Types.TrashYellow], new Vector2(20 * GraphicsDevice.Viewport.Width / 800, 650 * GraphicsDevice.Viewport.Height / 800), Color.White);
            spriteBatch.DrawString(spritefont, "= SPEED BOOST", new Vector2(130 * GraphicsDevice.Viewport.Width / 800, 480 * GraphicsDevice.Viewport.Height / 800), Color.Black);
            spriteBatch.DrawString(spritefont, "= SLOW DOWN", new Vector2(130 * GraphicsDevice.Viewport.Width / 800, 580 * GraphicsDevice.Viewport.Height / 800), Color.Black);
            spriteBatch.DrawString(spritefont, "= FLAMETHROWER", new Vector2(130 * GraphicsDevice.Viewport.Width / 800, 680 * GraphicsDevice.Viewport.Height / 800), Color.Black);

            spriteBatch.End();
        }

        void DrawMainGame(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Green);
            Viewport viewport = graphics.GraphicsDevice.Viewport;

            spriteBatch.Begin(SpriteSortMode.BackToFront,
                        BlendState.AlphaBlend,
                        SamplerState.LinearWrap,
                        DepthStencilState.Default,
                        RasterizerState.CullNone,
                        null,
                        cam.get_transformation(GraphicsDevice));

            spriteBatch.Draw(backgroundTile, new Vector2(-10000, -10000), tileBackgroundBounds, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);


            foreach (Entity entity in entityList)
            {
                spriteBatch.Draw(textureList[entity.type], entity.bounds, Color.White);
            }

            foreach (SnakePlayer player in players)
            {
                player.Draw(spriteBatch);
            }

            spriteBatch.End();

            /* HUD */
            spriteBatch.Begin();
            for (int i = 0; i < players.Count(); i++)
            {
                spriteBatch.DrawString(spritefont, "P"+(i+1).ToString()+": " + players[i].score.ToString(), new Vector2(i*(viewport.Width / players.Count()), 0), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
                spriteBatch.Draw(textureList[(int)Types.TranslucentBox], new Vector2(i * (viewport.Width / players.Count()), 20), Color.White);
                if (((int)Types.BagYellow <= players[i].bodyTypes[players[i].selectedPowerup]) && (players[i].bodyTypes[players[i].selectedPowerup] <= (int)Types.BagBlack))
                {
                    spriteBatch.Draw(textureList[players[i].bodyTypes[players[i].selectedPowerup]], new Vector2(i * (viewport.Width / players.Count())+1, 21), Color.White);
                }
            }
            spriteBatch.DrawString(spritefont, matchTimeRemaining.Minutes.ToString() + ":" + matchTimeRemaining.Seconds.ToString("D2"), new Vector2(viewport.Width / 2 - 20, 20), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
            if (matchStartsIn.TotalSeconds > 1)
            {
                spriteBatch.DrawString(spritefont, "0:" + matchStartsIn.Seconds.ToString("D2"), new Vector2(viewport.Width / 2 - 20, 300), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
                if (players.Count() > 1)
                {
                    spriteBatch.DrawString(spritefont, "P2: WASD", new Vector2(200 * GraphicsDevice.Viewport.Width / 800, 450 * GraphicsDevice.Viewport.Height / 800), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
                    spriteBatch.DrawString(spritefont, "P1: ^<v>", new Vector2(530 * GraphicsDevice.Viewport.Width / 800, 450 * GraphicsDevice.Viewport.Height / 800), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
                }
            }
            if (gameState == GameState.PauseScreen)
            {
                spriteBatch.DrawString(spritefont, "PAUSED", new Vector2(viewport.Width / 2 - 30, 300), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
            }
            spriteBatch.End();

        }

        void DrawWinnerScreen(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            Viewport viewport = graphics.GraphicsDevice.Viewport;

            spriteBatch.Begin();

            for (int i = 0; i < numPlayers; i++)
            {
                spriteBatch.DrawString(spritefont, "Player " + (i + 1).ToString() + " score: " + players[i].score.ToString(), new Vector2(300 * GraphicsDevice.Viewport.Width / 800, (i * 25 + 300) * GraphicsDevice.Viewport.Height / 800), Color.White);
            }
            if (numPlayers > 1)
            {
                if (winner == 3)
                {
                    spriteBatch.DrawString(spritefont, "IT'S A TIE!", new Vector2(340 * GraphicsDevice.Viewport.Width / 800, 500 * GraphicsDevice.Viewport.Height / 800), Color.White);
                }
                else
                {
                    spriteBatch.DrawString(spritefont, "THE WINNER IS PLAYER " + (winner).ToString() + "!", new Vector2(300 * GraphicsDevice.Viewport.Width / 800, 500 * GraphicsDevice.Viewport.Height / 800), Color.White);
                }
            }
            if (garbageAtEnd > 10)
            {
                spriteBatch.DrawString(spritefont, ((numPlayers > 1) ? "BUT" : "") + " YOU FAILED TO SAVE THE ENVIRONMENT", new Vector2(250 * GraphicsDevice.Viewport.Width / 800, 520 * GraphicsDevice.Viewport.Height / 800), Color.White);
            }

            spriteBatch.End();
        }

    }
}