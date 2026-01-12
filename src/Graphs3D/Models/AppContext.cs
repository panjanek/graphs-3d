using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing.IndexedProperties;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media;
using System.Windows.Threading;
using Graphs3D.Gpu;
using Graphs3D.Graphs;
using Graphs3D.Graphs.Sokoban;
using Graphs3D.Graphs.TicTacToe;
using Graphs3D.Gui;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Timer = System.Timers.Timer;

namespace Graphs3D.Models
{
    public class AppContext
    {
        public static readonly Brush[] BrushesColors = [Brushes.Yellow, Brushes.Magenta, Brushes.Cyan, Brushes.Red, Brushes.Green, Brushes.Blue, Brushes.White, Brushes.Gray ];

        public const int PosWidth = 300;

        public const int PosHeight = 300;

        public Simulation simulation;

        public MainWindow mainWindow;

        public OpenGlRenderer renderer;

        public ConfigWindow configWindow;

        public byte[] pixels = new byte[PosWidth * PosHeight * 4];

        public bool positionDrawn;

        public AppContext(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            simulation = new Simulation();
            configWindow = new ConfigWindow(this);
            renderer = new OpenGlRenderer(mainWindow.placeholder, this);
            configWindow.Show();
            configWindow.Activate();
        }

        public void StartNewGraph(IGraph graph)
        {
            lock (this)
            {
                renderer.Select(0);
                simulation.StartNewGraph(graph);
                renderer.UploadGraph();
                renderer.ResetOrigin();
                graph.NavigateTo = idx => renderer.AnimateTo(idx);
            }
        }

        public void DrawPosition(int idx)
        {
            var canvas = configWindow.PositionCanvas;
            WpfUtil.DispatchRender(canvas.Dispatcher, () =>
            {
                positionDrawn = simulation.graph.DrawPosition(idx, canvas);
                CanvasUtil.ReadPixelData(canvas, pixels);
                renderer.UploadImage(pixels);
            });
        }

        public void ExpandOne(int idx)
        {
            renderer.DownloadNodes();
            if (simulation.ExpandOne(idx))
            {
                SetupPathHighlight();
                renderer.UploadGraph();
                if (renderer.SelectedIdx.HasValue)
                    DrawPosition(renderer.SelectedIdx.Value);
            }
        }

        public void ExpandMany(int count)
        {
            renderer.DownloadNodes();
            simulation.Expand(count);
            SetupPathHighlight();
            renderer.UploadGraph();
        }

        public void ExpandAll(bool stopOnWin = false)
        {
            ExpandMany(10);
            DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(150) };
            timer.Tick += (s, e) =>
            {
                ExpandMany(10);
                if (simulation.graph.IsFinished() || (stopOnWin && simulation.nodes.Any(n => n.win > 0)))
                    timer.Stop();
            };
            timer.Start();
        }

        public void SetupPathHighlight(int? explicitIdx = null)
        {
            int? nodeIdx = configWindow.PathHighlighed ? (explicitIdx.HasValue ? explicitIdx.Value : renderer.SelectedIdx) : null;
            if (nodeIdx.HasValue)
            {
                var path = simulation.PathToRoot(nodeIdx.Value).ToArray();
                for (int e = 0; e < simulation.edges.Length; e++)
                {
                    simulation.edges[e].flags = 4;
                    for (int p = 0; p < path.Length - 1; p++)
                    {
                        if ((simulation.edges[e].a == path[p] && simulation.edges[e].b == path[p + 1]) ||
                            (simulation.edges[e].b == path[p] && simulation.edges[e].a == path[p + 1]))
                        {
                            simulation.edges[e].flags = 3;
                        }
                    }
                }

                var hashset = path.ToHashSet();
                for (int i = 0; i < simulation.nodeFlags.Length; i++)
                    simulation.nodeFlags[i] = hashset.Contains(i) ? 0 : 3;
            }
            else
            {
                for (int e = 0; e < simulation.edges.Length; e++)
                    simulation.edges[e].flags = 0;
                Array.Clear(simulation.nodeFlags);
            }

            renderer.UploadFlags();
        }
    }
}
