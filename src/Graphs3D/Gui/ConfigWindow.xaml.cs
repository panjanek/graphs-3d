using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Linq.Expressions;
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
using Microsoft.WindowsAPICodePack.Dialogs;
using Graphs3D.Models;
using Graphs3D.Utils;
using AppContext = Graphs3D.Models.AppContext;

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
        public ConfigWindow(AppContext app)
        {
            this.app = app;
            InitializeComponent();
            customTitleBar.MouseLeftButtonDown += (s, e) => { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); };
            minimizeButton.Click += (s, e) => WindowState = WindowState.Minimized;
            Closing += (s, e) => { e.Cancel = true; WindowState = WindowState.Minimized; };
            ContentRendered += (s, e) => { UpdateActiveControls(); UpdatePassiveControls(); };
            restartButton.PreviewKeyDown += (s, e) => e.Handled = true;
            recordButton.PreviewKeyDown += (s, e) => e.Handled = true;
            backButton.PreviewKeyDown += (s, e) => e.Handled = true;
            backButton.Click += (s, e) => app.renderer.ResetOrigin();
            restartButton.Click += (s, e) => 
            { 

            };

            KeyDown += (s, e) => app.mainWindow.MainWindow_KeyDown(s, e);
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

        private void global_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (graph != null && !updating)
            {
                var newGraph = WpfUtil.GetComboSelectionAsString(graph);
                if (!string.IsNullOrWhiteSpace(newGraph))
                {
                    if (newGraph != app.simulation.graph?.GetType().Name)
                    {
                        app.simulation.StartSimulation();
                        app.renderer.UploadGraph();
                        UpdateActiveControls();
                        UpdatePassiveControls();
                        if (app.renderer.Paused)
                        {
                            app.renderer.Paused = false;
                            app.renderer.Step();
                            app.renderer.Paused = true;
                        }
                    }
                }

            }
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
            WpfUtil.SetComboStringSelection(graph, app.simulation.graph?.GetType().Name);
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
        }
    }
}
