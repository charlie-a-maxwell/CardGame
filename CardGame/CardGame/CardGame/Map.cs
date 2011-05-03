using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace CardGame
{
    class MapView
    {
        CardClass[,] map;
        ContentManager cm = null;
        Texture2D lineTex;
        Texture2D WallTex;
        Vector2 center = new Vector2(0,0);
        Vector2 selectedCardLoc;
        CardClass selectedCard;
        SpriteFont font;

        public MapView()
        {
            // This would be where to create the map of whatever size. We are starting with 5 by 5, but the gate is out of that.
            // So we need an extra one on both sides. The other extras will just be marked as filled by something.
            map = new CardClass[7,7];
            selectedCardLoc.X = -1;
            selectedCardLoc.Y = -1;
        }

        public void SetContentManager(ContentManager c)
        {
            cm = c;
            font = c.Load<SpriteFont>("CourierNew");
            WallTex = c.Load<Texture2D>("Wall");
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

            sb.Draw(lineTex, p1, null, c, angle, Vector2.Zero, new Vector2(length, 2), SpriteEffects.None, 0.9f);
        }

        public void SetCenter(Vector2 cent)
        {
            center = cent;
        }

        public void DrawOutline(SpriteBatch sb)
        {
            int maxCardWidth = CardClass.cardWidth * map.GetLength(1);
            int maxCardHeight = CardClass.cardHeight * map.GetLength(0);

            DrawLine(sb, new Vector2(center.X, maxCardHeight + center.Y), new Vector2(maxCardWidth + center.X, maxCardHeight + center.Y), Color.Black);
            DrawLine(sb, new Vector2(maxCardWidth + center.X, center.Y), new Vector2(maxCardWidth + center.X, maxCardHeight + center.Y), Color.Black);

            DrawLine(sb, new Vector2(center.X, center.Y), new Vector2(center.X, maxCardHeight + center.Y), Color.Black);
            DrawLine(sb, new Vector2(center.X, center.Y), new Vector2(maxCardWidth + center.X, center.Y), Color.Black);

        }

        public void RenderMap(SpriteBatch sb)
        {
          
            int maxCardWidth = CardClass.cardWidth * (map.GetLength(1)-2);
            int maxCardHeight = CardClass.cardHeight * (map.GetLength(0)-2);

            Vector2 origin;
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    DrawLine(sb, new Vector2(center.X + CardClass.cardWidth, CardClass.cardHeight * (i + 1) + center.Y), new Vector2(maxCardWidth + center.X + CardClass.cardWidth, CardClass.cardHeight * (i + 1) + center.Y), Color.Black);
                    DrawLine(sb, new Vector2(CardClass.cardWidth * (j + 1) + center.X, center.Y + CardClass.cardHeight), new Vector2(CardClass.cardWidth * (j + 1) + center.X, maxCardHeight + center.Y + CardClass.cardHeight), Color.Black);
                    
                    origin = new Vector2(j*CardClass.cardWidth + center.X, i*CardClass.cardHeight + center.Y);
                    if (i == 0 || j == 0 || i == map.GetLength(0) - 1 || j == map.GetLength(1) - 1)
                        sb.Draw(WallTex, new Rectangle((int)origin.X, (int)origin.Y, CardClass.cardWidth, CardClass.cardHeight), Color.White);
                    else 
                    if (map[i, j] != null)
                        map[i,j].Render(sb, origin );
                }
            }

            DrawOutline(sb);


            if (selectedCard != null)
            {
                int originX = 30;
                int originY = 30;
                int width = 95;
                int height = 145;
                int xMargin = 5;
                int yMargin = 5;
                Vector2 fontOrigin;
                int[,] cardMove = selectedCard.GetCardType().GetMove();

                float xSize = (width - xMargin * 2) / cardMove.GetLength(1);
                float ySize = (height - CardClass.cardHeight - yMargin * 2) / cardMove.GetLength(0);
                float offsetX = xMargin + originX;
                float offsetY = originY + CardClass.cardHeight + yMargin * 2;


                sb.Draw(lineTex, new Rectangle(originX,originY,width,height), Color.White);
                DrawLine(sb, new Vector2(originX, originY), new Vector2(originX, originY + height), Color.Black);
                DrawLine(sb, new Vector2(originX, originY), new Vector2(originX + width, originY), Color.Black);
                DrawLine(sb, new Vector2(originX + width, originY), new Vector2(originX + width, originY + height), Color.Black);
                DrawLine(sb, new Vector2(originX, originY + height), new Vector2(originX + width, originY + height), Color.Black);

                selectedCard.Render(sb, new Vector2(originX + (width - CardClass.cardWidth) / 2, originY + yMargin));

                for (int i = 0; i < cardMove.GetLength(0); i++)
                {
                    for (int j = 0; j < cardMove.GetLength(1); j++)
                    {
                        if (cardMove[i, j] != 0)
                        {
                            DrawLine(sb, new Vector2(xSize * j + offsetX, ySize * i + offsetY), new Vector2(xSize * j + offsetX, ySize * (i + 1) + offsetY), Color.Black);
                            DrawLine(sb, new Vector2(xSize * j + offsetX, ySize * i + offsetY), new Vector2(xSize * (j + 1) + offsetX, ySize * i + offsetY), Color.Black);
                            DrawLine(sb, new Vector2(xSize * (j + 1) + offsetX, ySize * i + offsetY), new Vector2(xSize * (j + 1) + offsetX, ySize * (i + 1) + offsetY), Color.Black);
                            DrawLine(sb, new Vector2(xSize * j + offsetX, ySize * (i + 1) + offsetY), new Vector2(xSize * (j + 1) + offsetX, ySize * (i + 1) + offsetY), Color.Black);

                            fontOrigin = font.MeasureString(cardMove[i, j].ToString());
                            sb.DrawString(font, cardMove[i, j].ToString(), new Vector2(xSize * j + offsetX + fontOrigin.X, ySize * i + offsetY + fontOrigin.Y), Color.Black, 0, fontOrigin, 1.0f, SpriteEffects.None, 0.5f);
                        }
                    }
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

        private Vector2 ConvertScreenCoordToMap(Vector2 pos)
        {
            Vector2 mapLoc = new Vector2(-1, -1);

            float maxMapX = center.X + CardClass.cardWidth * map.GetLength(1);
            float maxMapY = center.Y + CardClass.cardHeight * map.GetLength(0);

            if (pos.X > center.X && pos.X < maxMapX && pos.Y > center.Y && pos.Y < maxMapY)
            {
                mapLoc.X = (int)((pos.X - center.X) / CardClass.cardWidth);
                mapLoc.Y = (int)((pos.Y - center.Y) / CardClass.cardHeight);
            }

            return mapLoc;
        }

        public void ScreenCardSelect(Vector2 pos)
        {
            Vector2 mapLoc = ConvertScreenCoordToMap(pos);
            if (mapLoc.X >= 0 && mapLoc.Y >= 0)
            {
                selectedCard = map[(int)mapLoc.Y,(int)mapLoc.X];
                selectedCardLoc.X = mapLoc.X;
                selectedCardLoc.Y = mapLoc.Y;
            }
        }

        public void MoveCard(Vector2 pos)
        {
            Vector2 mapLoc = ConvertScreenCoordToMap(pos);
            if (mapLoc.X >= 1 && mapLoc.Y >= 1 && mapLoc.X < map.GetLength(0) -1 && mapLoc.Y < map.GetLength(1) -1 && selectedCard != null)
            {
                int[,] moveOption = selectedCard.GetCardType().GetMove();
                int transX = (int)(mapLoc.X - selectedCardLoc.X) + 2;
                int transY = (int)(mapLoc.Y - selectedCardLoc.Y) + 2;

                if (transX == 2 && transY == 2) // center of the move map
                    return;

                if (transX > 0 && transY > 0 && transX < moveOption.GetLength(0) && transY < moveOption.GetLength(1) && moveOption[transY, transX] != 0)
                {
                    map[(int)mapLoc.Y, (int)mapLoc.X] = selectedCard;
                    map[(int)selectedCardLoc.Y, (int)selectedCardLoc.X] = null;
                    selectedCardLoc.X = mapLoc.X;
                    selectedCardLoc.Y = mapLoc.Y;
                }
                else
                {
                    selectedCard = null;
                    selectedCardLoc.X = -1;
                    selectedCardLoc.Y = -1;
                }
            }
            else
            {
                selectedCard = null;
                selectedCardLoc.X = -1;
                selectedCardLoc.Y = -1;
            }
        }

        public void HandleMouseClick(Vector2 pos)
        {
            if (selectedCard == null)
            {
                ScreenCardSelect(pos);
            }
            else
            {
                MoveCard(pos);
            }
        }

    }
}
