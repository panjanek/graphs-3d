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

        public SokobanXY playerVisualPos;

        public static SokobanXY[] stack;

        public static bool[,] visited;

        public override string Key => key;

        public bool dead;

        private double? distance;

        public SokobanNode(string startPosition)
        {
            (position, playerPos) = SokobanUtil.ReadPositionFromString(startPosition);
            playerVisualPos = playerPos;
            visited = new bool[position.GetLength(0), position.GetLength(1)];
            stack = new SokobanXY[position.GetLength(0) * position.GetLength(1)];
            NormalizePosition();
            key = SokobanUtil.SerializePositionToString(position, playerPos);
        }

        public SokobanNode(SokobanNode prev, SokobanMove move)
        {
            position = new int[prev.position.GetLength(0), prev.position.GetLength(1)];
            GraphUtil.Copy2D(prev.position, position);
            SokobanXY newBoxPos = move.boxToPush;
            newBoxPos.X += move.dir.X;
            newBoxPos.Y += move.dir.Y;
            position[newBoxPos.X, newBoxPos.Y] = position[newBoxPos.X, newBoxPos.Y] == SokobanNode.EMPTY ? SokobanNode.BOX : SokobanNode.BOXONTARGET;
            position[move.boxToPush.X, move.boxToPush.Y] = position[move.boxToPush.X, move.boxToPush.Y] == SokobanNode.BOX ? SokobanNode.EMPTY : SokobanNode.TARGET;
            playerPos = move.boxToPush;
            playerVisualPos = playerPos;
            NormalizePosition();
            parentIdx = prev.idx;
            key = SokobanUtil.SerializePositionToString(position, playerPos);
            if (SokobanUtil.IsWin(position))
            {
                leaf = true;
                win = SokobanGraph.ColorOk;
                player = SokobanGraph.ColorOk;
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

        public double GetHeuristicDistance()
        {
            if (distance.HasValue)
                return distance.Value;

            if (dead)
                return double.MaxValue;

            List<SokobanXY> boxes = new List<SokobanXY>();
            List<SokobanXY> targets = new List<SokobanXY>();
            int boxOnTargets = 0;
            for(int y=0; y<position.GetLength(1); y++)
                for(int x=0; x<position.GetLength(0); x++)
                {
                    if (DeadlockUtil.IsDeadlock(position))
                    { 
                        dead = true;
                        var a = key;
                        return double.MaxValue;
                    }
                        
                    if (position[x, y] == TARGET)
                        targets.Add(new SokobanXY(x, y));
                    if (position[x, y] == BOX)
                        boxes.Add(new SokobanXY(x, y));
                       
                    if (position[x, y] == BOXONTARGET)
                        boxOnTargets++;

                }

            int dist = 0;
            foreach (var box in boxes)
                foreach (var target in targets)
                    dist += Math.Abs(box.X - target.X) + Math.Abs(box.Y - target.Y);



            return dist - boxOnTargets * 100;

        }
    }
}
