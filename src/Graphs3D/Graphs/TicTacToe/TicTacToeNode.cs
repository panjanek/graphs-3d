using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphs3D.Graphs.TicTacToe
{
    public class TicTacToeNode : GraphNodeBase
    {
        public int[,] board;

        private string key;
        public TicTacToeNode(int size, int player)
        {
            board = new int[size, size];
            for (int x = 0; x < board.GetLength(0); x++)
                for (int y = 0; y < board.GetLength(1); y++)
                    board[x, y] = 2;
            this.player = player;
            key = GetKey(board);
        }

        public TicTacToeNode(TicTacToeNode prev, int x, int y, int player)
        {
            board = new int[prev.board.GetLength(0), prev.board.GetLength(1)];
            for (int i = 0; i < board.GetLength(0); i++)
                for (int j = 0; j < board.GetLength(1); j++)
                    board[i, j] = prev.board[i, j];
            board[x, y] = player;
            this.player = player;
            parentIdx = prev.idx;
            key = GetKey(board);
            var win = CheckForWin();
            if (win.HasValue)
            {
                leaf = true;
                this.win = player;
            }
            else if (IsTerminal())
            {
                leaf = true;
                this.win = -1;
            }
        }

        public static string GetKey(int[,] state)
        {
            StringBuilder sb = new StringBuilder();
            for (int x = 0; x < state.GetLength(0); x++)
            {
                for (int y = 0; y < state.GetLength(1); y++)
                {
                    sb.Append(state[x, y] == 2 ? " " : state[x, y] == 0 ? "X" : "O");
                }

                sb.Append("\n");
            }
            return sb.ToString();
        }

        private bool IsTerminal()
        {
            for (int x = 0; x < board.GetLength(0); x++)
                for (int y = 0; y < board.GetLength(1); y++)
                    if (board[x, y] == 2)
                        return false;
            return true;
        }

        private int? CheckForWin()
        {
            int size = board.GetLength(0);
            int da0 = 0;
            int da1 = 0;
            int db0 = 0;
            int db1 = 0;
            for (int i = 0; i < size; i++)
            {
                int h0 = 0;
                int h1 = 0;
                int v0 = 0;
                int v1 = 0;
                for (int j = 0; j < size; j++)
                {
                    if (board[i, j] == 0) h0++;
                    if (board[i, j] == 1) h1++;
                    if (board[j, i] == 0) v0++;
                    if (board[j, i] == 1) v1++;
                }

                if (h0 == size || v0 == size)
                    return 0;

                if (h1 == size || v1 == size)
                    return 1;

                if (board[i, i] == 0) da0++;
                if (board[i, i] == 1) da1++;
                if (board[i, size - i - 1] == 0) db0++;
                if (board[i, size - i - 1] == 1) db1++;
            }

            if (da0 == size || db0 == size)
                return 0;

            if (da1 == size || db1 == size)
                return 1;

            return null;
        }

        public override string Key => key;
    }
}
