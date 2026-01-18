using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Graphs3D.Graphs;
using Graphs3D.Graphs.Bloxorz;
using Graphs3D.Graphs.Geometry;
using Graphs3D.Graphs.Klotski;
using Graphs3D.Graphs.Sokoban;
using Graphs3D.Graphs.TicTacToe;
using Graphs3D.Models;
using Graphs3D.Utils;
using Microsoft.WindowsAPICodePack.Dialogs;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using AppContext = Graphs3D.Models.AppContext;
using Button = System.Windows.Controls.Button;

namespace Graphs3D.Gui
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public unsafe partial class ConfigWindow : Window
    {
        private AppContext app;

        private bool updating;

        public string recordDir;

        public bool PathHighlighed => highlightCheckbox.IsChecked == true;

        public bool ImageVisible =>  showImageCheckbox.IsChecked == true;

        public bool AutomaticDistance => autoDistCheckbox.IsChecked == true;

        public int NavigationMode => navigationCombo.SelectedIndex == -1 ? 0 : navigationCombo.SelectedIndex;

        public void TogglePathHighlight()
        {
            highlightCheckbox.IsChecked = !(highlightCheckbox.IsChecked == true);
            app.SetupPathHighlight();
        }

        public void ToggleImageVisible() => showImageCheckbox.IsChecked = !(showImageCheckbox.IsChecked == true);

        public void SetAutomaticDistance(bool enabled) => autoDistCheckbox.IsChecked = enabled;

        public Canvas PositionCanvas => positionCanvas;

        public ConfigWindow(AppContext app)
        {
            this.app = app;
            
            InitializeComponent();
            positionCanvas.Width = AppContext.PosWidth;
            positionCanvas.Height = AppContext.PosHeight;
            customTitleBar.MouseLeftButtonDown += (s, e) => { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); };
            minimizeButton.Click += (s, e) => WindowState = WindowState.Minimized;
            Closing += (s, e) => { e.Cancel = true; WindowState = WindowState.Minimized; };
            ContentRendered += (s, e) => { UpdateActiveControls(); UpdatePassiveControls(); };
            Loaded += ConfigWindow_Loaded;

            foreach(var button in WpfUtil.FindVisualChildren<Button>(this))
                button.PreviewKeyDown += (s, e) => e.Handled = true;

            centerButton.Click += (s, e) => { 
                navigationCombo.SelectedIndex = 0;
                app.renderer.AdaptCameraDistanceToGraphSize(); 
            };
            restartButton.Click += (s, e) => app.StartNewGraph(WpfUtil.GetTagAsObject<Func<IGraph>>(graphCombo.SelectedItem)());
            
            highlightCheckbox.Click += (s, e) => {
                app.renderer.ResetHighlighting(highlightCheckbox.IsChecked == true ? 1.0f : app.simulation.unhighlightedAlpha);
                app.SetupPathHighlight();
            };

            stopButton.Click += (s, e) => app.StopAnimation();

            navigationCombo.SelectionChanged += (s, e) =>
            {
                if (NavigationMode == 0)
                    SetAutomaticDistance(true);
                if (NavigationMode == 2)
                    app.renderer.StartFreeNavigation();
            };

            expandBtn.Click += (s, e) =>
            {
                if (expandCombo.SelectedIndex == 0)
                    app.ExpandMany();
                else if (expandCombo.SelectedIndex == 1)
                    app.ExpandAll(true);
                else if (expandCombo.SelectedIndex == 2)
                    app.ExpandAll(false);

            };

            animateToBtn.Click += (s, e) =>
            {
                if (animateToCombo.SelectedIndex == 0)
                    app.renderer.AnimateTo(0);
                else if (animateToCombo.SelectedIndex == 1)
                    app.AnimateToWinningNode();
                else if (animateToCombo.SelectedIndex == 2)
                {
                    app.animation?.Stop();
                    var winnngIdx = app.simulation.GetWinningNode();
                    if (winnngIdx.HasValue)
                    {
                        app.renderer.AnimateTo(winnngIdx.Value);
                        return;
                    }

                    var bestIdx = app.simulation.graph.GetBestNode();
                    if (bestIdx.HasValue)
                        app.renderer.AnimateTo(bestIdx.Value);
                }

            };

            KeyDown += (s, e) => app.mainWindow.MainWindow_KeyDown(s, e);
        }


        private void ConfigWindow_Loaded(object sender, RoutedEventArgs e)
        {
            graphCombo.Items.Add(new ComboBoxItem() { Content = "Sokoban online 45", Tag = new Func<IGraph>(() => new SokobanGraph("maps.sokoban-online45.txt")) });

            //graphCombo.Items.Add(new ComboBoxItem() { Content = "Sliding test", Tag = new Func<IGraph>(() => new KlotskiGraph("maps.klotski-sliding2.txt")) });
            graphCombo.Items.Add(new ComboBoxItem() { Content = "Klotski canon", Tag = new Func<IGraph>(() => new KlotskiGraph("maps.klotski-canon.txt")) });
            graphCombo.Items.Add(new ComboBoxItem() { Content = "Sliding puzzle", Tag = new Func<IGraph>(() => new KlotskiGraph("maps.klotski-sliding.txt")) });
            
            graphCombo.Items.Add(new ComboBoxItem() { Content = "Klotski test", Tag = new Func<IGraph>(() => new KlotskiGraph("maps.klotski-test.txt")) });
            graphCombo.Items.Add(new ComboBoxItem() { Content = "Bloxorz custom", Tag = new Func<IGraph>(() => new BloxorzGraph("maps.bloxorz-custom.txt")) });
            graphCombo.Items.Add(new ComboBoxItem() { Content = "Bloxorz flat", Tag = new Func<IGraph>(() => new BloxorzGraph("maps.bloxorz-flat.txt")) });
            graphCombo.Items.Add(new ComboBoxItem() { Content = "Bloxorz 3", Tag = new Func<IGraph>(() => new BloxorzGraph("maps.bloxorz3.txt")) });
            graphCombo.Items.Add(new ComboBoxItem() { Content = "Bloxorz 1", Tag = new Func<IGraph>(() => new BloxorzGraph("maps.bloxorz1.txt")) });
            graphCombo.Items.Add(new ComboBoxItem() { Content = "Sokoban Junior 3", Tag = new Func<IGraph>(() => new SokobanGraph("maps.sokoban-jr3.txt")) });
            graphCombo.Items.Add(new ComboBoxItem() { Content = "Sokoban Junior 4", Tag = new Func<IGraph>(() => new SokobanGraph("maps.sokoban-jr4.txt")) });
            graphCombo.Items.Add(new ComboBoxItem() { Content = "Sokoban classic level 2", Tag = new Func<IGraph>(() => new SokobanGraph("maps.sokoban-classic2a.txt")) });
            graphCombo.Items.Add(new ComboBoxItem() { Content = "Sokoban classic level 1", Tag = new Func<IGraph>(() => new SokobanGraph("maps.sokoban-classic1.txt")) });
            graphCombo.Items.Add(new ComboBoxItem() { Content = "Sokoban online 5", Tag = new Func<IGraph>(() => new SokobanGraph("maps.sokoban-online5.txt")) });
            graphCombo.Items.Add(new ComboBoxItem() { Content = "Tic Tac Toe 3x3", Tag = new Func<IGraph>(() => new TicTacToeGraph(3)) });
            graphCombo.Items.Add(new ComboBoxItem() { Content = "Cylinder 10x20", Tag = new Func<IGraph>(() => new LatticeGraph(20, 20, true, false)) });
            graphCombo.Items.Add(new ComboBoxItem() { Content = "Torus 30x60", Tag = new Func<IGraph>(() => new LatticeGraph(30, 60, true, true)) });
            graphCombo.Items.Add(new ComboBoxItem() { Content = "Torus 100x200", Tag = new Func<IGraph>(() => new LatticeGraph(100, 200, true, true)) });
        }

        private void global_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (graphCombo != null && !updating && app.renderer!=null)
            {
                app.renderer.Paused = false;
                app.StartNewGraph(WpfUtil.GetTagAsObject<Func<IGraph>>(graphCombo.SelectedItem)());
                UpdateActiveControls();
                UpdatePassiveControls();
            }
        }

        public void SelectGraph(int nr)
        {
            graphCombo.SelectedIndex = nr;
        }

        private void Record_Click(object sender, RoutedEventArgs e)
        {
            if (recordButton.IsChecked == true)
            {
                var dialog = new CommonOpenFileDialog { IsFolderPicker = true, Title = "Select folder to save frames as PNG files" };
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    recordDir = dialog.FileName;
                else
                    recordButton.IsChecked = false;
            }
            else
            {
                recordDir = null;
            }

            e.Handled = true;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!updating)
            {
                var tag = WpfUtil.GetTagAsString(sender);
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    ReflectionUtil.SetObjectValue<float>(app.simulation, tag, (float)e.NewValue);
                    UpdatePassiveControls();
                }
            }
        }

        private void infoText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var tag = WpfUtil.GetTagAsString(sender);
            if (!string.IsNullOrWhiteSpace(tag))
                WpfUtil.FindVisualChildren<Slider>(this).Where(s => WpfUtil.GetTagAsString(s) == tag).FirstOrDefault()?.Focus();
            e.Handled = true;
        }

        public void UpdateActiveControls()
        {
            updating = true;
            foreach (var slider in WpfUtil.FindVisualChildren<Slider>(this))
            {
                var tag = WpfUtil.GetTagAsString(slider);
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    slider.Value = ReflectionUtil.GetObjectValue<float>(app.simulation, tag);
                }
            }
            updating = false;
        }

        public void UpdatePassiveControls()
        {
            foreach (var text in WpfUtil.FindVisualChildren<TextBlock>(this))
                    WpfUtil.UpdateTextBlockForSlider(this, text, app.simulation);

            var itemExpand = (ComboBoxItem)expandCombo.Items[0];
            itemExpand.Content = $"Next {(int)app.simulation.expansionSpeed} nodes";

        }
    }
}
