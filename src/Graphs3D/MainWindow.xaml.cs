using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Graphs3D.Gpu;
using Graphs3D.Graphs.Sokoban;
using Graphs3D.Graphs.TicTacToe;
using Graphs3D.Gui;
using Graphs3D.Models;
using Graphs3D.Utils;
using AppContext = Graphs3D.Models.AppContext;
using Application = System.Windows.Application;

namespace Graphs3D
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool uiPending;

        private DateTime lastCheckTime;

        private long lastCheckFrameCount;

        private AppContext app;

        private Random rnd = new Random(1);

        public MainWindow()
        {
            InitializeComponent();
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        private void parent_Loaded(object sender, RoutedEventArgs e)
        {
            app = new AppContext(this);
            app.configWindow.SelectGraph(0);
            KeyDown += MainWindow_KeyDown;
            System.Timers.Timer systemTimer = new System.Timers.Timer() { Interval = 10 };
            systemTimer.Elapsed += SystemTimer_Elapsed;
            systemTimer.Start();
            DispatcherTimer infoTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1.0) };
            infoTimer.Tick += InfoTimer_Tick;
            infoTimer.Start();
        }
        public void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    app.renderer.Paused = !app.renderer.Paused;
                    e.Handled = true;
                    break;
                case Key.Escape:
                    app.renderer.StopTracking();
                    e.Handled = true;
                    break;
                case Key.Z:
                    app.ExpandMany();
                    e.Handled = true;
                    break;
                case Key.E:
                    app.ExpandAll(true);
                    e.Handled = true;
                    break;
                case Key.W:
                    app.AnimateToWinningNode();
                    e.Handled = true;
                    break;
                case Key.R:
                    app.renderer.AnimateTo(0);
                    e.Handled = true;
                    break;
                case Key.H:
                    app.configWindow.TogglePathHighlight();
                    e.Handled = true;
                    break;
                case Key.I:
                    app.configWindow.ToggleImageVisible();
                    e.Handled = true;
                    break;
                case Key.Up:
                    var selectedIdx = app.renderer.SelectedIdx;
                    if (!selectedIdx.HasValue)
                        return;

                    var winning = app.simulation.GetWinningPath();
                    var nr = winning.IndexOf(selectedIdx.Value);
                    if (nr == -1) nr = 0;
                    if (winning.Count > 0 && nr > -1 && nr < winning.Count - 1)
                    { 
                        app.renderer.AnimateTo(winning[nr+1]);
                    }
                    else
                    {
                        var children = app.simulation.GetChildren(selectedIdx.Value);
                        if (children.Count > 0)
                            app.renderer.AnimateTo(children[rnd.Next(children.Count)]);
                    }

                    e.Handled = true;
                    break;
                case Key.Down:
                    selectedIdx = app.renderer.SelectedIdx;
                    if (selectedIdx.HasValue && selectedIdx.Value < app.simulation.nodes.Length)
                    {
                        var parentIdx = app.simulation.nodes[selectedIdx.Value].parent;
                        app.renderer.AnimateTo(parentIdx);
                    }

                    e.Handled = true;
                    break;
            }
        }

        private void SystemTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (!uiPending)
            {
                uiPending = true;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        app.renderer.Step();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    finally
                    {
                        uiPending = false;
                    }

                    uiPending = false;
                }), DispatcherPriority.Render);
            }
        }

        private void InfoTimer_Tick(object? sender, EventArgs e)
        {
            var now = DateTime.Now;
            var timespan = now - lastCheckTime;
            double frames = app.renderer.FrameCounter - lastCheckFrameCount;
            if (timespan.TotalSeconds >= 0.0001)
            {
                double fps = frames / timespan.TotalSeconds;
                Title = $"Graphs3D. " +
                        $"fps:{fps.ToString("0.0")} "+
                        $"nodes:{app.simulation.config.nodesCount} "+
                        $"edges:{app.simulation.config.edgesCount} "+
                        $"cells:{app.simulation.config.useCells} ";

                if (!string.IsNullOrWhiteSpace(app.configWindow.recordDir))
                {
                    Title += $"[recording to {app.configWindow.recordDir}] ";
                }

                lastCheckFrameCount = app.renderer.FrameCounter;
                lastCheckTime = now;
            }
        }
    }
}