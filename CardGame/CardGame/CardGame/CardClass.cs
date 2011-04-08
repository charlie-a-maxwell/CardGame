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
        public const int cardWidth = 10;
        public const int cardHeight = 10;


        CardClass()
        {
            type = CardType.Soldier;
            texture = null;
        }

        CardClass(CardType t)
        {
            type = t;
            texture = null;
        }

        CardClass(CardType t, string tm)
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
                sb.Draw(texture, origin, Color.White);
        }

    }
}
