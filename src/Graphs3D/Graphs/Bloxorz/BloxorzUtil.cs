using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Graphs3D.Graphs.Sokoban;

namespace Graphs3D.Graphs.Bloxorz
{
    public static class BloxorzUtil
    {
        public static int CharToSymbol(char c)
        {
            switch (c)
            {
                case ' ':
                    return BloxorzNode.MAP_VOID;
                case '#':
                    return BloxorzNode.MAP_SPACE;
                default:
                    return SokobanNode.EMPTY;

            }
        }

        public static char SymbolToChar(int s)
        {
            switch (s)
            {
                case BloxorzNode.MAP_VOID:
                    return ' ';
                case BloxorzNode.MAP_SPACE:
                    return '#';
                default:
                    return '!';
            }
        }

        public static (int[,], BloxorzCoord, int, int, BloxorzCoord) ReadPositionFromString(string str)
        {
            var lines = str.Split('\n').Select(l => l.Trim('\r')).ToArray();
            var posLine = lines[0];
            var posSplit = posLine.Split(',');
            BloxorzCoord playerPos = new BloxorzCoord();
            playerPos.X = int.Parse(posSplit[0]);
            playerPos.Y = int.Parse(posSplit[1]);
            var playerLen = int.Parse(posSplit[2]);
            var playerOrient = int.Parse(posSplit[3]);
            BloxorzCoord targetPos = new BloxorzCoord();
            targetPos.X = int.Parse(posSplit[4]);
            targetPos.Y = int.Parse(posSplit[5]);

            lines = lines.Skip(1).ToArray();
            int width = lines.Max(l => l.Length);
            int height = lines.Length;
            var map = new int[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    map[x, y] = x < lines[y].Length ? BloxorzUtil.CharToSymbol(lines[y][x]) : BloxorzNode.MAP_VOID;
            return (map, playerPos, playerLen, playerOrient, targetPos);
        }

        public static string SerializePositionToString(BloxorzNode node)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{node.playerPos.X},{node.playerPos.Y},{node.playerLen},{node.playerOrientation},{node.targetPos.X},{node.targetPos.Y}");
            for (int y = 0; y < node.map.GetLength(1); y++)
            {
                for (int x = 0; x < node.map.GetLength(0); x++)
                {
                    sb.Append(SymbolToChar(node.map[x, y]));
                }
                sb.Append("\n");
            }

            return sb.ToString();
        }

        public static (BloxorzCoord, int) NewCoord(BloxorzCoord currentCoord, int currentOrientation, BloxorzCoord move, int len)
        {
            if (currentOrientation == BloxorzNode.ORIENT_VERTICAL)
            {
                return
                    (
                        new BloxorzCoord(currentCoord.X + ((move.X <= 0) ? len * move.X : 1), 
                                         currentCoord.Y + ((move.Y <= 0) ? len * move.Y : 1)),
                        (move.X != 0) ? BloxorzNode.ORIENT_RIGHT : BloxorzNode.ORIENT_DOWN
                    );
            }
            else if (currentOrientation == BloxorzNode.ORIENT_RIGHT)
            {
                return
                    (
                        new BloxorzCoord(currentCoord.X + ((move.X >= 0) ? move.X * len : move.X), 
                                         currentCoord.Y + ((move.X == 0) ? move.Y : 0)),
                        (move.X != 0) ? BloxorzNode.ORIENT_VERTICAL : BloxorzNode.ORIENT_RIGHT
                    );
            }
            else //down
            {
                return
                    (
                        new BloxorzCoord(currentCoord.X + ((move.Y == 0) ? move.X : 0),
                                         currentCoord.Y + ((move.Y >= 0) ? move.Y * len : move.Y)),
                        (move.Y != 0 ? BloxorzNode.ORIENT_VERTICAL : BloxorzNode.ORIENT_DOWN)
                    );
            }
        }

        public static bool IsAllowed(int[,] map, BloxorzCoord pos, int orient, int len)
        {
            var w = map.GetLength(0);
            var h = map.GetLength(1);
            if (orient == BloxorzNode.ORIENT_VERTICAL)
                return pos.X >= 0 && pos.Y >= 0 && pos.X < w && pos.Y < h && map[pos.X, pos.Y] != BloxorzNode.MAP_VOID;

            var dir = orient == BloxorzNode.MOVE_RIGHT ? new BloxorzCoord(1, 0) : new BloxorzCoord(0, 1);
            for (int i = 0; i < len; i++)
            {
                int occupyX = pos.X + dir.X * i;
                int occupyY = pos.Y + dir.Y * i;
                if (occupyX < 0 || occupyX >= w || occupyY < 0 || occupyY >= h || map[pos.X, pos.Y] == BloxorzNode.MAP_VOID)
                    return false;
            }

            return true;
        }
    }
}
