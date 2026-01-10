using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Graphs3D.Gpu;
using Graphs3D.Utils;
using Brushes = System.Windows.Media.Brushes;
using ComboBox = System.Windows.Controls.ComboBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;

namespace Graphs3D.Gui
{
    public static class WpfUtil
    {
        public static void DispatchRender(Dispatcher dispatcher, Action action)
        {
            dispatcher.BeginInvoke(
            DispatcherPriority.Render,
            new Action(() => action()));
        }

        public static bool CheckIfHit(Shape shape, double x, double y)
        {
            var left = (double)shape.GetValue(Canvas.LeftProperty);
            var top = (double)shape.GetValue(Canvas.TopProperty);
            var w = shape.Width;
            var h = shape.Height;
            return (x >= left && x <= left + w && y >= top && y <= top + h);
        }

        public static string GetComboSelectionAsString(ComboBox combo)
        {
            if (combo.SelectedItem is ComboBoxItem)
            {
                var item = (ComboBoxItem)combo.SelectedItem;
                return item.Content?.ToString();
            }

            return null;
        }

        public static int GetComboSelectionAsInt(ComboBox combo)
        {
            var str = GetComboSelectionAsString(combo);
            return int.Parse(str);
        }

        public static void SetComboStringSelection(ComboBox combo, string value, bool byTag = false)
        {
            foreach (var item in combo.Items)
            {
                if (item is ComboBoxItem)
                {
                    var comboItem = item as ComboBoxItem;
                    comboItem.IsSelected = byTag ? (GetTagAsString(comboItem) == value) : (comboItem.Content?.ToString() == value);
                }
            }
        }

        public static string GetTagAsString(object element)
        {
            if (element is FrameworkElement)
            {
                var el = (FrameworkElement)element;
                if (el.Tag is string)
                    return el.Tag as string;
                else
                    return null;
            }
            else
                return null;
        }

        public static int GetTagAsInt(object element)
        {
            return int.Parse(GetTagAsString(element));
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent)
        where T : DependencyObject
        {
            if (parent == null)
                yield break;

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is T t)
                    yield return t;

                foreach (var descendant in FindVisualChildren<T>(child))
                    yield return descendant;
            }
        }

        public static void UpdateTextBlockForSlider(FrameworkElement parent, TextBlock text, object recipe)
        {
            var tag = WpfUtil.GetTagAsString(text);
            if (!string.IsNullOrWhiteSpace(tag))
            {
                string format = "0.000";
                var slider = WpfUtil.FindVisualChildren<Slider>(parent).FirstOrDefault(s => WpfUtil.GetTagAsString(s) == tag);
                if (slider != null)
                {
                    switch (slider.SmallChange)
                    {
                        case 1:
                            format = "0";
                            break;
                        case 0.1:
                            format = "0.0";
                            break;
                        case 0.01:
                            format = "0.00";
                            break;
                        case 0.001:
                            format = "0.000";
                            break;
                        case 0.0001:
                            format = "0.0000";
                            break;
                    }
                }

                var value = ReflectionUtil.GetObjectValue<float>(recipe, tag);
                text.Text = value.ToString(format, CultureInfo.InvariantCulture);
                text.Background = Brushes.Black;
                text.Foreground = Brushes.White;
            }
        }

        public static void RaiseMouseDown(UIElement targetElement, double x, double y, MouseButton button = MouseButton.Left)
        {
            if (targetElement == null)
                throw new ArgumentNullException(nameof(targetElement));

            targetElement.Dispatcher.Invoke(() =>
            {
                // Ensure the element can receive input
                if (!targetElement.IsVisible || !targetElement.IsEnabled)
                    return;

                var mouseDevice = InputManager.Current.PrimaryMouseDevice;

                var args = new MouseButtonEventArgs(
                    mouseDevice,
                    Environment.TickCount,
                    button)
                {
                    RoutedEvent = UIElement.MouseDownEvent,
                    Source = targetElement
                };

                // Set position via override
                typeof(MouseEventArgs)
                    .GetProperty("Position", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?
                    .SetValue(args, new Point(x, y));

                targetElement.RaiseEvent(args);
            });
        }
    }
}
