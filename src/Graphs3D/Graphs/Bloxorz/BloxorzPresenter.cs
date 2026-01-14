using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Graphs3D.Graphs.Sokoban;
using Graphs3D.Gui;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Colors = System.Windows.Media.Colors;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace Graphs3D.Graphs.Bloxorz
{
    public class BloxorzPresenter
    {
        private BloxorzGraph graph;

        private Canvas canvas;

        private List<Rectangle> spaces;

        private Rectangle playerInside;

        private Rectangle playerOutside;

        private double cellWidth;

        private double cellHeight;

        private double marginLeft;

        private double marginTop;

        private List<Line> arrowLines;

        private List<Polygon> arrowPointers;

        public BloxorzPresenter(BloxorzGraph graph)
        {
            this.graph = graph;
        }

        public bool Draw(Canvas canv, BloxorzNode node)
        {
            lock (this)
            {
                if (this.canvas == null)
                    Initialize(canv, node.map);

                int spaceNr = 0;
                for (int y = 0; y < node.map.GetLength(1); y++)
                    for (int x = 0; x < node.map.GetLength(0); x++)
                    {
                        if (node.map[x, y] != BloxorzNode.MAP_VOID)
                            PositionNexSpace(ref spaceNr, x, y, node.map[x,y], x == node.targetPos.X && y == node.targetPos.Y);
                    }

                var left = marginLeft + node.playerPos.X * cellWidth + 0.05* cellWidth;
                var top = marginTop + node.playerPos.Y * cellHeight + 0.05 * cellHeight;
                var w = (node.playerOrientation == BloxorzNode.ORIENT_RIGHT ? cellWidth * node.playerLen : cellWidth) - 0.1*cellWidth;
                var h = (node.playerOrientation == BloxorzNode.ORIENT_DOWN ? cellHeight * node.playerLen : cellHeight) - 0.1*cellHeight;
                playerOutside.SetValue(Canvas.LeftProperty, left);
                playerOutside.SetValue(Canvas.TopProperty, top);
                playerOutside.Width = w;
                playerOutside.Height = h;
                playerInside.SetValue(Canvas.LeftProperty, left+0.25*cellWidth);
                playerInside.SetValue(Canvas.TopProperty, top+0.25*cellHeight);
                playerInside.Width = w - 0.5 * cellWidth;
                playerInside.Height = h - 0.5 * cellHeight;

                arrowLines.ForEach(l => l.Visibility = System.Windows.Visibility.Collapsed);
                arrowPointers.ForEach(l => l.Visibility = System.Windows.Visibility.Collapsed);
                var transitions = graph.GetAvailableTransitions(node);
                int arrowsCount = 0;
                foreach (var trans in transitions)
                    PositionNextArrow(ref arrowsCount, node.playerPos, node.playerOrientation, node.playerLen, trans);

                return true;
            }
        }

        private void PositionNextArrow(ref int arrowsCount, BloxorzCoord playerPos, int playerOrient, int playerLen, BloxorzTransition trans)
        {
            var offsetY = trans.move.Y * cellWidth * 0.2;
            var offsetX = trans.move.X * cellWidth * 0.2;
            double right = 0;
            double down = 0;

            if (playerOrient == BloxorzNode.ORIENT_RIGHT)
            {
                if (trans.move.X == 1) right = cellWidth * (playerLen - 1);
                if (trans.move.Y != 0) right = 0.5* cellWidth * (playerLen - 1);
            }
            else if (playerOrient == BloxorzNode.ORIENT_DOWN)
            {
                if (trans.move.Y == 1) down = cellHeight * (playerLen - 1);
                if (trans.move.X != 0) down = 0.5 * cellHeight * (playerLen - 1);
            }

            arrowLines[arrowsCount].X1 = right + marginLeft + playerPos.X * cellWidth + cellWidth / 2 + offsetX;
            arrowLines[arrowsCount].Y1 = down+marginTop + playerPos.Y * cellHeight + cellHeight / 2 + offsetY;
            arrowLines[arrowsCount].X2 = right+marginLeft + (playerPos.X + trans.move.X) * cellWidth + cellWidth / 2 - offsetX * 2;
            arrowLines[arrowsCount].Y2 = down+marginTop + (playerPos.Y + trans.move.Y) * cellHeight + cellHeight / 2 - offsetY * 2;
            arrowLines[arrowsCount].Visibility = System.Windows.Visibility.Visible;
            arrowLines[arrowsCount].Tag = trans;

            arrowPointers[arrowsCount].Points[0] = new Point(arrowLines[arrowsCount].X2 + offsetX * 1.5, arrowLines[arrowsCount].Y2 + offsetY * 1.5);
            arrowPointers[arrowsCount].Points[1] = new Point(arrowLines[arrowsCount].X2 - offsetY * 1.5, arrowLines[arrowsCount].Y2 - offsetX * 1.5);
            arrowPointers[arrowsCount].Points[2] = new Point(arrowLines[arrowsCount].X2 + offsetY * 1.5, arrowLines[arrowsCount].Y2 + offsetX * 1.5);
            arrowPointers[arrowsCount].Visibility = System.Windows.Visibility.Visible;
            arrowPointers[arrowsCount].Tag = trans;

            arrowsCount++;
        }

        private void PositionNexSpace(ref int nr, int x, int y, int type, bool isTarget)
        {
            spaces[nr].SetValue(Canvas.LeftProperty, marginLeft + x * cellWidth);
            spaces[nr].SetValue(Canvas.TopProperty, marginTop + y * cellHeight);
            spaces[nr].Fill = Brushes.DarkGreen;
            if (isTarget)
                spaces[nr].Fill = Brushes.LightGreen;
            nr++;
        }

        private void Initialize(Canvas canv, int[,] map)
        {
            this.canvas = canv;
            canvas.Children.Clear();
            canvas.Background = new SolidColorBrush(Color.FromArgb(255, 32, 16, 16));
            spaces = new List<Rectangle>();
            var size = Math.Max(map.GetLength(0), map.GetLength(1));
            cellWidth = canv.Width / size;
            cellHeight = canv.Height / size;
            marginLeft = (canv.Width - map.GetLength(0) * cellWidth) / 2;
            marginTop = (canv.Height - map.GetLength(1) * cellHeight) / 2;
            playerOutside = CanvasUtil.AddRect(canvas, 0, 0, 0, 0, 2, Brushes.Yellow, Brushes.LightGray, null, 10);
            playerInside = CanvasUtil.AddRect(canvas, 0, 0, 0, 0, 0, Brushes.Transparent, Brushes.Black, null, 20);
            for (int y = 0; y < map.GetLength(1); y++)
                for (int x = 0; x < map.GetLength(0); x++)
                {
                    if (map[x, y] != BloxorzNode.MAP_VOID)
                        spaces.Add(CanvasUtil.AddRect(canvas, 0, 0, cellWidth + 1, cellHeight + 1, 2, Brushes.Black, Brushes.Brown));
                }

            arrowLines = new List<Line>();
            arrowPointers = new List<Polygon>();

            var arrowBrush = new SolidColorBrush(Color.FromArgb(128, 160, 160, 160));
            for (int a=0; a<4; a++)
            {
                var line = CanvasUtil.AddLine(canvas, 0, 0, 0, 0, 10, arrowBrush, null, 200);
                line.MouseDown += (s, e) => { HandleClick(s); e.Handled = true; };
                arrowLines.Add(line);
                var poly = CanvasUtil.AddPoly(canvas, [new Point(), new Point(), new Point()], 0, Brushes.Transparent, arrowBrush, null, 200);
                poly.MouseDown += (s, e) => { HandleClick(s); e.Handled = true; };
                arrowPointers.Add(poly);
            }
        }

        public void Click(double x, double y)
        {
            foreach (var line in WpfUtil.FindVisualChildren<Line>(canvas))
                if (WpfUtil.CheckIfHit(line, x, y))
                    HandleClick(line);

            foreach (var poly in WpfUtil.FindVisualChildren<Polygon>(canvas))
                if (WpfUtil.CheckIfHit(poly, x, y))
                    HandleClick(poly);
        }

        private void HandleClick(object sender)
        {
            var transition = WpfUtil.GetTagAsObject<BloxorzTransition>(sender);
            if (transition != null)
            {
                if (graph.NavigateTo != null)
                    graph.NavigateTo(transition.node.idx);
            }
        }

    }
}
