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
    public enum GameType { Player1, Player2};

    class MapView : Screen
    {
        public CardClass[,] map;
        Texture2D MapBackground;
        Texture2D Gate1;
        Texture2D Gate2;
        Vector2 center = new Vector2(0,0);
        List<Turn> turns = new List<Turn>(2);
        Turn activeTurn;
        PlayerTurn winner;
        bool over = false;
        public int deploy = (2 * 3);
        public static int spacing = 5;
        List<CardType> cardTypes;
        GameType type = GameType.Player1;

        public MapView(string n, GraphicsDevice gd) : base(n)
        {
            // This would be where to create the map of whatever size. We are starting with 5 by 5, but the gate is out of that.
            // So we need an extra one on both sides. The other extras will just be marked as filled by something.
            map = new CardClass[7, 7];

            turns.Add(new Turn1(map.GetLength(1), map.GetLength(0)));
            if (type == GameType.Player1)
                turns.Add(new AITurn(map.GetLength(1), map.GetLength(0), this));
            else
                turns.Add(new Turn2(map.GetLength(1), map.GetLength(0)));

            cardTypes = new List<CardType>();

            activeTurn = turns[0];
            winner = 0;

            SetCenter(new Vector2((gd.Viewport.Width - CardClass.cardWidth * map.GetLength(1)) / 2, (gd.Viewport.Height - CardClass.cardHeight * map.GetLength(1)) / 2));
        }

        public MapView() : base()
        {
            // This would be where to create the map of whatever size. We are starting with 5 by 5, but the gate is out of that.
            // So we need an extra one on both sides. The other extras will just be marked as filled by something.
            map = new CardClass[7,7];

            turns[0] = new Turn1(map.GetLength(0), map.GetLength(1));
            if (type == GameType.Player1)
                turns[1] = new AITurn(map.GetLength(0), map.GetLength(1), this);
            else
                turns[1] = new Turn2(map.GetLength(0), map.GetLength(1));


            cardTypes = new List<CardType>();

            activeTurn = turns[0];
            winner = 0;
        }

        protected void LoadDecks()
        {

            XmlSerializer serializer = new XmlSerializer(typeof(List<string>));
            Stream deckstream = File.Open("Content/Deck1.xml", FileMode.Open);
            List<string> list = (List<string>)serializer.Deserialize(deckstream);
            deckstream.Close();

            turns[0].BuildDeck(list, cardTypes);
            //BuildPlayerDeck(list, PlayerTurn.Player1);

            deckstream = File.Open("Content/Deck2.xml", FileMode.Open);
            list = (List<string>)serializer.Deserialize(deckstream);
            deckstream.Close();

            turns[1].BuildDeck(list, cardTypes);
            //BuildPlayerDeck(list, PlayerTurn.Player2);
        }

        public override void LoadContent(ContentManager cm)
        {
            base.LoadContent(cm);

            MapBackground = cm.Load<Texture2D>("MapBack");
            Gate1 = cm.Load<Texture2D>("DoorPlayer1");
            Gate2 = cm.Load<Texture2D>("DoorPlayer2");

            foreach (Turn t in turns)
                t.LoadTexture(cm);

            foreach (CardType cc in cardTypes)
            {
                if (cc != null)
                    cc.LoadTexture(cm);
            }
        }

        public void SetCenter(Vector2 cent)
        {
            center = cent;
            foreach (Turn t in turns)
                t.SetCenterLoc(cent, map.GetLength(1), map.GetLength(0));
            
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

        public override void Render(SpriteBatch sb, GraphicsDevice device)
        {
            int maxCardWidth = CardClass.cardWidth * (map.GetLength(1)-2);
            int maxCardHeight = CardClass.cardHeight * (map.GetLength(0)-2);
            Vector2 hor = new Vector2(CardClass.cardWidth, 0);
            Vector2 ver = new Vector2(0, CardClass.cardHeight);
            Color outlineColor = Color.Black;

            Vector2 origin;
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
                        map[i, j].Render(sb, spacing);
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
                foreach (Turn t in turns)
                    t.Render(sb);
                text = (winner == PlayerTurn.Player1 ? "Player 1" : "Player 2");
            }
            else 
            {
                activeTurn.RenderHand(sb);
                text = activeTurn.GetPlayerTurn().ToString();
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


            foreach (Turn t in turns)
                t.RenderDeck(sb);
        }

        public bool PlaceCard(CardClass card, int x, int y)
        {
            return PlaceCard(card, x, y, 0, 0);
        }


        public bool PlaceCard(CardClass card, int x, int y, int transX, int transY)
        {
            if ( x >= 0 && y >= 0 && x < map.GetLength(1) && y < map.GetLength(0))
            {
                CardClass replacedCard = map[y, x];
                CardClass winner = card;
                int modifier = 0;
                bool forwardMove = false;
                if (!(transX == 0 && transY == 0))
                {
                    modifier = card.GetMove()[transY, transX].modifier;
                    forwardMove = card.GetMove()[transY, transX].y == transX;
                }
                
                if (replacedCard != null && replacedCard.player != card.player)
                    // Check to see which one wins in this battle.
                    winner = replacedCard.GetCardType().GetStat() <= card.GetCardType().GetStat() + modifier ? card : replacedCard;
                else if (replacedCard != null)
                {
                    string type = card.GetCardType().typeName.Substring(0, card.GetCardType().typeName.Length - 1);
                    string replaceType = replacedCard.GetCardType().typeName.Substring(0, replacedCard.GetCardType().typeName.Length - 1);
                    string lastLetter = card.GetCardType().typeName.Substring(card.GetCardType().typeName.Length - 1);
                    if (type.Equals("Soldier") && replaceType.Equals("Soldier"))
                    {
                        foreach (CardType ct in cardTypes)
                        {
                            if (ct.typeName.ToLower().ToString().Equals("stack" + lastLetter.ToLower()))
                            {
                                winner = new CardClass(ct, card.player);
                                winner.SetLocation(replacedCard.GetLoc(), true);
                                break;
                            }
                        }

                        if (winner == null)
                            return false;
                    }
                    else
                        // tried to move onto own card.
                        return false;
                }

                if (y == activeTurn.GateLane())
                    return false;

                // Check for movement into the gates
                Turn other = turns.Find(delegate(Turn t) { return t != activeTurn; });
                if (other != null && y == other.GateLane())
                {
                    if (!forwardMove || x != 3)
                    {
                        return false;
                    }
                }

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
                    turns[0].RemoveCardFromHand(card);
                else if (card.player == PlayerTurn.Player2)
                    turns[1].RemoveCardFromHand(card);

                return true;
            }

            return false;
        }

        public Vector2 ConvertScreenCoordToMap(Vector2 pos)
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
            bool cardFound = false;
            foreach(CardClass card in map)
            {
                if (card != null && card.Intersect(pos))
                {
                    card.Selected = true;
                    cardFound = true;
                    ConvertScreenCoordToMap(pos);
                    break;
                }
            }

            if (!cardFound)
            {
                CardClass c = activeTurn.SelectCard(pos);
                if (c != null)
                    c.Selected = true;
            }
        }

        private void SwitchTurns()
        {
            CheckWin();
            if (activeTurn.HandCount() < 3)
                activeTurn.AddToHand();

            if (deploy > 0)
                deploy--;

            activeTurn = (turns[0] == activeTurn ? turns[1] : turns[0]);
            ResetSelectedCard();
        }

        public Turn GetTurn(PlayerTurn pt)
        {
            if (turns[0].GetPlayerTurn() == pt)
                return turns[0];
            else
                return turns[1];
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
            if ((turns[0].HandCount() == 0 && !PlayerCardsLeft(PlayerTurn.Player1)) || map[map.GetLength(0) - 1, 3] != null)
            {
                // player 2 wins by kill!
                winner = PlayerTurn.Player2;
                over = true;
            }

            if ((turns[1].HandCount() == 0 && !PlayerCardsLeft(PlayerTurn.Player2)) || map[0, 3] != null)
            {
                // player 1 wins by kill!
                winner = PlayerTurn.Player1;
                over = true;
            }
        }

        public void MoveCard(CardClass card, Vector2 pos)
        {
            Vector2 mapLoc = ConvertScreenCoordToMap(pos);
            MoveCard(card, (int)mapLoc.X, (int)mapLoc.Y);
        }

        public void MoveCard(CardClass card, int x, int y)
        {
            Vector2 mapLoc = new Vector2(x, y);
            // Can't move to the sides.
            if (mapLoc.X == 0 || mapLoc.X == map.GetLength(0) - 1)
                return;

            if (card == null)
                return;

            Vector2 cardLoc = ConvertScreenCoordToMap(card.GetLoc());

            // Handle first card placement.
            if (cardLoc.X == -1 && cardLoc.Y == -1)
            { 
                // placing a new card.
                if (activeTurn.InDeploymentZone(mapLoc) && map[(int)mapLoc.Y, (int)mapLoc.X] == null && PlaceCard(card, (int)mapLoc.X, (int)mapLoc.Y))
                    SwitchTurns();
                else
                    ResetSelectedCard();
            }
            // Handle card movement.
            else if (deploy == 0 && mapLoc.X >= 0 && mapLoc.Y >= 0 && mapLoc.X < map.GetLength(0) && mapLoc.Y < map.GetLength(1) && card.player == activeTurn.GetPlayerTurn())
            {
                MoveLocation[,] moveOption = card.GetMove();
                int transX = (int)(mapLoc.X - cardLoc.X) + 2;
                int transY = (int)(mapLoc.Y - cardLoc.Y) + 2;

                if (transX == 2 && transY == 2) // center of the move map IE Early out
                    return;

                // Check for the actual move.
                if (transX >= 0 && transY >= 0 && transX < moveOption.GetLength(0) && transY < moveOption.GetLength(1) && moveOption[transY, transX] != null)
                {
                    bool good = false;
                    good = RecursiveCardMovement(card, cardLoc, moveOption[transY, transX], moveOption, transX, transY);
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

        private bool RecursiveCardMovement(CardClass card, Vector2 cardLoc, MoveLocation currentLoc, MoveLocation[,] map, int transX, int transY)
        {
            if (transX == 2 && transY == 2)
            {
                return true;
            }

            bool placed = false;


            Stack<MoveLocation> order = new Stack<MoveLocation>(2);
            while (currentLoc.x >= 0 && currentLoc.y >= 0 && !(currentLoc.x == 2 && currentLoc.y == 2))
            {
                order.Push(currentLoc);
                currentLoc = map[currentLoc.x, currentLoc.y];
            }
            order.Push(currentLoc);

            MoveLocation next;
            while (order.Count > 1)
            {
                currentLoc = order.Pop();
                next = order.Peek();
                placed = PlaceCard(card, (int)cardLoc.X + next.y - 2, (int)cardLoc.Y + next.x - 2, next.y, next.x) || placed;
                if (!placed)
                    return placed;
            }


            //if (currentLoc.x >= 0 && currentLoc.y >= 0 && !(currentLoc.x == 2 && currentLoc.y == 2))
            //{
            //    if (!RecursiveCardMovement(card, cardLoc, map[currentLoc.x, currentLoc.y], map, currentLoc.y, currentLoc.x))
            //        return recurse;
            //}
            ////PlaceCard(selectedCard, (int)mapLoc.X, (int)mapLoc.Y, moveOption[transY, transX].modifier)
            placed = PlaceCard(card, (int)cardLoc.X + transX - 2, (int)cardLoc.Y + transY - 2, transX, transY) || placed;
            return placed;
        }

        public override void Update(GameTime gt)
        {
            activeTurn.Update(gt);
        }

        private void ResetSelectedCard()
        {
            //selectedCard = null;
            //selectedCardLoc.X = -1;
            //selectedCardLoc.Y = -1;
            CardClass selectedCard = null;
            foreach (CardClass c in map)
            {
                if (c != null && c.Selected)
                {
                    selectedCard = c;
                    break;
                }
            }

            if (selectedCard == null)
            {
                selectedCard = activeTurn.GetSelectedCard();
            }

            if (selectedCard != null)
                selectedCard.Selected = false;
        }

        public override void HandleMouseClick(Vector2 pos)
        {
            if (over)
                return;

            CardClass selectedCard = null;

            foreach (CardClass c in map)
                if (c != null && c.Selected)
                    selectedCard = c;

            if (selectedCard == null)
                selectedCard = activeTurn.GetSelectedCard();

            if (selectedCard == null)
            {
                ScreenCardSelect(pos);
            }
            else
            {
                MoveCard(selectedCard, pos);
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

            deploy = 6;
            over = false;

            //turns[0] = new Turn1(map.GetLength(1), map.GetLength(0));
            //turns[1] = new Turn2(map.GetLength(1), map.GetLength(0));
            //player1Hand = new Hand(PlayerTurn.Player1);
            //player2Hand = new Hand(PlayerTurn.Player2);
            //player1Hand.SetRenderLoc(new Vector2(center.X - (map.GetLength(1) * CardClass.cardWidth) / 2.0f - 40, center.Y + (map.GetLength(0) * CardClass.cardHeight) - CardClass.cardHeight));
            //player2Hand.SetRenderLoc(new Vector2(center.X + (map.GetLength(1) * CardClass.cardWidth) - 20 + CardClass.cardWidth * 4, center.Y));

            SetCenter(center);


            LoadDecks();

            foreach (Turn t in turns)
            {
                t.ClearHand();
                t.ShuffleDeck();

                for (int i = 0; i < 5; i++)
                {
                    t.AddToHand();
                }
            }

            Random rand = new Random();
            activeTurn = turns[rand.Next(turns.Count -1)];
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
