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
        int deploy = (2 * 3);
        public static int spacing = 5;

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
            Vector2 fontOrigin = font.MeasureString(s);
            sb.DrawString(font, s, loc + fontOrigin, c, 0, fontOrigin, scale, SpriteEffects.None, 0.5f);
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
            Vector2 hor = new Vector2(CardClass.cardWidth, 0);
            Vector2 ver = new Vector2(0, CardClass.cardHeight);

            Vector2 origin;
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {                    
                    origin = new Vector2(j*(CardClass.cardWidth + spacing) + center.X, i*(CardClass.cardHeight + spacing) + center.Y);
                    if (!(i == 0 || j == 0 || i == map.GetLength(0) - 1 || j == map.GetLength(1) - 1))
                    {
                        DrawLine(sb, origin, origin + hor, Color.Black, 0.8f);
                        DrawLine(sb, origin, origin + ver, Color.Black, 0.8f);

                        DrawLine(sb, origin + ver, origin + ver + hor, Color.Black, 0.8f);
                        DrawLine(sb, origin + hor, origin + hor + ver, Color.Black, 0.8f);

                    }


                    if (map[i, j] != null)
                    {
                        map[i, j].Render(sb, false, spacing);
                    }

                    if (i == 0 && j == 3)
                        sb.Draw(Door2Tex, new Rectangle((int)origin.X, (int)origin.Y, CardClass.cardWidth, CardClass.cardHeight), Color.White);
                    if (i == map.GetLength(0) - 1 && j == 3)
                        sb.Draw(Door1Tex, new Rectangle((int)origin.X, (int)origin.Y, CardClass.cardWidth, CardClass.cardHeight), Color.White);

                }
            }

            if (selectedCard != null )
                selectedCard.Render(sb, true, spacing);

            //DrawOutline(sb);
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

            if (deploy > 0)
            {
                DrawText(sb, "DEPLOYMENT PHASE", new Vector2(center.X + maxCardWidth / 2, center.Y - 25), Color.Red, 1.5f);
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
                CardClass winner = card;
                if (replacedCard != null && replacedCard.player != card.player)
                    // Check to see which one wins in this battle.
                    winner = replacedCard.GetCardType().GetStat() <= card.GetCardType().GetStat() + modifier ? card : replacedCard;
                else if (replacedCard != null)
                    // tried to move onto own card.
                    return false;
                Vector2 oldLoc = ConvertScreenCoordToMap(card.GetPrevLocation());
                // Cards can't move into their old position.
                if ((int)oldLoc.X == x && (int)oldLoc.Y == y)
                    return false;
                map[y, x] = winner;
                Vector2 clear = ConvertScreenCoordToMap(card.GetLoc());
                if (clear.Y >= 0 && clear.X >= 0)
                    map[(int)clear.Y, (int)clear.X] = null;
                winner.SetLocation(center + new Vector2(x * (CardClass.cardWidth + spacing), y * (CardClass.cardHeight + spacing)));

                if (card.player == PlayerTurn.Player1)
                    player1Hand.RemoveCard(card);
                else if (card.player == PlayerTurn.Player2)
                    player2Hand.RemoveCard(card);

                return (winner == card);
            }

            return false;
        }

        private Vector2 ConvertScreenCoordToMap(Vector2 pos)
        {
            Vector2 mapLoc = new Vector2(-1, -1);

            float maxMapX = center.X + (CardClass.cardWidth + spacing) * map.GetLength(1);
            float maxMapY = center.Y + (CardClass.cardHeight + spacing) * map.GetLength(0);

            if (pos.X > center.X && pos.X < maxMapX && pos.Y > center.Y && pos.Y < maxMapY)
            {
                mapLoc.X = (int)((pos.X - center.X) / (CardClass.cardWidth + spacing));
                mapLoc.Y = (int)((pos.Y - center.Y) / (CardClass.cardHeight + spacing));
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
            if (currentTurn == PlayerTurn.Player1 && player1Hand.Count < 3)
                player1Hand.AddCard(player1Deck.GetTopCard());

            if (currentTurn == PlayerTurn.Player2 && player2Hand.Count < 3)
                player2Hand.AddCard(player2Deck.GetTopCard());

            if (deploy > 0)
                deploy--;

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
                if (currentTurn == PlayerTurn.Player1 && mapLoc.Y == map.GetLength(1) - 2 && mapLoc.X > 0 && mapLoc.X < map.GetLength(0)-1 && PlaceCard(selectedCard, (int)mapLoc.X, (int)mapLoc.Y))
                {
                    SwitchTurns();
                }
                else if (currentTurn == PlayerTurn.Player2 && mapLoc.Y == 1 && mapLoc.X > 0 && mapLoc.X < map.GetLength(0)-1 && PlaceCard(selectedCard, (int)mapLoc.X, (int)mapLoc.Y))
                {
                    SwitchTurns();
                }
                else
                {
                    ResetSelectedCard();
                }
            }
            // Handle card movement.
            else if (deploy == 0 && mapLoc.X >= 0 && mapLoc.Y >= 0 && mapLoc.X < map.GetLength(0) && mapLoc.Y < map.GetLength(1) && selectedCard != null && selectedCard.player == currentTurn)
            {
                MoveLocation[,] moveOption = selectedCard.GetMove();
                int transX = (int)(mapLoc.X - selectedCardLoc.X) + 2;
                int transY = (int)(mapLoc.Y - selectedCardLoc.Y) + 2;

                if (transX == 2 && transY == 2) // center of the move map
                    return;

                if (mapLoc.X == 0 || mapLoc.X == map.GetLength(0) - 1)
                    return;
                // Check for movement into the gates
                if (mapLoc.Y == 0)
                {
                    if (currentTurn == PlayerTurn.Player1 && mapLoc.X == 3)
                    {
                        PlaceCard(selectedCard, (int)mapLoc.X, (int)mapLoc.Y, moveOption[transY, transX].modifier);
                        SwitchTurns();
                    }

                    return;
                }
                // Check for movement into the gates
                if (mapLoc.Y == map.GetLength(1)-1)
                {
                    if (currentTurn == PlayerTurn.Player2 && mapLoc.X == 3)
                    {
                        PlaceCard(selectedCard, (int)mapLoc.X, (int)mapLoc.Y, moveOption[transY, transX].modifier);
                        SwitchTurns();
                    }

                    return;
                }

                // Check for the actual move.
                if (transX >= 0 && transY >= 0 && transX < moveOption.GetLength(0) && transY < moveOption.GetLength(1) && moveOption[transY, transX] != null)
                {
                    RecursiveCardMovement(moveOption[transY, transX], moveOption, transX, transY);
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

        private bool RecursiveCardMovement(MoveLocation currentLoc, MoveLocation[,] map, int transX, int transY)
        {
            if (transX == 2 && transY == 2)
                return true;

            if (currentLoc.x >= 0 && currentLoc.y >= 0 && !(currentLoc.x == 2 && currentLoc.y == 2))
            {
                if (!RecursiveCardMovement(map[currentLoc.x, currentLoc.y], map, currentLoc.y, currentLoc.x))
                    return false;
            }
            //PlaceCard(selectedCard, (int)mapLoc.X, (int)mapLoc.Y, moveOption[transY, transX].modifier)
            return PlaceCard(selectedCard, (int)selectedCardLoc.X + transX - 2, (int)selectedCardLoc.Y + transY - 2, currentLoc.modifier);
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

            //if (currentTurn == PlayerTurn.Player1 && player1Deck.Intersect(pos))
            //{
            //    player1Hand.AddCard(player1Deck.GetTopCard());
            //    SwitchTurns();
            //}
            //else if (currentTurn == PlayerTurn.Player2 && player2Deck.Intersect(pos))
            //{
            //    player2Hand.AddCard(player2Deck.GetTopCard());
            //    SwitchTurns();
            //}
            //else 
                
            if (selectedCard == null)
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
