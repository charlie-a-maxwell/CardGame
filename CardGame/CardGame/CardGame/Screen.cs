using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using System.Xml.Serialization;
using System.IO;

namespace CardGame
{
    public abstract class ScreenObject
    {
        protected Vector2 loc;
        protected Screen parent;

        public ScreenObject(Vector2 l, Screen p)
        {
            parent = p;
            loc = l;
        }

        public void SetLoc(Vector2 l)
        {
            loc = l;
        }

        public abstract bool TestClick(Vector2 point);

        public abstract void LoadContent(ContentManager cm);

        public abstract void Render(SpriteBatch sb);
    }

    public delegate void OnClick(object sender);

    public class Button : ScreenObject
    {
        protected string text;
        protected float width, height;
        public bool selected;
        public event OnClick handler;

        public Button(Vector2 l, Screen p, string t)
            : base(l, p)
        {
            handler = null;
            text = t;
        }

        public Button(Vector2 l, Screen p, string t, OnClick oc):base(l, p)
        {
            handler = oc;
            text = t;
        }

        public void Click(object sender)
        {
            if (handler != null)
                handler(sender);
        }

        public override void LoadContent(ContentManager cm)
        {
            Vector2 size = Screen.MeasureString(text);
            width = size.X + 4;
            height = size.Y + 4;
        }

        public override bool TestClick(Vector2 point)
        {
            return point.X > loc.X && point.X < loc.X + width && point.Y > loc.Y && point.Y < loc.Y + height;
        }

        public override void Render(SpriteBatch sb)
        {
            Screen.DrawLine(sb, loc, new Vector2(loc.X + width, loc.Y), Color.Black, height); 
            Screen.DrawText(sb, text, new Vector2(loc.X + 2, loc.Y + 2), Color.GhostWhite, 1.0f);
            if (selected)
            {
                Screen.DrawLine(sb, new Vector2(loc.X - 3, loc.Y - 3), new Vector2(loc.X + width + 3, loc.Y - 3), Color.White);
                Screen.DrawLine(sb, new Vector2(loc.X + width + 3, loc.Y - 3), new Vector2(loc.X + width + 3, loc.Y + height + 3), Color.White);
                Screen.DrawLine(sb, new Vector2(loc.X + width + 3, loc.Y + height + 3), new Vector2(loc.X - 3, loc.Y + height + 3), Color.White);
                Screen.DrawLine(sb, new Vector2(loc.X - 3, loc.Y + height + 3), new Vector2(loc.X - 3, loc.Y - 3), Color.White); 
            }
        }
    }

    public class TextBox : ScreenObject
    {
        protected String text;
        protected float width, height;
        public bool selected;

        public TextBox(Vector2 l, Screen p, String t)
            : base(l, p)
        {
            text = Screen.FormatTextToWidth(t, 100.0f);
        }

        public TextBox(Vector2 l, Screen p, String t, float w, float h)
            : base(l, p)
        {
            width = w;
            height = h;
            text = Screen.FormatTextToWidth(t, w);
        }

        public override void LoadContent(ContentManager cm)
        {
        }

        public override bool TestClick(Vector2 point)
        {
            return point.X > loc.X && point.X < loc.X + width && point.Y > loc.Y && point.Y < loc.Y + height;
        }

        public override void Render(SpriteBatch sb)
        {
            Screen.DrawUnalteredText(sb, text, loc, Color.Black, 1.0f);
        }
    }

    public abstract class Screen
    {
        protected static Texture2D lineTex;
        protected static SpriteFont font = null;
        protected string name;
        protected ScreenManager manager;
        protected List<ScreenObject> screenObjects;


        public Screen()
        {
            name = "unknown";
            screenObjects = new List<ScreenObject>();
        }

        public Screen(string n)
        {
            name = n;
            screenObjects = new List<ScreenObject>();
        }

        public void SetScreenManager(ScreenManager sm)
        {
            manager = sm;
        }

        public virtual void LoadContent(ContentManager cm)
        {
            if (font == null)
                font = cm.Load<SpriteFont>("CourierNew");

            foreach (ScreenObject so in screenObjects)
                so.LoadContent(cm);
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
            DrawLine(sb, p1, p2, c, width, 0.9f);
        }

        public static void DrawLine(SpriteBatch sb, Vector2 p1, Vector2 p2, Color c, float width, float z)
        {
            float angle = (float)Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
            float length = Vector2.Distance(p1, p2);

            sb.Draw(lineTex, p1, null, c, angle, Vector2.Zero, new Vector2(length, width), SpriteEffects.None, z);
        }

        public static void DrawText(SpriteBatch sb, string s, Vector2 loc)
        {
            DrawText(sb, s, loc, Color.Black, 1.0f, SpriteEffects.None);
        }

        public static void DrawText(SpriteBatch sb, string s, Vector2 loc, Color c, float scale)
        {
            DrawText(sb, s, loc, c, scale, SpriteEffects.None);
        }

        public static void DrawText(SpriteBatch sb, string s, Vector2 loc, Color c, float scale, SpriteEffects effect)
        {
            if (font == null) return;
            Vector2 fontOrigin = font.MeasureString(s);
            sb.DrawString(font, s, loc + fontOrigin / 2, c, 0, fontOrigin / 2, scale, effect, 0.0f);
        }

        public static void DrawUnalteredText(SpriteBatch sb, string s, Vector2 loc, Color c, float scale)
        {
            if (font == null) return;
            sb.DrawString(font, s, loc, c, 0, new Vector2(0,0), scale, SpriteEffects.None, 0.0f);
        }

        public static String FormatTextToWidth(String s, float width)
        {
            int lines = 0;
            return FormatTextToWidth(s, width, out lines);
        }

        public static String FormatTextToWidth(String s, float width, out int numLines)
        {
            String outString = String.Empty;
            String[] array = s.Split(' ');
            String line = String.Empty;

            foreach (String word in array)
            {
                if (font.MeasureString(line + word).Length() > width)
                {
                    outString = outString + line + '\n';
                    line = String.Empty;
                }
                line = line + word + ' ';
            }
            outString = outString + line;

            numLines = outString.Split('\n').Length;
            return outString;
        }

        public static Vector2 MeasureString(string s)
        {
            if (font == null) return new Vector2(0,0);
            return font.MeasureString(s);
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
            foreach (ScreenObject so in screenObjects)
            {
                if (so.TestClick(pos) && so is Button)
                {
                    (so as Button).Click(this);
                    break;
                }
            }
        }

        public virtual void HandleMouseMove(Vector2 pos)
        {
            foreach (ScreenObject so in screenObjects)
            {
                if (so.TestClick(pos) && so is Button)
                {
                    (so as Button).selected = true;
                }
                else if (so is Button)
                {
                    (so as Button).selected = false;
                }
            }
        }

        protected ScreenObject GetSelectedObject()
        {
            foreach (ScreenObject so in screenObjects)
                if (so is Button && (so as Button).selected)
                    return so;

            return null;
        }

        public virtual void HandleKeydown(Keys[] k)
        {
            try
            {
                foreach (Keys t in k)
                {
                    switch (t)
                    {
                        case Keys.Enter:
                            ScreenObject so = GetSelectedObject();
                            if (so != null && so is Button)
                                (so as Button).Click(this);
                            break;
                    }
                }
            }
            catch (Exception e) { }
        }

        public virtual bool Init()
        {
            return true;
        }

        public virtual void Update(GameTime gt)
        {

        }

        public string GetName()
        {
            return name;
        }
    }

    public class SplashScreen : Screen
    {
        Texture2D tex;

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
            screenObjects.Add(new Button(new Vector2(220, 475), this, "One Player", new OnClick(Player1_Click)));
            screenObjects.Add(new Button(new Vector2(340, 475), this, "Two Player", new OnClick(Player2_Click)));
            screenObjects.Add(new Button(new Vector2(460, 475), this, "Tutorial", new OnClick(Tutorial_Click)));
            return true;
        }

        public void Player1_Click(object sender)
        {
            if (manager != null)
            {
                manager.SetCurrentScreenByName("Map");
                (manager.GetCurrentScreen() as MapView).StartGame(GameType.Player1);
            }
        }


        public void Player2_Click(object sender)
        {
            if (manager != null)
            {
                manager.SetCurrentScreenByName("Map");
                (manager.GetCurrentScreen() as MapView).StartGame(GameType.Player2);
            }
        }

        public void Tutorial_Click(object sender)
        {
            if (manager != null)
            {
                manager.SetCurrentScreenByName("Tutorial");
            }
        }

        public override void LoadContent(ContentManager cm)
        {
            base.LoadContent(cm);
            tex = cm.Load<Texture2D>("SplashScreen");
        }

        public override void Render(SpriteBatch sb, GraphicsDevice device)
        {
            foreach (ScreenObject so in screenObjects)
            {
                so.Render(sb);
            }
            sb.Draw(tex, new Vector2(device.Viewport.Width / 2, device.Viewport.Height / 2), null, Color.White, 0.0f, new Vector2(tex.Width / 2.0f, tex.Height / 2.0f), 1.0f, SpriteEffects.None, 0.0f);
        }
    }

    public class ScreenManager
    {
        List<Screen> screens;
        Screen currentScreen;

        public ScreenManager()
        {
            screens = new List<Screen>();
            currentScreen = null;
        }

        public void Render(SpriteBatch sb, GraphicsDevice device, GameTime gt)
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

        public void HandleMouseMove(Vector2 pos)
        {
            if (currentScreen != null)
                currentScreen.HandleMouseMove(pos);
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

    public class TutorialInfo
    {
        public string TextureName;
        [XmlIgnore]
        public Texture2D image;
        public string text;
        public string Title;
    }

    public class TutorialScreen : Screen
    {
        Texture2D tex;
        int currIndex = 0;
        List<TutorialInfo> info;

        public TutorialScreen()
            : base()
        {
        }

        public TutorialScreen(string s)
            : base(s)
        {
        }

        public override bool Init()
        {
            info = new List<TutorialInfo>();

            screenObjects.Add(new Button(new Vector2(220, 475), this, "<- Previous", new OnClick(Previous_Click)));
            screenObjects.Add(new Button(new Vector2(540, 475), this, "Next ->", new OnClick(Next_Click)));

            if (File.Exists("Content/GameOptions.xml"))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<TutorialInfo>));
                Stream options = File.Open("Content/TutorialInfo.xml", FileMode.Open);
                info = (List<TutorialInfo>)serializer.Deserialize(options);
                options.Close();
            }

            return true;
        }

        public void Next_Click(object sender)
        {
            if (currIndex < info.Count-1)
                currIndex++;
        }


        public void Previous_Click(object sender)
        {
            if (currIndex == 0)
                manager.SetCurrentScreenByName("SplashScreen");
            else
                currIndex--;
        }


        public override void LoadContent(ContentManager cm)
        {
            base.LoadContent(cm);
            tex = cm.Load<Texture2D>("MapBack");
            foreach (TutorialInfo i in info)
            {
                if (i.TextureName.Length > 0)
                    i.image = cm.Load<Texture2D>(i.TextureName);
                else
                    i.image = null;
            }
        }

        public override void Render(SpriteBatch sb, GraphicsDevice device)
        {
            foreach (ScreenObject so in screenObjects)
            {
                so.Render(sb);
            }

            DrawText(sb, info[currIndex].Title, new Vector2(device.Viewport.Width / 2 - MeasureString(info[currIndex].Title).X /2, 30), Color.Black, 2.0f);

            int height = (int)MeasureString("A").Y;
            int lines = 0;
            string text = string.Empty;
            if (info[currIndex].image != null)
            {
                float ratio = 0.0f;
                if (info[currIndex].image.Width > info[currIndex].image.Height)
                    ratio = 400.0f / info[currIndex].image.Width;
                else
                    ratio = 200.0f / info[currIndex].image.Height;

                sb.Draw(info[currIndex].image, new Vector2(device.Viewport.Width / 2, device.Viewport.Height / 2 - 200), null, Color.White, 0.0f, new Vector2((info[currIndex].image.Width * ratio) / 2, (info[currIndex].image.Height * ratio) / 2), ratio, SpriteEffects.None, 0.0f);
                
                text = FormatTextToWidth(info[currIndex].text, 600, out lines);
                sb.Draw(tex, new Rectangle(device.Viewport.Width / 2 - 280, device.Viewport.Height / 2 - 5, 610, height * lines), null, Color.White); 
                DrawUnalteredText(sb, text, new Vector2(device.Viewport.Width / 2 - 275, device.Viewport.Height / 2), Color.Black, 1.0f);
            }
            else
            {
                text = FormatTextToWidth(info[currIndex].text, 600, out lines);
                sb.Draw(tex, new Rectangle(device.Viewport.Width / 2 - 280, device.Viewport.Height / 2 - 225, 610, height * lines), null, Color.White); 

                DrawUnalteredText(sb, text, new Vector2(device.Viewport.Width / 2 - 275, device.Viewport.Height / 2 - 220), Color.Black, 1.0f);
            }
        }
    }
}
