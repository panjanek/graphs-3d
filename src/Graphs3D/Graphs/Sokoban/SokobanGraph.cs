using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Graphs3D.Graphs.TicTacToe;
using Graphs3D.Models;
using Graphs3D.Utils;
using Microsoft.VisualBasic;
using MS.WindowsAPICodePack.Internal;
using OpenTK.Graphics.OpenGL;

namespace Graphs3D.Graphs.Sokoban
{
    //https://borgar.net/programs/sokoban/#Sokoban
    public class SokobanGraph : GraphBase<SokobanNode>, IGraph
    {
        public const int ColorOk = 4;

        public const int ColorDeadend = 3;

        private int width;

        private int height;

        public static SokobanXY[] stack;

        public static bool[,] visited;

        public static SokobanXY[] dirs = new SokobanXY[4] { new SokobanXY(-1, 0), new SokobanXY(1, 0), new SokobanXY(0, -1), new SokobanXY(0, 1) };

        private SokobanPresenter presenter;

        public SokobanGraph(string resourceName)
        {
            var root = new SokobanNode(ResourceUtil.LoadStringFromResource(resourceName));
            width = root.position.GetLength(0);
            height = root.position.GetLength(1);
            visited = new bool[width, height];
            stack = new SokobanXY[width * height];
            AddNode(root);
            presenter = new SokobanPresenter(this);
        }

        protected override void InternalExpandNode(SokobanNode parent)
        {
            var moves = GenerateMoves(parent);
            foreach(var move in moves)
            {
                var next = new SokobanNode(parent, move);
                AddNode(next);
            }
        }

        protected override GraphNodeBase GetBestNodeToExpand()
        {
            var best = graphNodes.Where(n => !n.expanded).OrderBy(n => n.GetHeuristicDistance()).FirstOrDefault();
            return best;
        }

        public override void PostExpandActions()
        {
            if (graphNodes.Any(n => n.win > 0) && graphNodes.All(n => n.expanded))
            {
                for (int i = 0; i < graphNodes.Count; i++)
                    graphNodes[i].color = -1;

                //for each node, if there is no legal path to win, mark as dead
                for (int i = 0; i < graphNodes.Count; i++)
                    if (graphNodes[i].color == -1 && graphNodes[i].parentIdx.HasValue)
                    {
                        var start = graphNodes[i];
                        var subgraph = new List<SokobanNode>();
                        GraphUtil.SearchGraph(start, n => GenerateMoves(n).Select(move => new SokobanNode(n, move)).ToList(), n => subgraph.Add(n));
                        if (!subgraph.Any(n => n.win > 0))
                            foreach (var n in subgraph)
                            {
                                n.color = ColorDeadend;
                                n.dead = true;
                            }
                        else
                            start.color = ColorOk;
                    }
            }
            else
            {
                //for each dead node, mark all; it's successors as dead
                for (int i = 0; i < graphNodes.Count; i++)
                {
                    graphNodes[i].color = graphNodes[i].dead ? ColorDeadend : ColorOk;
                    if (!graphNodes[i].dead)
                        continue;

                    GraphUtil.SearchGraph(graphNodes[i], n => GenerateMoves(n)
                                                    .Select(move => new SokobanNode(n, move))
                                                    .Where(n=> keyedNodes.ContainsKey(n.Key) && !n.dead).ToList(), n =>
                                                    {
                                                        n.color = ColorDeadend;
                                                        n.dead = true;
                                                    });
                }
            }

            graphNodes[0].color = 0;
            for (int i = 0; i < graphNodes.Count; i++)
                SetInternalNodeAttributes(i, graphNodes[i].color);

            for (int e = 0; e < internalEdges.Count; e++)
                SetInternalEdgeAttributes(e, internalNodes[(int)(internalEdges[e].a)].color == 3 || internalNodes[(int)(internalEdges[e].b)].color == ColorDeadend ? ColorDeadend : ColorOk);
        }

        public List<SokobanTransition> GetAvailableTransitions(SokobanNode parent)
        {
            var moves = GenerateMoves(parent);
            var result = new List<SokobanTransition>();
            foreach (var move in moves)
            {
                var next = new SokobanNode(parent, move);
                if (keyedNodes.ContainsKey(next.Key))
                {
                    result.Add(new SokobanTransition() { move = move, node = keyedNodes[next.Key] });
                }
            }

            return result;
        }

        private List<SokobanMove> GenerateMoves(SokobanNode parent)
        {
            List<SokobanMove> moves = new List<SokobanMove>();
            Array.Clear(visited, 0, visited.Length);
            stack[0] = parent.playerPos;
            visited[parent.playerPos.X, parent.playerPos.Y] = true;
            int stackTop = 0;
            SokobanXY p;
            while (stackTop >= 0)
            {
                p = stack[stackTop];
                stackTop--;

                //try push
                for (int d = 0; d < dirs.Length; d++)
                {
                    var dir = dirs[d];
                    var testBox = new SokobanXY(p, dir, 1);
                    var testBehind = new SokobanXY(p, dir, 2);
                    if ((parent.position[testBox.X, testBox.Y] == SokobanNode.BOX || parent.position[testBox.X, testBox.Y] == SokobanNode.BOXONTARGET) &&
                        (parent.position[testBehind.X, testBehind.Y] == SokobanNode.EMPTY || parent.position[testBehind.X, testBehind.Y] == SokobanNode.TARGET))
                        moves.Add(new SokobanMove() { boxToPush = testBox, dir = dir });

                }

                //walk
                for (int d = 0; d < dirs.Length; d++)
                {
                    var dir = dirs[d];
                    var testWalk = new SokobanXY(p, dir, 1);
                    if (!visited[testWalk.X, testWalk.Y] &&
                        (parent.position[testWalk.X, testWalk.Y] == SokobanNode.EMPTY || parent.position[testWalk.X, testWalk.Y] == SokobanNode.TARGET))
                    {
                        stackTop++;
                        stack[stackTop] = testWalk;
                        visited[testWalk.X, testWalk.Y] = true;
                    }
                }
            }

            return moves;

        }

        public override bool DrawPosition(int idx, Canvas canvas) => presenter.Draw(canvas, graphNodes[idx]);

        public override void Click(double x, double y) => presenter.Click(x, y);
    }

    public class SokobanMove
    {
        public SokobanXY boxToPush;

        public SokobanXY dir;
    }

    public class SokobanTransition
    {
        public SokobanMove move;

        public SokobanNode node;
    }
}
