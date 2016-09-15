using System;
using System.Collections.Generic;
using ZXing.Common;
using ZXing.Common.ReedSolomon;
using ZXing.QrCode.Internal;

namespace Disco.Services.Documents
{
    public static class QRCodeBinaryEncoder
    {

        public static byte[] Encode(byte[] Content, int Width, int Height)
        {
            var ecLevel = ErrorCorrectionLevel.L;
            var bits = new BitArray();

            var mode = Mode.BYTE;
            var bitsNeeded = 4 +
                mode.getCharacterCountBits(ZXing.QrCode.Internal.Version.getVersionForNumber(1)) +
                (Content.Length * 8);

            var version = ChooseVersion(bitsNeeded, ecLevel);
            var ecBlocks = version.getECBlocksForLevel(ecLevel);
            var totalByteCapacity = version.TotalCodewords - ecBlocks.TotalECCodewords;
            var totalBitCapacity = totalByteCapacity << 3;

            // Write the mode marker (BYTE)
            bits.appendBits(mode.Bits, 4);

            // Write the number of bytes
            bits.appendBits(Content.Length, mode.getCharacterCountBits(version));

            // Write the bytes
            for (int i = 0; i < Content.Length; i++)
            {
                bits.appendBits(Content[i], 8);
            }

            // Terminate the bit stream
            //  - Write Termination Mode if space
            for (int i = 0; i < 4 && bits.Size < totalBitCapacity; ++i)
            {
                bits.appendBit(false);
            }

            // Add 8-bit alignment padding
            var bitsInLastByte = bits.Size & 0x07;
            if (bitsInLastByte > 0)
            {
                for (int i = bitsInLastByte; i < 8; i++)
                {
                    bits.appendBit(false);
                }
            }

            // Fill remain space with padding patterns
            var paddingBytes = totalByteCapacity - bits.SizeInBytes;
            for (int i = 0; i < paddingBytes; ++i)
            {
                bits.appendBits((i & 0x01) == 0 ? 0xEC : 0x11, 8);
            }

            // Interleave data bits with error correction code.
            var finalBits = interleaveWithECBytes(bits, version.TotalCodewords, totalByteCapacity, ecBlocks.NumBlocks);

            //  Choose the mask pattern and set to "qrCode".
            int dimension = version.DimensionForVersion;
            ByteMatrix matrix = new ByteMatrix(dimension, dimension);
            int maskPattern = chooseMaskPattern(finalBits, ecLevel, version, matrix);

            // Build the matrix and set it to "qrCode".
            MatrixUtil.buildMatrix(finalBits, ecLevel, version, maskPattern, matrix);

            // Render matrix to bytes
            return scaleMatrix(matrix.Array, Width, Height);
        }

        private static ZXing.QrCode.Internal.Version ChooseVersion(int RequiredBits, ErrorCorrectionLevel ECLevel)
        {
            // In the following comments, we use numbers of Version 7-H.
            for (int versionNum = 1; versionNum <= 40; versionNum++)
            {
                var version = ZXing.QrCode.Internal.Version.getVersionForNumber(versionNum);
                // numBytes = 196
                int numBytes = version.TotalCodewords;
                // getNumECBytes = 130
                var ecBlocks = version.getECBlocksForLevel(ECLevel);
                int numEcBytes = ecBlocks.TotalECCodewords;
                // getNumDataBytes = 196 - 130 = 66
                int numDataBytes = numBytes - numEcBytes;
                int totalInputBytes = (RequiredBits + 7) / 8;
                if (numDataBytes >= totalInputBytes)
                {
                    return version;
                }
            }
            throw new ArgumentException("Data too big", nameof(RequiredBits));
        }

        private static BitArray interleaveWithECBytes(BitArray bits, int numTotalBytes, int numDataBytes, int numRSBlocks)
        {
            // Step 1.  Divide data bytes into blocks and generate error correction bytes for them. We'll
            // store the divided data bytes blocks and error correction bytes blocks into "blocks".
            int dataBytesOffset = 0;
            int maxNumDataBytes = 0;
            int maxNumEcBytes = 0;

            // Since, we know the number of reedsolmon blocks, we can initialize the vector with the number.
            var blocks = new List<Tuple<byte[], byte[]>>(numRSBlocks);

            for (int i = 0; i < numRSBlocks; ++i)
            {

                int numDataBytesInBlock;
                int numEcBytesInBlock;
                getNumDataBytesAndNumECBytesForBlockID(
                    numTotalBytes, numDataBytes, numRSBlocks, i,
                    out numDataBytesInBlock, out numEcBytesInBlock);

                byte[] dataBytes = new byte[numDataBytesInBlock];
                bits.toBytes(8 * dataBytesOffset, dataBytes, 0, numDataBytesInBlock);
                byte[] ecBytes = generateECBytes(dataBytes, numEcBytesInBlock);
                blocks.Add(new Tuple<byte[], byte[]>(dataBytes, ecBytes));

                maxNumDataBytes = Math.Max(maxNumDataBytes, numDataBytesInBlock);
                maxNumEcBytes = Math.Max(maxNumEcBytes, ecBytes.Length);
                dataBytesOffset += numEcBytesInBlock;
            }

            BitArray result = new BitArray();

            // First, place data blocks.
            for (int i = 0; i < maxNumDataBytes; ++i)
            {
                foreach (Tuple<byte[], byte[]> block in blocks)
                {
                    byte[] dataBytes = block.Item1;
                    if (i < dataBytes.Length)
                    {
                        result.appendBits(dataBytes[i], 8);
                    }
                }
            }
            // Then, place error correction blocks.
            for (int i = 0; i < maxNumEcBytes; ++i)
            {
                foreach (Tuple<byte[], byte[]> block in blocks)
                {
                    byte[] ecBytes = block.Item2;
                    if (i < ecBytes.Length)
                    {
                        result.appendBits(ecBytes[i], 8);
                    }
                }
            }

            return result;
        }

        private static void getNumDataBytesAndNumECBytesForBlockID(int numTotalBytes, int numDataBytes, int numRSBlocks, int blockID, out int numDataBytesInBlock, out int numECBytesInBlock)
        {
            // numRsBlocksInGroup2 = 196 % 5 = 1
            int numRsBlocksInGroup2 = numTotalBytes % numRSBlocks;
            // numRsBlocksInGroup1 = 5 - 1 = 4
            int numRsBlocksInGroup1 = numRSBlocks - numRsBlocksInGroup2;
            // numTotalBytesInGroup1 = 196 / 5 = 39
            int numTotalBytesInGroup1 = numTotalBytes / numRSBlocks;
            // numTotalBytesInGroup2 = 39 + 1 = 40
            int numTotalBytesInGroup2 = numTotalBytesInGroup1 + 1;
            // numDataBytesInGroup1 = 66 / 5 = 13
            int numDataBytesInGroup1 = numDataBytes / numRSBlocks;
            // numDataBytesInGroup2 = 13 + 1 = 14
            int numDataBytesInGroup2 = numDataBytesInGroup1 + 1;
            // numEcBytesInGroup1 = 39 - 13 = 26
            int numEcBytesInGroup1 = numTotalBytesInGroup1 - numDataBytesInGroup1;
            // numEcBytesInGroup2 = 40 - 14 = 26
            int numEcBytesInGroup2 = numTotalBytesInGroup2 - numDataBytesInGroup2;

            if (blockID < numRsBlocksInGroup1)
            {

                numDataBytesInBlock = numDataBytesInGroup1;
                numECBytesInBlock = numEcBytesInGroup1;
            }
            else
            {
                numDataBytesInBlock = numDataBytesInGroup2;
                numECBytesInBlock = numEcBytesInGroup2;
            }
        }

        private static byte[] generateECBytes(byte[] dataBytes, int numEcBytesInBlock)
        {
            int numDataBytes = dataBytes.Length;
            int[] toEncode = new int[numDataBytes + numEcBytesInBlock];
            for (int i = 0; i < numDataBytes; i++)
            {
                toEncode[i] = dataBytes[i] & 0xFF;

            }
            new ReedSolomonEncoder(GenericGF.QR_CODE_FIELD_256).encode(toEncode, numEcBytesInBlock);

            byte[] ecBytes = new byte[numEcBytesInBlock];
            for (int i = 0; i < numEcBytesInBlock; i++)
            {
                ecBytes[i] = (byte)toEncode[numDataBytes + i];

            }
            return ecBytes;
        }

        private static int chooseMaskPattern(BitArray bits, ErrorCorrectionLevel ecLevel, ZXing.QrCode.Internal.Version version, ByteMatrix matrix)
        {
            int minPenalty = Int32.MaxValue;  // Lower penalty is better.
            int bestMaskPattern = -1;
            // We try all mask patterns to choose the best one.
            for (int maskPattern = 0; maskPattern < QRCode.NUM_MASK_PATTERNS; maskPattern++)
            {

                MatrixUtil.buildMatrix(bits, ecLevel, version, maskPattern, matrix);
                int penalty = MaskUtil.applyMaskPenaltyRule1(matrix)
                    + MaskUtil.applyMaskPenaltyRule2(matrix)
                    + MaskUtil.applyMaskPenaltyRule3(matrix)
                    + MaskUtil.applyMaskPenaltyRule4(matrix);
                if (penalty < minPenalty)
                {

                    minPenalty = penalty;
                    bestMaskPattern = maskPattern;
                }
            }
            return bestMaskPattern;
        }

        private static byte[] scaleMatrix(byte[][] matrix, int Width, int Height)
        {
            var matrixWidth = matrix[0].Length;
            var matrixHeight = matrix.Length;
            Width = Math.Max(Width, matrixWidth);
            Height = Math.Max(Height, matrixHeight);
            var byteColumns = (Width + 7) / 8;
            var outputBytes = new byte[byteColumns * Height];
            var scale = Math.Min(Width / (matrixWidth + 1), Height / (matrixHeight + 1));
            var offsetX = (Width - (matrixWidth * scale)) / 2;
            var offsetY = (Height - (matrixHeight * scale)) / 2;
            // initialize output bytes
            for (int i = 0; i < outputBytes.Length; i++)
            {
                outputBytes[i] = 0xFF;
            }
            // render row
            for (int rowIndex = 0; rowIndex < matrixHeight; rowIndex++)
            {
                var rowMatrix = matrix[rowIndex];
                var rowLocation = ((rowIndex * scale) + offsetY) * byteColumns;
                var bitOffset = offsetX;
                for (int c = 0; c < matrixWidth; c++)
                {
                    if (rowMatrix[c] == 1)
                    {
                        for (int cS = 0; cS < scale; cS++)
                        {
                            int index = rowLocation + (bitOffset / 8);
                            outputBytes[index] = (byte)(outputBytes[index] ^ ((byte)(0x80 >> (bitOffset % 8))));
                            bitOffset++;
                        }
                    }
                    else
                    {
                        bitOffset += scale;
                    }
                }
                // Write row for scale
                for (int i = 1; i < scale; i++)
                {
                    var offsetLocation = rowLocation + (i * byteColumns);
                    Array.Copy(outputBytes, rowLocation, outputBytes, offsetLocation, byteColumns);
                }
            }

            return outputBytes;
        }

    }
}
