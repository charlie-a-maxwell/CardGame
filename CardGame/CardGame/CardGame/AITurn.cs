using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Storage;

namespace CardGame
{
    class MapState
    {
        public PlayerTurn pt;
        public CardType ct;
        public Vector2 prevLoc;
        public MapState(PlayerTurn p, CardType c, Vector2 pl)
        {
            pt = p;
            ct = c;
            prevLoc = pl;
        }
    }

    class DecisionNode
    {
        List<DecisionNode> children;
        public MapState[,] currState;
        public Vector2 cardStart;
        public Vector2 cardEnd;
        public PlayerTurn pt;
        public int Value;

        public DecisionNode(int maxX, int maxY, PlayerTurn p)
        {
            currState = new MapState[maxX, maxY];
            children = new List<DecisionNode>(10);
            cardStart = new Vector2(-1, -1);
            cardEnd = new Vector2(-1, -1);
            pt = p;
            Value = 0;
        }

        public void CopyState(MapState[,] state)
        {
            currState = new MapState[state.GetLength(0), state.GetLength(1)];

            for (int i = 0; i < state.GetLength(0); i++)
            {
                for (int j = 0; j < state.GetLength(1); j++)
                {
                    currState[i, j] = state[i, j];
                }
            }
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
            MapState[,] ms = new MapState[X, Y];
            for(int i = 0; i < map.map.GetLength(0); i++)
            {
                for (int j = 0; j < map.map.GetLength(1); j++)
                {
                    CardClass cc = map.map[i, j];
                    if (cc != null)
                    {
                        ms[i, j] = new MapState(cc.player, cc.GetCardType(), map.ConvertScreenCoordToMap(cc.GetPrevLocation()));
                    }
                }
            }
            rootNode = new DecisionNode(X, Y, PlayerTurn.Player2);

            rootNode.currState = ms;

            DecisionNode solution = null;
            while (depth < 4)
            {
                DLS(rootNode, depth, int.MinValue, int.MaxValue);
                depth++;
            }

            int value = int.MinValue;
            foreach (DecisionNode n in rootNode.GetChildren())
            {
                if (n.Value > value)
                {
                    value = n.Value;
                    solution = n;
                }
            }

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
            if (node.currState[0, 3] != null)
                return true;
            else
                return false;
        }

        private int DLS(DecisionNode node, int depth, int alpha, int beta)
        {
            if (depth >= 0)
            {
                if (TestWin(node))
                {
                    int value = 10;
                    if (node.pt == turn)
                        value = -10;

                    node.Value = value;
                    return value;
                }

                if (node.GetChildren().Count == 0)
                    ExpandNode(node, depth);


                int score = 0;

                if (node.pt == turn)
                {
                    // Maximize!
                    foreach (DecisionNode n in node.GetChildren())
                    {
                        score = DLS(n, depth - 1, alpha, beta);
                        if (score > alpha)
                            alpha = score;
                        if (alpha >= beta)
                        {
                            node.Value = alpha;
                            return alpha;
                        }
                    }

                    node.Value = alpha;
                    return alpha;
                }
                else
                {
                    // Minimize!
                    foreach (DecisionNode n in node.GetChildren())
                    {
                        score = DLS(n, depth - 1, alpha, beta);
                        if (score < beta)
                            beta = score;
                        if (alpha >= beta)
                        {
                            node.Value = beta;
                            return beta;
                        }
                    }

                    node.Value = beta;
                    return beta;
                }

            }
            else
            {
                
                // find the closest value for the 2 different teams.
                int score = 0;
                if (node.pt == turn)
                    score = SecondPlayerWin(node.currState);
                else
                    score = FirstPlayerWin(node.currState);

                node.Value = score;
                return score;
            }

            return 0;
        }

        private int FirstPlayerWin(MapState[,] map)
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i,j] != null && map[i, j].pt == PlayerTurn.Player1)
                    {
                        return (i + j);
                    }
                }
            }

            return 0;
        }

        private int SecondPlayerWin(MapState[,] map)
        {
            int score = 10;
            for (int i = map.GetLength(0)-1; i >= 0; i--)
            {
                for (int j =  map.GetLength(1)-1; j >= 0; j--)
                {
                    if (map[i, j] != null && map[i, j].pt == PlayerTurn.Player2)
                    {
                        return score - (map.GetLength(0) - i) - (map.GetLength(1) - j);
                    }
                }
            }

            return 0;
        }

        private bool CheckForValidMove(CardType ct, MapState[,] map, int locX, int locY, int X, int Y)
        {
            if (locX > 0 && locX < map.GetLength(0) && locY > 0 && locY < map.GetLength(1))
            {
                int tempX = locX;
                int tempY = locY;
                int offX = X;
                int offY = Y;
                MoveLocation ml = ct.moves[X, Y];
                do
                {
                    tempX = locX + offX - 2;
                    tempY = locY + offY - 2;
                    if (tempX <= 0 || tempX >= map.GetLength(0) - 1 || tempY <= 0 || tempY >= map.GetLength(1) - 1)
                        return false;
                    else if (map[tempX, tempY] != null && map[tempX, tempY].prevLoc.X == tempX && map[tempX, tempY].prevLoc.Y == tempY)
                        return false;
                    else if (map[tempX, tempY] != null && map[tempX, tempY].ct.player == ct.player)
                        return false;
                    else if (map[tempX, tempY] != null && map[tempX, tempY].ct.GetStat() > ct.GetStat() + ml.modifier)
                        return false;

                    offX = ml.x;
                    offY = ml.y;
                    ml = ct.moves[ml.x, ml.y];
                } while (ml != null && !(ml.x == 2 && ml.y == 2));

                return true;
            }

            return false;
        }

        private void ExpandNode(DecisionNode node, int depth)
        {
            MapState[,] state = node.currState;

            MapState s;

            MoveLocation[,] moveMap;

            // Add hand placements.

            if (node.pt == this.turn)
            {
                for (int h = 0; h < hand.Count; h++)
                {
                    for (int p = 1; p < 5; p++)
                    {
                        if (node.currState[1, p] == null)
                        {
                            DecisionNode dn = new DecisionNode(X, Y, (node.pt == PlayerTurn.Player1 ? PlayerTurn.Player2 : PlayerTurn.Player1));
                            dn.cardStart = new Vector2(-1, h);
                            dn.cardEnd = new Vector2(p, 1);
                            dn.CopyState(state);
                            dn.currState[1, p] = new MapState(PlayerTurn.Player1, hand.SelectCard(h).GetCardType(), new Vector2(-1,-1));

                            node.AddChild(dn);
                        }
                    }
                }
            }

            if (map.deploy - depth > 0) return;

            for (int i = 1; i < state.GetLength(0)-1; i++)
            {
                for (int j = 1; j < state.GetLength(1)-1; j++)
                {
                    s = state[i, j];
                    if (s != null && s.pt == node.pt)
                    {
                        moveMap = s.ct.moves;

                        for (int mi = 0; mi < moveMap.GetLength(0); mi++)
                        {
                            for (int mj = 0; mj < moveMap.GetLength(1); mj++)
                            {
                                int newX = j + (mj -2);
                                int newY = i + (mi -2);
                                if (moveMap[mi, mj] != null && !(mi== 2 && mj == 2) && CheckForValidMove(s.ct, state, i, j, mi, mj))
                                {
                                    DecisionNode dn = new DecisionNode(X, Y, (node.pt == PlayerTurn.Player1 ? PlayerTurn.Player2 : PlayerTurn.Player1));
                                    dn.cardStart = new Vector2(i, j);
                                    dn.cardEnd = new Vector2(newX, newY);
                                    dn.CopyState(state);
                                    s.prevLoc.X = j;
                                    s.prevLoc.Y = i;
                                    dn.currState[newY, newX] = s;
                                    dn.currState[i, j] = null;

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
