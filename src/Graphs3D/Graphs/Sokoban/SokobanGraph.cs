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

        private Canvas canvas;

        public static SokobanXY[] stack;

        public static bool[,] visited;

        public static SokobanXY[] dirs = new SokobanXY[4] { new SokobanXY(-1, 0), new SokobanXY(1, 0), new SokobanXY(0, -1), new SokobanXY(0, 1) };

        public SokobanGraph()
        {
            var root = new SokobanNode(ResourceUtil.LoadStringFromResource("maps.sokoban1.txt"));
            width = root.position.GetLength(0);
            height = root.position.GetLength(1);
            visited = new bool[width, height];
            stack = new SokobanXY[width * height];
            SokobanNode.visited = new bool[width, height];
            SokobanNode.stack = new SokobanXY[width * height];
            AddNode(root);
        }

        protected override void InternalExpandNode(SokobanNode parent)
        {
            Array.Clear(visited, 0, visited.Length);
            stack[0] = parent.playerPos;
            visited[parent.playerPos.X, parent.playerPos.Y] = true;
            int stackTop = 0;
            SokobanXY p;
            int added = 0;
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
                    {
                        var next = new SokobanNode(parent, testBox, dir);
                        AddNode(next);
                        added++;
                    }

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

            if (added == 0)
                parent.leaf = true;
        }
    }
}
