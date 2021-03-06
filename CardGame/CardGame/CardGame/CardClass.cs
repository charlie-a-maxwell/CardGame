﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Storage;
using System.Xml.Serialization;


namespace CardGame
{
    public class MoveLocation
    {
        public MoveLocation()
        {
            x = 0;
            y = 0;
            modifier = 0;
        }
        public MoveLocation(int mod)
        {
            x = 0;
            y = 0;
            modifier = mod;
        }
        public int x;
        public int y;
        public int modifier;

        public override string ToString()
        {
            return modifier.ToString();
        }
    }

    public class CardType
    {
        
        public string typeName;
        public string textureName;
        public Texture2D texture;
        public Texture2D textureLarge;
        [XmlIgnore]
        private PlayerTurn _player;
        [XmlIgnore]
        private bool turnSet = false;
        [XmlIgnore]
        public PlayerTurn player
        {
            get
            {
                if (!turnSet)
                {
                    if (typeName[typeName.Length - 1] == 'A')
                        _player = PlayerTurn.Player1;
                    else
                        _player = PlayerTurn.Player2;
                    turnSet = true;
                }

                return _player;
            }
        }

        [XmlIgnore]
        private int[,] moveOptions;
        [XmlIgnore]
        public int numberOfMoves = 0;
        public int weight = 0;
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
        [XmlIgnore]
        private MoveLocation[,] _moves = null;

        [XmlIgnore]
        public MoveLocation[,] moves
        {
            get
            {
                if (_moves == null)
                {
                    int[,] tempMoves;
                    if (player == PlayerTurn.Player1)
                    {
                        tempMoves = moveOptions;
                    }
                    else
                    {
                        tempMoves = new int[moveOptions.GetLength(0), moveOptions.GetLength(1)];

                        for (int i = 0; i < tempMoves.GetLength(0); i++)
                        {
                            for (int j = 0; j < tempMoves.GetLength(1); j++)
                            {
                                tempMoves[tempMoves.GetLength(0) - i - 1, j] = moveOptions[i, j];
                            }
                        }
                    }

                    _moves = ParseIntMoves(tempMoves);
                }

                return _moves;
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
                textureLarge = cm.Load<Texture2D>(textureName+"L");
            }
        }

        private MoveLocation[,] ParseIntMoves(int[,] m)
        {
            MoveLocation[,] temp = new MoveLocation[m.GetLength(0), m.GetLength(1)];
            for (int i = 0; i < temp.GetLength(0); i++)
            {
                for (int j = 0; j < temp.GetLength(1); j++)
                {
                    if (m[i, j] == -999)
                        temp[i, j] = null;
                    else
                    {
                        int tempX = -1;
                        int tempY = -1;
                        int locX = 0;
                        int locY = 0;
                        int dist = int.MaxValue;
                        int tempDist = 0;
                        int midX = temp.GetLength(0) / 2;
                        int midY = temp.GetLength(1) / 2;


                        temp[i, j] = new MoveLocation(m[i, j]);
                        for (int k = 0; k < 9; k++)
                        {
                            locX = (k % 3) - 1;
                            locY = (int)(k / 3) - 1;
                            if ((locX + i >= 0 && locX + i < temp.GetLength(0)) && (locY + j >= 0 && locY + j < temp.GetLength(1)) && (m[locX + i, locY + j] != -999))
                            {
                                tempDist = (((locX + i) - midX) * ((locX + i) - midX)) + (((locY + j) - midY) * ((locY + j) -midY));
                                if (tempDist < dist)
                                {
                                    tempX = locX + i;
                                    tempY = locY + j;
                                    dist = tempDist;
                                }
                            }
                        }
                        numberOfMoves++;

                        if (tempX != -1 && tempY != -1)
                        {
                            temp[i, j].x = tempX;
                            temp[i, j].y = tempY;
                        }
                    }
                }
            }
            return temp;
        }
    }

    public class CardClass
    {
        CardType type;

        public const int cardWidth = 50;
        public const int cardHeight = 75;
        public PlayerTurn player
        {
            get
            {
                return type.player;
            }
        }
        Vector2 loc;
        Vector2 oldLoc;
        Vector2 movingLoc;
        static Color outlineColor = Color.Yellow;
        static Texture2D circleTex = null;
        private bool placed = false;
        private bool moving = false;
        public bool Selected = false;

        public static void SetCircleText(Texture2D tex)
        {
            circleTex = tex;
        }

        public CardClass(CardType t)
        {
            type = t;
            loc = new Vector2(-1, -1);
            oldLoc = new Vector2(-1, -1);
            movingLoc = new Vector2(-1, -1);
        }

        public CardClass(CardType t, PlayerTurn pt)
        {
            type = t;
            loc = new Vector2(-1, -1);
            oldLoc = new Vector2(-1, -1);
        }

        public void SetMoving(Vector2 v)
        {
            movingLoc = v;
            moving = true;
        }

        public Vector2 GetMovingLoc()
        {
            return movingLoc;
        }

        public void EndMoving()
        {
            moving = false;
        }

        public void Render(SpriteBatch sb)
        {
            Render(sb, 0);
        }

        public void Render(SpriteBatch sb, int space)
        {
            Vector2 curLoc = loc;
            if (moving)
            {
                curLoc = movingLoc;
            }

            SpriteEffects effect = SpriteEffects.None;
            Color textColor = (player == PlayerTurn.Player1 ? Color.Black : Color.DarkRed);

            if (placed && player == PlayerTurn.Player2)
                effect = SpriteEffects.FlipVertically;

            if (type.texture != null)
                sb.Draw(type.texture, new Rectangle((int)curLoc.X, (int)curLoc.Y, cardWidth, cardHeight), null, Color.White, 0.0f, new Vector2(0, 0), effect, 0.5f);

            sb.Draw(circleTex, curLoc + new Vector2((cardWidth / 2.0f) - 15, (player == PlayerTurn.Player1 || !placed ? cardHeight - 30 : 0)), null, Color.White, 0.0f, new Vector2(0, 0), 0.18f, SpriteEffects.None, 0.0f);

            Screen.DrawText(sb, type.GetStat().ToString(), curLoc + new Vector2((cardWidth / 2.0f) - 9, (player == PlayerTurn.Player1 || !placed ? cardHeight - 30 : 0) + 4), textColor, 1.0f, effect);



            if (Selected)
            {
                // Incoming Magic numbers!
                int originX = 30;
                int originY = 30;

                // TEST OF NEW AND IMPROVED TEXTURE LARGES!

                int width = cardWidth * 3;
                int height = cardHeight * 3;
                int xMargin = 5;
                int yMargin = 5;
                //int[,] cardMove = type.GetMove();   //GetMove();

                Vector2 origin = new Vector2();
                Vector2 hor = new Vector2(CardClass.cardWidth, 0);
                Vector2 ver = new Vector2(0, CardClass.cardHeight);

                //int stat = type.GetStat();

                if (type.textureLarge != null)
                    sb.Draw(type.textureLarge, new Rectangle(originX, originX, width, height), null, Color.White, 0.0f, new Vector2(0, 0), SpriteEffects.None, 0.5f);


                if (oldLoc.X != -1 && oldLoc.Y != -1)
                {
                    Color border = outlineColor;
                    for (int i = 0; i < type.moves.GetLength(0); i++)
                    {
                        for (int j = 0; j < type.moves.GetLength(1); j++)
                        {
                            if (type.moves[i, j] != null)
                            {
                                origin = new Vector2((int)loc.X + (cardWidth + space) * (j - 2), (int)loc.Y + (cardHeight + space) * (i - 2));

                                if (origin == oldLoc)
                                    border = Color.DarkRed;
                                else
                                    border = outlineColor;
                                Screen.DrawLine(sb, origin, origin + hor, border, 0.9f);
                                Screen.DrawLine(sb, origin, origin + ver, border, 0.9f);
                                Screen.DrawLine(sb, origin + ver, origin + ver + hor, border, 0.9f);
                                Screen.DrawLine(sb, origin + hor, origin + hor + ver, border, 0.9f);
                            }
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
            if (circleTex == null)
                circleTex = cm.Load<Texture2D>("Circle");
        }

        public void SetLocation(Vector2 l, bool updateLoc)
        {
            if (updateLoc)
            {
                oldLoc = loc;
                placed = true;
            }
            loc = l;
        }

        public Vector2 GetPrevLocation()
        {
            return oldLoc;
        }

        public Vector2 GetLoc()
        {
            return loc;
        }

        public bool Intersect(Vector2 point)
        {
            if (point.X > loc.X && point.Y > loc.Y && point.X < loc.X + cardWidth && point.Y < loc.Y + cardHeight)
                return true;
            return false;
        }

        public MoveLocation[,] GetMove()
        {
            return type.moves;
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
            if (card != null && card.player == owner)
            {
                Vector2 offset = new Vector2(CardClass.cardWidth, 0);
                if (owner == PlayerTurn.Player1)
                    card.SetLocation(renderLoc + offset*hand.Count, false);
                else
                    card.SetLocation(renderLoc - offset * hand.Count, false);
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
                    if (owner == PlayerTurn.Player1)
                        cc.SetLocation(renderLoc + offset * count, false);
                    else
                        cc.SetLocation(renderLoc - offset * count, false);
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

        public CardClass GetSelectedCard()
        {
            CardClass found = null;

            foreach (CardClass card in hand)
            {
                if (card.Selected)
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
                card.Render(sb);
            }
        }

        public void Clear()
        {
            hand.Clear();
        }

        public List<CardClass> GetCardList()
        {
            return hand;
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
                sb.Draw(deckTex, new Rectangle((int)renderLoc.X, (int)renderLoc.Y, CardClass.cardWidth, CardClass.cardHeight), Color.White);
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

        public void ClearDeck()
        {
            deck.Clear();
        }
    }
}
