using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphs3D.Graphs.Bloxorz;
using Graphs3D.Models;

namespace Graphs3D.Graphs.Klotski
{
    public static class KlotskiUtil
    {
        public static (int[,], Dictionary<int, List<KlotskiXY>>, Dictionary<int, List<KlotskiXY>>, Dictionary<int, int>) ReadPositionFromString(string str)
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

            var same = SameShapePieces(pieces);
            return (map, pieces, winCondition, same);
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
                        sb.Append((char)(node.same[node.map[x,y]]));
                }
                sb.Append("\n");
            }

            var res = sb.ToString();
            return res;
        }

        public static Dictionary<int, int> SameShapePieces(Dictionary<int, List<KlotskiXY>> pieces)
        {
            Dictionary<int, KlotskiXY> corners = new Dictionary<int, KlotskiXY>();
            foreach (var piece in pieces)
                corners[piece.Key] = new KlotskiXY(piece.Value.Min(p=>p.X), piece.Value.Min(p => p.Y));
            var subkeys = new Dictionary<int, string>();
            foreach (var pc in pieces)
            {
                var normalized = pc.Value.Select(p => new KlotskiXY(p.X - corners[pc.Key].X, p.Y - corners[pc.Key].Y)).OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
                subkeys[pc.Key] = string.Join(",", normalized.Select(n => n.X + "-" + n.Y));
            }

            Dictionary<int, int> same = new Dictionary<int, int>();
            foreach (var pieceId in pieces.Keys.OrderBy(i => i))
                same[pieceId] = subkeys.Where(s => subkeys[s.Key] == subkeys[pieceId]).OrderBy(s => s.Key).First().Key;

            return same;
        }

    }
}
