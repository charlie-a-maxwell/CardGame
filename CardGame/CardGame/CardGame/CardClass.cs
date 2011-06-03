using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Storage;
using System.Xml.Serialization;


namespace CardGame
{
    public class CardType
    {
        
        public string typeName;
        public string textureName;
        public Texture2D texture;
        [XmlIgnore]
        private int[,] moveOptions;
        
        public string moveString
        {
            get
            {
                string data = "";
                for (int i = 0; i < moveOptions.GetLength(0); i++)
                {
                    for (int j = 0; j < moveOptions.GetLength(1); j++)
                    {
                        data += moveOptions[i, j] + ",";
                    }
                    data = data.TrimEnd(',');
                    data += "|";
                }
                data = data.TrimEnd('|');

                return data;
            }

            set
            {
                string data = value;

                string[] rows = data.Split('|');
                moveOptions = new int[rows.Length, rows[0].Split(',').Length];
                int i= 0, j=0;
                foreach (string col in rows)
                {
                    foreach (string item in col.Split(','))
                        moveOptions[i, j++] = Convert.ToInt32(item);
                    j = 0;
                    i++;
                }
            }
        }

        public CardType()
        {
        }

        public CardType(string name, string texName)
        {
            typeName = name;
            textureName = texName;
            moveOptions = new int[5, 5];
        }

        public CardType(string name, string texName, int[,] move)
        {
            typeName = name;
            textureName = texName;
            moveOptions = new int[5, 5];

            SetMove(move);
        }

        public int GetStat()
        {
            return moveOptions[2,2];
        }

        public void SetMove(int[,] move)
        {
            if (move.GetLength(0) == moveOptions.GetLength(0) && move.GetLength(1) == moveOptions.GetLength(1))
            {
                moveOptions = (int[,])move.Clone();
            }
        }

        public int[,] GetMove()
        {
            return moveOptions;
        }

        public void LoadTexture(ContentManager cm)
        {
            if (textureName != null && textureName.Length > 0 && texture == null)
            {
                texture = cm.Load<Texture2D>(textureName);
            }
        }
    }

    public class CardClass
    {
        CardType type;

        public const int cardWidth = 50;
        public const int cardHeight = 50;
        public PlayerTurn player;
        Vector2 loc;
        Vector2 oldLoc;

        public CardClass(CardType t)
        {
            player = PlayerTurn.Player1;
            type = t;
            loc = new Vector2(-1, -1);
            oldLoc = new Vector2(-1, -1);
        }

        public CardClass(CardType t, PlayerTurn pt)
        {
            player = pt;
            type = t;
            loc = new Vector2(-1, -1);
            oldLoc = new Vector2(-1, -1);
        }

        public void Render(SpriteBatch sb)
        {
            Render(sb, false);
        }

        public void Render(SpriteBatch sb, bool selected)
        {
            if (type.texture != null)
                sb.Draw(type.texture, new Rectangle((int)loc.X, (int)loc.Y, cardWidth, cardHeight), (player == PlayerTurn.Player1 ? Color.Red : Color.LightBlue));

            if (selected)
            {
                // Incoming Magic numbers!
                int originX = 30;
                int originY = 30;
                int width = 95;
                int height = 145;
                int xMargin = 5;
                int yMargin = 5;
                int[,] cardMove = GetCardType().GetMove();

                float xSize = (width - xMargin * 2) / cardMove.GetLength(1);
                float ySize = (height - CardClass.cardHeight - yMargin * 2) / cardMove.GetLength(0);
                float offsetX = xMargin + originX;
                float offsetY = originY + CardClass.cardHeight + yMargin * 2;


                MapView.FillColor(sb, originX, originY, width, height, Color.White);
                MapView.DrawLine(sb, new Vector2(originX, originY), new Vector2(originX, originY + height), Color.Black);
                MapView.DrawLine(sb, new Vector2(originX, originY), new Vector2(originX + width, originY), Color.Black);
                MapView.DrawLine(sb, new Vector2(originX + width, originY), new Vector2(originX + width, originY + height), Color.Black);
                MapView.DrawLine(sb, new Vector2(originX, originY + height), new Vector2(originX + width, originY + height), Color.Black);

                //selectedCard.Render(sb, new Vector2(originX + (width - CardClass.cardWidth) / 2, originY + yMargin));

                for (int i = 0; i < cardMove.GetLength(0); i++)
                {
                    for (int j = 0; j < cardMove.GetLength(1); j++)
                    {
                        if (cardMove[i, j] != 0)
                        {
                            MapView.DrawLine(sb, new Vector2(xSize * j + offsetX, ySize * i + offsetY), new Vector2(xSize * j + offsetX, ySize * (i + 1) + offsetY), Color.Black);
                            MapView.DrawLine(sb, new Vector2(xSize * j + offsetX, ySize * i + offsetY), new Vector2(xSize * (j + 1) + offsetX, ySize * i + offsetY), Color.Black);
                            MapView.DrawLine(sb, new Vector2(xSize * (j + 1) + offsetX, ySize * i + offsetY), new Vector2(xSize * (j + 1) + offsetX, ySize * (i + 1) + offsetY), Color.Black);
                            MapView.DrawLine(sb, new Vector2(xSize * j + offsetX, ySize * (i + 1) + offsetY), new Vector2(xSize * (j + 1) + offsetX, ySize * (i + 1) + offsetY), Color.Black);
                            MapView.DrawText(sb, cardMove[i, j].ToString(), new Vector2(xSize * j + offsetX, ySize * i + offsetY));

                            MapView.DrawLine(sb, new Vector2((int)loc.X + cardWidth * (j - 2), (int)loc.Y + cardHeight * (i - 2)), new Vector2((int)loc.X + cardWidth * (j - 2), (int)loc.Y + cardHeight * (i - 1)), Color.Orange);
                            MapView.DrawLine(sb, new Vector2((int)loc.X + cardWidth * (j - 2), (int)loc.Y + cardHeight * (i - 2)), new Vector2((int)loc.X + cardWidth * (j - 1), (int)loc.Y + cardHeight * (i - 2)), Color.Orange);
                            MapView.DrawLine(sb, new Vector2((int)loc.X + cardWidth * (j - 1), (int)loc.Y + cardHeight * (i - 2)), new Vector2((int)loc.X + cardWidth * (j - 1), (int)loc.Y + cardHeight * (i - 1)), Color.Orange);
                            MapView.DrawLine(sb, new Vector2((int)loc.X + cardWidth * (j - 2), (int)loc.Y + cardHeight * (i - 1)), new Vector2((int)loc.X + cardWidth * (j - 1), (int)loc.Y + cardHeight * (i - 1)), Color.Orange);

                            if (type.texture != null)
                                sb.Draw(type.texture, new Rectangle(originX + (width - CardClass.cardWidth) / 2, originX + (width - CardClass.cardWidth) / 2, cardWidth, cardHeight), (player == PlayerTurn.Player1 ? Color.Red : Color.LightBlue));

                        }
                    }
                }

            }
        }

        public CardType GetCardType()
        {
            return type;
        }

        public void LoadTexture(ContentManager cm)
        {
            type.LoadTexture(cm);
        }

        public void SetLocation(Vector2 l)
        {
            oldLoc = loc;
            loc = l;
        }

        public Vector2 GetPrevLocation()
        {
            return oldLoc;
        }

        public bool Intersect(Vector2 point)
        {
            if (point.X > loc.X && point.Y > loc.Y && point.X < loc.X + cardWidth && point.Y < loc.Y + cardHeight)
                return true;
            return false;
        }
    }

    public class Hand
    {
        List<CardClass> hand;
        PlayerTurn owner;
        Vector2 renderLoc;

        public Hand(PlayerTurn player)
        {
            hand = new List<CardClass>();
            owner = player;
            renderLoc = new Vector2(0, 0);
        }

        public void SetRenderLoc(Vector2 loc)
        {
            renderLoc = loc;
        }

        public bool AddCard(CardClass card)
        {
            if (card.player == owner)
            {
                Vector2 offset = new Vector2(CardClass.cardWidth, 0);
                card.SetLocation(renderLoc + offset*hand.Count);
                hand.Add(card);
                return true;
            }
            return false;
        }

        public int Count
        {
            get
            {
                return hand.Count;
            }
        }

        public bool RemoveCard(CardClass card)
        {
            bool found = false;
            Vector2 offset = new Vector2(CardClass.cardWidth, 0);
            int count = 0;
            foreach (CardClass cc in hand)
            {
                if (cc == card && !found)
                {
                    found = true;
                    count--;
                }
                else if (found)
                {
                    cc.SetLocation(renderLoc + offset * count);
                }
                count++;
            }
            return hand.Remove(card);
        }

        public CardClass SelectCard(int i)
        {
            if (i >= 0 && i < hand.Count)
            {
                return hand[i];
            }

            return null;
        }

        public CardClass SelectCard(Vector2 pos)
        {
            CardClass found = null;
            foreach (CardClass card in hand)
            {
                if (card.Intersect(pos))
                {
                    found = card;
                    break;
                }
            }

            return found;
        }

        public void Render(SpriteBatch sb)
        {
            Render(sb, renderLoc, null);
        }

        public void Render(SpriteBatch sb, CardClass selectedCard)
        {
            Render(sb, renderLoc, selectedCard);
        }

        public void Render(SpriteBatch sb, Vector2 origin, CardClass selectedCard)
        {
            CardClass card;
            for (int i = 0; i < hand.Count; i++ )
            {
                card = hand[i];
                card.Render(sb, card == selectedCard);
            }
        }
    }

    public class Deck
    {
        Stack<CardClass> deck;
        PlayerTurn owner;
        static Random rand = new Random();
        Vector2 renderLoc;
        Texture2D deckTex;

        public Deck(PlayerTurn player)
        {
            deck = new Stack<CardClass>();
            owner = player;
        }

        public void SetTexure(Texture2D tex)
        {
            deckTex = tex;
        }

        public void SetLoc(Vector2 v)
        {
            renderLoc = v;
        }

        public CardClass GetTopCard()
        {
            if (deck.Count > 0)
                return deck.Pop();
            return null;
        }

        public void ShuffleCurrentDeck()
        {
            CardClass[] deckArray = deck.ToArray();
            int first, second;
            int length = deckArray.Length;
            CardClass temp;
            for (int i = 0; i < 30; i++)
            {
                first = rand.Next(length);
                second = rand.Next(length);
                temp = deckArray[first];
                deckArray[first] = deckArray[second];
                deckArray[second] = temp;
            }

            deck = new Stack<CardClass>(deckArray);
        }

        public void Render(SpriteBatch sb)
        {
            if (deckTex != null && deck.Count > 0)
                sb.Draw(deckTex, new Rectangle((int)renderLoc.X, (int)renderLoc.Y, CardClass.cardWidth, CardClass.cardHeight), (owner == PlayerTurn.Player1 ? Color.Red : Color.LightBlue));
        }

        public bool Intersect(Vector2 point)
        {
            if (point.X > renderLoc.X && point.Y > renderLoc.Y && point.X < renderLoc.X + CardClass.cardWidth && point.Y < renderLoc.Y + CardClass.cardHeight)
                return true;
            return false;
        }


        public bool AddCard(CardClass card)
        {
            if (card.player == owner)
            {
                deck.Push(card);
                return true;
            }
            return false;
        }

        public void SendToGraveYard()
        {

        }
    }
}
