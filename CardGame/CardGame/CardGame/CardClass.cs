using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;


namespace CardGame
{
    public class CardType
    {
        string typeName;
        string textureName;
        public Texture2D texture;
        int[,] moveOptions;
        int stat;

        public CardType(string name, string texName)
        {
            typeName = name;
            textureName = texName;
            moveOptions = new int[5, 5];
        }

        public CardType(string name, string texName, int[,] move, int s)
        {
            typeName = name;
            textureName = texName;
            moveOptions = new int[5, 5];
            stat = s;

            SetMove(move);
        }

        public int GetStat()
        {
            return stat;
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

        public CardClass(CardType t)
        {
            player = PlayerTurn.Player1;
            type = t;
        }

        public CardClass(CardType t, PlayerTurn pt)
        {
            player = pt;
            type = t;
        }

        public void Render(SpriteBatch sb, Vector2 origin)
        {
            if (type.texture != null)
                sb.Draw(type.texture, new Rectangle((int)origin.X, (int)origin.Y, cardWidth, cardHeight), (player == PlayerTurn.Player1 ? Color.Red: Color.LightBlue));
        }

        public CardType GetCardType()
        {
            return type;
        }

        public void LoadTexture(ContentManager cm)
        {
            type.LoadTexture(cm);
        }
    }

    public class Deck
    {
        Stack<CardClass> deck;

        public Deck()
        {
            deck = new Stack<CardClass>();
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
            Random rand = new Random();
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

        public void SendToGraveYard()
        {

        }
    }
}
