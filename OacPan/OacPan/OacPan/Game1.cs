using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace OacPan
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D mapTexture;
        Texture2D pacMan;
        Point prevDir = new Point (0,0);
        List<Ghost> ghosts;

        int[][] map = new int[][]{
                         new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                         new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                         new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                         new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                         new int[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                         new int[] { 1, 0, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 0, 1},
                         new int[] { 1, 0, 1, 0, 0, 1, 0, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 0, 0, 0, 1, 0, 1, 1, 1, 1, 0, 1},
                         new int[] { 1, 0, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 0, 1},
                         new int[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                         new int[] { 1, 0, 1, 1, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, 0, 1},
                         new int[] { 1, 0, 1, 1, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, 0, 1},
                         new int[] { 1, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1},
                         new int[] { 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1},
                         new int[] { 0, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0},
                         new int[] { 0, 0, 0, 0, 0, 1, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0},
                         new int[] { 0, 0, 0, 0, 0, 1, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0},
                         new int[] { 1, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1},
                         new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                         new int[] { 1, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1},
                         new int[] { 0, 0, 0, 0, 0, 1, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0},
                         new int[] { 0, 0, 0, 0, 0, 1, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0},
                         new int[] { 0, 0, 0, 0, 0, 1, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0},
                         new int[] { 1, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1},
                         new int[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                         new int[] { 1, 0, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 0, 1},
                         new int[] { 1, 0, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 0, 1},
                         new int[] { 1, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 1},
                         new int[] { 1, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 1},
                         new int[] { 1, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 1},
                         new int[] { 1, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1},
                         new int[] { 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1},
                         new int[] { 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1},
                         new int[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                         new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                         new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                         new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}};

        int currDirection = -1;
        Point pacManLoc;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 224;
            graphics.PreferredBackBufferHeight = 288;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            pacManLoc.X = 112;
            pacManLoc.Y = 212;

            ghosts = new List<Ghost>(4);
            //ghosts.Add(new Ghost(Ghost.GhostType.Blinky));
            ghosts.Add(new Ghost(Ghost.GhostType.Pinky));


            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            mapTexture = Content.Load<Texture2D>("Map");
            pacMan = Content.Load<Texture2D>("PacMan");

            foreach (Ghost g in ghosts)
            {
                g.LoadTexture(Content);
            }

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            int dir = 0;
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                dir = 1;

            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                dir = 2;


            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                dir = 3;

            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                dir = 4;


            Point pmapLoc = GetMapLoc(pacManLoc,0,0);

            switch (dir)
            {
                case 1:
                    if (map[pmapLoc.Y - 1][pmapLoc.X] == 0)
                    {
                        prevDir.Y = -1;
                        prevDir.X = 0;
                    }
                    break;

                case 2:
                    if (map[pmapLoc.Y][pmapLoc.X + 1] == 0)
                    {
                        prevDir.Y = 0;
                        prevDir.X = 1;
                    }
                    break;

                case 3:
                    if (map[pmapLoc.Y + 1][pmapLoc.X] == 0)
                    {
                        prevDir.Y = 1;
                        prevDir.X = 0;
                    }
                    break;

                case 4:
                    if (map[pmapLoc.Y][pmapLoc.X - 1] == 0)
                    {
                        prevDir.Y = 0;
                        prevDir.X = -1;
                    }
                    break;
            }

            pmapLoc = GetMapLoc(pacManLoc,prevDir.X, prevDir.Y);

            if ((prevDir.X != 0 || prevDir.Y != 0) && map[pmapLoc.Y][pmapLoc.X] == 0)
            {

                pacManLoc.X += prevDir.X;
                pacManLoc.Y += prevDir.Y;

                if (prevDir.X == 0)
                {
                    int Xloc = (pacManLoc.X / 8)* 8 + 4;
                    pacManLoc.X -= pacManLoc.X - Xloc;
                }
                else if (prevDir.Y == 0)
                {
                    int Yloc = (pacManLoc.Y / 8) * 8 + 4;
                    pacManLoc.Y -= pacManLoc.Y - Yloc;
                }
            }

            foreach (Ghost g in ghosts)
            {
                GhostMovement(g);
            }


            base.Update(gameTime);
        }

        protected Point GetMapLoc(Point curLoc, int xOffset, int yOffset)
        {
            Point loc = new Point(-1, -1);

            loc.X = (curLoc.X + xOffset) / 8;
            loc.Y = (curLoc.Y + yOffset) / 8;
            return loc;
        }

        protected void GhostMovement(Ghost g)
        {
            Point targetLoc = new Point(-1, -1);
            Point mapLoc = GetMapLoc(g.loc, 0, 0);
            if (g.loc.X - (mapLoc.X * 8 + 4) == 0 && g.loc.Y - (mapLoc.Y * 8 + 4) == 0)
            {
                switch (g.mode)
                {
                    case Ghost.GhostMode.Running:
                        switch (g.type)
                        {
                            case Ghost.GhostType.Blinky:
                                targetLoc = GetMapLoc(pacManLoc,0, 0);
                                break;

                            case Ghost.GhostType.Inky:
                                Ghost Blinky= null;
                                foreach (Ghost gs in ghosts)
                                    if (gs.type == Ghost.GhostType.Blinky)
                                    {
                                        Blinky = gs;
                                        break;
                                    }

                                if (Blinky != null)
                                {
                                    Point blinkLoc = GetMapLoc(Blinky.loc, 0, 0);
                                    targetLoc = GetMapLoc(pacManLoc, prevDir.X * 2, prevDir.Y * 2);
                                    targetLoc.X = targetLoc.X + (targetLoc.X - blinkLoc.X);
                                    targetLoc.Y = targetLoc.Y + (targetLoc.Y - blinkLoc.Y);
                                }
                                else
                                {
                                    targetLoc = g.scatterLoc;
                                }
                                break;

                            case Ghost.GhostType.Pinky:
                                targetLoc = GetMapLoc(pacManLoc, prevDir.X * 4, prevDir.Y * 4);
                                break;

                            case Ghost.GhostType.Clyde:
                                Point clydeLoc = GetMapLoc(g.loc, 0, 0);
                                targetLoc = GetMapLoc(pacManLoc, 0, 0);
                                if ((targetLoc.X - clydeLoc.X) * (targetLoc.X - clydeLoc.X) + (targetLoc.Y - clydeLoc.Y) * (targetLoc.Y - clydeLoc.Y) < 16)
                                    targetLoc = g.scatterLoc;
                                break;
                        }
                        break;

                    case Ghost.GhostMode.CenterPen:
                        g.mode = Ghost.GhostMode.Running;
                        targetLoc = GetMapLoc(new Point(112,120), 0, 0);
                        break;

                    case Ghost.GhostMode.OutPen:
                        g.mode = Ghost.GhostMode.Running;
                        targetLoc = GetMapLoc(pacManLoc, 0, 0);
                        break;

                    case Ghost.GhostMode.Pen:
                        targetLoc = GetMapLoc(g.loc, 0, 0);
                        break;
                }

                int minDistSqr = Int32.MaxValue;
                int distSqr = 0;
                int dir = 0;
                if (map[mapLoc.Y - 1][mapLoc.X] == 0 && g.lastDir.Y != 1)
                {
                    distSqr = (targetLoc.X - mapLoc.X) * (targetLoc.X - mapLoc.X) + (targetLoc.Y - (mapLoc.Y - 1)) * (targetLoc.Y - (mapLoc.Y - 1));
                    if (distSqr < minDistSqr)
                    {
                        minDistSqr = distSqr;
                        dir = 1;
                    }
                }

                if (map[mapLoc.Y][mapLoc.X + 1] == 0 && g.lastDir.X != -1)
                {
                    distSqr = (targetLoc.X - (mapLoc.X + 1)) * (targetLoc.X - (mapLoc.X + 1)) + (targetLoc.Y - mapLoc.Y ) * (targetLoc.Y - mapLoc.Y);
                    if (distSqr < minDistSqr)
                    {
                        minDistSqr = distSqr;
                        dir = 2;
                    }
                }

                if (map[mapLoc.Y + 1][mapLoc.X] == 0 && g.lastDir.Y != -1)
                {
                    distSqr = (targetLoc.X - mapLoc.X) * (targetLoc.X - mapLoc.X) + (targetLoc.Y - (mapLoc.Y + 1)) * (targetLoc.Y - (mapLoc.Y + 1));
                    if (distSqr < minDistSqr)
                    {
                        minDistSqr = distSqr;
                        dir = 3;
                    }
                }

                if (map[mapLoc.Y][mapLoc.X - 1] == 0 && g.lastDir.X != 1)
                {
                    distSqr = (targetLoc.X - (mapLoc.X - 1)) * (targetLoc.X - (mapLoc.X - 1)) + (targetLoc.Y - mapLoc.Y) * (targetLoc.Y - mapLoc.Y);
                    if (distSqr < minDistSqr)
                    {
                        minDistSqr = distSqr;
                        dir = 4;
                    }
                }

                switch (dir)
                {
                    case 1:
                        g.lastDir.Y = -1;
                        g.lastDir.X = 0;
                        break;

                    case 2:
                        g.lastDir.Y = 0;
                        g.lastDir.X = 1;
                        break;

                    case 3:
                        g.lastDir.Y = 1;
                        g.lastDir.X = 0;
                        break;

                    case 4:
                        g.lastDir.Y = 0;
                        g.lastDir.X = -1;
                        break;
                }
            }

            g.loc.Y += g.lastDir.Y;
            g.loc.X += g.lastDir.X;

        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            spriteBatch.Draw(mapTexture, new Rectangle(0, 0, 224, 288), Color.White);

            spriteBatch.Draw(pacMan, new Rectangle(pacManLoc.X, pacManLoc.Y, 16, 16), Color.White);

            foreach (Ghost g in ghosts)
            {
                spriteBatch.Draw(g.tex, new Rectangle(g.loc.X, g.loc.Y, 16, 16), Color.White);
            }

            spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }

    public class Ghost
    {
        public enum GhostType { Blinky, Inky, Pinky, Clyde };
        public enum GhostMode { Pen, CenterPen, OutPen, LeavePen, Running };
        public Point loc;
        public Point lastDir;
        public GhostType type;
        public Texture2D tex;
        public Point scatterLoc;
        public GhostMode mode;

        public Ghost()
        {
            loc.Y = 40;
            loc.X = 16;

            lastDir.X = 0;
            lastDir.Y = 0;
            tex = null;
            type = GhostType.Blinky;

            scatterLoc.Y = 4;
            scatterLoc.X = 204;

            mode = GhostMode.Pen;


            int test = 1;
        }

        public Ghost(GhostType t)
        {
            loc.Y = 44;
            loc.X = 20;

            lastDir.X = 0;
            lastDir.Y = 0;
            tex = null;
            type = t;

            switch (t)
            {
                case GhostType.Blinky:
                    scatterLoc.Y = 4;
                    scatterLoc.X = 204;

                    mode = GhostMode.Running;
                    break;

                case GhostType.Inky:
                    scatterLoc.Y = 220;
                    scatterLoc.X = 276;

                    mode = GhostMode.Pen;
                    break;

                case GhostType.Pinky:
                    scatterLoc.Y = 4;
                    scatterLoc.X = 20;

                    mode = GhostMode.CenterPen;
                    break;

                case GhostType.Clyde:
                    scatterLoc.Y = 4;
                    scatterLoc.X = 276;

                    mode = GhostMode.Pen;
                    break;
            }
        }

        public void LoadTexture(ContentManager content)
        {
            switch (type)
            {
                case GhostType.Blinky:
                    tex = content.Load<Texture2D>("Blinky");
                    break;

                case GhostType.Inky:
                    tex = content.Load<Texture2D>("Inky");
                    break;

                case GhostType.Pinky:
                    tex = content.Load<Texture2D>("Pinky");
                    break;

                case GhostType.Clyde:
                    tex = content.Load<Texture2D>("Clyde");
                    break;
            }
        }
        
    }
}
