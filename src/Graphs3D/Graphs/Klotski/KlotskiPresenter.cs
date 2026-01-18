using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Graphs3D.Graphs.Bloxorz;
using Graphs3D.Gui;
using OpenTK.Graphics.ES20;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Colors = System.Windows.Media.Colors;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace Graphs3D.Graphs.Klotski
{
    public class KlotskiPresenter
    {
        public static readonly Brush[] BrushesColors = [Brushes.Yellow, Brushes.Magenta, Brushes.Cyan, Brushes.Red, Brushes.Green, Brushes.Blue, Brushes.White, Brushes.Gray];

        private const double Spacing = 0.1;

        private KlotskiGraph graph;

        private Canvas canvas;

        private double cellWidth;

        private double cellHeight;

        private double marginLeft;

        private double marginTop;

        private List<Rectangle> boxes;

        private List<Line> connectors;

        private List<Line> arrowLines;

        private List<Polygon> arrowPointers;

        private Dictionary<int, int> colors;

        private Brush brickBrush;

        public KlotskiPresenter(KlotskiGraph graph)
        {
            this.graph = graph;
        }

        public bool Draw(Canvas canv, KlotskiNode node)
        {
            lock (this)
            {
                if (this.canvas == null)
                    Initialize(canv, node.map, node.pieces, node.same);

                int boxNr = 0;
                int connectorNr = 0;
                for (int y = 0; y < node.map.GetLength(1); y++)
                    for (int x = 0; x < node.map.GetLength(0); x++)
                    {
                        if (node.map[x, y] != KlotskiNode.MAP_SPACE)
                        {
                            int sameNr = node.map[x, y] == KlotskiNode.MAP_WALL ? 0 : node.same[node.map[x, y]];
                            PositionNexBox(ref boxNr, x, y, node.map[x, y], sameNr);
                            if (node.map[x, y] != KlotskiNode.MAP_WALL)
                            {
                                var pieceId = node.map[x, y];
                                foreach(var dir in KlotskiGraph.AllDirections)
                                {
                                    var test = new KlotskiXY(x+dir.X, y+dir.Y);
                                    if (node.map[test.X, test.Y] == pieceId)
                                        PositionNextConnector(ref connectorNr, x, y, dir.X, dir.Y, sameNr);
                                }
                            }
                        }
                    }

                arrowLines.ForEach(l => l.Visibility = System.Windows.Visibility.Collapsed);
                arrowPointers.ForEach(l => l.Visibility = System.Windows.Visibility.Collapsed);
                var transitions = graph.GetAvailableTransitions(node);
                int arrowsCount = 0;
                foreach (var trans in transitions)
                    PositionNextArrow(ref arrowsCount, node.pieces[trans.move.pieceId], trans);

                return true;
            }
        }

        private void PositionNextArrow(ref int arrowsCount, List<KlotskiXY> piece, KlotskiTransition trans)
        {
            var x = piece.Average(p => p.X);
            var y = piece.Average(p => p.Y);
            var offsetY = trans.move.dir.Y * cellWidth * 0.2;
            var offsetX = trans.move.dir.X * cellWidth * 0.2;
            arrowLines[arrowsCount].X1 = marginLeft + x * cellWidth + cellWidth / 2 + offsetX;
            arrowLines[arrowsCount].Y1 = marginTop + y * cellHeight + cellHeight / 2 + offsetY;
            arrowLines[arrowsCount].X2 = marginLeft + (x + trans.move.dir.X) * cellWidth + cellWidth / 2 - offsetX * 2;
            arrowLines[arrowsCount].Y2 = marginTop + (y + trans.move.dir.Y) * cellHeight + cellHeight / 2 - offsetY * 2;
            arrowLines[arrowsCount].Visibility = System.Windows.Visibility.Visible;
            arrowLines[arrowsCount].Tag = trans;

            arrowPointers[arrowsCount].Points[0] = new Point(arrowLines[arrowsCount].X2 + offsetX * 1.5, arrowLines[arrowsCount].Y2 + offsetY * 1.5);
            arrowPointers[arrowsCount].Points[1] = new Point(arrowLines[arrowsCount].X2 - offsetY * 1.5, arrowLines[arrowsCount].Y2 - offsetX * 1.5);
            arrowPointers[arrowsCount].Points[2] = new Point(arrowLines[arrowsCount].X2 + offsetY * 1.5, arrowLines[arrowsCount].Y2 + offsetX * 1.5);
            arrowPointers[arrowsCount].Visibility = System.Windows.Visibility.Visible;
            arrowPointers[arrowsCount].Tag = trans;

            arrowsCount++;
        }

        private void PositionNexBox(ref int nr, int x, int y, int type, int pieceNr)
        {
            boxes[nr].SetValue(Canvas.LeftProperty, marginLeft + x * cellWidth);
            boxes[nr].SetValue(Canvas.TopProperty, marginTop + y * cellHeight);
            if (type == KlotskiNode.MAP_WALL)
            {
                boxes[nr].Fill = brickBrush;
                boxes[nr].StrokeThickness = 0;
                boxes[nr].Stroke = Brushes.Transparent;
            }
            else
            {
                boxes[nr].Fill = BrushesColors[colors[pieceNr] % BrushesColors.Length];
                boxes[nr].StrokeThickness = cellWidth*0.1;
                boxes[nr].Stroke = Brushes.Transparent;
            }

            nr++;
        }

        private void PositionNextConnector(ref int nr, int x, int y, int dx, int dy, int pieceNr)
        {
            connectors[nr].Stroke = BrushesColors[colors[pieceNr] % BrushesColors.Length];
            if (dy == 0)
            {
                connectors[nr].X1 = marginLeft + ((dx == -1) ? x * cellWidth-1 : x * cellWidth + cellWidth*(1 - Spacing))+2;
                connectors[nr].X2 = connectors[nr].X1;
                connectors[nr].Y1 = marginTop + y * cellHeight + cellHeight * Spacing*0.5+0.2;
                connectors[nr].Y2 = marginTop + y * cellHeight + cellHeight * (1-Spacing * 0.5)+0.6;
            }

            if (dx == 0)
            {
                connectors[nr].Y1 = marginTop + ((dy == -1) ? y * cellHeight-1 : y * cellHeight + cellHeight * (1 - Spacing))+2;
                connectors[nr].Y2 = connectors[nr].Y1;
                connectors[nr].X1 = marginLeft + x * cellWidth + cellWidth * Spacing*0.5+0.2;
                connectors[nr].X2 = marginLeft + x * cellWidth + cellHeight * (1 - Spacing * 0.5)+0.6;
            }
            nr++;
        }

        private void Initialize(Canvas canv, int[,] map, Dictionary<int, List<KlotskiXY>> pieces, Dictionary<int, int> same)
        {
            this.canvas = canv;
            canvas.Children.Clear();
            canvas.Background = Brushes.Transparent;
            boxes = new List<Rectangle>();
            connectors = new List<Line>();
            var size = Math.Max(map.GetLength(0), map.GetLength(1));
            cellWidth = canv.Width / size;
            cellHeight = canv.Height / size;
            marginLeft = (canv.Width - map.GetLength(0) * cellWidth) / 2;
            marginTop = (canv.Height - map.GetLength(1) * cellHeight) / 2;
            CanvasUtil.AddRect(canv, marginLeft, marginTop, cellWidth * map.GetLength(0), cellHeight * map.GetLength(1), 0, Brushes.Transparent, Brushes.Black);
            for (int y = 0; y < map.GetLength(1); y++)
                for (int x = 0; x < map.GetLength(0); x++)
                {
                    if (map[x, y] != KlotskiNode.MAP_SPACE)
                        boxes.Add(CanvasUtil.AddRect(canvas, 0, 0, cellWidth + 1, cellHeight + 1, 2, Brushes.Black, Brushes.Brown));
                    for (int c=0; c<4; c++)
                    {
                        connectors.Add(CanvasUtil.AddLine(canvas, 0, 0, 0, 0, cellWidth * Spacing, Brushes.Transparent, null, 100));
                    }
                }

            brickBrush = CanvasUtil.CreateBrickBrush(Color.FromArgb(255, 32, 32, 32), Color.FromArgb(255, 48, 48, 48), cellWidth * 0.6, cellHeight * 0.25, cellWidth * 0.05);
            arrowLines = new List<Line>();
            arrowPointers = new List<Polygon>();
            var arrowBrush = new SolidColorBrush(Color.FromArgb(128, 160, 160, 160));
            for (int a = 0; a < 4*pieces.Count; a++)
            {
                var line = CanvasUtil.AddLine(canvas, 0, 0, 0, 0, 10, arrowBrush, null, 200);
                line.MouseDown += (s, e) => { HandleClick(s); e.Handled = true; };
                arrowLines.Add(line);
                var poly = CanvasUtil.AddPoly(canvas, [new Point(), new Point(), new Point()], 0, Brushes.Transparent, arrowBrush, null, 200);
                poly.MouseDown += (s, e) => { HandleClick(s); e.Handled = true; };
                arrowPointers.Add(poly);
            }

            colors = new Dictionary<int, int>();
            int colorNr = 0;
            foreach(var piece in pieces.OrderBy(p=>p.Key))
            {
                int pieceNr = same[piece.Key];
                colors[pieceNr] = colorNr;
                colorNr++;
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
            var transition = WpfUtil.GetTagAsObject<KlotskiTransition>(sender);
            if (transition != null)
            {
                if (graph.NavigateTo != null)
                    graph.NavigateTo(transition.node.idx);
            }
        }
    }
}
