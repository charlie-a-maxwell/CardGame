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
        static Texture2D lineTex;
        Texture2D WallTex;
        Texture2D Door1Tex;
        Texture2D Door2Tex;
        Vector2 center = new Vector2(0,0);
        Vector2 selectedCardLoc;
        CardClass selectedCard;
        static SpriteFont font;
        Hand player1Hand;
        Hand player2Hand;
        Deck player1Deck;
        Deck player2Deck;
        PlayerTurn currentTurn;
        PlayerTurn winner;
        bool over = false;

        public MapView()
        {
            // This would be where to create the map of whatever size. We are starting with 5 by 5, but the gate is out of that.
            // So we need an extra one on both sides. The other extras will just be marked as filled by something.
            map = new CardClass[7,7];
            selectedCardLoc.X = -1;
            selectedCardLoc.Y = -1;
            player1Hand = new Hand(PlayerTurn.Player1);
            player2Hand = new Hand(PlayerTurn.Player2);
            player1Deck = new Deck(PlayerTurn.Player1);
            player2Deck = new Deck(PlayerTurn.Player2);

            currentTurn = PlayerTurn.Player1;
            winner = 0;
        }

        public void SetContentManager(ContentManager c)
        {
            cm = c;
            font = c.Load<SpriteFont>("CourierNew");
            WallTex = c.Load<Texture2D>("Wall");
            Door1Tex = c.Load<Texture2D>("DoorPlayer1");
            Door2Tex = c.Load<Texture2D>("DoorPlayer2");
        }

        public void SetGraphics(GraphicsDevice gd)
        {
            lineTex = new Texture2D(gd, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
            lineTex.SetData(new[] { Color.White });
            SetCenter(new Vector2((gd.Viewport.Width - CardClass.cardWidth * map.GetLength(1)) / 2, (gd.Viewport.Height - CardClass.cardHeight * map.GetLength(1)) / 2));
        }

        public static void DrawLine(SpriteBatch sb, Vector2 p1, Vector2 p2, Color c)
        {
            float angle = (float)Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
            float length = Vector2.Distance(p1, p2);

            sb.Draw(lineTex, p1, null, c, angle, Vector2.Zero, new Vector2(length, 2), SpriteEffects.None, 0.9f);
        }

        public static void DrawText(SpriteBatch sb, string s, Vector2 loc)
        {
            Vector2 fontOrigin = font.MeasureString(s);
            sb.DrawString(font, s, loc + fontOrigin, Color.Black, 0, fontOrigin, 1.0f, SpriteEffects.None, 0.5f);
        }

        public static void FillColor(SpriteBatch sb,int originX, int originY, int width, int height, Color color)
        {
            sb.Draw(lineTex, new Rectangle(originX, originY, width, height), color);
        }

        public void SetCenter(Vector2 cent)
        {
            center = cent;
            player1Hand.SetRenderLoc(new Vector2(center.X + 30, center.Y + CardClass.cardHeight * map.GetLength(0) + 30));
            player2Hand.SetRenderLoc(new Vector2(center.X + 30, 40));

            player1Deck.SetLoc(new Vector2(center.X + CardClass.cardWidth * map.GetLength(1) + 30, center.Y + CardClass.cardHeight * map.GetLength(0) + 30));
            player2Deck.SetLoc(new Vector2(center.X + CardClass.cardWidth * map.GetLength(1) + 30, 40));
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
                        map[i,j].Render(sb, false);

                    if (i == 0 && j == 3)
                        sb.Draw(Door2Tex, new Rectangle((int)origin.X, (int)origin.Y, CardClass.cardWidth, CardClass.cardHeight), Color.White);
                    if (i == map.GetLength(0) - 1 && j == 3)
                        sb.Draw(Door1Tex, new Rectangle((int)origin.X, (int)origin.Y, CardClass.cardWidth, CardClass.cardHeight), Color.White);

                }
            }

            if (selectedCard != null)
                selectedCard.Render(sb, true);

            DrawOutline(sb);
            if (over && winner == PlayerTurn.Player1)
            {
                player1Hand.Render(sb, selectedCard);
                player2Hand.Render(sb, selectedCard);
                DrawText(sb, "Player 1 Wins!", new Vector2(center.X + maxCardWidth / 2, 10));
            }
            else if (over && winner == PlayerTurn.Player2)
            {
                player1Hand.Render(sb, selectedCard);
                player2Hand.Render(sb, selectedCard);
                DrawText(sb, "Player 2 Wins!", new Vector2(center.X + maxCardWidth / 2, 10));
            } 
            else if (currentTurn == PlayerTurn.Player1)
            {
                player1Hand.Render(sb, selectedCard);
                DrawText(sb, "Player 1 Turn", new Vector2(center.X + maxCardWidth / 2, 10));
            }
            else
            {
                player2Hand.Render(sb, selectedCard);
                DrawText(sb, "Player 2 Turn", new Vector2(center.X + maxCardWidth / 2, 10));
            }

            player1Deck.Render(sb);
            player2Deck.Render(sb);
        }

        public bool PlaceCard(CardClass card, int x, int y)
        {
            return PlaceCard(card, x, y, 0);
        }


        public bool PlaceCard(CardClass card, int x, int y, int modifier)
        {
            if ( x >= 0 && y >= 0 && x < map.GetLength(1) && y < map.GetLength(0))
            {
                if (cm != null)
                    card.LoadTexture(cm);

                CardClass replacedCard = map[y, x];
                if (replacedCard != null && replacedCard.player != card.player)
                    // Check to see which one wins in this battle.
                    card = replacedCard.GetCardType().GetStat() <= card.GetCardType().GetStat() + modifier ? card : replacedCard;
                else if (replacedCard != null)
                    // tried to move onto own card.
                    return false;
                Vector2 oldLoc = ConvertScreenCoordToMap(card.GetPrevLocation());
                // Cards can't move into their old position.
                if ((int)oldLoc.X == x && (int)oldLoc.Y == y)
                    return false;
                map[y, x] = card;
                if (selectedCardLoc.Y >= 0 && selectedCardLoc.X >= 0)
                    map[(int)selectedCardLoc.Y, (int)selectedCardLoc.X] = null;
                card.SetLocation(center + new Vector2(x * CardClass.cardWidth, y * CardClass.cardHeight));

                if (card.player == PlayerTurn.Player1)
                    player1Hand.RemoveCard(card);
                else if (card.player == PlayerTurn.Player2)
                    player2Hand.RemoveCard(card);

                return true;
            }

            return false;
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
            foreach(CardClass card in map)
            {
                if (card != null && card.Intersect(pos))
                {
                    selectedCard = card;
                    selectedCardLoc = ConvertScreenCoordToMap(pos);
                    break;
                }
            }

            if (selectedCard == null)
            {
                if (currentTurn == PlayerTurn.Player1)
                    selectedCard = player1Hand.SelectCard(pos);
                else
                    selectedCard = player2Hand.SelectCard(pos);
                selectedCardLoc.X = -1;
                selectedCardLoc.Y = -1;
            }
        }

        private void SwitchTurns()
        {
            CheckWin();
            currentTurn = (currentTurn == PlayerTurn.Player1 ? PlayerTurn.Player2 : PlayerTurn.Player1);
            ResetSelectedCard();
        }

        private bool PlayerCardsLeft(PlayerTurn pt)
        {
            foreach (CardClass card in map)
            {
                if (card != null && card.player == pt)
                    return true;
            }

            return false;
        }

        private void CheckWin()
        {
            if ((player1Hand.Count == 0 && !PlayerCardsLeft(PlayerTurn.Player1)) || map[map.GetLength(0)-1, 3] != null)
            {
                // player 2 wins by kill!
                winner = PlayerTurn.Player2;
                over = true;
            }

            if ((player2Hand.Count == 0 && !PlayerCardsLeft(PlayerTurn.Player2)) || map[0, 3] != null)
            {
                // player 1 wins by kill!
                winner = PlayerTurn.Player1;
                over = true;
            }
        }

        public void MoveCard(Vector2 pos)
        {
            Vector2 mapLoc = ConvertScreenCoordToMap(pos);
            // Handle first card placement.
            if (selectedCard != null && selectedCardLoc.X == -1 && selectedCardLoc.Y == -1)
            { 
                // placing a new card.
                if (currentTurn == PlayerTurn.Player1 && mapLoc.Y == map.GetLength(0) - 2 && PlaceCard(selectedCard, (int)mapLoc.X, (int)mapLoc.Y))
                {
                    SwitchTurns();
                }
                else if (currentTurn == PlayerTurn.Player2 && mapLoc.Y == 1 && PlaceCard(selectedCard, (int)mapLoc.X, (int)mapLoc.Y))
                {
                    SwitchTurns();
                }
                else
                {
                    ResetSelectedCard();
                }
            }
            // Handle card movement.
            else if (mapLoc.X >= 0 && mapLoc.Y >= 0 && mapLoc.X < map.GetLength(0) && mapLoc.Y < map.GetLength(1) && selectedCard != null && selectedCard.player == currentTurn)
            {
                int[,] moveOption = selectedCard.GetCardType().GetMove();
                int transX = (int)(mapLoc.X - selectedCardLoc.X) + 2;
                int transY = (int)(mapLoc.Y - selectedCardLoc.Y) + 2;

                if (transX == 2 && transY == 2) // center of the move map
                    return;

                if (mapLoc.X == 0 || mapLoc.X == map.GetLength(0) - 1)
                    return;
                if (mapLoc.Y == 0)
                {
                    if (currentTurn == PlayerTurn.Player1 && mapLoc.X == 3)
                    {
                        PlaceCard(selectedCard, (int)mapLoc.X, (int)mapLoc.Y, moveOption[transY, transX]);
                        SwitchTurns();
                    }

                    return;
                }

                if (mapLoc.Y == map.GetLength(1)-1)
                {
                    if (currentTurn == PlayerTurn.Player2 && mapLoc.X == 3)
                    {
                        PlaceCard(selectedCard, (int)mapLoc.X, (int)mapLoc.Y, moveOption[transY, transX]);
                        SwitchTurns();
                    }

                    return;
                }

                if (transX >= 0 && transY >= 0 && transX < moveOption.GetLength(0) && transY < moveOption.GetLength(1) && moveOption[transY, transX] != 0 && PlaceCard(selectedCard, (int)mapLoc.X, (int)mapLoc.Y, moveOption[transY, transX]))
                {
                    SwitchTurns();
                }
                else
                {
                    ResetSelectedCard();
                }
            }
            else
            {
                ResetSelectedCard();
            }
        }

        private void ResetSelectedCard()
        {
            selectedCard = null;
            selectedCardLoc.X = -1;
            selectedCardLoc.Y = -1;
        }

        public void HandleMouseClick(Vector2 pos)
        {
            if (over)
                return;

            if (currentTurn == PlayerTurn.Player1 && player1Deck.Intersect(pos))
            {
                player1Hand.AddCard(player1Deck.GetTopCard());
                SwitchTurns();
            }
            else if (currentTurn == PlayerTurn.Player2 && player2Deck.Intersect(pos))
            {
                player2Hand.AddCard(player2Deck.GetTopCard());
                SwitchTurns();
            }
            else if (selectedCard == null)
            {
                ScreenCardSelect(pos);
            }
            else
            {
                MoveCard(pos);
            }
        }

        public void SetPlayerDeck(Deck d, PlayerTurn pt)
        {
            switch (pt)
            {
                case PlayerTurn.Player1:
                    player1Deck = d;
                    break;

                case PlayerTurn.Player2:
                    player2Deck = d;
                    break;
            }
        }

        public void StartGame()
        {
            player1Deck.ShuffleCurrentDeck();

            for (int i = 0; i < 5; i++)
            {
                player1Hand.AddCard(player1Deck.GetTopCard());
            }

            player2Deck.ShuffleCurrentDeck();

            for (int i = 0; i < 5; i++)
            {
                player2Hand.AddCard(player2Deck.GetTopCard());
            }

            player1Deck.SetLoc(new Vector2(center.X + CardClass.cardWidth * map.GetLength(1) + 30, center.Y + CardClass.cardHeight * map.GetLength(0) + 30));
            player2Deck.SetLoc(new Vector2(center.X + CardClass.cardWidth * map.GetLength(1) + 30, 40));

        }
    }
}
