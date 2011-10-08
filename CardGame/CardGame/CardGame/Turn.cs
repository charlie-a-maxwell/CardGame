using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Storage;


namespace CardGame
{
    abstract class Turn : Entity
    {
        protected PlayerTurn turn;
        protected Hand hand;
        protected Deck deck;
        protected Vector2 deployment;

        public Turn(PlayerTurn pt, Vector2 loc): base(loc)
        {
            turn = pt;
            hand = new Hand(pt);
            deck = new Deck(pt);
        }

        public PlayerTurn GetPlayerTurn()
        {
            return turn;
        }

        public override void Render(SpriteBatch sb)
        {
        }

        public void RenderHand(SpriteBatch sb)
        {
            hand.Render(sb);
        }

        public void RenderDeck(SpriteBatch sb)
        {
            deck.Render(sb);
        }

        public override void Update(GameTime gt)
        {

        }

        public abstract void SetCenterLoc(Vector2 center, int maxX, int maxY);

        public void RemoveCardFromHand(CardClass cc)
        {
            hand.RemoveCard(cc);
        }

        public CardClass SelectCard(Vector2 loc)
        {
            return hand.SelectCard(loc);
        }

        public int HandCount()
        {
            return hand.Count;
        }

        public void AddToHand()
        {
            hand.AddCard(deck.GetTopCard());
        }

        public bool InDeploymentZone(Vector2 mapLoc)
        {
            return (mapLoc.Y == deployment.Y);
        }

        public void BuildDeck(List<string> list, List<CardType> cardTypes)
        {
            deck.ClearDeck();
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
                    deck.AddCard(new CardClass(type, turn));
                }
            }
        }

        public void ShuffleDeck()
        {
            deck.ShuffleCurrentDeck();
        }

        public abstract int GateLane();
    }

    class Turn1 : Turn
    {
        public Turn1(int maxX, int maxY) : base(PlayerTurn.Player1, new Vector2(0,0))
        {
            //mapLoc.Y == map.GetLength(1) - 2 && mapLoc.X > 0 && mapLoc.X < map.GetLength(0)-1
            deployment = new Vector2(0, maxY - 2);
        }

        public override void SetCenterLoc(Vector2 center, int maxX, int maxY)
        {
            hand.SetRenderLoc(new Vector2(center.X - (maxX * CardClass.cardWidth) / 2.0f - 40, center.Y + (maxY * CardClass.cardHeight) - CardClass.cardHeight));
            deck.SetLoc(new Vector2(center.X - (maxX * CardClass.cardWidth) / 2.0f, center.Y + (maxY * CardClass.cardHeight) - CardClass.cardHeight * 2));
        }

        public override int GateLane()
        {
            return (int)deployment.Y + 1;
        }

        public override void LoadTexture(ContentManager cm)
        {
            deck.SetTexure(cm.Load<Texture2D>("DeckBackA"));
        }
    }

    class Turn2 : Turn
    {
        public Turn2(int maxX, int maxY) : base(PlayerTurn.Player2, new Vector2(0,0))
        {
            //mapLoc.Y == 1 && mapLoc.X > 0 && mapLoc.X < map.GetLength(0) - 1
            deployment = new Vector2(0, 1);
        }

        public override void SetCenterLoc(Vector2 center, int maxX, int maxY)
        {
            hand.SetRenderLoc(new Vector2(center.X + (maxX * CardClass.cardWidth) - 20 + CardClass.cardWidth * 4, center.Y));
            deck.SetLoc(new Vector2(center.X + (maxX * CardClass.cardWidth) - 20, center.Y + CardClass.cardHeight));
        }

        public override int GateLane()
        {
            return (int)deployment.Y - 1;
        }

        public override void LoadTexture(ContentManager cm)
        {
            deck.SetTexure(cm.Load<Texture2D>("DeckBackD"));
        }
    }
}
