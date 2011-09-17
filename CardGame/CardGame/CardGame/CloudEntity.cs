using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace CardGame
{
    class CloudEntity : Entity
    {
        double rot;
        double rotSpeed;
        double dis;
        double dis2;
        float spin;
        float spinSpeed;
        static Random rand = new Random();
        Vector2 rendLoc;

        public CloudEntity(Vector2 l) : base(l) 
        {
            rot = rand.NextDouble() * (Math.PI * 2);
            rotSpeed = (rand.NextDouble() + 0.1) / 1000.0f;
            spinSpeed = (float)(rand.NextDouble() + 0.1) / 500.0f;
            rendLoc = new Vector2(0, 0);
            dis = rand.Next(130, 300);
            dis2 = rand.Next(320, 450);
        }

        public override void LoadTexture(ContentManager cm)
        {
            int r = rand.Next(1)+1;

            tex = cm.Load<Texture2D>("CloudT" + r);

            int maxDis;
            maxDis = (tex.Width > tex.Height ? tex.Width : tex.Height);
            maxDis = maxDis >> 2;
            dis += maxDis;
            dis2 += maxDis;
        }

        public override void Update(GameTime gt)
        {
            rot += rotSpeed;
            if (rot > Math.PI * 2)
                rot = rot % (Math.PI * 2);

            rendLoc.X = loc.X + (float)(dis * Math.Cos(rot));
            rendLoc.Y = loc.Y + (float)(dis2 * Math.Sin(rot));

            spin += spinSpeed;
            if (spin > Math.PI * 2)
                spin = spin % (float)(Math.PI * 2);
        }

        public override void Render(SpriteBatch sb)
        {
            if (tex != null)
            {
                sb.Draw(tex, rendLoc, null, Color.White, spin, new Vector2((float)tex.Width / 2.0f, (float)tex.Height / 2.0f), 0.5f, SpriteEffects.None, 0.2f);
            }
        }
    }
}
