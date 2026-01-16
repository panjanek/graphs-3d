using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphs3D.Graphs.Bloxorz;

namespace Graphs3D.Graphs.Klotski
{
    public class KlotskiNode : GraphNodeBase
    {
        public const int MAP_SPACE = 0;

        public const int MAP_WALL = -1;

        public override string Key => key;

        public int[,] map;

        public Dictionary<int, List<KlotskiXY>> pieces;

        private string key;

        public KlotskiNode(string startPosition)
        {
            (map, pieces) = KlotskiUtil.ReadPositionFromString(startPosition);
            key = KlotskiUtil.SerializePositionToString(this);
            color = KlotskiGraph.NormalColor;

        }

        public KlotskiNode(KlotskiNode prev, KlotskiMove move)
        {
            map = new int[prev.map.GetLength(0), prev.map.GetLength(1)];
            GraphUtil.Copy2D(prev.map, map);
            pieces = new Dictionary<int, List<KlotskiXY>>();
            foreach(var pc in prev.pieces)
                pieces[pc.Key] = pc.Value.ToList();
            
            var oldPiece = prev.pieces[move.pieceId];
            var newPiece = new List<KlotskiXY>();
            foreach(var p in oldPiece)
            {
                newPiece.Add(new KlotskiXY(p.X + move.dir.X, p.Y + move.dir.Y));
                map[p.X, p.Y] = 0;
            }

            pieces[move.pieceId] = newPiece;
            foreach (var p in newPiece)
                map[p.X, p.Y] = move.pieceId;


            parentIdx = prev.idx;
            key = KlotskiUtil.SerializePositionToString(this);
            color = BloxorzGraph.ColorOk;
            /*
            if (IsWin())
            {
                leaf = true;
                win = BloxorzGraph.ColorWin;
                color = BloxorzGraph.ColorWin;
            }*/
        }

        public List<KlotskiMove> GenerateMoves()
        {
            List<KlotskiMove> moves = new List<KlotskiMove>();
            foreach(var pieceId in pieces.Keys.OrderBy(i=>i).ToList())
            {
                var piece = pieces[pieceId];
                foreach (var dir in KlotskiGraph.Directions)
                {
                    bool canMove = true;
                    foreach (var currPos in piece)
                    {
                        var newPos = new KlotskiXY(currPos.X + dir.X, currPos.Y + dir.Y);
                        if (map[newPos.X, newPos.Y] != MAP_SPACE && map[newPos.X, newPos.Y] != pieceId)
                        {
                            canMove = false;
                            break;
                        }
                    }

                    if (canMove)
                        moves.Add(new KlotskiMove(pieceId, dir));
                }
            }


            return moves;
        }
    }

    public struct KlotskiXY
    {
        public KlotskiXY() { }
        public KlotskiXY(int x, int y) { X = x;  Y = y; }
        public int X;

        public int Y;
    }

    public struct KlotskiMove
    {
        public KlotskiMove() { }

        public KlotskiMove(int id, KlotskiXY d) { pieceId = id; dir = d; }

        public int pieceId;

        public KlotskiXY dir;
    }
}
