using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace CardGame
{
    class Map
    {
        CardClass[,] map;
        Map()
        {
            // This would be where to create the map of whatever size. We are starting with 5 by 5, but the gate is out of that.
            // So we need an extra one on both sides. The other extras will just be marked as filled by something.
            map = new CardClass[6,6];
        }

        public void RenderMap(SpriteBatch sb)
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    map[i,j].Render(sb, new Vector2(j*CardClass.cardWidth, i*CardClass.cardHeight));
                }
            }
        }

        public void PlaceCard(CardClass card, int x, int y)
        {
            if (map[y, x] == null)
            {
                map[y, x] = card;
            }
        }
    }
}
