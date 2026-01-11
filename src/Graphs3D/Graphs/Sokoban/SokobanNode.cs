using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Graphs3D.Graphs.TicTacToe;
using Microsoft.VisualBasic;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace Graphs3D.Graphs.Sokoban
{
    public class SokobanNode : GraphNodeBase
    {
        public const int EMPTY = 0;

        public const int WALL = 1;

        public const int BOX = 2;

        public const int TARGET = 3;

        public const int BOXONTARGET = 4;

        private string key;

        public int[,] position;

        public SokobanXY playerPos;

        public static SokobanXY[] stack;

        public static bool[,] visited;

        public override string Key => key;

        public SokobanNode(string startPosition)
        {
            (position, playerPos) = SokobanUtil.ReadPositionFromString(startPosition);
            visited = new bool[position.GetLength(0), position.GetLength(1)];
            stack = new SokobanXY[position.GetLength(0) * position.GetLength(1)];
            NormalizePosition();
            key = SokobanUtil.SerializePositionToString(position, playerPos);
        }

        public SokobanNode(SokobanNode prev, SokobanXY boxToPush, SokobanXY dir)
        {
            position = new int[prev.position.GetLength(0), prev.position.GetLength(1)];
            GraphUtil.Copy2D(prev.position, position);
            SokobanXY newBoxPos = boxToPush;
            newBoxPos.X += dir.X;
            newBoxPos.Y += dir.Y;
            position[newBoxPos.X, newBoxPos.Y] = position[newBoxPos.X, newBoxPos.Y] == SokobanNode.EMPTY ? SokobanNode.BOX : SokobanNode.BOXONTARGET;
            position[boxToPush.X, boxToPush.Y] = position[boxToPush.X, boxToPush.Y] == SokobanNode.BOX ? SokobanNode.EMPTY : SokobanNode.TARGET;
            playerPos = boxToPush;
            NormalizePosition();

            parentIdx = prev.idx;
            key = SokobanUtil.SerializePositionToString(position, playerPos);
            if (SokobanUtil.IsWin(position))
            {
                leaf = true;
                win = 1;
            }
        }

        private void NormalizePosition()
        {
            Array.Clear(visited, 0, visited.Length);
            stack[0] = playerPos;
            visited[playerPos.X, playerPos.Y] = true;
            int stackTop = 0;
            SokobanXY p;
            SokobanXY normalized = new SokobanXY(int.MaxValue, int.MaxValue);
            var debug = SokobanUtil.SerializePositionToString(position, playerPos);
            while (stackTop >= 0)
            {
                p = stack[stackTop];
                stackTop--;

                if (p.Y < normalized.Y)
                    normalized = p;

                if (p.Y == normalized.Y && p.X < normalized.X)
                    normalized = p;

                

                //walk
                for (int d = 0; d < SokobanGraph.dirs.Length; d++)
                {
                    var dir = SokobanGraph.dirs[d];
                    var testWalk = new SokobanXY(p, dir, 1);
                    if (!visited[testWalk.X, testWalk.Y] &&
                        (position[testWalk.X, testWalk.Y] == SokobanNode.EMPTY || position[testWalk.X, testWalk.Y] == SokobanNode.TARGET))
                    {
                        stackTop++;
                        stack[stackTop] = testWalk;
                        visited[testWalk.X, testWalk.Y] = true;
                    }
                }
            }

            playerPos = normalized;
        }
    }
}
