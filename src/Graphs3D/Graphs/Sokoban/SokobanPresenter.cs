using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Graphs3D.Gui;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Colors = System.Windows.Media.Colors;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace Graphs3D.Graphs.Sokoban
{
    public class SokobanPresenter
    {
        private SokobanGraph graph;

        private Canvas canvas;

        private List<Rectangle> walls;

        private List<Rectangle> boxes;

        private List<Rectangle> targets;

        private List<Line> arrowLines;

        private List<Polygon> arrowPointers;

        private double cellWidth;

        private double cellHeight;

        private double marginLeft;

        private double marginTop;

        private Ellipse player;
        public SokobanPresenter(SokobanGraph graph) 
        {
            this.graph = graph;
        }

        public bool Draw(Canvas canv, SokobanNode node)
        {
            lock (this)
            {
                if (this.canvas == null)
                    Initialize(canv, node.position);

                int wallNr = 0;
                int boxNr = 0;
                int targetNr = 0;
                for (int y = 0; y < node.position.GetLength(1); y++)
                    for (int x = 0; x < node.position.GetLength(0); x++)
                    {
                        if (node.position[x, y] == SokobanNode.WALL)
                            PositionNexWall(ref wallNr, x, y);
                        if (node.position[x, y] == SokobanNode.BOX || node.position[x, y] == SokobanNode.BOXONTARGET)
                            PositionNextBox(ref boxNr, x, y);
                        if (node.position[x, y] == SokobanNode.TARGET || node.position[x, y] == SokobanNode.BOXONTARGET)
                            PositionNextTarget(ref targetNr, x, y);
                    }

                player.SetValue(Canvas.LeftProperty, marginLeft + node.playerVisualPos.X * cellWidth + cellWidth * 0.15);
                player.SetValue(Canvas.TopProperty, marginTop + node.playerVisualPos.Y * cellHeight + cellHeight * 0.15);

                arrowLines.ForEach(l => l.Visibility = System.Windows.Visibility.Collapsed);
                arrowPointers.ForEach(l => l.Visibility = System.Windows.Visibility.Collapsed);
                var transitions = graph.GetAvailableTransitions(node);
                int arrowsCount = 0;
                foreach (var trans in transitions)
                    PositionNextArrow(ref arrowsCount, trans);

                return true;
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
            var transition = WpfUtil.GetTagAsObject<SokobanTransition>(sender);
            if (transition != null)
            {
                var boxRect = WpfUtil.FindVisualChildren<Rectangle>(canvas)
                                      .Where(r=>r.Tag!=null && r.Tag is SokobanXY)
                                      .FirstOrDefault(r => ((SokobanXY)r.Tag).X == transition.move.boxToPush.X && ((SokobanXY)r.Tag).Y == transition.move.boxToPush.Y);
                if (graph.NavigateTo != null)
                    graph.NavigateTo(transition.node.idx);
            }
        }

        private void PositionNextArrow(ref int arrowsCount, SokobanTransition trans)
        {
            var offsetY = trans.move.dir.Y * cellWidth * 0.2;
            var offsetX = trans.move.dir.X * cellWidth * 0.2;
            arrowLines[arrowsCount].X1 = marginLeft+trans.move.boxToPush.X * cellWidth + cellWidth / 2 + offsetX;
            arrowLines[arrowsCount].Y1 = marginTop+trans.move.boxToPush.Y * cellHeight + cellHeight / 2 + offsetY;
            arrowLines[arrowsCount].X2 = marginLeft+(trans.move.boxToPush.X + trans.move.dir.X) * cellWidth + cellWidth / 2 - offsetX*2;
            arrowLines[arrowsCount].Y2 = marginTop+(trans.move.boxToPush.Y + trans.move.dir.Y) * cellHeight + cellHeight / 2 - offsetY*2;
            arrowLines[arrowsCount].Visibility = System.Windows.Visibility.Visible;
            arrowLines[arrowsCount].Tag = trans;

            arrowPointers[arrowsCount].Points[0] = new Point(arrowLines[arrowsCount].X2 + offsetX * 1.5, arrowLines[arrowsCount].Y2 + offsetY * 1.5);
            arrowPointers[arrowsCount].Points[1] = new Point(arrowLines[arrowsCount].X2 - offsetY * 1.5, arrowLines[arrowsCount].Y2 - offsetX * 1.5);
            arrowPointers[arrowsCount].Points[2] = new Point(arrowLines[arrowsCount].X2 + offsetY * 1.5, arrowLines[arrowsCount].Y2 + offsetX * 1.5);
            arrowPointers[arrowsCount].Visibility = System.Windows.Visibility.Visible;
            arrowPointers[arrowsCount].Tag = trans;

            arrowsCount++;
        }

        private void PositionNexWall(ref int wallNr, int x, int y)
        {
            walls[wallNr].SetValue(Canvas.LeftProperty, marginLeft + x * cellWidth);
            walls[wallNr].SetValue(Canvas.TopProperty, marginTop + y * cellHeight);
            wallNr++;
        }

        private void PositionNextBox(ref int boxNr, int x, int y)
        {
            boxes[boxNr].SetValue(Canvas.LeftProperty, marginLeft + x * cellWidth + 0.1*cellWidth);
            boxes[boxNr].SetValue(Canvas.TopProperty, marginTop + y * cellHeight + 0.1*cellHeight);
            boxes[boxNr].Tag = new SokobanXY(x, y);
            boxNr++;
        }

        private void PositionNextTarget(ref int targetNr, int x, int y)
        {
            targets[targetNr].SetValue(Canvas.LeftProperty, marginLeft + x * cellWidth + 0.05*cellWidth);
            targets[targetNr].SetValue(Canvas.TopProperty, marginTop + y * cellHeight + 0.05*cellHeight);
            targetNr++;
        }

        private void Initialize(Canvas canv, int[,] map)
        {
            this.canvas = canv;
            canvas.Children.Clear();
            canvas.Background = new SolidColorBrush(Color.FromArgb(255, 32, 16, 16));
            walls = new List<Rectangle>();
            boxes = new List<Rectangle>();
            targets = new List<Rectangle>();
            arrowLines = new List<Line>();
            arrowPointers = new List<Polygon>();

            var size = Math.Max(map.GetLength(0), map.GetLength(1));
            cellWidth = canv.Width / size;
            cellHeight = canv.Height / size;
            marginLeft = (canv.Width - map.GetLength(0) * cellWidth) / 2;
            marginTop = (canv.Height - map.GetLength(1) * cellHeight) / 2;

            var boxBrush = CanvasUtil.CreateDiagonalStripeBrush(Colors.SandyBrown, Colors.Yellow, 5, 45);
            var targetBrush = new SolidColorBrush(Color.FromArgb(96, 0, 255, 0));
            var brickBrush = CanvasUtil.CreateBrickBrush(Colors.LightGray, Colors.DarkGray, cellWidth * 0.6, cellHeight * 0.25, cellWidth*0.05);
            var arrowBrush = new SolidColorBrush(Color.FromArgb(128, 160, 160, 160));

            player = CanvasUtil.AddEllipse(canvas, 0, 0, cellWidth*0.7, cellHeight*0.7, 5, Brushes.Cyan, Brushes.Yellow, null, 100);
            for (int y=0; y< map.GetLength(1); y++)
                for(int x=0; x<map.GetLength(0); x++)
                {
                    if (map[x, y] == SokobanNode.WALL)
                        walls.Add(CanvasUtil.AddRect(canvas, 0, 0, cellWidth+1, cellHeight+1, 0, Brushes.Brown, brickBrush));
                    else if (map[x,y] == SokobanNode.BOX)
                        boxes.Add(CanvasUtil.AddRect(canvas, 0, 0, cellWidth*0.8, cellHeight*0.8, 2, Brushes.SandyBrown, boxBrush, null, 50));
                    else if (map[x, y] == SokobanNode.TARGET)
                        targets.Add(CanvasUtil.AddRect(canvas, 0, 0, cellWidth*0.9, cellHeight*0.9, 0, Brushes.Transparent, targetBrush, null, 0));

                    if (map[x, y] == SokobanNode.BOX)
                        for (int b = 0; b < 4; b++)
                        {
                            var line = CanvasUtil.AddLine(canvas, 0, 0, 0, 0, 10, arrowBrush, null, 200);
                            line.MouseDown += (s, e) => { HandleClick(s); e.Handled = true; };
                            arrowLines.Add(line);
                            var poly = CanvasUtil.AddPoly(canvas, [new Point(), new Point(), new Point()], 0, Brushes.Transparent, arrowBrush, null, 200);
                            poly.MouseDown += (s, e) => { HandleClick(s); e.Handled = true; };
                            arrowPointers.Add(poly);
                        }
                }
        }
    }

    public class PointerTag
    {
        public Rectangle rect;

        public SokobanMove move;
    }
}
