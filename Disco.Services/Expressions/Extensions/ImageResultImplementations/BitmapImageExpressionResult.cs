using System;
using System.Drawing;
using System.IO;

namespace Disco.Services.Expressions.Extensions.ImageResultImplementations
{
    public class BitmapImageExpressionResult : BaseImageExpressionResult
    {
        public Image Image { get; set; }

        public BitmapImageExpressionResult(Image Image)
        {
            if (Image == null)
                throw new ArgumentNullException("Image");

            this.Image = Image;
        }

        public override MemoryStream GetImage(int width, int height)
        {
            return RenderBitmapImage(Image, width, height);
        }

        public override MemoryStream GetImage(out int width, out int height)
        {
            var image = Image;
            
            width = image.Width;
            height = image.Height;
            
            return OutputBitmapImage(image);
        }
    }
}
