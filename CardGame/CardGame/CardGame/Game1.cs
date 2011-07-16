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
using System.Xml.Serialization;
using System.IO;

namespace CardGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    /// 

    public enum PlayerTurn { Player1, Player2 };

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        MouseHandler mouseHandler;
        ScreenManager sm;
        private long lastKeyDown = 0;
        private const long keyRefresh = 200;
        Effect effect;
        RenderTarget2D cloudsRenderTarget;
        Texture2D cloudMap;
        Texture2D cloudStaticMap;
        Texture2D background;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            sm = new ScreenManager();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Screen.SetGraphics(GraphicsDevice);
            mouseHandler = new MouseHandler();
            mouseHandler.leftMouseDown += new MouseHandler.LeftMouseDown(sm.HandleMouseClick);
            spriteBatch = new SpriteBatch(GraphicsDevice);

            sm.AddScreen(new MapView("Map", GraphicsDevice));
            sm.AddScreen(new SplashScreen("SplashScreen"));

            sm.InitAll();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            sm.LoadContentAll(Content);

            CardClass.SetCircleText(Content.Load<Texture2D>("Circle"));
            mouseHandler.SetTexture(Content.Load<Texture2D>("Cursor1"));

            effect = Content.Load<Effect>("Effect");

            cloudsRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight, 1, SurfaceFormat.Vector4);
            cloudStaticMap = CreateStaticMap(32);
            cloudMap = new Texture2D(GraphicsDevice, cloudsRenderTarget.Width, cloudsRenderTarget.Height, 1, TextureUsage.None, cloudsRenderTarget.Format);
            background = Content.Load<Texture2D>("Cloud");
            // TODO: use this.Content to load your game content here
        }

        private Texture2D CreateStaticMap(int resolution)
        {
            Random rand = new Random();
            Color[] noisyColors = new Color[resolution * resolution];
            for (int x = 0; x < resolution; x++)
                for (int y = 0; y < resolution; y++)
                    noisyColors[x + y * resolution] = new Color(new Vector3((float)rand.Next(1000) / 1000.0f, 0, 0));

            Texture2D noiseImage = new Texture2D(GraphicsDevice, resolution, resolution, 1, TextureUsage.None, SurfaceFormat.Color);
            noiseImage.SetData(noisyColors);
            return noiseImage;
        }

        protected override void BeginRun()
        {
            base.BeginRun();
            sm.SetCurrentScreenByName("SplashScreen");

            sm.GetCurrentScreen().Init();
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

            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                int x = Mouse.GetState().X;
                int y = Mouse.GetState().Y;
            }

            if (lastKeyDown <= 0 && Keyboard.GetState().GetPressedKeys().Length > 0)
            {
                sm.HandleKeydown(Keyboard.GetState().GetPressedKeys());
                lastKeyDown = keyRefresh;
            }
            else if (lastKeyDown > 0)
                lastKeyDown -= (long)gameTime.ElapsedGameTime.TotalMilliseconds;

            // TODO: Add your update logic here
            mouseHandler.Update(gameTime);
            sm.Update(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // TODO: Add your drawing code here
            float time = (float)gameTime.TotalGameTime.TotalMilliseconds / 100.0f;
            GeneratePerlinNoise(time);


            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Blue, 1.0f, 0);

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            Screen.FillColor(spriteBatch, 0, 0, GraphicsDevice.DisplayMode.Width, GraphicsDevice.DisplayMode.Height, Color.DarkSlateGray);
            spriteBatch.Draw(cloudMap, Vector2.Zero, Color.White);
            spriteBatch.End();

            spriteBatch.Begin();
            sm.Render(spriteBatch, GraphicsDevice);
            mouseHandler.Render(spriteBatch);
            spriteBatch.End();
            
            base.Draw(gameTime);
        }

        private void GeneratePerlinNoise(float time)
        {
            GraphicsDevice.SetRenderTarget(0, cloudsRenderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Green, 1.0f, 0);

            effect.CurrentTechnique = effect.Techniques["PerlinNoise"];
            effect.Parameters["xOvercast"].SetValue(1.7f);
            effect.Parameters["xTime"].SetValue(time / 500.0f);
            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.None, Matrix.Identity);
            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                spriteBatch.Draw(cloudStaticMap, new Rectangle(0,0,GraphicsDevice.DisplayMode.Width, GraphicsDevice.DisplayMode.Height) , Color.White);
                pass.End();
            }
            spriteBatch.End();
            effect.End();


            GraphicsDevice.SetRenderTarget(0, null);
            cloudMap = cloudsRenderTarget.GetTexture();
        }
    }

    public class MouseHandler
    {
        private Vector2 pos;
        private Texture2D tex;
        private long lastMouseDown;
        private const long mouseRefresh = 200;
        public delegate void LeftMouseDown(Vector2 pos);

        public LeftMouseDown leftMouseDown = null;

        public MouseHandler()
        {
            pos = new Vector2(0, 0);
            tex = null;
            lastMouseDown = mouseRefresh;
        }

        public void Update(GameTime gameTime)
        {
            MouseState state = Mouse.GetState();
            this.pos.X = state.X;
            this.pos.Y = state.Y;
            if (state.LeftButton == ButtonState.Pressed && leftMouseDown != null && lastMouseDown < 0)
            {
                leftMouseDown(pos);
                lastMouseDown = mouseRefresh;
            }
            else
                lastMouseDown -= (long)gameTime.ElapsedGameTime.TotalMilliseconds;
                
        }

        public void SetTexture(Texture2D t)
        {
            tex = t;
        }

        public void Render(SpriteBatch sb)
        {
            if (tex != null)
                sb.Draw(tex, pos, Color.White);
            //               sb.Draw(tex, pos, null, Color.White, 0.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, 1.0f);
      
        }
    }
}
