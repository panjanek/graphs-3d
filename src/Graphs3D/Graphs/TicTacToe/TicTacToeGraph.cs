using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace Graphs3D.Graphs.TicTacToe
{
    public class TicTacToeGraph : GraphBase<TicTacToeNode>, IGraph
    {
        private int size;

        private int[,] tmp;
        public TicTacToeGraph(int size) 
        {
            this.size = size;
            tmp = new int[size, size];
            var root = new TicTacToeNode(size, 0);
            AddNode(root);
        }

        protected override void InternalExpandNode(TicTacToeNode parent)
        {
            if (parent.player < 0)
                return;

            var playerToGo = 1 - parent.player;
            for(int x=0; x<size; x++)
                for(int y=0; y<size; y++)
                {
                    if (parent.board[x,y] == 2)
                    {
                        var newNode = new TicTacToeNode(parent, x, y, playerToGo);
                        if (!CheckSymmetry(newNode.board))
                            continue;

                        AddNode(newNode);
                    }
                }
        }

        private bool CheckSymmetry(int[,] test)
        {
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    tmp[x, y] = test[size - 1 - x, y];
            if (keyedNodes.ContainsKey(TicTacToeNode.GetKey(tmp)))
                return false;

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    tmp[x, y] = test[x, size - 1 - y];
            if (keyedNodes.ContainsKey(TicTacToeNode.GetKey(tmp)))
                return false;

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    tmp[x, y] = test[size - 1 - x, size - 1 - y];
            if (keyedNodes.ContainsKey(TicTacToeNode.GetKey(tmp)))
                return false;

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    tmp[x, y] = test[y, x];
            if (keyedNodes.ContainsKey(TicTacToeNode.GetKey(tmp)))
                return false;
       
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    tmp[x, y] = test[y, size - 1 - x];
            if (keyedNodes.ContainsKey(TicTacToeNode.GetKey(tmp)))
                return false;

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    tmp[x, y] = test[size - 1 - y, x];
            if (keyedNodes.ContainsKey(TicTacToeNode.GetKey(tmp)))
                return false;

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    tmp[x, y] = test[size - 1 - y, size - 1 - x];
            if (keyedNodes.ContainsKey(TicTacToeNode.GetKey(tmp)))
                return false;

            return true;
        }
    }
}
