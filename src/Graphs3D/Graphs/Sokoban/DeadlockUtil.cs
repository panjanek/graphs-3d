using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphs3D.Graphs.Sokoban
{
    public static class DeadlockUtil
    {
        public static bool HasAreaThatCannotBeReached(SokobanNode node)
        {
            return false;
        }


        public static bool IsDeadlock(int[,] map)
        {
            int h = map.GetLength(0);
            int w = map.GetLength(1);

            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    if (map[y, x] != SokobanNode.BOX)
                        continue;

                    if (IsCornerDeadlock(map, x, y))
                        return true;
                    
                    if (Is2x2Deadlock(map, x, y))
                        return true;
                }
            }

            return false;
        }


        static bool IsCornerDeadlock(int[,] map, int x, int y)
        {
            bool wallUp = map[y - 1, x] == SokobanNode.WALL;
            bool wallDown = map[y + 1, x] == SokobanNode.WALL;
            bool wallLeft = map[y, x - 1] == SokobanNode.WALL;
            bool wallRight = map[y, x + 1] == SokobanNode.WALL;

            return (wallUp && wallLeft) ||
                   (wallUp && wallRight) ||
                   (wallDown && wallLeft) ||
                   (wallDown && wallRight);
        }
        static bool Is2x2Deadlock(int[,] map, int x, int y)
        {
            for (int dy = -1; dy <= 0; dy++)
            {
                for (int dx = -1; dx <= 0; dx++)
                {
                    if (IsBlocked(map[y + dy, x + dx]) &&
                        IsBlocked(map[y + dy + 1, x + dx]) &&
                        IsBlocked(map[y + dy, x + dx + 1]) &&
                        IsBlocked(map[y + dy + 1, x + dx + 1]))
                    {
                        if (ContainsBoxNotOnTarget(map, x + dx, y + dy))
                            return true;
                    }
                }
            }

            return false;
        }

        static bool IsBlocked(int c)
        {
            return c == SokobanNode.WALL || c == SokobanNode.BOX || c == SokobanNode.BOXONTARGET;
        }

        static bool ContainsBoxNotOnTarget(int[,] map, int x, int y)
        {
            for (int yy = y; yy <= y + 1; yy++)
                for (int xx = x; xx <= x + 1; xx++)
                    if (map[yy, xx] == SokobanNode.BOX)
                        return true;

            return false;
        }
    }
}
