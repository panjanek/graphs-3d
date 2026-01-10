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
using System.Windows.Forms.Design;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace Graphs3D.Graphs.TicTacToe
{
    public class TicTacToeGraph : GraphBase<TicTacToeNode>, IGraph
    {
        private int size;

        private int[,] tmp;

        private Canvas canvas;

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

            for(int x=0; x<size; x++)
                for(int y=0; y<size; y++)
                    if (parent.board[x,y] == 2)
                        AddNode(CreateNextMove(parent, x, y));
        }

        private TicTacToeNode CreateNextMove(TicTacToeNode parent, int x, int y)
        {
            var newNode = new TicTacToeNode(parent, x, y, 1 - parent.player);
            newNode.level = parent.level + 1;
            var exist = CheckSymmetry(newNode.board);
            if (exist != null)
            {
                exist.parentIdx = parent.idx;
                return exist;
            }
            else
                return newNode;
        }

        public override bool DrawPosition(int idx, Canvas canvas)
        {
            this.canvas = canvas;
            var node = graphNodes[idx];
            canvas.Children.Clear();
            canvas.Background = System.Windows.Media.Brushes.Black;
            var w = canvas.Width / size;
            var h = canvas.Height / size;
            var clickableBrush = new SolidColorBrush(Color.FromArgb(255,48, 48, 32));
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    if (y > 0) CanvasUtil.AddLine(canvas, 0.1 * w, y * h, size * w - 0.1 * w, y * h, 2, Brushes.White);
                    if (x > 0) CanvasUtil.AddLine(canvas, x * w, 0.1 * h, x * w, size * h - 0.1 * h, 2, Brushes.White);
                    if (node.board[x, y] == 0)
                    {
                        CanvasUtil.AddEllipse(canvas, x * w + 0.15 * w, y * h + 0.15 * h, w * 0.7, h * 0.7, 10, AppContext.BrushesColors[0], Brushes.Transparent);
                    }
                    else if (node.board[x, y] == 1)
                    {
                        CanvasUtil.AddLine(canvas, x * w + 0.15 * w, y * h + 0.15 * h, x * w + 0.85 * w, y * h + 0.85 * h, 10, AppContext.BrushesColors[1]);
                        CanvasUtil.AddLine(canvas, x * w + 0.85 * w, y * h + 0.15 * h, x * w + 0.15 * w, y * h + 0.85 * h, 10, AppContext.BrushesColors[1]);
                    }

                    string str = "";
                    int? clickableIdx = null;
                    if (node.parentIdx.HasValue && graphNodes[node.parentIdx.Value].board[x, y] != node.board[x, y] && node.board[x, y] != 2)
                    {
                        str = $"to parent: {node.idx} -> {node.parentIdx}";
                        clickableIdx = node.parentIdx.Value;
                    }
                    else if (node.board[x, y] == 2)
                    {
                        var child = CreateNextMove(node, x, y);
                        if (keyedNodes.ContainsKey(child.Key) && child.board[x, y] != node.board[x, y] && child.level > node.level)
                        {
                            child = keyedNodes[child.Key];
                            clickableIdx = child.idx;
                            str = $"to child: {node.idx} -> {child.idx}";
                        }
                    }

                    if (clickableIdx.HasValue)
                    {
                        CanvasUtil.AddRect(canvas, x * w + 0.05 * w, y * h + 0.05 * h, w * 0.9, h * 0.9, 0, Brushes.Black, clickableBrush, null, -10);
                        var clickable = CanvasUtil.AddRect(canvas, x * w + 0.05 * w, y * h + 0.05 * h, w * 0.9, h * 0.9, 0, Brushes.Transparent, Brushes.Transparent, clickableIdx.Value.ToString(), 10);
                        clickable.MouseDown += (s, e) => { if (NavigateTo != null) NavigateTo(WpfUtil.GetTagAsInt(s)); };
                        clickable.ToolTip = str;
                    }
                }

            return true;
        }

        public override void Click(double x, double y)
        {
            foreach(var rect in WpfUtil.FindVisualChildren<Rectangle>(canvas))
            {
                var left = (double)rect.GetValue(Canvas.LeftProperty);
                var top = (double)rect.GetValue(Canvas.TopProperty);
                var w = rect.Width;
                var h = rect.Height;
                if (x >= left && x <= left+w && y >= top && y <= top+h)
                {
                    var tag = WpfUtil.GetTagAsString(rect);
                    if (int.TryParse(tag, out var idx))
                        if (NavigateTo != null) NavigateTo(idx);
                }
            }
        }

        private TicTacToeNode CheckSymmetry(int[,] test)
        {
            TicTacToeNode existing; 

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    tmp[x, y] = test[size - 1 - x, y];
            if (keyedNodes.TryGetValue(TicTacToeNode.GetKey(tmp), out existing))
                return existing;

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    tmp[x, y] = test[x, size - 1 - y];
            if (keyedNodes.TryGetValue(TicTacToeNode.GetKey(tmp), out existing))
                return existing;

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    tmp[x, y] = test[size - 1 - x, size - 1 - y];
            if (keyedNodes.TryGetValue(TicTacToeNode.GetKey(tmp), out existing))
                return existing;

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    tmp[x, y] = test[y, x];
            if (keyedNodes.TryGetValue(TicTacToeNode.GetKey(tmp), out existing))
                return existing;

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    tmp[x, y] = test[y, size - 1 - x];
            if (keyedNodes.TryGetValue(TicTacToeNode.GetKey(tmp), out existing))
                return existing;

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    tmp[x, y] = test[size - 1 - y, x];
            if (keyedNodes.TryGetValue(TicTacToeNode.GetKey(tmp), out existing))
                return existing;

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    tmp[x, y] = test[size - 1 - y, size - 1 - x];
            if (keyedNodes.TryGetValue(TicTacToeNode.GetKey(tmp), out existing))
                return existing;

            return null;
        }
    }
}
