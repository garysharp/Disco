using Disco.Models.BI.Expressions;
using System;
using System.Drawing;
using System.IO;

namespace Disco.Services.Expressions.Extensions.ImageResultImplementations
{
    public abstract class BaseImageExpressionResult : IImageExpressionResult
    {
        public byte Quality { get; set; }
        public bool LosslessFormat { get; set; }
        public bool ShowField { get; set; }
        public string BackgroundColour { get; set; }
        public bool BackgroundPreferTransparent { get; set; }

        public BaseImageExpressionResult()
        {
            LosslessFormat = false;
            Quality = 90;
            ShowField = false;
            BackgroundPreferTransparent = true;
        }

        public abstract Stream GetImage(int Width, int Height);

        protected Stream RenderImage(Image SourceImage, int Width, int Height)
        {
            if (SourceImage == null)
                throw new ArgumentNullException("SourceImage");
            if (Width <= 0)
                throw new ArgumentOutOfRangeException("Width", "Width must be > 0");
            if (Height <= 0)
                throw new ArgumentOutOfRangeException("Height", "Height must be > 0");

            Brush backgroundBrush = null;
            if (!LosslessFormat || !BackgroundPreferTransparent)
            {
                if (string.IsNullOrEmpty(BackgroundColour))
                    backgroundBrush = Brushes.White;
                else
                    backgroundBrush = new SolidBrush(ColorTranslator.FromHtml(BackgroundColour));
            }

            using (Image resizedImage = SourceImage.ResizeImage(Width, Height, backgroundBrush))
            {
                return OutputImage(resizedImage);
            }
        }

        protected Stream OutputImage(Image SourceImage)
        {
            MemoryStream imageStream = new MemoryStream();
            if (LosslessFormat)
            { // Lossless Format - PNG
                SourceImage.SavePng(imageStream);
            }
            else
            { // Lossy Format - JPG
                byte quality = Math.Min((byte)100, Math.Max((byte)1, Quality));
                SourceImage.SaveJpg(quality, imageStream);
            }
            imageStream.Position = 0;
            return imageStream;
        }
    }
}
