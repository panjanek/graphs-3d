using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;
using Graphs3D.Gui;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using Brushes = System.Windows.Media.Brushes;
using AppContext = Graphs3D.Models.AppContext;

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
            if (parent.leaf)
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

        public void DrawPosition(int idx, Canvas canvas)
        {
            var node = graphNodes[idx];
            canvas.Children.Clear();
            canvas.Background = System.Windows.Media.Brushes.Black;
            var w = canvas.Width / size;
            var h = canvas.Height / size;
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    if (y > 0) CanvasUtil.AddLine(canvas, 0.1 * w, y * h, size * w - 0.1 * w, y * h, 2, Brushes.White);
                    if (x > 0) CanvasUtil.AddLine(canvas, x * w, 0.1 * h, x * w, size * h - 0.1 * h, 2, Brushes.White);
                    if (node.board[x, y] == 0)
                    {
                        CanvasUtil.AddEllipse(canvas, x * w + 0.1 * w, y * h + 0.1 * h, w * 0.7, h * 0.7, 10, AppContext.BrushesColors[0], Brushes.Black);
                    }
                    else if (node.board[x, y] == 1)
                    {
                        CanvasUtil.AddLine(canvas, x * w + 0.15 * w, y * h + 0.15 * h, x * w + 0.85 * w, y * h + 0.85 * h, 10, AppContext.BrushesColors[1]);
                        CanvasUtil.AddLine(canvas, x * w + 0.85 * w, y * h + 0.15 * h, x * w + 0.15 * w, y * h + 0.85 * h, 10, AppContext.BrushesColors[1]);
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
