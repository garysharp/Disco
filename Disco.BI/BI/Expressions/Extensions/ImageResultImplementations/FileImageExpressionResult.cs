using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace Disco.BI.Expressions.Extensions.ImageResultImplementations
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
            using (Image SourceImage = Bitmap.FromFile(this.AbsoluteFilePath))
            {
                return this.RenderImage(SourceImage, Width, Height);
            }
        }
    }
}
