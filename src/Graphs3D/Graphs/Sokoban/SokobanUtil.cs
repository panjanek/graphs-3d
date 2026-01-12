using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Graphs3D.Graphs.Sokoban
{
    public static class SokobanUtil
    {
        public static char SymbolToChar(int s)
        {
            switch (s)
            {
                case SokobanNode.EMPTY:
                    return ' ';
                case SokobanNode.WALL:
                    return '#';
                case SokobanNode.BOX:
                    return '*';
                case SokobanNode.TARGET:
                    return '.';
                case SokobanNode.BOXONTARGET:
                    return '+';
                default:
                    return '!';
            }
        }

        public static int CharToSymbol(int c)
        {
            switch (c)
            {
                case ' ':
                    return SokobanNode.EMPTY;
                case '#':
                    return SokobanNode.WALL;
                case '*':
                    return SokobanNode.BOX;
                case '.':
                    return SokobanNode.TARGET;
                case '+':
                    return SokobanNode.BOXONTARGET;
                default:
                    return SokobanNode.EMPTY;

            }
        }

        public static (int[,], SokobanXY) ReadPositionFromString(string str)
        {
            var lines = str.Split('\n').Select(l=>l.Trim('\r')).ToArray();
            var posLine = lines[0];
            var posSplit = posLine.Split(',');
            var playerPos = new SokobanXY(int.Parse(posSplit[0]), int.Parse(posSplit[1]));
            lines = lines.Skip(1).ToArray();
            int width = lines.Max(l => l.Length);
            int height = lines.Length;
            var position = new int[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    position[x, y] = x < lines[y].Length ? SokobanUtil.CharToSymbol(lines[y][x]) : SokobanNode.EMPTY;
            return (position, playerPos);
        }

        public static string SerializePositionToString(int[,] pos, SokobanXY playerPos)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{playerPos.X},{playerPos.Y}");
            for (int y = 0; y < pos.GetLength(1); y++)
            {
                for (int x = 0; x < pos.GetLength(0); x++)
                {
                    sb.Append(SymbolToChar(pos[x, y]));
                }
                sb.Append("\n");
            }

            return sb.ToString();
        }

        public static bool IsWin(int[,] pos)
        {
            for (int y = 0; y < pos.GetLength(1); y++)
            {
                for (int x = 0; x < pos.GetLength(0); x++)
                {
                    if (pos[x, y] == SokobanNode.BOX || pos[x, y] == SokobanNode.TARGET)
                        return false;
                }
            }

            return true;
        }

    }

    public struct SokobanXY
    {
        public SokobanXY() { }

        public SokobanXY(int x, int y)
        {
            X = x;
            Y = y;
        }

        public SokobanXY(SokobanXY pos, SokobanXY dir, int mult)
        {
            X = pos.X + dir.X * mult;
            Y = pos.Y + dir.Y * mult;
        }

        public int X;

        public int Y;
    }
}
