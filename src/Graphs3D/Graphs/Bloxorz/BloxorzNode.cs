using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Graphs3D.Graphs.Sokoban;

namespace Graphs3D.Graphs.Bloxorz
{
    public class BloxorzNode : GraphNodeBase
    {
        public const int MAP_VOID = 0;

        public const int MAP_SPACE = 1;

        public const int ORIENT_VERTICAL = 0;

        public const int ORIENT_RIGHT = 1;

        public const int ORIENT_DOWN = 2;

        public override string Key => key;

        private string key;

        public int[,] map;

        public BloxorzCoord playerPos;

        public int playerLen;

        public int playerOrientation;

        public BloxorzCoord targetPos;

        public BloxorzNode(string startPosition)
        {
            (map, playerPos, playerLen, playerOrientation, targetPos) = BloxorzUtil.ReadPositionFromString(startPosition);
            key = BloxorzUtil.SerializePositionToString(this);
        }

        public BloxorzNode(BloxorzNode prev, BloxorzCoord move)
        {
            map = new int[prev.map.GetLength(0), prev.map.GetLength(1)];
            GraphUtil.Copy2D(prev.map, map);
            (playerPos, playerOrientation) = BloxorzUtil.NewCoord(prev.playerPos, prev.playerOrientation, move, prev.playerLen);
            playerLen = prev.playerLen;
            targetPos = prev.targetPos;
            parentIdx = prev.idx;
            key = BloxorzUtil.SerializePositionToString(this);
            if (IsWin())
            {
                leaf = true;
                win = BloxorzGraph.ColorOk;
                color = BloxorzGraph.ColorOk;
            }
        }

        public List<BloxorzCoord> GenerateMoves()
        {
            List<BloxorzCoord> moves = new List<BloxorzCoord>();
            if (IsWin())
                return moves;

            foreach(var move in BloxorzGraph.PossibleMoves)
            {
                (var newPosition, var newOrientation) = BloxorzUtil.NewCoord(playerPos, playerOrientation, move, playerLen);
                if (BloxorzUtil.IsAllowed(map, newPosition, newOrientation, playerLen))
                    moves.Add(move);

            }
            return moves;
        }



        public bool IsWin()
        {
            return playerPos.X == targetPos.X && playerPos.Y == targetPos.Y && playerOrientation == ORIENT_VERTICAL;
        }
    }

    public struct BloxorzCoord
    {
        public BloxorzCoord() { }

        public BloxorzCoord(int x, int y) { X = x; Y = y; }

        public int X;

        public int Y;
    }

    public class BloxorzTransition
    {
        public BloxorzCoord move;

        public BloxorzNode node;
    }
}
