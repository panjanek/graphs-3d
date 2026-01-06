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
            randomButton.PreviewKeyDown += (s, e) => e.Handled=true;
            restartButton.PreviewKeyDown += (s, e) => e.Handled = true;
            saveButton.PreviewKeyDown += (s, e) => e.Handled = true;
            loadButton.PreviewKeyDown += (s, e) => e.Handled = true;
            recordButton.PreviewKeyDown += (s, e) => e.Handled = true;
            backButton.PreviewKeyDown += (s, e) => e.Handled = true;
            backButton.Click += (s, e) => app.renderer.ResetOrigin();
            restartButton.Click += (s, e) => 
            { 
                app.simulation.InitializeParticles(app.simulation.config.nodesCount);
                app.renderer.UploadGraph();
            };

            randomButton.Click += (s, e) =>
            {
                app.simulation.InitializeParticles(app.simulation.config.nodesCount);
                app.simulation.seed++;
                app.renderer.UploadGraph();
                app.renderer.ResetOrigin();
            };

            saveButton.Click += (s, e) =>
            {
                var dialog = new CommonSaveFileDialog { Title = "Save configuration json file", DefaultExtension = "json" };
                dialog.Filters.Add(new CommonFileDialogFilter("JSON files", "*.json"));
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    SimFactory.SaveToFile(app.simulation, dialog.FileName);
                    PopupMessage.Show(app.mainWindow, $"Config saved to {dialog.FileName}");
                }
            };

            loadButton.Click += (s, e) =>
            {
                var dialog = new CommonOpenFileDialog { Title = "Open configuration json file", DefaultExtension = "json" };
                dialog.Filters.Add(new CommonFileDialogFilter("JSON files", "*.json"));
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    var newSim = SimFactory.LoadFromFile(dialog.FileName);
                    app.simulation = newSim;
                    app.renderer.UploadGraph();
                    UpdateActiveControls();
                    UpdatePassiveControls();
                    PopupMessage.Show(app.mainWindow, $"Config loaded from {dialog.FileName}");
                    if (app.renderer.Paused)
                    {
                        app.renderer.Paused = false;
                        app.renderer.Step();
                        app.renderer.Paused = true;
                    }
                }
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
            if (fieldSize != null && particlesCount != null && !updating)
            {
                var newParticleCountStr = WpfUtil.GetComboSelectionAsString(particlesCount);
                var newSizeStr = WpfUtil.GetComboSelectionAsString(fieldSize);
                if (!string.IsNullOrWhiteSpace(newParticleCountStr) && !string.IsNullOrWhiteSpace(newSizeStr))
                {
                    var newParticleCount = int.Parse(newParticleCountStr);
                    var sizeSplit = newSizeStr.Split('x');
                    var newSize = int.Parse(sizeSplit[0]);
                    if (newParticleCount != app.simulation.config.nodesCount ||
                        newSize != app.simulation.config.fieldSize)
                    {
                        app.simulation.StartSimulation(newParticleCount, newSize);
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
            WpfUtil.SetComboStringSelection(fieldSize, $"{app.simulation.config.fieldSize}x{app.simulation.config.fieldSize}x{app.simulation.config.fieldSize}");
            WpfUtil.SetComboStringSelection(particlesCount, app.simulation.config.nodesCount.ToString());
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
