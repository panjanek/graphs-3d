using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphs3D.Graphs.Bloxorz;

namespace Graphs3D.Graphs.Klotski
{
    public static class KlotskiUtil
    {
        public static (int[,], Dictionary<int, List<KlotskiXY>>, Dictionary<int, List<KlotskiXY>>) ReadPositionFromString(string str)
        {
            var lines = str.Split('\n').Select(l => l.Trim('\r').Trim(' ')).ToArray();
            var mapLines = lines;
            int winIdx = lines.ToList().IndexOf("-");
            if (winIdx > -1)
                mapLines = lines.Take(winIdx).ToArray();

            int width = mapLines[0].Length;
            int height = mapLines.Length;
            int[,] map = new int[width, height];
            Dictionary<int, List<KlotskiXY>> pieces = new Dictionary<int, List<KlotskiXY>>();
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    var c = x < mapLines[y].Length ? mapLines[y][x] : ' ';
                    if (c == ' ')
                        map[x, y] = KlotskiNode.MAP_SPACE;
                    else if (c == '#')
                        map[x, y] = KlotskiNode.MAP_WALL;
                    else
                    {
                        int nr = ((int)c);
                        map[x, y] = nr;
                        if (pieces.ContainsKey(nr))
                            pieces[nr].Add(new KlotskiXY(x, y));
                        else
                            pieces[nr] = [new KlotskiXY(x, y)];
                    }

                }

            Dictionary<int, List<KlotskiXY>> winCondition = new Dictionary<int, List<KlotskiXY>>();
            if (winIdx > -1)
            {
                var winLines = lines.Skip(winIdx+1).ToArray();
                for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                    {
                        var c = x < winLines[y].Length ? winLines[y][x] : ' ';
                        if (c != ' ' && c != '#')
                        {
                            int nr = ((int)c);
                            if (winCondition.ContainsKey(nr))
                                winCondition[nr].Add(new KlotskiXY(x, y));
                            else
                                winCondition[nr] = [new KlotskiXY(x, y)];
                        }

                    }
            }

            return (map, pieces, winCondition);
        }

        public static string SerializePositionToString(KlotskiNode node)
        {
            var sb = new StringBuilder();
            for (int y = 0; y < node.map.GetLength(1); y++)
            {
                for (int x = 0; x < node.map.GetLength(0); x++)
                {
                    if (node.map[x, y] == KlotskiNode.MAP_WALL)
                        sb.Append('#');
                    else if (node.map[x, y] == KlotskiNode.MAP_SPACE)
                        sb.Append(' ');
                    else
                        sb.Append((char)(node.map[x,y]));
                }
                sb.Append("\n");
            }

            return sb.ToString();
        }

    }
}
