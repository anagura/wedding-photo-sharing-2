using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NetFrameworkFunctions.Utility
{
    /// <summary>
    /// ImageConverter
    /// </summary>
    public static class ImageConverter
    {
        /// <summary>
        /// xamlをimage変換
        /// </summary>
        /// <param name="inputXaml"></param>
        /// <returns></returns>
        public static byte[] ConvertFromXaml(string inputXaml)
        {
            var pngBytes = new byte[] { };
#if true
            var pngCreationThread =
            new Thread(() => { pngBytes = GenerateFromXaml(inputXaml); })
            {
                IsBackground = false
            };
            pngCreationThread.SetApartmentState(ApartmentState.STA);
            pngCreationThread.Start();
            pngCreationThread.Join();
#else
            pngBytes = GenerateFromXaml(inputXaml);
#endif
            return pngBytes;
        }

        /// <summary>
        /// xamlよりバイト配列生成
        /// </summary>
        /// <param name="xaml"></param>
        /// <returns></returns>
        private static byte[] GenerateFromXaml(string xaml)
        {
            if (XamlReader.Parse(xaml) is FrameworkElement element)
            {
                return GetPngImage(element);
            }

            return Enumerable.Empty<byte>().ToArray();
        }

        /// <summary>
        /// png取得
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private static byte[] GetPngImage(FrameworkElement element)
        {
            var size = new Size(double.PositiveInfinity, double.PositiveInfinity);
            element.Measure(size);
            element.Arrange(new Rect(element.DesiredSize));
            var renderTarget =
              new RenderTargetBitmap((int)element.RenderSize.Width,
                                     (int)element.RenderSize.Height,
                                     96, 96,
                                     PixelFormats.Pbgra32);
            var sourceBrush = new VisualBrush(element);
            var drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawRectangle(
                sourceBrush, null, new Rect(
                                        new Point(0, 0),
                                        new Point(element.RenderSize.Width,
                                        element.RenderSize.Height)));
            renderTarget.Render(drawingVisual);
            var pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(renderTarget));
            var outputStream = new MemoryStream();
            pngEncoder.Save(outputStream);
            return outputStream.ToArray();
        }
    }
}
