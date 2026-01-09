using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing.IndexedProperties;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Graphs3D.Gpu;
using Graphs3D.Graphs;
using Graphs3D.Gui;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace Graphs3D.Models
{
    public class AppContext
    {
        public static readonly Brush[] BrushesColors = [Brushes.Yellow, Brushes.Magenta, Brushes.Cyan, Brushes.Red, Brushes.Green, Brushes.Blue, Brushes.White, Brushes.Gray ];

        public Simulation simulation;

        public MainWindow mainWindow;

        public OpenGlRenderer renderer;

        public ConfigWindow configWindow;

        public byte[] pixels = new byte[300 * 300 * 4];

        public void StartNewGraph(IGraph graph)
        {
            lock (this)
            {
                simulation.StartNewGraph(graph);
                renderer.UploadGraph();
                renderer.ResetOrigin();
            }
        }

        public void DrawPosition(int idx)
        {
            var canvas = configWindow.PositionCanvas;
            canvas.Dispatcher.BeginInvoke(
                DispatcherPriority.Render,
                new Action(() =>
                {
                    simulation.graph.DrawPosition(idx, canvas);
                    CanvasUtil.ReadPixelData(canvas, pixels);
                    renderer.UploadImage(pixels);
                }));
        }
    }
}
