using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;


namespace CardGame
{
    enum CardType { Soldier, Rogue, Joker, Spearman, Defender };

    class CardClass
    {
        Texture2D texture;
        CardType type;
        string textureName;
        public const int cardWidth = 50;
        public const int cardHeight = 50;


        public CardClass()
        {
            type = CardType.Soldier;
            texture = null;
        }

        public CardClass(CardType t)
        {
            type = t;
            texture = null;
        }

        public CardClass(CardType t, string tm)
        {
            type = t;
            textureName = tm;
            texture = null;
        }

        public void LoadTexture(ContentManager cm)
        {
            if (textureName != null && textureName.Length > 0 && texture == null)
            {
                texture = cm.Load<Texture2D>(textureName);
            }
        }

        public void Render(SpriteBatch sb, Vector2 origin)
        {
            if (texture != null)
                sb.Draw(texture, new Rectangle((int)origin.X, (int)origin.Y, cardWidth, cardHeight), Color.White);
        }

    }
}
