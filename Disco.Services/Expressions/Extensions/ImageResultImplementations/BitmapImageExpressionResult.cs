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

        public override Stream GetImage(int Width, int Height)
        {
            return RenderImage(Image, Width, Height);
        }
    }
}
