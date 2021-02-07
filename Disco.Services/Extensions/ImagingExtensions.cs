using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Disco.Services
{
    public static class ImagingExtensions
    {

        public static Bitmap RotateImage(this Image Source, float Angle, Brush BackgroundColor = null, bool ResizeIfOver45Deg = true)
        {
            var destWidth = Source.Width;
            var destHeight = Source.Height;
            var resizedDest = false;

            if (ResizeIfOver45Deg && ((Angle > 45 && Angle < 135) || (Angle < -45 && Angle > -135)))
            {
                destWidth = Source.Height;
                destHeight = Source.Width;
                resizedDest = true;
            }

            var destination = new Bitmap(destWidth, destHeight);
            destination.SetResolution(Source.HorizontalResolution, Source.VerticalResolution);

            using (Graphics destinationGraphics = Graphics.FromImage(destination))
            {
                destinationGraphics.CompositingQuality = CompositingQuality.HighQuality;
                destinationGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                destinationGraphics.SmoothingMode = SmoothingMode.HighQuality;

                if (BackgroundColor != null)
                    destinationGraphics.FillRectangle(BackgroundColor, destinationGraphics.VisibleClipBounds);

                float offsetWidth = destWidth / 2;
                float offsetHeight = destHeight / 2;

                destinationGraphics.TranslateTransform(offsetWidth, offsetHeight);
                destinationGraphics.RotateTransform(Angle);

                RectangleF destinationLocation;

                if (resizedDest)
                    destinationLocation = new RectangleF(
                        offsetHeight * -1, offsetWidth * -1,
                        destHeight, destWidth);
                else
                    destinationLocation = new RectangleF(
                        offsetWidth * -1, offsetHeight * -1,
                        destWidth, destHeight);

                destinationGraphics.DrawImage(Source, destinationLocation, new RectangleF(0, 0, Source.Width, Source.Height), GraphicsUnit.Pixel);
            }
            return destination;
        }

        public static Bitmap ResizeImage(this Image Source, int TargetWidth, int TargetHeight, Brush BackgroundColor = null)
        {
            var destination = new Bitmap(TargetWidth, TargetHeight);
            destination.SetResolution(72, 72);
            using (Graphics destinationGraphics = Graphics.FromImage(destination))
            {
                destinationGraphics.CompositingQuality = CompositingQuality.HighQuality;
                destinationGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                destinationGraphics.SmoothingMode = SmoothingMode.HighQuality;

                if (BackgroundColor != null)
                    destinationGraphics.FillRectangle(BackgroundColor, destinationGraphics.VisibleClipBounds);

                destinationGraphics.DrawImageResized(Source);
            }

            return destination;
        }

        public static Bitmap ResizeImage(this Image Source, int MaxHeight, Brush BackgroundColor = null)
        {
            // Determine Width
            int Height = (Source.Height > MaxHeight) ?
                MaxHeight :
                Source.Height;

            int Width = (Source.Height > Height) ?
                (int)(((float)Height / Source.Height) * Source.Width) :
                Source.Width;

            Bitmap destination = new Bitmap(Width, Height);
            destination.SetResolution(72, 72);
            using (Graphics destinationGraphics = Graphics.FromImage(destination))
            {
                destinationGraphics.CompositingQuality = CompositingQuality.HighQuality;
                destinationGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                destinationGraphics.SmoothingMode = SmoothingMode.HighQuality;

                if (BackgroundColor != null)
                    destinationGraphics.FillRectangle(BackgroundColor, destinationGraphics.VisibleClipBounds);

                float ratio = Math.Min((float)(destination.Width) / (float)(Source.Width), (float)(destination.Height) / (float)(Source.Height));

                destinationGraphics.DrawImageResized(Source, ratio);
            }

            return destination;
        }

        public static RectangleF CalculateResize(int SourceWidth, int SourceHeight, int TargetWidth, int TargetHeight, out float scaleRatio)
        {
            scaleRatio = Math.Min((float)(TargetWidth) / SourceWidth, (float)(TargetHeight) / SourceHeight);

            float width = SourceWidth * scaleRatio,
                height = SourceHeight * scaleRatio,
                x = 0,
                y = 0;

            if (width < TargetWidth)
                x = (TargetWidth - width) / 2;

            if (height < TargetHeight)
                y = (TargetHeight - height) / 2;

            return new RectangleF(x, y, width, height);
        }

        public static RectangleF CalculateResize(int SourceWidth, int SourceHeight, int TargetWidth, int TargetHeight)
        {
            float scaleRatio;

            return CalculateResize(SourceWidth, SourceHeight, TargetHeight, TargetHeight, out scaleRatio);
        }

        public static RectangleF CalculateResize(this Image Source, int TargetWidth, int TargetHeight, out float scaleRatio)
        {
            return CalculateResize(Source.Width, Source.Height, TargetWidth, TargetHeight, out scaleRatio);
        }

        public static RectangleF CalculateResize(this Image Source, int TargetWidth, int TargetHeight)
        {
            return CalculateResize(Source.Width, Source.Height, TargetHeight, TargetHeight);
        }

        public static void DrawImageResized(this Graphics graphics, Image SourceImage)
        {
            RectangleF clipBounds = graphics.VisibleClipBounds;
            var resizeBounds = SourceImage.CalculateResize((int)clipBounds.Width, (int)clipBounds.Height);

            graphics.DrawImage(SourceImage, resizeBounds, new RectangleF(0, 0, SourceImage.Width, SourceImage.Height), GraphicsUnit.Pixel);
        }

        public static void DrawImageResized(this Graphics graphics, Image SourceImage, float? Scale = null, float LocationX = -1, float LocationY = -1)
        {
            RectangleF clipBounds = graphics.VisibleClipBounds;
            if (Scale == null) // Calculate Scale
                Scale = Math.Min(clipBounds.Width / SourceImage.Width, clipBounds.Height / SourceImage.Height);
            float newWidth = SourceImage.Width * Scale.Value;
            float newHeight = SourceImage.Height * Scale.Value;
            float newLeft = LocationX;
            float newTop = LocationY;

            if (newLeft < 0 || newTop < 0)
            {
                if (newWidth < clipBounds.Width)
                    newLeft = (clipBounds.Width - newWidth) / 2;
                else
                    newLeft = 0;
                if (newHeight < clipBounds.Height)
                    newTop = (clipBounds.Height - newHeight) / 2;
                else
                    newTop = 0;
            }
            newLeft += clipBounds.Left;
            newTop += clipBounds.Top;

            graphics.DrawImage(SourceImage, new RectangleF(newLeft, newTop, newWidth, newHeight), new RectangleF(0, 0, SourceImage.Width, SourceImage.Height), GraphicsUnit.Pixel);
        }

        public static void DrawImageResized(this Graphics graphics, Image SourceImage, float Scale, float LocationX, float LocationY)
        {
            RectangleF clipBounds = graphics.VisibleClipBounds;
            
            float width = SourceImage.Width * Scale,
                height = SourceImage.Height * Scale,
                x = LocationX,
                y = LocationY;

            x += clipBounds.Left;
            y += clipBounds.Top;

            graphics.DrawImage(SourceImage, new RectangleF(x, y, width, height), new RectangleF(0, 0, SourceImage.Width, SourceImage.Height), GraphicsUnit.Pixel);
        }

        public static void EmbedIconOverlay(this Image Source, Image Icon)
        {
            int top = Math.Max(0, Source.Height - Icon.Height);
            int left = Math.Max(0, Source.Width - Icon.Width);

            using (Graphics sourceGraphics = Graphics.FromImage(Source))
            {
                sourceGraphics.DrawImage(Icon, left, top);
            }
        }

        public static void SavePng(this Image Source, string Filename)
        {
            using (FileStream outStream = new FileStream(Filename, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                SavePng(Source, outStream);
                outStream.Flush();
                outStream.Close();
            }
        }

        public static void SavePng(this Image Source, Stream OutStream)
        {
            Source.Save(OutStream, ImageFormat.Png);
        }

        public static Stream SavePng(this Image Source)
        {
            MemoryStream outStream = new MemoryStream();
            Source.SavePng(outStream);
            outStream.Position = 0;
            return outStream;
        }

        public static Stream SaveJpg(this Image Source, int Quality)
        {
            MemoryStream outStream = new MemoryStream();
            Source.SaveJpg(Quality, outStream);
            outStream.Position = 0;
            return outStream;
        }

        public static void SaveJpg(this Image Source, int Quality, string Filename)
        {
            using (FileStream outStream = new FileStream(Filename, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                SaveJpg(Source, Quality, outStream);
                outStream.Flush();
                outStream.Close();
            }
        }

        public static void SaveJpg(this Image Source, int Quality, Stream OutStream)
        {
            ImageCodecInfo jpgCodec = ImageCodecInfo.GetImageEncoders().Where(c => c.MimeType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (jpgCodec != null)
            {
                if (Quality < 0 || Quality > 100)
                    throw new ArgumentOutOfRangeException("Quality", "Quality must be a positive integer <= 100");
                using (EncoderParameters ep = new EncoderParameters(1))
                {
                    ep.Param[0] = new EncoderParameter(Encoder.Quality, Quality);
                    Source.Save(OutStream, jpgCodec, ep);
                }
            }
            else
            {
                // Fallback
                Source.Save(OutStream, ImageFormat.Jpeg);
            }
        }

        public static Color InterpolateColours(this Color Start, Color End, double Progress)
        {
            if (Progress > 1 || Progress < 0)
                throw new ArgumentOutOfRangeException("Progress", "Progress must be >= 0 && <= 1");

            return Color.FromArgb(
                (byte)(Start.A * (1 - Progress) + (End.A * Progress)),
                (byte)(Start.R * (1 - Progress) + (End.R * Progress)),
                (byte)(Start.G * (1 - Progress) + (End.G * Progress)),
                (byte)(Start.B * (1 - Progress) + (End.B * Progress))
                );
        }

        public static RectangleF Multiply(this RectangleF Other, float Multiplier)
        {
            return new RectangleF(Other.X * Multiplier, Other.Y * Multiplier, Other.Width * Multiplier, Other.Height * Multiplier);
        }

        public static RectangleF Divide(this RectangleF Other, float Divisor)
        {
            return new RectangleF(Other.X / Divisor, Other.Y / Divisor, Other.Width / Divisor, Other.Height / Divisor);
        }

    }
}
