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
        int WinValue = 20;
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
            //MapState[,] ms = new MapState[X, Y];
            //for(int i = 0; i < map.map.GetLength(0); i++)
            //{
            //    for (int j = 0; j < map.map.GetLength(1); j++)
            //    {
            //        CardClass cc = map.map[i, j];
            //        if (cc != null)
            //        {
            //            ms[i, j] = new MapState(cc.player, cc.GetCardType(), map.ConvertScreenCoordToMap(cc.GetPrevLocation()));
            //        }
            //    }
            //}
            rootNode = new DecisionNode(X, Y, PlayerTurn.Player2);

            //rootNode.currState = ms;

            Watch.Reset();
            Watch.Start();
            DecisionNode solution = null;
            while (depth < maxDepth)
            {
                DLS(rootNode, depth, int.MinValue, int.MaxValue);
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
            else
                return false;
        }

        private int ChildNodeSort(DecisionNode n1, DecisionNode n2)
        {
            return n1.Value - n2.Value;
        }

        private int DLS(DecisionNode node, int depth, int alpha, int beta)
        {

            //if (Watch.ElapsedMilliseconds >= 500)
            //    return node.Value; // no more time


            if (depth == maxDepth)
            {
                return node.Value;
            }

            //if (TestWin(node))
            //{
            //    node.Value = WinValue * (node.pt == turn ? 1 : -1);
            //    return node.Value;
            //}

            if (node.GetChildren().Count == 0)
            {
                ExpandNode(node, depth);
                node.GetChildren().Sort(ChildNodeSort);
            }


            int value = 0;

            // Maximize!
            int b = beta;
            bool firstChild = true;
            CardClass movedCard;

            foreach (DecisionNode n in node.GetChildren())
            {
                if (n.cardStart.X == -1)
                {
                    Turn activeTurn = map.GetTurn(node.pt);
                    movedCard = activeTurn.SelectCard((int)n.cardStart.Y);
                }
                else
                    movedCard = map.map[(int)n.cardStart.Y, (int)n.cardStart.X];

                if (movedCard != null)
                    map.MoveCard(movedCard, (int)n.cardEnd.X, (int)n.cardEnd.Y, false);

                value = -DLS(n, depth - 1, -b, -alpha);
                if (alpha < value && value < beta && !firstChild)
                    value = -DLS(n, depth - 1, -beta, -alpha);

                if (movedCard != null)
                    map.UndoMove();

                firstChild = false;
                alpha = Math.Max(alpha, value);
                if (alpha >= beta)
                {
                    node.Value = alpha;
                    return alpha;
                }
                b = alpha + 1;
            }

            node.Value = alpha;
            return alpha;
        }

        private int NodeEval(CardClass[,] ms, PlayerTurn currTurn)
        {
            int score = 0;
            Turn activeTurn = map.GetTurn(currTurn);
            Turn otherTurn = map.GetTurn(currTurn == PlayerTurn.Player1 ? PlayerTurn.Player2 : PlayerTurn.Player1);
            for (int i = 0; i < ms.GetLength(0); i++)
            {
                for (int j = 0; j < ms.GetLength(1); j++)
                {
                    if (ms[i, j] != null && ms[i, j].GetCardType().player == currTurn)
                    {
                        score += ms[i, j].GetCardType().GetStat() + ms[i, j].GetCardType().numberOfMoves;
                        score += (Math.Abs(otherTurn.GateLane() - i) + Math.Abs(2 - j)) * (Math.Abs(otherTurn.GateLane() - i) + Math.Abs(2 - j));
                        if (otherTurn.GateLane() == i && j == 3)
                            score += 200;
                    }
                    else if (ms[i, j] != null)
                    {
                        score -= ms[i, j].GetCardType().GetStat() + ms[i, j].GetCardType().numberOfMoves;
                        score -= (Math.Abs(otherTurn.GateLane() - i) + Math.Abs(3 - j)) * (Math.Abs(otherTurn.GateLane() - i) + Math.Abs(3 - j));
                        if (activeTurn.GateLane() == i && j == 3)
                            score -= 200;
                    }
                }
            }

            return score;
        }

        //private bool CheckForValidMove(CardType ct, MapState[,] map, int locY, int locX, int Y, int X)
        //{
        //    if (locX > 0 && locX < map.GetLength(0) && locY > 0 && locY < map.GetLength(1))
        //    {
        //        if (map[locY, locX] != null && map[locY, locX].prevLoc.X == (locX + X - 2) && map[locY, locX].prevLoc.Y == (locY + Y - 2)) return false;

        //        int tempX = locX;
        //        int tempY = locY;
        //        int offX = X;
        //        int offY = Y;
        //        MoveLocation ml = ct.moves[Y, X];
        //        bool forwardMove;
        //        Turn otherTurn = this.map.GetTurn(ct.player == PlayerTurn.Player1 ? PlayerTurn.Player2 : PlayerTurn.Player1);
        //        do
        //        {
        //            tempX = locX + offX - 2;
        //            tempY = locY + offY - 2;
        //            forwardMove = ml.y == offY;

        //            if ((tempX <= 0 || tempX >= map.GetLength(0) - 1 || tempY <= 0 || tempY >= map.GetLength(1)))
        //            {
        //                if (!(otherTurn.GateLane() == tempY && tempX == 3 && forwardMove))
        //                    return false;
        //            }

        //            if (map[locY, locX] != null && map[locY, locX].prevLoc.Y == tempY && map[locY, locX].prevLoc.X == tempX)
        //                return false;
        //            else if (map[tempY, tempX] != null && map[tempY, tempX].ct.player == ct.player)
        //                return false;
        //            else if (map[tempY, tempX] != null && map[tempY, tempX].ct.GetStat() > ct.GetStat() + ml.modifier)
        //                return false;

        //            offY = ml.x;
        //            offX = ml.y;
        //            ml = ct.moves[ml.x, ml.y];
        //        } while (ml != null && !(ml.x == offY && ml.y == offX));

        //        return true;
        //    }

        //    return false;
        //}

        private void ExpandNode(DecisionNode node, int depth)
        {
            CardClass[,] state = map.map;

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


                            node.AddChild(dn);
                        }
                    }
                }
            }

            if (map.deploy - depth >= 0) return;

            for (int i = 1; i < state.GetLength(0)-1; i++)
            {
                for (int j = 1; j < state.GetLength(1)-1; j++)
                {
                    s = state[i, j];
                    if (s != null && s.GetCardType().player == node.pt)
                    {
                        moveMap = s.GetCardType().moves;

                        for (int mi = 0; mi < moveMap.GetLength(0); mi++)
                        {
                            for (int mj = 0; mj < moveMap.GetLength(1); mj++)
                            {
                                int newX = j + (mj -2);
                                int newY = i + (mi -2);
                                if (moveMap[mi, mj] != null && !(mi== 2 && mj == 2) && map.TestRecursiveCardMovement(map.map[i, j], new Vector2(newX, newY), s.GetMove()[mi, mj], s.GetMove(), mj, mi))
                                {
                                    DecisionNode dn = new DecisionNode(X, Y, (node.pt == PlayerTurn.Player1 ? PlayerTurn.Player2 : PlayerTurn.Player1));
                                    dn.cardStart = new Vector2(i, j);
                                    dn.cardEnd = new Vector2(newX, newY);
                                    dn.moveI = mi;
                                    dn.moveJ = mj;

                                    dn.Value = NodeEval(map.map, dn.pt);

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
