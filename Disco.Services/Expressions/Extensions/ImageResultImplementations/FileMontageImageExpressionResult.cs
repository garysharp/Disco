using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;

namespace Disco.Services.Expressions.Extensions.ImageResultImplementations
{
    public class FileMontageImageExpressionResult : BaseImageExpressionResult
    {
        public List<string> AbsoluteFilePaths { get; set; }
        public bool MontageHorizontalLayout { get; set; }
        public bool MontageVerticalLayout { get; set; }
        public bool MontageTableLayout { get; set; }
        public int Padding { get; set; }

        public FileMontageImageExpressionResult(List<string> AbsoluteFilePaths)
        {
            if (AbsoluteFilePaths == null)
                throw new ArgumentNullException("AbsoluteFilePaths");
            if (AbsoluteFilePaths.Count == 0)
                throw new ArgumentException("AbsoluteFilePaths is empty", "AbsoluteFilePaths");

            this.AbsoluteFilePaths = AbsoluteFilePaths;
            MontageTableLayout = true;
            Padding = 4;
        }

        public override Stream GetImage(int Width, int Height)
        {
            return DoLayout(Width, Height);
        }

        public override Stream GetImage()
        {
            return DoLayout(width: null, height: null);
        }

        private Stream DoLayout(int? width, int? height)
        {
            List<Image> images = new List<Image>();
            try
            {
                // Load Images
                foreach (string imageFilePath in AbsoluteFilePaths)
                    images.Add(Image.FromFile(imageFilePath));

                if (!width.HasValue || !height.HasValue)
                {
                    int maxWidth, maxHeight;
                    if (MontageHorizontalLayout)
                    {
                        maxWidth = images.Sum(i => i.Width);
                        maxHeight = images.Max(i => i.Height);
                    }else if (MontageVerticalLayout)
                    {
                        maxWidth = images.Max(i => i.Width);
                        maxHeight = images.Sum(i => i.Height);
                    }
                    else // table layout
                    {
                        var itemAverageSize = new SizeF(images.Average(i => (float)i.Size.Width), images.Average(i => (float)i.Size.Height));
                        var stageSize = new Size((int)(itemAverageSize.Width / 2f * (images.Count + 1)), (int)(itemAverageSize.Height / 2f * (images.Count + 1)));

                        var calculatedLayout = CalculateColumnCount(stageSize, itemAverageSize, images.Count);
                        maxWidth = (int)(calculatedLayout.Item1 * itemAverageSize.Width);
                        maxHeight = (int)(calculatedLayout.Item2 * itemAverageSize.Height);
                    }
                    width = width ?? maxWidth;
                    height = height ?? maxHeight;
                }

                // Build Montage
                using (Bitmap montageImage = new Bitmap(width.Value, height.Value))
                {
                    using (Graphics montageGraphics = Graphics.FromImage(montageImage))
                    {
                        montageGraphics.CompositingQuality = CompositingQuality.HighQuality;
                        montageGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        montageGraphics.SmoothingMode = SmoothingMode.HighQuality;

                        // Draw Background
                        if (!LosslessFormat || !BackgroundPreferTransparent)
                        {
                            Brush backgroundBrush = Brushes.White;
                            if (!string.IsNullOrEmpty(BackgroundColour))
                                backgroundBrush = new SolidBrush(ColorTranslator.FromHtml(BackgroundColour));
                            montageGraphics.FillRectangle(backgroundBrush, montageGraphics.VisibleClipBounds);
                        }

                        if (MontageHorizontalLayout)
                            DoHorizontalLayout(images, montageGraphics);
                        else if (MontageVerticalLayout)
                            DoVirticalLayout(images, montageGraphics);
                        else
                            DoTableLayout(images, montageGraphics);
                    }
                    return OutputImage(montageImage);
                }
            }
            catch (Exception) { throw; }
            finally
            {
                // Dispose of any Images
                if (images != null)
                    foreach (Image i in images)
                        i.Dispose();
            }
        }

        private void DoHorizontalLayout(List<Image> Images, Graphics MontageGraphics)
        {

            float imageScale;
            float imagePosition = 0;
            int imagesWidthTotal = Images.Sum(i => i.Width);
            int imagesHeightMax = Images.Max(i => i.Height);
            int imagesPadding = ((Images.Count - 1) * Padding);

            imageScale = (float)(MontageGraphics.VisibleClipBounds.Width - imagesPadding) / (float)imagesWidthTotal;
            if ((MontageGraphics.VisibleClipBounds.Height / (float)imagesHeightMax) < imageScale)
                imageScale = (float)MontageGraphics.VisibleClipBounds.Height / (float)imagesHeightMax;
            foreach (Image image in Images)
            {
                MontageGraphics.DrawImageResized(image, imageScale, imagePosition, 0);
                imagePosition += (imageScale * image.Width) + Padding;
            }
        }
        private void DoVirticalLayout(List<Image> Images, Graphics MontageGraphics)
        {
            float imageScale;
            float imagePosition = 0;
            int imagesWidthMax = Images.Max(i => i.Width);
            int imagesHeightTotal = Images.Sum(i => i.Height);
            int imagesPadding = ((Images.Count - 1) * Padding);

            imageScale = (float)(MontageGraphics.VisibleClipBounds.Height - imagesPadding) / (float)imagesHeightTotal;
            if ((MontageGraphics.VisibleClipBounds.Width / (float)imagesWidthMax) < imageScale)
                imageScale = (float)MontageGraphics.VisibleClipBounds.Width / (float)imagesWidthMax;
            foreach (Image image in Images)
            {
                MontageGraphics.DrawImageResized(image, imageScale, 0, imagePosition);
                imagePosition += (imageScale * image.Height) + Padding;
            }
        }
        private void DoTableLayout(List<Image> Images, Graphics MontageGraphics)
        {
            var stageSize = MontageGraphics.VisibleClipBounds.Size.ToSize();
            var itemAverageSize = new SizeF(Images.Average(i => (float)i.Size.Width), Images.Average(i => (float)i.Size.Height));

            var calculatedLayout = CalculateColumnCount(stageSize, itemAverageSize, Images.Count);

            SizeF cellSize = new SizeF((MontageGraphics.VisibleClipBounds.Width - ((calculatedLayout.Item1 - 1) * Padding)) / calculatedLayout.Item1,
                                        (MontageGraphics.VisibleClipBounds.Height - ((calculatedLayout.Item2 - 1) * Padding)) / calculatedLayout.Item2);

            int imageIndex = 0;
            for (int rowIndex = 0; rowIndex < calculatedLayout.Item2; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < calculatedLayout.Item1; columnIndex++)
                {
                    if (imageIndex < Images.Count)
                    {
                        var image = Images[imageIndex];
                        var cellPoint = new PointF((cellSize.Width * columnIndex) + (Padding * columnIndex), (cellSize.Height * rowIndex) + (Padding * rowIndex));
                        MontageGraphics.SetClip(new RectangleF(cellPoint, cellSize));
                        MontageGraphics.DrawImageResized(image);
                        imageIndex++;
                    }
                    else
                        break;
                }
            }
        }

        private Tuple<int, int, double> CalculateColumnCount(Size StageSize, SizeF ItemAverageSize, int ItemCount)
        {
            double? bestUsedSpace = null;
            int bestColumnCount = 1;
            int bestRowCount = 1;
            double bestItemRatio = 1;

            for (int columnCount = 1; columnCount <= ItemCount; columnCount++)
            {
                int rowCount = (int)Math.Ceiling((double)ItemCount / (double)columnCount);

                int requiredWidthPadding = (columnCount - 1) * Padding;
                int requiredHeightPadding = (rowCount - 1) * Padding;
                Size usableStageSize = new Size(StageSize.Width - requiredWidthPadding, StageSize.Height - requiredHeightPadding);
                double stageWidthRatio = (float)usableStageSize.Width / (float)usableStageSize.Height;
                double stageHeightRatio = (float)usableStageSize.Height / (float)usableStageSize.Width;

                int requiredWidth = (int)Math.Ceiling(ItemAverageSize.Width * columnCount);
                int requiredHeight = (int)Math.Ceiling(ItemAverageSize.Height * rowCount);

                int usedSpace = requiredWidth * requiredHeight;
                int stageArea = Math.Max((requiredWidth * (int)Math.Ceiling(requiredWidth * stageHeightRatio)),
                                            (requiredHeight * (int)Math.Ceiling(requiredHeight * stageWidthRatio)));
                double usedStageSpace = (double)usedSpace / stageArea;
                if (bestUsedSpace == null || bestUsedSpace < usedStageSpace)
                {
                    bestUsedSpace = usedStageSpace;
                    bestColumnCount = columnCount;
                    bestRowCount = rowCount;
                    bestItemRatio = Math.Min((double)usableStageSize.Width / (double)requiredWidth, (double)usableStageSize.Height / (double)requiredHeight);
                }
            }

            return new Tuple<int, int, double>(bestColumnCount, bestRowCount, bestItemRatio);
        }


    }
}
