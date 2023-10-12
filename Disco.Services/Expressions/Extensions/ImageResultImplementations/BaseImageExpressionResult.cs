using Disco.Models.Services.Expressions.Extensions;
using System;
using System.Drawing;
using System.IO;

namespace Disco.Services.Expressions.Extensions.ImageResultImplementations
{
    public abstract class BaseImageExpressionResult : IImageExpressionResult
    {
        public byte Quality { get; set; }
        public ImageExpressionFormat Format { get; set; }
        public bool ShowField { get; set; }
        public string BackgroundColour { get; set; }
        public bool BackgroundPreferTransparent { get; set; }

        public BaseImageExpressionResult()
        {
            Format = ImageExpressionFormat.Jpeg;
            Quality = 90;
            ShowField = false;
            BackgroundPreferTransparent = true;
        }

        public abstract MemoryStream GetImage(int width, int height);
        public abstract MemoryStream GetImage(out int width, out int height);

        protected MemoryStream RenderBitmapImage(Image SourceImage, int Width, int Height)
        {
            if (SourceImage == null)
                throw new ArgumentNullException(nameof(SourceImage));
            if (Width <= 0)
                throw new ArgumentOutOfRangeException(nameof(Width), "Width must be > 0");
            if (Height <= 0)
                throw new ArgumentOutOfRangeException(nameof(Height), "Height must be > 0");
            if (Format != ImageExpressionFormat.Jpeg && Format != ImageExpressionFormat.Png)
                throw new NotSupportedException($"The format {Format} is not supported by this method");

            Brush backgroundBrush = null;
            if (Format == ImageExpressionFormat.Jpeg || !BackgroundPreferTransparent)
            {
                if (string.IsNullOrEmpty(BackgroundColour))
                    backgroundBrush = Brushes.White;
                else
                    backgroundBrush = new SolidBrush(ColorTranslator.FromHtml(BackgroundColour));
            }

            using (Image resizedImage = SourceImage.ResizeImage(Width, Height, backgroundBrush))
            {
                return OutputBitmapImage(resizedImage);
            }
        }

        protected MemoryStream OutputBitmapImage(Image SourceImage)
        {
            if (Format != ImageExpressionFormat.Jpeg && Format != ImageExpressionFormat.Png)
                throw new NotSupportedException($"The format {Format} is not supported by this method");

            MemoryStream imageStream = new MemoryStream();
            if (Format == ImageExpressionFormat.Png)
            { // Lossless Format - PNG
                SourceImage.SavePng(imageStream);
            }
            else if (Format == ImageExpressionFormat.Jpeg)
            { // Lossy Format - JPG
                var quality = Math.Min(100, Math.Max(1, (int)Quality));
                SourceImage.SaveJpg(quality, imageStream);
            }
            else
                throw new NotSupportedException($"Unexpected format {Format}");

            imageStream.Position = 0;
            return imageStream;
        }
    }
}
