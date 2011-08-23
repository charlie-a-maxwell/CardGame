using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace CardGame
{
    abstract class Entity
    {
        protected Vector2 loc;
        protected Texture2D tex;

        public Entity(Vector2 l)
        {
            loc.X = l.X;
            loc.Y = l.Y;
            tex = null;
        }

        public abstract void Update(GameTime gt);

        public abstract void LoadTexture(ContentManager cm);

        public virtual void Render(SpriteBatch sb)
        {
            if (tex != null)
            {
                sb.Draw(tex, loc, Color.White);
            }
        }
    }
}
