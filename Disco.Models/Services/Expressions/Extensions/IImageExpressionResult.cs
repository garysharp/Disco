using System.IO;

namespace Disco.Models.Services.Expressions.Extensions
{
    public interface IImageExpressionResult
    {
        MemoryStream GetImage(int width, int height);
        MemoryStream GetImage(out int width, out int height);
        byte Quality { get; set; }
        ImageExpressionFormat Format { get; set; }
        bool ShowField { get; set; }
        string BackgroundColour { get; set; }
        bool BackgroundPreferTransparent { get; set; }
    }
}
