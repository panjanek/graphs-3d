using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
