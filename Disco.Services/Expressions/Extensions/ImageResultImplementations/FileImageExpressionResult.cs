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

        public override Stream GetImage(int Width, int Height)
        {
            using (Image SourceImage = Bitmap.FromFile(AbsoluteFilePath))
            {
                return RenderImage(SourceImage, Width, Height);
            }
        }

        public override Stream GetImage()
        {
            var stream = new MemoryStream();
            using (var fileStream = File.OpenRead(AbsoluteFilePath))
                fileStream.CopyTo(stream);
            stream.Position = 0;
            return stream;
        }
    }
}
