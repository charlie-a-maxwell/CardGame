using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Storage;
using System.Diagnostics;

namespace CardGame
{
    class DecisionNode
    {
        List<DecisionNode> children;
        public Vector2 cardPrevLoc;
        public Vector2 cardStart;
        public Vector2 cardEnd;
        public PlayerTurn pt;
        public int Value;
        public int moveI;
        public int moveJ;

        public DecisionNode(int maxX, int maxY, PlayerTurn p)
        {
            children = new List<DecisionNode>(10);
            cardStart = new Vector2(-1, -1);
            cardEnd = new Vector2(-1, -1);
            cardPrevLoc = new Vector2(-1, -1);
            pt = p;
            Value = 0;
        }

        public void AddChild(DecisionNode dn)
        {
            children.Add(dn);
        }

        public List<DecisionNode> GetChildren()
        {
            return children;
        }
    }

    class AITurn : Turn2
    {
        DecisionNode rootNode;
        int X;
        int Y;
        MapView map;
        Random rand = new Random();
        int WinValue = 600;
        int maxDepth = 6;
        Stopwatch Watch = new Stopwatch();

        public AITurn(int maxX, int maxY, MapView mv)
            : base(maxX, maxY)
        {
            X = maxX;
            Y = maxY;
            rootNode = new DecisionNode(maxX, maxY, PlayerTurn.Player2);
            map = mv;
        }

        public override void Update(GameTime gt)
        {
            int depth = 0;
            CardClass[,] temp = new CardClass[7,7];
            for (int i = 0; i < map.map.GetLength(0); i++)
            {
                for (int j = 0; j < map.map.GetLength(1); j++)
                {
                    CardClass cc = map.map[i, j];
                    if (cc != null)
                    {
                        temp[i, j] = new CardClass(cc.GetCardType(), cc.GetCardType().player);
                    }
                }
            }
            rootNode = new DecisionNode(X, Y, PlayerTurn.Player2);

            //rootNode.currState = ms;

            Watch.Reset();
            Watch.Start();
            DecisionNode solution = null;
           // while (depth <= maxDepth)
            {
                DLS(rootNode, 0, -999999, 999999);
                //if (Watch.ElapsedMilliseconds >= 500)
                //    break;
                depth++;
            }
            Watch.Stop();
            int value = int.MinValue;
            foreach (DecisionNode n in rootNode.GetChildren())
            {
                if (n.Value > value)
                {
                    value = n.Value;
                    solution = n;
                }
            }

            if (solution == null && rootNode.GetChildren().Count > 0)
                solution = rootNode.GetChildren()[0];

            if (solution != null)
            {
                CardClass cc;
                if (solution.cardStart.X < 0)
                    cc = hand.SelectCard((int)solution.cardStart.Y);
                else
                    cc = map.map[(int)solution.cardStart.X, (int)solution.cardStart.Y];
                map.MoveCard(cc, (int)solution.cardEnd.X, (int)solution.cardEnd.Y);
            }
        }

        private bool TestWin(DecisionNode node)
        {
            if (map.map[0, 3] != null)
                return true;
            else if (map.map[5, 3] != null)
                return true;
            else
                return false;
        }

        private int ChildNodeSort(DecisionNode n1, DecisionNode n2)
        {
            return n2.Value - n1.Value;
        }

        private int DLS(DecisionNode node, int depth, int alpha, int beta)
        {

            //if (Watch.ElapsedMilliseconds >= 500)
            //    return node.Value; // no more time


            if (depth == maxDepth)
            {
                node.Value = NodeEval(map.map, node.pt, node.cardStart, node.cardEnd) * (node.pt == PlayerTurn.Player2 ? 1 : -1);
                return node.Value;
            }

            //if (TestWin(node))
            //{
            //    node.Value = WinValue;
            //    return node.Value;
            //}

            if (node.GetChildren().Count == 0)
            {
                ExpandNode(node, depth);
                node.GetChildren().Sort(ChildNodeSort);
            }

            // Maximize!
            CardClass movedCard;

            if (node.GetChildren().Count == 0)
            {
                node.Value = NodeEval(map.map, node.pt, node.cardStart, node.cardEnd) * (node.pt == PlayerTurn.Player2 ? 1 : -1);
                return node.Value;
            }

            int value = 0;
            foreach (DecisionNode n in node.GetChildren())
            {
                if (n.cardStart.X == -1)
                {
                    Turn activeTurn = map.GetTurn(node.pt);
                    movedCard = activeTurn.SelectCard((int)n.cardStart.Y);
                }
                else
                    movedCard = map.map[(int)n.cardStart.X, (int)n.cardStart.Y];

                bool placed = false;
                placed = map.MoveCardAI(movedCard, (int)n.cardEnd.X, (int)n.cardEnd.Y, depth);

                value = -DLS(n, depth+1, -beta, -alpha);

                if (placed)
                    map.UndoMove(movedCard);

                if (value >= beta)
                {
                    node.Value = beta;
                    return beta;
                }
                if (value >= alpha)
                    alpha = value;
            }

            node.Value = alpha;
            return alpha;
        }

        private int DLSRoot(DecisionNode node, int depth, int alpha, int beta)
        {

            ExpandNode(node, depth);

            // Maximize!
            CardClass movedCard;

            if (node.GetChildren().Count == 0)
            {
                node.Value = NodeEval(map.map, node.pt, node.cardStart, node.cardEnd) * (node.pt == PlayerTurn.Player2 ? 1 : -1);
                return node.Value;
            }

            int value = 0;
            foreach (DecisionNode n in node.GetChildren())
            {
                if (n.cardStart.X == -1)
                {
                    Turn activeTurn = map.GetTurn(node.pt);
                    movedCard = activeTurn.SelectCard((int)n.cardStart.Y);
                }
                else
                    movedCard = map.map[(int)n.cardStart.X, (int)n.cardStart.Y];

                bool placed = false;
                placed = map.MoveCardAI(movedCard, (int)n.cardEnd.X, (int)n.cardEnd.Y, depth);

                value = -DLS(n, depth + 1, -beta, -alpha);

                if (placed)
                    map.UndoMove(movedCard);

                if (value >= beta)
                {
                    node.Value = beta;
                    return beta;
                }
                if (value >= alpha)
                    alpha = value;
            }

            node.Value = alpha;
            return alpha;
        }

        private int NodeEval(CardClass[,] ms, PlayerTurn currTurn, Vector2 cardStart, Vector2 cardEnd)
        {
            int score = 0;
            Turn activeTurn = map.GetTurn(currTurn);
            Turn otherTurn = map.GetTurn(currTurn == PlayerTurn.Player1 ? PlayerTurn.Player2 : PlayerTurn.Player1);
            CardClass cc = null;
            for (int i = 0; i < ms.GetLength(0); i++)
            {
                for (int j = 0; j < ms.GetLength(1); j++)
                {
                    if (i == cardStart.X && j == cardStart.Y)
                    {
                        cc = null;
                    }
                    else if (i == cardEnd.X && j == cardEnd.Y)
                    {
                        int x = (int)cardStart.X;
                        int y = (int)cardStart.Y;
                        if (x == -1 && otherTurn.GetPlayerTurn() == PlayerTurn.Player2)
                            cc = otherTurn.SelectCard(y);
                        else
                            cc =  ms[x, y];
                    }
                    else
                    {
                        cc = ms[i, j];
                    }


                    if (cc != null && cc.GetCardType().player != currTurn)
                    {
                        score += cc.GetCardType().GetStat() + cc.GetCardType().numberOfMoves;
                        score += (Math.Abs(otherTurn.GateLane() - i) + Math.Abs(3 - j)) * (Math.Abs(otherTurn.GateLane() - i) + Math.Abs(3 - j));
                        if (otherTurn.GateLane() == i && j == 3)
                            score += WinValue;
                    }
                    else if (cc != null)
                    {
                        score -= cc.GetCardType().GetStat() + cc.GetCardType().numberOfMoves;
                        score -= (Math.Abs(activeTurn.GateLane() - i) + Math.Abs(3 - j)) * (Math.Abs(activeTurn.GateLane() - i) + Math.Abs(3 - j));
                        if (activeTurn.GateLane() == i && j == 3)
                            score -= WinValue;
                    }
                }
            }

            return score;
        }

        private void ExpandNode(DecisionNode node, int depth)
        {
            CardClass s;
            MoveLocation[,] moveMap;

            // Add hand placements.

            if (node.pt == this.turn)
            {
                for (int h = 0; h < hand.Count; h++)
                {
                    for (int p = 1; p <= 5; p++)
                    {
                        if (map.map[1, p] == null)
                        {
                            DecisionNode dn = new DecisionNode(X, Y, (node.pt == PlayerTurn.Player1 ? PlayerTurn.Player2 : PlayerTurn.Player1));
                            dn.cardStart = new Vector2(-1, h);
                            dn.cardEnd = new Vector2(p, 1);
                            dn.moveI = 2;
                            dn.moveJ = 2;

                            //dn.Value = NodeEval(map.map, dn.pt, dn.cardStart, dn.cardEnd);

                            node.AddChild(dn);
                        }
                    }
                }
            }

            if (map.deploy - depth > 0) return;

            for (int i = 1; i < map.map.GetLength(0) - 1; i++)
            {
                for (int j = 1; j < map.map.GetLength(1) - 1; j++)
                {
                    s = map.map[i, j];
                    if (s != null && s.GetCardType().player == node.pt)
                    {
                        moveMap = s.GetCardType().moves;

                        for (int mi = 0; mi < moveMap.GetLength(0); mi++)
                        {
                            for (int mj = 0; mj < moveMap.GetLength(1); mj++)
                            {
                                int newX = j + (mj -2);
                                int newY = i + (mi -2);

                                if (newX <= 0 || newX >= map.map.GetLength(0) - 1)
                                    continue;

                                if (moveMap[mi, mj] != null && !(mi== 2 && mj == 2) && map.TestRecursiveCardMovement(s, new Vector2(j, i), s.GetMove()[mi, mj], s.GetMove(), mj, mi))
                                {
                                    DecisionNode dn = new DecisionNode(X, Y, (node.pt == PlayerTurn.Player1 ? PlayerTurn.Player2 : PlayerTurn.Player1));
                                    dn.cardStart = new Vector2(i, j);
                                    dn.cardEnd = new Vector2(newX, newY);
                                    dn.moveI = mi;
                                    dn.moveJ = mj;

                                    //dn.Value = NodeEval(map.map, dn.pt, dn.cardStart, dn.cardEnd);

                                    node.AddChild(dn);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

/// Last Test
