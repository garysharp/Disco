using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Drawing.Imaging;

namespace Disco.BI.Extensions
{
    public static class UtilityExtensions
    {

        public static string StreamToString(this System.IO.Stream stream)
        {
            if (stream.Position != 0 && stream.CanSeek)
            {
                stream.Position = 0;
            }
            using (System.IO.StreamReader sr = new System.IO.StreamReader(stream))
            {
                return sr.ReadToEnd();
            }
        }

        #region Image Extensions

        public static Bitmap RotateImage(this Image Source, float Angle, Brush BackgroundColor = null, bool ResizeIfOver45Deg = true)
        {
            int destWidth = Source.Width;
            int destHeight = Source.Height;
            bool resizedDest = false;

            if (ResizeIfOver45Deg && ((Angle > 45 && Angle < 135) || (Angle < -45 && Angle > -135)))
            {
                destWidth = Source.Height;
                destHeight = Source.Width;
                resizedDest = true;
            }

            Bitmap destination = new Bitmap(destWidth, destHeight);
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

        public static Bitmap ResizeImage(this Image Source, int Width, int Height, Brush BackgroundColor = null)
        {
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
        public static ImageMontage BuildImageMontage(this IEnumerable<Image> Images, int MaxHeight = -1, int MaxWidth = -1, bool EnforceDimensions = false)
        {
            if (EnforceDimensions && (MaxHeight < 0 || MaxWidth < 0))
                throw new ArgumentOutOfRangeException("EnforceDimensions", "Dimensions can only be enforced when a MaxHeight and MaxWidth is supplied");

            Dictionary<Image, int> imageLocations = new Dictionary<Image, int>();
            double imageScale = 1.0;
            int totalHeight = Images.Max(i => i.Height);
            int totalWidth = Images.Sum(i => i.Width);
            if (MaxHeight > 0 && totalHeight > MaxHeight)
            {
                imageScale = (double)MaxHeight / (double)totalHeight;
            }
            if (MaxWidth > 0 && totalWidth > MaxWidth)
            {
                imageScale = System.Math.Min(imageScale, (double)MaxWidth / (double)totalWidth);
            }
            int scaledHeight = EnforceDimensions ? MaxHeight : (int)System.Math.Round((double)totalHeight * imageScale);
            int scaledWidth = EnforceDimensions ? MaxWidth : (int)System.Math.Round((double)totalWidth * imageScale);
            System.Drawing.Bitmap imageResult = new System.Drawing.Bitmap(scaledWidth, scaledHeight);
            imageResult.SetResolution(72f, 72f);

            using (Graphics g = Graphics.FromImage(imageResult))
            {
                g.FillRectangle(Brushes.White, new Rectangle(Point.Empty, imageResult.Size));

                int imageResultNextOffset = 0;
                foreach (Image i in Images)
                {
                    Rectangle imagePosition = new Rectangle(imageResultNextOffset, 0, (int)System.Math.Round((double)i.Width * imageScale), (int)System.Math.Round((double)i.Height * imageScale));
                    System.Drawing.Rectangle imageSize = new System.Drawing.Rectangle(0, 0, i.Width, i.Height);
                    g.DrawImage(i, imagePosition, imageSize, System.Drawing.GraphicsUnit.Pixel);
                    imageLocations[i] = imageResultNextOffset;
                    imageResultNextOffset += imagePosition.Width;
                }
            }

            return new ImageMontage() { Montage = imageResult, MontageScale = imageScale, MontageSourceImageOffsets = imageLocations };
        }
        public class ImageMontage : IDisposable
        {

            public Image Montage { get; set; }
            public double MontageScale { get; set; }
            public Dictionary<Image, int> MontageSourceImageOffsets { get; set; }

            public void Dispose()
            {
                if (Montage != null)
                {
                    Montage.Dispose();
                    Montage = null;
                }
                if (MontageSourceImageOffsets != null)
                {
                    MontageSourceImageOffsets.Clear();
                    MontageSourceImageOffsets = null;
                }
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
        #endregion
    }
}
