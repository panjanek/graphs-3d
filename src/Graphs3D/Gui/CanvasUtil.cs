using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

        public static void ReadPixelData(Canvas canvas, byte[] pixels)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap(
                (int)canvas.ActualWidth,
                (int)canvas.ActualHeight,
                96,   // DPI X
                96,   // DPI Y
                PixelFormats.Pbgra32);

            int width = rtb.PixelWidth;
            int height = rtb.PixelHeight;
            int stride = width * 4;

            if (pixels.Length != height * stride)
                throw new Exception($"pixel buffer should have {width * height * 4} bytes instead of {pixels.Length}");

            rtb.Render(canvas);


            rtb.CopyPixels(pixels, stride, 0);


            for (int i = 0; i < pixels.Length; i++)
                if (pixels[i] > 0)
                    Console.WriteLine();


            for (int i = 0; i < pixels.Length/4; i++)
            {

                //pixels[i * 4 + 0] = 255;
                pixels[i * 4 + 3] = 255;
            }
        }
    }
}
