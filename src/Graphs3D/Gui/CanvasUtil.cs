using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace Graphs3D.Gui
{
    public static class CanvasUtil
    {
        public static Line AddLine(Canvas canvas, double x1, double y1, double x2, double y2, double thickness, Brush stroke, object tag = null, int? zIndex = null)
        {
            Line line = new Line() { StrokeThickness = thickness, Stroke = stroke, X1 = x1, Y1 = y1, X2 = x2, Y2 = y2 };
            if (zIndex.HasValue)
                line.SetValue(Canvas.ZIndexProperty, zIndex.Value);
            if (tag != null)
                line.Tag = tag;
            canvas.Children.Add(line);
            return line;
        }

        public static Ellipse AddEllipse(Canvas canvas, double left, double top, double width, double height, double thickness, Brush stroke, Brush fill, object tag = null, int? zIndex = null)
        {
            Ellipse el = new Ellipse() { Fill = fill, Stroke = stroke, StrokeThickness = thickness, Width =width, Height = height };
            el.SetValue(Canvas.LeftProperty, left);
            el.SetValue(Canvas.TopProperty, top);
            if (zIndex.HasValue)
                el.SetValue(Canvas.ZIndexProperty, zIndex.Value);
            if (tag != null)
                el.Tag = tag;
            canvas.Children.Add(el);
            return el;
        }

        public static Rectangle AddRect(Canvas canvas, double left, double top, double width, double height, double thickness, Brush stroke, Brush fill, object tag = null, int? zIndex = null)
        {
            Rectangle rect = new Rectangle() { Fill = fill, Stroke = stroke, StrokeThickness = thickness, Width = width, Height = height };
            rect.SetValue(Canvas.LeftProperty, left);
            rect.SetValue(Canvas.TopProperty, top);
            if (zIndex.HasValue)
                rect.SetValue(Canvas.ZIndexProperty, zIndex.Value);
            if (tag != null)
                rect.Tag = tag;
            canvas.Children.Add(rect);
            return rect;
        }

        public static Brush CreateDiagonalStripeBrush(Color color1, Color color2, double stripeWidth = 6, double angleDegrees = 45)
        {
            double size = stripeWidth * 2;

            var group = new DrawingGroup();

            // First stripe
            group.Children.Add(
                new GeometryDrawing(
                    new SolidColorBrush(color1),
                    null,
                    new RectangleGeometry(new Rect(0, 0, stripeWidth, size))));

            // Second stripe
            group.Children.Add(
                new GeometryDrawing(
                    new SolidColorBrush(color2),
                    null,
                    new RectangleGeometry(new Rect(stripeWidth, 0, stripeWidth, size))));

            var brush = new DrawingBrush(group)
            {
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, size, size),
                ViewportUnits = BrushMappingMode.Absolute,
                Transform = new RotateTransform(angleDegrees)
            };

            // Freeze for performance (important)
            brush.Freeze();

            return brush;
        }

        public static Brush CreateBrickBrush(
           Color brickColor,
           Color mortarColor,
           double brickWidth = 40,
           double brickHeight = 20,
           double mortarThickness = 2)
        {
            double tileWidth = brickWidth * 3;
            double tileHeight = brickHeight * 2;

            var group = new DrawingGroup();

            // Mortar background
            group.Children.Add(
                new GeometryDrawing(
                    new SolidColorBrush(mortarColor),
                    null,
                    new RectangleGeometry(new Rect(0, 0, tileWidth, tileHeight))));

            double bw = brickWidth - mortarThickness;
            double bh = brickHeight - mortarThickness;

            // ---- Row 1 (even, no offset)
            for (int i = 0; i < 3; i++)
            {
                group.Children.Add(CreateBrick(
                    brickColor,
                    i * brickWidth + mortarThickness,
                    mortarThickness,
                    bw,
                    bh));
            }

            // ---- Row 2 (odd, half-brick offset)
            double offset = brickWidth / 2;

            for (int i = 0; i < 3; i++)
            {
                group.Children.Add(CreateBrick(
                    brickColor,
                    offset + i * brickWidth + mortarThickness,
                    brickHeight + mortarThickness,
                    bw,
                    bh));
            }

            group.Children.Add(CreateBrick(
                brickColor,
                0,
                brickHeight + mortarThickness,
                bw/2,
                bh));

            var brush = new DrawingBrush(group)
            {
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, tileWidth, tileHeight),
                ViewportUnits = BrushMappingMode.Absolute
            };

            brush.Freeze();
            return brush;
        }

        private static GeometryDrawing CreateBrick(
            Color color,
            double x,
            double y,
            double width,
            double height)
        {
            return new GeometryDrawing(
                new SolidColorBrush(color),
                null,
                new RectangleGeometry(new Rect(x, y, width, height)));
        }

        public static void ReadPixelData(Canvas canvas, byte[] pixels)
        {
            canvas.UpdateLayout();
            var size = canvas.RenderSize;

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

            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                dc.PushTransform(new ScaleTransform(1, -1));
                dc.PushTransform(new TranslateTransform(0, -height));
                var vb = new VisualBrush(canvas);
                dc.DrawRectangle(vb, null, new Rect(0, 0, width, height));
            }

            rtb.Render(dv);
            rtb.CopyPixels(pixels, stride, 0);
            for (int i = 0; i < pixels.Length / 4; i++)
            {
                pixels[i * 4 + 3] = 255;
                var r = pixels[i * 4 + 0];
                pixels[i * 4 + 0] = pixels[i * 4 + 2];
                pixels[i * 4 + 2] = r;
            }
        }
    }
}
