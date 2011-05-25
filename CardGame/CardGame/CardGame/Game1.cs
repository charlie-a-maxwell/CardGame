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
        MapView map;
        MouseHandler mouseHandler;
        List<CardType> cardTypes;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            cardTypes = new List<CardType>();
            map = new MapView();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            mouseHandler = new MouseHandler();
            mouseHandler.leftMouseDown += new MouseHandler.LeftMouseDown(leftMouseDown);

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

            map.SetGraphics(GraphicsDevice);
            map.SetContentManager(Content);

            XmlSerializer serializer = new XmlSerializer(typeof(List<CardType>));
            Stream cardTypeFile = File.Open("Content/CardTypes.xml", FileMode.Open);
            cardTypes = (List<CardType>)serializer.Deserialize(cardTypeFile);
            cardTypeFile.Close();

            LoadDecks();
            
            mouseHandler.SetTexture(Content.Load<Texture2D>("Cursor1"));

            foreach (CardType cc in cardTypes)
            {
                if (cc != null)
                    cc.LoadTexture(Content);
            }


            // TODO: use this.Content to load your game content here
        }

        protected void LoadDecks()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Stack<CardClass>));
            Stream deckstream = File.Open("Content/PlayerDeck1.xml", FileMode.Open);
            Stack<CardClass> stack = (Stack<CardClass>)serializer.Deserialize(deckstream);
            deckstream.Close();

            Deck deck = new Deck(PlayerTurn.Player1);
            deck.AddCards(stack);
            map.SetPlayerDeck(deck, PlayerTurn.Player1);

            deckstream = File.Open("Content/PlayerDeck2.xml", FileMode.Open);
            stack = (Stack<CardClass>)serializer.Deserialize(deckstream);
            deckstream.Close();

            deck = new Deck(PlayerTurn.Player2);
            deck.AddCards(stack);
            map.SetPlayerDeck(deck, PlayerTurn.Player2);
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

            // TODO: Add your update logic here
            mouseHandler.Update(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            spriteBatch.Begin();
            map.RenderMap(spriteBatch);

            mouseHandler.Render(spriteBatch);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void leftMouseDown(Vector2 pos)
        {
            map.HandleMouseClick(pos);
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
