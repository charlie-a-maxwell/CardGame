using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace CardGame
{
    class Map
    {
        CardClass[,] map;
        ContentManager cm = null;
        Texture2D lineTex;
        Vector2 center = new Vector2(0,0);

        public Map()
        {
            // This would be where to create the map of whatever size. We are starting with 5 by 5, but the gate is out of that.
            // So we need an extra one on both sides. The other extras will just be marked as filled by something.
            map = new CardClass[7,7];
        }

        public void SetContentManager(ContentManager c)
        {
            cm = c;
            foreach (CardClass cc in map)
            {
                if (cc != null)
                    cc.LoadTexture(cm);
            }
        }

        public void SetGraphics(GraphicsDevice gd)
        {
            lineTex = new Texture2D(gd, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
            lineTex.SetData(new[] { Color.White });
            center = new Vector2((gd.Viewport.Width - CardClass.cardWidth * map.GetLength(1)) / 2, (gd.Viewport.Height - CardClass.cardHeight * map.GetLength(1)) / 2);
        }

        private void DrawLine(SpriteBatch sb, Vector2 p1, Vector2 p2, Color c)
        {
            float angle = (float)Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
            float length = Vector2.Distance(p1, p2);

            sb.Draw(lineTex, p1, null, c, angle, Vector2.Zero, new Vector2(length, 2), SpriteEffects.None, 0);
        }

        public void SetCenter(Vector2 cent)
        {
            center = cent;
        }

        public void RenderMap(SpriteBatch sb)
        {
          
            int maxCardWidth = CardClass.cardWidth * map.GetLength(1);
            int maxCardHeight = CardClass.cardHeight * map.GetLength(0);

            DrawLine(sb, new Vector2(center.X, maxCardHeight + center.Y), new Vector2(maxCardWidth + center.X, maxCardHeight + center.Y), Color.Black);
            DrawLine(sb, new Vector2(maxCardWidth + center.X, center.Y), new Vector2(maxCardWidth + center.X, maxCardHeight + center.Y), Color.Black);

            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    DrawLine(sb, new Vector2(center.X, CardClass.cardHeight * i + center.Y), new Vector2(maxCardWidth + center.X, CardClass.cardHeight * i + center.Y), Color.Black);
                    DrawLine(sb, new Vector2(CardClass.cardWidth * j + center.X, center.Y), new Vector2(CardClass.cardWidth * j +center.X, maxCardHeight + center.Y), Color.Black);

                    if (map[i, j] != null && i > 0 && j > 0)
                        map[i,j].Render(sb, new Vector2(j*CardClass.cardWidth + center.X, i*CardClass.cardHeight + center.Y));
                }
            }
        }

        public void PlaceCard(CardClass card, int x, int y)
        {
            if (map[y, x] == null && x > 0 && y > 0 && x < map.GetLength(1) && y < map.GetLength(0))
            {
                if (cm != null)
                    card.LoadTexture(cm);
                map[y, x] = card;
            }
        }
    }
}
