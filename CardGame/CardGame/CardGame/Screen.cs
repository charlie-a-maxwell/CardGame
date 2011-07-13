using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;


namespace CardGame
{
    abstract class Screen
    {
        protected static Texture2D lineTex;
        protected static SpriteFont font = null;
        protected string name;
        protected ScreenManager manager;

        public Screen()
        {
            name = "unknown";
        }

        public Screen(string n)
        {
            name = n;
        }

        public void SetScreenManager(ScreenManager sm)
        {
            manager = sm;
        }

        public virtual void LoadContent(ContentManager cm)
        {
            if (font == null)
                font = cm.Load<SpriteFont>("CourierNew");
        }

        public static void SetGraphics(GraphicsDevice gd)
        {
            lineTex = new Texture2D(gd, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
            lineTex.SetData(new[] { Color.White });
        }

        public static void DrawLine(SpriteBatch sb, Vector2 p1, Vector2 p2, Color c)
        {
            DrawLine(sb, p1, p2, c, 2.0f);
        }

        public static void DrawLine(SpriteBatch sb, Vector2 p1, Vector2 p2, Color c, float width)
        {
            float angle = (float)Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
            float length = Vector2.Distance(p1, p2);

            sb.Draw(lineTex, p1, null, c, angle, Vector2.Zero, new Vector2(length, width), SpriteEffects.None, 0.9f);
        }

        public static void DrawText(SpriteBatch sb, string s, Vector2 loc)
        {
            DrawText(sb, s, loc, Color.Black, 1.0f);
        }

        public static void DrawText(SpriteBatch sb, string s, Vector2 loc, Color c, float scale)
        {
            if (font == null) return;
            Vector2 fontOrigin = font.MeasureString(s);
            sb.DrawString(font, s, loc + fontOrigin / 2, c, 0, fontOrigin / 2, scale, SpriteEffects.None, 0.0f);
        }

        public static void FillColor(SpriteBatch sb, int originX, int originY, int width, int height, Color color)
        {
            if (lineTex == null) return;
            sb.Draw(lineTex, new Rectangle(originX, originY, width, height), color);
        }

        public virtual void Render(SpriteBatch sb, GraphicsDevice device)
        {
        }

        public virtual void HandleMouseClick(Vector2 pos)
        {
        }

        public virtual void HandleKeydown(Keys[] k)
        {
        }

        public virtual bool Init()
        {
            return true;
        }

        public void Update(GameTime gt)
        {

        }

        public string GetName()
        {
            return name;
        }
    }

    class SplashScreen : Screen
    {
        public SplashScreen()
            : base()
        {
        }

        public SplashScreen(string s)
            : base(s)
        {
        }

        public override bool Init()
        {
            return true;
        }

        public override void HandleMouseClick(Vector2 pos)
        {
            if (manager != null)
            {
                manager.SetCurrentScreenByName("Map");
                (manager.GetCurrentScreen() as MapView).StartGame();
            }
        }

        public override void HandleKeydown(Keys[] k)
        {
            try
            {
                foreach (Keys t in k)
                {
                    switch (t)
                    {
                        case Keys.Enter:
                            if (manager != null)
                            {
                                manager.SetCurrentScreenByName("Map");
                                (manager.GetCurrentScreen() as MapView).StartGame();
                            }
                            break;
                    }
                }
            }
            catch (Exception e){ }
        }

        public override void LoadContent(ContentManager cm)
        {
            
        }

        public override void Render(SpriteBatch sb, GraphicsDevice device)
        {
            DrawText(sb, "Test Splash Screen", new Vector2(device.Viewport.Width / 2 - 40, device.Viewport.Height / 2), Color.White, 2.0f);
            DrawText(sb, "Click or press Enter to start.", new Vector2(device.Viewport.Width / 2 - 130, device.Viewport.Height / 2 + 20), Color.White, 2.0f);
        }
    }

    class ScreenManager
    {
        List<Screen> screens;
        Screen currentScreen;

        public ScreenManager()
        {
            screens = new List<Screen>();
            currentScreen = null;
        }

        public void Render(SpriteBatch sb, GraphicsDevice device)
        {
            if (currentScreen != null)
                currentScreen.Render(sb, device);
        }

        public void Update(GameTime gt)
        {
            if (currentScreen != null)
                currentScreen.Update(gt);
        }

        public void AddScreen(Screen s)
        {
            s.SetScreenManager(this);
            screens.Add(s);
        }

        public void InitAll()
        {
            foreach (Screen s in screens)
                s.Init();
        }

        public void LoadContentAll(ContentManager c)
        {
            foreach (Screen s in screens)
                s.LoadContent(c);
        }

        public void SetCurrentScreenByName(string n)
        {
            foreach (Screen s in screens)
            {
                if (s.GetName() == n)
                {
                    currentScreen = s;
                }
            }
        }

        public void HandleMouseClick(Vector2 pos)
        {
            if (currentScreen != null)
                currentScreen.HandleMouseClick(pos);
        }

        public Screen GetCurrentScreen()
        {
            return currentScreen;
        }

        public void HandleKeydown(Keys[] k)
        {
            if (currentScreen != null)
                currentScreen.HandleKeydown(k);
        }
    }
}
