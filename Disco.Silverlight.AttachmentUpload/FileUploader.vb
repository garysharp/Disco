Imports System.Text
Imports Disco.Silverlight.AttachmentUpload.BI

Public Class FileUploader

    Public Property Url As Uri
    Public Property Stream As IO.Stream
    Public Property StreamFilename As String
    Public Property StreamMimeType As String
    Public Property Form As Dictionary(Of String, String)

    Private _HttpSend As HttpWebRequest
    Private _UploadBoundaryValue As String

    Private _Complete As UploadComplete

    Public Delegate Sub UploadComplete(Sender As FileUploader, Success As Boolean, Id As Integer)

    Public Sub New(Url As Uri, Stream As IO.Stream, StreamFilename As String, StreamMimeType As String, Form As Dictionary(Of String, String), Optional Complete As UploadComplete = Nothing)
        Me.Url = Url
        Me.Stream = Stream
        Me.StreamFilename = StreamFilename
        Me.StreamMimeType = StreamMimeType
        Me.Form = Form
        Me._Complete = Complete

        Start()
    End Sub

    Public Sub Start()
        Stream.Position = 0
        Me._HttpSend = WebRequest.Create(App.UploadUrl)
        Me._UploadBoundaryValue = ("----------------------------" & DateTime.Now.Ticks.ToString("x"))
        Me._HttpSend.ContentType = "multipart/form-data; boundary=" & Me._UploadBoundaryValue
        Me._HttpSend.Method = "POST"
        Me._HttpSend.BeginGetRequestStream(New AsyncCallback(AddressOf Me.BeginSendRequest), Nothing)
    End Sub

    Private Sub BeginSendRequest(ByVal result As IAsyncResult)
        Using requestStream As IO.Stream = _HttpSend.EndGetRequestStream(result)
            Dim format As String = ("{0}--" & _UploadBoundaryValue & "{0}Content-Disposition: form-data; name=""{1}"";{0}{0}{2}")
            Dim pair As KeyValuePair(Of String, String)
            For Each pair In Form
                Dim str4 As String = String.Format(format, Environment.NewLine, pair.Key, pair.Value)
                Dim buffer3 As Byte() = Encoding.UTF8.GetBytes(str4)
                requestStream.Write(buffer3, 0, buffer3.Length)
            Next
            Dim s As String = String.Format(("{0}--" & _UploadBoundaryValue & "{0}Content-Disposition: form-data; name=""{1}""; filename=""{2}""{0}Content-Type: {3}{0}{0}"), Environment.NewLine, "File", Me.StreamFilename, Me.StreamMimeType)
            Dim bytes As Byte() = Encoding.UTF8.GetBytes(s)
            requestStream.Write(bytes, 0, bytes.Length)
            Me.Stream.CopyTo(requestStream)
            Me.Stream.Close()
            Me.Stream.Dispose()
            Dim buffer As Byte() = Encoding.UTF8.GetBytes(String.Format("{0}--{1}{0}", Environment.NewLine, Me._UploadBoundaryValue))
            requestStream.Write(buffer, 0, buffer.Length)
            requestStream.Flush()
            requestStream.Close()
        End Using
        _HttpSend.BeginGetResponse(New AsyncCallback(AddressOf Me.BeginGetResponse), Nothing)
    End Sub

    Private Sub BeginGetResponse(ByVal result As IAsyncResult)
        Dim response As HttpWebResponse = _HttpSend.EndGetResponse(result)

        Dim responseId As Integer = -1

        If response.StatusCode = 200 Then
            Dim responseString = response.GetResponseStream.StreamToString()
            Integer.TryParse(responseString, responseId)
        End If

        If _Complete IsNot Nothing Then
            _Complete.Invoke(Me, (response.StatusCode = 200), responseId)
        End If
    End Sub

End Class
