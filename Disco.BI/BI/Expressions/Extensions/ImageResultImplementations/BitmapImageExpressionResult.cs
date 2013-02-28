using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace Disco.BI.Expressions.Extensions.ImageResultImplementations
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
            return this.RenderImage(this.Image, Width, Height);
        }
    }
}
