using System;
using System.Drawing;
using System.IO;

namespace Disco.Services.Expressions.Extensions.ImageResultImplementations
{
    public class FileImageExpressionResult : BaseImageExpressionResult
    {
        public string AbsoluteFilePath { get; set; }

        public FileImageExpressionResult(string AbsoluteFilePath)
        {
            if (string.IsNullOrWhiteSpace(AbsoluteFilePath))
                throw new ArgumentNullException("AbsoluteFilePath");
            if (!File.Exists(AbsoluteFilePath))
                throw new FileNotFoundException("Image not found", AbsoluteFilePath);

            this.AbsoluteFilePath = AbsoluteFilePath;
        }

        public override MemoryStream GetImage(int width, int height)
        {
            using (var sourceImage = Image.FromFile(AbsoluteFilePath))
            {
                return RenderBitmapImage(sourceImage, width, height);
            }
        }

        public override MemoryStream GetImage(out int width, out int height)
        {
            var stream = new MemoryStream();
            using (var fileStream = File.OpenRead(AbsoluteFilePath))
                fileStream.CopyTo(stream);
            stream.Position = 0;

            using (var sourceImage = Image.FromStream(stream))
            {
                width = sourceImage.Width;
                height = sourceImage.Height;
            }
            stream.Position = 0;

            return stream;
        }
    }
}
