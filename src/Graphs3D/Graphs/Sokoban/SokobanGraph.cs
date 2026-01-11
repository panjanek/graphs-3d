using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Graphs3D.Graphs.TicTacToe;
using Graphs3D.Utils;
using Microsoft.VisualBasic;
using MS.WindowsAPICodePack.Internal;
using OpenTK.Graphics.OpenGL;

namespace Graphs3D.Graphs.Sokoban
{
    public class SokobanGraph : GraphBase<SokobanNode>, IGraph
    {
        private int width;

        private int height;

        public static SokobanXY[] stack;

        public static bool[,] visited;

        public static SokobanXY[] dirs = new SokobanXY[4] { new SokobanXY(-1, 0), new SokobanXY(1, 0), new SokobanXY(0, -1), new SokobanXY(0, 1) };

        private SokobanPresenter presenter;

        public SokobanGraph()
        {
            var root = new SokobanNode(ResourceUtil.LoadStringFromResource("maps.sokoban3.txt"));
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
                var next = new SokobanNode(parent, move.boxToPush, move.dir);
                AddNode(next);
            }
        }

        public override void PostExpandActions()
        {
            if (graphNodes.Any(n => n.win == 1) && graphNodes.All(n => n.expanded))
            {
                for(int i=0; i<graphNodes.Count; i++)
                {
                    var testing = graphNodes[i];
                    if (!testing.parentIdx.HasValue)
                    {
                        testing.player = 0;
                        SetInternal(i, 0);
                        continue;
                    }

                    var wantReturnTo = graphNodes[testing.parentIdx.Value];
                    var moves = GenerateMoves(testing);
                    bool canReturn = false;
                    foreach(var move in moves)
                    {
                        var next = new SokobanNode(testing, move.boxToPush, move.dir);
                        if (next.Key == wantReturnTo.Key)
                        {
                            canReturn = true;
                            break;
                        }
                    }

                    if (!canReturn)
                        Console.WriteLine();
                    
                    testing.player = canReturn ? 0 : 3;
                    SetInternal(i, testing.player);
                }
            }
            else
            {
                for (int i = 0; i < graphNodes.Count; i++)
                {
                    graphNodes[i].player = 0;
                    SetInternal(i, 0);
                }
            }
        }

        private void SetInternal(int idx, int player)
        {
            var internalNode = internalNodes[idx];
            internalNode.player = player;
            internalNodes[idx] = internalNode;
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
    }

    public class SokobanMove
    {
        public SokobanXY boxToPush;

        public SokobanXY dir;
    }
}
