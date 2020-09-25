using functions.TextTemplates;


namespace functions.Utility
{
    /// <summary>
    /// 画像生成ユーティリティクラス
    /// </summary>
    public static class ImageGenerator
    {
        /// <summary>
        /// テンプレート等の情報よりバイト配列生成
        /// </summary>
        /// <param name="template"></param>
        /// <param name="modelName"></param>
        /// <param name="textMessage"></param>
        /// <returns></returns>
        public static byte[] Generate(TextImageTemplate template,
            string modelName, string textMessage)
        {
#if false
            var model = new
            {
                Name = modelName,
                Text = textMessage,
                Source = template.ImagePath,
            };
            var inputXaml = Engine.Razor
                .RunCompile(template.Template, template.ImageName, null, model);

            byte[] pngBytes = new byte[] { };
            Thread pngCreationThread =
                new Thread(delegate () { pngBytes = GenerateFromXaml(inputXaml); });
            pngCreationThread.IsBackground = true;
            pngCreationThread.SetApartmentState(ApartmentState.STA);
            pngCreationThread.Start();
            pngCreationThread.Join();

            return pngBytes;
#else
            return new byte[1];
#endif
        }

        /// <summary>
        /// xamlよりバイト配列生成
        /// </summary>
        /// <param name="xaml"></param>
        /// <returns></returns>
        private static byte[] GenerateFromXaml(string xaml)
        {
#if false
            if (XamlReader.Parse(xaml) is FrameworkElement element)
            {
                return GetPngImage(element);
            }

            return Enumerable.Empty<byte>().ToArray();
#else
            return new byte[1];
#endif
        }

#if false
        /// <summary>
        /// WPFプロパティ
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
            using DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawRectangle(
                sourceBrush, null, new Rect(
                                        new Point(0, 0),
                                        new Point(element.RenderSize.Width,
                                        element.RenderSize.Height)));
            renderTarget.Render(drawingVisual);
            var pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(renderTarget));
            using var outputStream = new MemoryStream();
            pngEncoder.Save(outputStream);
            return outputStream.ToArray();
        }
#endif
    }
}
