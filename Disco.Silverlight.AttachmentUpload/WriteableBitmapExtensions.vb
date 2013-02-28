Imports FluxJpeg.Core
Imports System.Windows.Media.Imaging
Imports System.Runtime.CompilerServices
Imports FluxJpeg.Core.Encoder

Module WriteableBitmapExtensions

    <Extension()>
    Public Function ToJpgStream(ByVal bitmap As WriteableBitmap) As IO.Stream
        Dim width As Integer = bitmap.PixelWidth
        Dim height As Integer = bitmap.PixelHeight
        Dim bands As Integer = 3
        Dim raster(bands - 1)(,) As Byte
        Dim i As Integer
        For i = 0 To bands - 1
            raster(i) = New Byte(width - 1, height - 1) {}
        Next i
        Dim row As Integer
        For row = 0 To height - 1
            Dim column As Integer
            For column = 0 To width - 1
                Dim pixel As Integer = bitmap.Pixels(((width * row) + column))
                raster(0)(column, row) = BitConverter.GetBytes(pixel >> 16)(0)
                raster(1)(column, row) = BitConverter.GetBytes(pixel >> 8)(0)
                raster(2)(column, row) = BitConverter.GetBytes(pixel)(0)
            Next column
        Next row
        Dim model As New ColorModel With {.colorspace = ColorSpace.RGB}
        Dim img As New Image(model, raster)

        Dim stream As New IO.MemoryStream

        Dim encoder = New JpegEncoder(img, 85, stream)
        encoder.Encode()
        stream.Seek(0, IO.SeekOrigin.Begin)

        Return stream
    End Function

End Module
