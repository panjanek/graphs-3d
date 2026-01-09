using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brush;

namespace Graphs3D.Gui
{
    public static class CanvasUtil
    {
        public static Line AddLine(Canvas canvas, double x1, double y1, double x2, double y2, double thickness, Brush stroke)
        {
            Line line = new Line() { StrokeThickness = thickness, Stroke = stroke, X1 = x1, Y1 = y1, X2 = x2, Y2 = y2 };
            canvas.Children.Add(line);
            return line;
        }

        public static Ellipse AddEllipse(Canvas canvas, double left, double top, double width, double height, double thickness, Brush stroke, Brush fill)
        {
            Ellipse el = new Ellipse() { Fill = fill, Stroke = stroke, StrokeThickness = thickness, Width =width, Height = height };
            el.SetValue(Canvas.LeftProperty, left);
            el.SetValue(Canvas.TopProperty, top);
            canvas.Children.Add(el);
            return el;
        }
    }
}
