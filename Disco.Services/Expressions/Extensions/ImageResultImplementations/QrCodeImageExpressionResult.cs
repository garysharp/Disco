using Disco.Models.Services.Expressions.Extensions;
using Disco.Services.Documents;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using ZXing.QrCode.Internal;

namespace Disco.Services.Expressions.Extensions.ImageResultImplementations
{
    public class QrCodeImageExpressionResult : BaseImageExpressionResult
    {
        public static Func<byte[], int, int, byte[]> CCITTG4EncoderCompressDelegate;
        public string Data { get; }
        public char ErrorCorrectionLevel { get; }
        private readonly ErrorCorrectionLevel ecLevel;

        public QrCodeImageExpressionResult(string data, char errorCorrectionLevel)
        {
            Data = data;
            ErrorCorrectionLevel = errorCorrectionLevel;

            switch (errorCorrectionLevel)
            {
                case 'l':
                case 'L':
                    ecLevel = ZXing.QrCode.Internal.ErrorCorrectionLevel.L;
                    break;
                case 'm':
                case 'M':
                    ecLevel = ZXing.QrCode.Internal.ErrorCorrectionLevel.M;
                    break;
                case 'q':
                case 'Q':
                    ecLevel = ZXing.QrCode.Internal.ErrorCorrectionLevel.Q;
                    break;
                case 'h':
                case 'H':
                    ecLevel = ZXing.QrCode.Internal.ErrorCorrectionLevel.H;
                    break;
                default:
                    throw new ArgumentException("Error correction level must be L, M, Q or H");
            }

            Format = ImageExpressionFormat.CcittG4;
            Quality = 90;
            ShowField = false;
            BackgroundPreferTransparent = true;
        }

        public override MemoryStream GetImage(int width, int height)
        {
            var rawImage = QRCodeBinaryEncoder.Encode(Data, ecLevel, width, height);
            return RenderStream(rawImage, width, height);
        }

        public override MemoryStream GetImage(out int width, out int height)
        {
            var rawImage = QRCodeBinaryEncoder.Encode(Data, ecLevel, out width, out height);
            return RenderStream(rawImage, width, height);
        }

        private MemoryStream RenderStream(byte[] data, int width, int height)
        {
            if (Format == ImageExpressionFormat.CcittG4)
            {
                if (CCITTG4EncoderCompressDelegate == null)
                    throw new InvalidOperationException($"The {CCITTG4EncoderCompressDelegate} delegate has not been initialized");

                var result = CCITTG4EncoderCompressDelegate(data, width, height);
                return new MemoryStream(result, 0, result.Length, false, true);
            }
            else
            {
                using (var bitmap = RenderBitmap(data, width, height))
                {
                    return OutputBitmapImage(bitmap);
                }
            }
        }

        private Image RenderBitmap(byte[] data, int width, int height)
        {
            var bitmap = new Bitmap(width, height, PixelFormat.Format1bppIndexed);
            bitmap.Palette.Entries[0] = Color.Black;
            bitmap.Palette.Entries[0] = Color.White;

            var pixelData = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);
            try
            {
                for (int y = 0; y < height; y++)
                {
                    var offset = y * width;
                    var buffer = new byte[(width + 7) / 8];
                    var bufferOffset = 0;
                    var bitOffset = 0;
                    for (int x = 0; x < width; x++)
                    {
                        var pixel = data[offset + x];
                        if (pixel != 0)
                        {
                            buffer[bufferOffset] = (byte)(buffer[bufferOffset] | (byte)((128) >> bitOffset));
                        }
                        if (bitOffset == 8)
                        {
                            bufferOffset++;
                            bitOffset = 0;
                        }
                    }
                    Marshal.Copy(buffer, 0, pixelData.Scan0, buffer.Length);
                }
            }
            finally
            {
                bitmap.UnlockBits(pixelData);
            }

            return bitmap;
        }
    }
}
