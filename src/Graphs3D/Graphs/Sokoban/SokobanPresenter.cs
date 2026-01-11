using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;
using Graphs3D.Gui;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Rectangle = System.Windows.Shapes.Rectangle;
using Color = System.Windows.Media.Color;
using Colors = System.Windows.Media.Colors;
using System.Windows.Media;

namespace Graphs3D.Graphs.Sokoban
{
    public class SokobanPresenter
    {
        private SokobanGraph graph;

        private Canvas canvas;

        private List<Rectangle> walls;

        private List<Rectangle> boxes;

        private List<Rectangle> targets;

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
            return true;
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
            canvas.Background = Brushes.Black;
            walls = new List<Rectangle>();
            boxes = new List<Rectangle>();
            targets = new List<Rectangle>();

            var size = Math.Max(map.GetLength(0), map.GetLength(1));
            cellWidth = canv.Width / size;
            cellHeight = canv.Height / size;
            marginLeft = (canv.Width - map.GetLength(0) * cellWidth) / 2;
            marginTop = (canv.Height - map.GetLength(1) * cellHeight) / 2;

            var boxBrush = CanvasUtil.CreateDiagonalStripeBrush(Colors.SandyBrown, Colors.Yellow, 5, 45);
            var targetBrush = new SolidColorBrush(Color.FromArgb(96, 0, 255, 0));
            var brickBrush = CanvasUtil.CreateBrickBrush(Colors.LightGray, Colors.DarkGray, cellWidth * 0.6, cellHeight * 0.25, cellWidth*0.05);

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

                }
        }
    }
}
