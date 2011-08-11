using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Xml.Serialization;
using System.IO;
using Microsoft.Xna.Framework.Input;


namespace CardGame
{
    class MapView : Screen
    {
        CardClass[,] map;
        Texture2D WallTex;
        Texture2D FireTex;
        Texture2D CloudTex;
        Texture2D MapBackground;
        Texture2D Gate1;
        Texture2D Gate2;
        Vector2 center = new Vector2(0,0);
        Vector2 selectedCardLoc;
        CardClass selectedCard;
        Hand player1Hand;
        Hand player2Hand;
        Deck player1Deck;
        Deck player2Deck;
        PlayerTurn currentTurn;
        PlayerTurn winner;
        bool over = false;
        int deploy = (2 * 3);
        public static int spacing = 5;
        List<CardType> cardTypes;

        public MapView(string n, GraphicsDevice gd) : base(n)
        {
            // This would be where to create the map of whatever size. We are starting with 5 by 5, but the gate is out of that.
            // So we need an extra one on both sides. The other extras will just be marked as filled by something.
            map = new CardClass[7, 7];
            selectedCardLoc.X = -1;
            selectedCardLoc.Y = -1;
            player1Hand = new Hand(PlayerTurn.Player1);
            player2Hand = new Hand(PlayerTurn.Player2);
            player1Deck = new Deck(PlayerTurn.Player1);
            player2Deck = new Deck(PlayerTurn.Player2);

            cardTypes = new List<CardType>();


            currentTurn = PlayerTurn.Player1;
            winner = 0;

            SetCenter(new Vector2((gd.Viewport.Width - CardClass.cardWidth * map.GetLength(1)) / 2, (gd.Viewport.Height - CardClass.cardHeight * map.GetLength(1)) / 2));
        }

        public MapView() : base()
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

            cardTypes = new List<CardType>();


            currentTurn = PlayerTurn.Player1;
            winner = 0;
        }

        protected void LoadDecks()
        {

            XmlSerializer serializer = new XmlSerializer(typeof(List<string>));
            Stream deckstream = File.Open("Content/Deck1.xml", FileMode.Open);
            List<string> list = (List<string>)serializer.Deserialize(deckstream);
            deckstream.Close();

            BuildPlayerDeck(list, PlayerTurn.Player1);

            deckstream = File.Open("Content/Deck2.xml", FileMode.Open);
            list = (List<string>)serializer.Deserialize(deckstream);
            deckstream.Close();

            BuildPlayerDeck(list, PlayerTurn.Player2);
        }

        protected void BuildPlayerDeck(List<string>list, PlayerTurn p)
        {
            Deck deck = new Deck(p);
            CardType type;
            foreach (string card in list)
            {
                type = cardTypes.Find(
                            delegate(CardType t)
                            {
                                return t.typeName.ToLower() == card.ToLower();
                            });
                if (type != null)
                {
                    deck.AddCard(new CardClass(type, p));
                }
            }

            switch (p)
            {
                case PlayerTurn.Player1:
                    player1Deck = deck;
                    player1Deck.SetLoc(new Vector2(center.X - (map.GetLength(1) * CardClass.cardWidth) / 2.0f - 40, center.Y + (map.GetLength(0) * CardClass.cardHeight) - CardClass.cardHeight * 2 - 40));
                    break;

                case PlayerTurn.Player2:
                    player2Deck = deck;
                    player2Deck.SetLoc(new Vector2(center.X + (map.GetLength(1) * CardClass.cardWidth) + CardClass.cardWidth * 4 - 30, center.Y + CardClass.cardHeight + 40));
                    break;
            }
        }

        public override void LoadContent(ContentManager cm)
        {
            base.LoadContent(cm);

            WallTex = cm.Load<Texture2D>("Wall");
            FireTex = cm.Load<Texture2D>("Fire");
            CloudTex = cm.Load<Texture2D>("Cloud");
            MapBackground = cm.Load<Texture2D>("MapBack");
            Gate1 = cm.Load<Texture2D>("DoorPlayer1");
            Gate2 = cm.Load<Texture2D>("DoorPlayer2");
            Texture2D deckTeck = cm.Load<Texture2D>("DeckBack");

            
            Deck.SetTexure(deckTeck);

            foreach (CardType cc in cardTypes)
            {
                if (cc != null)
                    cc.LoadTexture(cm);
            }
        }

        public void SetCenter(Vector2 cent)
        {
            center = cent;
            player1Hand.SetRenderLoc(new Vector2(center.X - (map.GetLength(1) * CardClass.cardWidth) / 2.0f - 40, center.Y + (map.GetLength(0)*CardClass.cardHeight) - CardClass.cardHeight));
            player2Hand.SetRenderLoc(new Vector2(center.X + (map.GetLength(1) * CardClass.cardWidth) - 20 + CardClass.cardWidth * 4, center.Y ));

            player1Deck.SetLoc(new Vector2(center.X - (map.GetLength(1) * CardClass.cardWidth) / 2.0f, center.Y + (map.GetLength(0) * CardClass.cardHeight) - CardClass.cardHeight * 2));
            player2Deck.SetLoc(new Vector2(center.X + (map.GetLength(1) * CardClass.cardWidth) - 20, center.Y));
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

        public override void Render(SpriteBatch sb, GraphicsDevice device, GameTime gt)
        {
            int maxCardWidth = CardClass.cardWidth * (map.GetLength(1)-2);
            int maxCardHeight = CardClass.cardHeight * (map.GetLength(0)-2);
            Vector2 hor = new Vector2(CardClass.cardWidth, 0);
            Vector2 ver = new Vector2(0, CardClass.cardHeight);
            Color outlineColor = Color.Black;

            Vector2 origin;

            if (MapBackground != null)
                sb.Draw(MapBackground, new Rectangle((int)center.X, (int)center.Y-10, map.GetLength(0) * (CardClass.cardWidth + spacing), map.GetLength(1) * (CardClass.cardHeight + spacing) + 15), Color.White);

            if (CloudTex != null)
            {
                origin = new Vector2((map.GetLength(0) - 3) * (CardClass.cardWidth + spacing) + center.X, (map.GetLength(1)-1) * (CardClass.cardHeight + spacing) + center.Y);
                sb.Draw(CloudTex, origin, null, Color.White, 0f, new Vector2(0,0), 0.4f, SpriteEffects.None, 0);
                origin = new Vector2(center.X, (map.GetLength(1) - 1) * (CardClass.cardHeight + spacing) + center.Y);
                sb.Draw(CloudTex, origin, null, Color.White, 0f, new Vector2(0, 0), 0.4f, SpriteEffects.FlipHorizontally, 0);
            }

            if (FireTex != null)
            {
                origin = new Vector2((map.GetLength(0) - 3) * (CardClass.cardWidth + spacing) + 30 + center.X, center.Y  + spacing + 20);
                sb.Draw(FireTex, origin, null, Color.White, 0f, new Vector2(0, 0), 0.4f, SpriteEffects.FlipVertically, 0);
                origin = new Vector2(center.X + 30, center.Y + spacing + 20);
                sb.Draw(FireTex, origin, null, Color.White, 0f, new Vector2(0, 0), 0.4f, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0);
            }

            Color trans = new Color(Color.LightGray, 0.3f);
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {                    
                    origin = new Vector2(j*(CardClass.cardWidth + spacing) + center.X, i*(CardClass.cardHeight + spacing) + center.Y);
                    if (!(i == 0 || j == 0 || i == map.GetLength(0) - 1 || j == map.GetLength(1) - 1))
                    {
                        outlineColor = Color.Black;

                        if (i == map.GetLength(0) - 2 || i == 1)
                        {
                            FillColor(sb, (int)origin.X, (int)origin.Y, CardClass.cardWidth, CardClass.cardHeight, trans);
                        }

                        DrawLine(sb, origin, origin + hor, outlineColor, 0.8f);
                        DrawLine(sb, origin, origin + ver, outlineColor, 0.8f);

                        DrawLine(sb, origin + ver, origin + ver + hor, outlineColor, 0.8f);
                        DrawLine(sb, origin + hor, origin + hor + ver, outlineColor, 0.8f);

                    }

                    if (map[i, j] != null)
                    {
                        map[i, j].Render(sb, gt, false, spacing);
                    }

                    if (i == 0 && j == 3)
                    {
                        if (Gate1 != null)
                        {
                            sb.Draw(Gate1, new Rectangle((int)origin.X, (int)origin.Y, CardClass.cardWidth, CardClass.cardHeight), Color.White);
                        }
                        else
                        {
                            DrawLine(sb, origin, origin + hor, outlineColor, 0.8f);
                            DrawLine(sb, origin, origin + ver, outlineColor, 0.8f);

                            DrawLine(sb, origin + ver, origin + ver + hor, outlineColor, 0.8f);
                            DrawLine(sb, origin + hor, origin + hor + ver, outlineColor, 0.8f);
                        }
                    }
                    if (i == map.GetLength(0) - 1 && j == 3)
                    {
                        if (Gate2 != null)
                        {
                            sb.Draw(Gate2, new Rectangle((int)origin.X, (int)origin.Y, CardClass.cardWidth, CardClass.cardHeight), Color.White);
                        }
                        else
                        {
                            DrawLine(sb, origin, origin + hor, outlineColor, 0.8f);
                            DrawLine(sb, origin, origin + ver, outlineColor, 0.8f);

                            DrawLine(sb, origin + ver, origin + ver + hor, outlineColor, 0.8f);
                            DrawLine(sb, origin + hor, origin + hor + ver, outlineColor, 0.8f);
                        }
                    }

                }
            }

            string text = "";
            Color textColor = Color.Black;
            //DrawOutline(sb);
            if (over)
            {
                player1Hand.Render(sb, selectedCard, gt);
                player2Hand.Render(sb, selectedCard, gt);
                text = (winner == PlayerTurn.Player1 ? "Player 1" : "Player 2");
            }
            else if (currentTurn == PlayerTurn.Player1)
            {
                player1Hand.Render(sb, selectedCard, gt);
                text = "Player 1";
            }
            else
            {
                player2Hand.Render(sb, selectedCard, gt);
                text = "Player 2";
            }

            if (over)
            {
                text += " wins! Press Esc to exit.";
            }
            else if (deploy > 0)
            {
                text += " deployment phase";
                textColor = Color.Red;
            }
            else
            {
                text += " turn";
            }

            if (text.Length > 0)
                DrawText(sb, text, new Vector2(center.X + maxCardWidth / 2, 10), textColor, 1.0f);


            player1Deck.Render(sb, gt);
            player2Deck.Render(sb, gt);

            if (selectedCard != null)
                selectedCard.Render(sb, gt, true, spacing);

        }

        public bool PlaceCard(CardClass card, int x, int y)
        {
            return PlaceCard(card, x, y, 0);
        }


        public bool PlaceCard(CardClass card, int x, int y, int modifier)
        {
            if ( x >= 0 && y >= 0 && x < map.GetLength(1) && y < map.GetLength(0))
            {
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
                winner.SetLocation(center + new Vector2(x * (CardClass.cardWidth + spacing), y * (CardClass.cardHeight + spacing)), true);

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
                if (currentTurn == PlayerTurn.Player1 && mapLoc.Y == map.GetLength(1) - 2 && mapLoc.X > 0 && mapLoc.X < map.GetLength(0)-1 && map[(int)mapLoc.Y, (int)mapLoc.X] == null && PlaceCard(selectedCard, (int)mapLoc.X, (int)mapLoc.Y))
                {
                    SwitchTurns();
                }
                else if (currentTurn == PlayerTurn.Player2 && mapLoc.Y == 1 && mapLoc.X > 0 && mapLoc.X < map.GetLength(0) - 1 && map[(int)mapLoc.Y, (int)mapLoc.X] == null && PlaceCard(selectedCard, (int)mapLoc.X, (int)mapLoc.Y))
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
                    if (currentTurn == PlayerTurn.Player1 && mapLoc.X == 3 && transX == 2)
                    {
                        PlaceCard(selectedCard, (int)mapLoc.X, (int)mapLoc.Y, moveOption[transY, transX].modifier);
                        SwitchTurns();
                    }

                    return;
                }
                // Check for movement into the gates
                if (mapLoc.Y == map.GetLength(1)-1)
                {
                    if (currentTurn == PlayerTurn.Player2 && mapLoc.X == 3 && transX == 2)
                    {
                        PlaceCard(selectedCard, (int)mapLoc.X, (int)mapLoc.Y, moveOption[transY, transX].modifier);
                        SwitchTurns();
                    }

                    return;
                }

                // Check for the actual move.
                if (transX >= 0 && transY >= 0 && transX < moveOption.GetLength(0) && transY < moveOption.GetLength(1) && moveOption[transY, transX] != null)
                {
                    bool good = false;
                    RecursiveCardMovement(moveOption[transY, transX], moveOption, transX, transY, out good);
                    if (good)
                    {
                        SwitchTurns();
                    }
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

        private bool RecursiveCardMovement(MoveLocation currentLoc, MoveLocation[,] map, int transX, int transY, out bool recurse)
        {
            if (transX == 2 && transY == 2)
            {
                recurse = true;
                return true;
            }

            if (currentLoc.x >= 0 && currentLoc.y >= 0 && !(currentLoc.x == 2 && currentLoc.y == 2))
            {
                if (!RecursiveCardMovement(map[currentLoc.x, currentLoc.y], map, currentLoc.y, currentLoc.x, out recurse))
                    return recurse;
            }
            //PlaceCard(selectedCard, (int)mapLoc.X, (int)mapLoc.Y, moveOption[transY, transX].modifier)
            bool place = PlaceCard(selectedCard, (int)selectedCardLoc.X + transX - 2, (int)selectedCardLoc.Y + transY - 2, currentLoc.modifier);
            recurse = place;
            return place;
        }

        private void ResetSelectedCard()
        {
            selectedCard = null;
            selectedCardLoc.X = -1;
            selectedCardLoc.Y = -1;
        }

        public override void HandleMouseClick(Vector2 pos)
        {
            if (over)
                return;
                
            if (selectedCard == null)
            {
                ScreenCardSelect(pos);
            }
            else
            {
                MoveCard(pos);
            }
        }

        public void StartGame()
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    map[i, j] = null;
                }
            }

            over = false;
            
            player1Hand = new Hand(PlayerTurn.Player1);
            player2Hand = new Hand(PlayerTurn.Player2);
            player1Hand.SetRenderLoc(new Vector2(center.X - (map.GetLength(1) * CardClass.cardWidth) / 2.0f - 40, center.Y + (map.GetLength(0) * CardClass.cardHeight) - CardClass.cardHeight));
            player2Hand.SetRenderLoc(new Vector2(center.X + (map.GetLength(1) * CardClass.cardWidth) - 20 + CardClass.cardWidth * 4, center.Y));

            LoadDecks();

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

            Random rand = new Random();
            currentTurn = (rand.Next(2) == 0 ? PlayerTurn.Player1 : PlayerTurn.Player2);
        }


        public override bool Init()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<CardType>));
            Stream cardTypeFile = File.Open("Content/CardTypes.xml", FileMode.Open);
            cardTypes = (List<CardType>)serializer.Deserialize(cardTypeFile);
            cardTypeFile.Close();

            return true;
        }

        public override void HandleKeydown(Keys[] k)
        {
            try
            {
                bool endProcessing = false;
                foreach (Keys t in k)
                {
                    switch (t)
                    {
                        case Keys.Escape:
                            if (over == true && manager != null)
                            {
                                manager.SetCurrentScreenByName("SplashScreen");
                            }
                            break;
                    }

                    if (endProcessing)
                        break;
                }
            }
            catch (Exception e) { }
        }
    }
}
