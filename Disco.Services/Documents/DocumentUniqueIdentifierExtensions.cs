using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services
{
    public static class DocumentUniqueIdentifierExtensions
    {

        public static DocumentUniqueIdentifier CreateUniqueIdentifier(this DocumentTemplate Template, DiscoDataContext Database, IAttachmentTarget Target, User Creator, DateTime Timestamp, int PageIndex)
        {
            return DocumentUniqueIdentifier.Create(Database, Template, Target, Creator, Timestamp, PageIndex);
        }

        public static byte[] BinaryEncode(this DocumentUniqueIdentifier Identifier, string Data)
        {
            return BinaryEncode(Data);
        }

        public static byte[] BinaryEncode(string Data)
        {
            if (Data == null)
                throw new ArgumentNullException(nameof(Data));

            byte[] result;

            if (Data.Length == 0)
            {
                // Zero-length Alpha Encode (1 byte)
                return new byte[] { 0x40 };
            }

            // Try Numeric Encode
            if (TryBinaryNumericEncode(Data, out result))
            {
                return result;
            }

            // Try CASES21 ST/DF Key Encode
            if (TryBinaryC21Encode(Data, out result))
            {
                return result;
            }

            // Try Alpha Encode
            if (TryBinaryAlphaEncode(Data, out result))
            {
                return result;
            }

            // Use UTF8 Encoding
            return BinaryUTF8Encode(Data);
        }

        public static string BinaryDecode(this DocumentUniqueIdentifier Identifier, byte[] Data, int Offset, out int NewOffset)
        {
            return BinaryDecode(Data, Offset, out NewOffset);
        }

        public static string BinaryDecode(byte[] Data, int Offset, out int NewOffset)
        {
            int encoding = Data[Offset] >> 6;

            switch (encoding)
            {
                case 0: // 00 - numeric encoding
                    return BinaryNumericDecode(Data, Offset, out NewOffset);
                case 1: // 01 - alpha encoding
                    return BinaryAlphaDecode(Data, Offset, out NewOffset);
                case 2: // 10 - C21 encoding
                    return BinaryC21Decode(Data, Offset, out NewOffset);
                case 3: // 11 - UTF8 encoding
                    return BinaryUTF8Decode(Data, Offset, out NewOffset);
                default:
                    throw new InvalidOperationException();
            }
        }

        public static bool TryBinaryNumericEncode(string Data, out byte[] Result)
        {
            // byte[0] = XXZZ YYYY
            // byte[1] = YYYY YYYY
            // byte[2] = YYYY YYYY
            // byte[3] = YYYY YYYY
            //              X = 00 = numeric encoded
            //              Z = number of leading zeros
            //              Y = number component < 0x0FFFFFFF (268,435,455)

            uint number;
            if (uint.TryParse(Data, out number) && number <= 0x0FFFFFFF)
            {
                Result = new byte[4];
                int leadingZeros = 0;

                for (leadingZeros = 0; leadingZeros <= 4; leadingZeros++)
                {
                    if (Data[leadingZeros] != '0')
                    {
                        break;
                    }
                }

                if (leadingZeros <= 3)
                {
                    Result[0] = (byte)((byte)(leadingZeros << 4) | (number >> 24));
                    Result[1] = (byte)(number >> 16);
                    Result[2] = (byte)(number >> 8);
                    Result[3] = (byte)(number);
                    return true;
                }
            }

            Result = null;
            return false;
        }

        public static string BinaryNumericDecode(byte[] Data, int Offset, out int NewOffset)
        {
            int leadingZeros = (Data[Offset] & 0x30) >> 4;
            int number = ((Data[Offset] & 0x0F) << 24) |
                (Data[Offset + 1] << 16) |
                (Data[Offset + 2] << 8) |
                (Data[Offset + 3]);

            NewOffset = Offset + 4;

            if (leadingZeros == 0)
            {
                return number.ToString();
            }
            else
            {
                var builder = new StringBuilder(12); // 12 = max number length
                builder.Append('0', leadingZeros);
                builder.Append(number);
                return builder.ToString();
            }
        }

        private const string AlphaEncodeMap = @"ABCDEFGHIJKLMNOPQRSTUVWXYZ-.01\";

        public static bool TryBinaryAlphaEncode(string Data, out byte[] Result)
        {
            // byte[0] = XXYY YYYY
            //              X = 01 = alpha encoded
            //              Y = data length <= 63
            // byte[1.] = AAAA ABBB
            // byte[2.] = BBCD DDDD
            //              A = first character index
            //              B = second character index
            //              C = not used
            //              D = third character index

            if (Data.Length == 0)
            {
                // Zero-length Alpha Encode (1 byte)
                Result = new byte[] { 0x40 };
                return true;
            }

            if (Data.Length > 0 && Data.Length <= 63)
            {
                Data = Data.ToUpperInvariant();
                var position = 1;
                var requiredBytes = ((Data.Length / 3) * 2);
                switch (Data.Length % 3)
                {
                    case 2:
                        requiredBytes += 3;
                        break;
                    case 1:
                        requiredBytes += 2;
                        break;
                    default:
                        requiredBytes++;
                        break;
                }

                Result = new byte[requiredBytes];
                Result[0] = (byte)(0x40 | Data.Length);
                for (int i = 0; i < Data.Length; i++)
                {
                    var charIndex = AlphaEncodeMap.IndexOf(Data[i]);
                    if (charIndex == -1)
                    {
                        Result = null;
                        return false;
                    }
                    switch (i % 3)
                    {
                        case 0:
                            Result[position] = (byte)(charIndex << 3);
                            break;
                        case 1:
                            Result[position] = (byte)(Result[position] | (charIndex >> 2));
                            Result[++position] = (byte)(charIndex << 6);
                            break;
                        case 2:
                            Result[position] = (byte)(Result[position] | charIndex);
                            position++;
                            break;
                    }
                }

                return true;
            }
            Result = null;
            return false;
        }

        public static string BinaryAlphaDecode(byte[] Data, int Offset, out int NewOffset)
        {
            var length = Data[Offset++] & 0x3F;
            return BinaryAlphaDecode(Data, Offset, length, out NewOffset);
        }

        private static string BinaryAlphaDecode(byte[] Data, int Offset, int Length, out int NewOffset)
        {
            var builder = new StringBuilder(Length);

            for (int i = 0; i < Length; i++)
            {
                switch (i % 3)
                {
                    case 0:
                        builder.Append(AlphaEncodeMap[Data[Offset++] >> 3]);
                        break;
                    case 1:
                        builder.Append(AlphaEncodeMap[((Data[Offset - 1] & 0x7) << 2) | ((Data[Offset++] >> 6) & 0x3)]);
                        break;
                    case 2:
                        builder.Append(AlphaEncodeMap[Data[Offset - 1] & 0x1F]);
                        break;
                }
            }

            NewOffset = Offset;
            return builder.ToString();
        }

        public static bool TryBinaryC21Encode(string Data, out byte[] Result)
        {
            // byte[0] = XXYY YYYY
            // byte[1] = YYYY YYYY
            //              X = 10 = C21 encoded
            //              Y = number component
            // byte[2] = AAAA ABBB
            // byte[3] = BBCC CCCD
            //              A,B,C = character component in
            //                      alpha encoded format

            short number;
            byte[] chars;
            if (Data.Length == 7 &&
                short.TryParse(Data.Substring(3), out number) &&
                number <= 9999 &&
                TryBinaryAlphaEncode(Data.Substring(0, 3), out chars))
            {
                Result = new byte[4];
                Result[0] = (byte)(0x80 | (number >> 8));
                Result[1] = (byte)number;
                Result[2] = chars[1];
                Result[3] = chars[2];
                return true;
            }

            Result = null;
            return false;
        }

        public static string BinaryC21Decode(byte[] Data, int Offset, out int NewOffset)
        {
            var number = ((Data[Offset++] & 0x3F) << 8) |
                (Data[Offset++]);
            var chars = BinaryAlphaDecode(Data, Offset, 3, out NewOffset);

            return $"{chars}{number:0000}";
        }

        public static byte[] BinaryUTF8Encode(string Data)
        {
            // byte[0] = XXYY YYYY
            //              X = 11 = UTF8 encoded
            //              Y = data length <= 63
            // byte[.] = AAAA AAAA
            //              A = UTF8 encoded string

            if (Data.Length == 0)
            {
                // Zero-length Alpha Encode (1 byte)
                return new byte[] { 0xC0 };
            }

            if (Data.Length <= 63)
            {
                var utf8Bytes = Encoding.UTF8.GetBytes(Data);
                if (utf8Bytes.Length <= 63)
                {
                    var result = new byte[1 + utf8Bytes.Length];
                    result[0] = (byte)(0xC0 | utf8Bytes.Length);
                    utf8Bytes.CopyTo(result, 1);
                    return result;
                }
            }

            throw new ArgumentException("Unable to encode the data. The input data is to long.");
        }

        public static string BinaryUTF8Decode(byte[] Data, int Offset, out int NewOffset)
        {
            var length = Data[Offset] & 0x3F;
            NewOffset = Offset + length + 1;
            return Encoding.UTF8.GetString(Data, Offset + 1, length);
        }
    }
}
