using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace CardGame
{
    abstract class Screen
    {
        protected static Texture2D lineTex;
        protected static SpriteFont font = null;

        public Screen()
        {
        }

        public virtual void LoadContent(ContentManager cm)
        {
            if (font == null)
                font = cm.Load<SpriteFont>("CourierNew");
        }

        public virtual void SetGraphics(GraphicsDevice gd)
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

        public virtual bool Init()
        {
            return true;
        }

        public void Update()
        {

        }
    }
}
