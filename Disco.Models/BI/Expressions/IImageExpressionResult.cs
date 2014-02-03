using System.IO;

namespace Disco.Models.BI.Expressions
{
    public interface IImageExpressionResult
    {
        Stream GetImage(int Width, int Height);
        byte Quality { get; set; }
        bool LosslessFormat { get; set; }
        bool ShowField { get; set; }
        string BackgroundColour { get; set; }
        bool BackgroundPreferTransparent { get; set; }
    }
}
