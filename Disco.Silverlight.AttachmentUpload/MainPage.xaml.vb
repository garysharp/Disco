Imports System.Windows.Navigation
Imports System.Windows.Browser

Partial Public Class MainPage
    Inherits UserControl

    Private _Navigator As JavascriptNavigator

    Private _UploadingAttachments As New List(Of FileUploader)

    Public Sub New()
        InitializeComponent()

        _Navigator = New JavascriptNavigator(Me.ContentFrame)
        HtmlPage.RegisterScriptableObject("Navigator", _Navigator)
    End Sub

    Private Sub ContentFrame_NavigationFailed(ByVal sender As Object, ByVal e As NavigationFailedEventArgs) Handles ContentFrame.NavigationFailed
        e.Handled = True
        Dim errorWindow As ChildWindow = New ErrorWindow(e.Uri)
        errorWindow.Show()
    End Sub

    Public Sub UploadAttachment(stream As IO.Stream, fileName As String, mimeType As String, comments As String)

        Dim form As New Dictionary(Of String, String)
        form.Add("comments", comments)

        Dim ua As New FileUploader(App.UploadUrl, stream, fileName, mimeType, form, New FileUploader.UploadComplete(AddressOf UploadComplete))

        _UploadingAttachments.Add(ua)

        'Me.NavigationGrid.Visibility = Windows.Visibility.Visible

    End Sub

    Private Sub UploadComplete(Sender As FileUploader, Success As Boolean, Id As Integer)
        If _UploadingAttachments.Contains(Sender) Then
            _UploadingAttachments.Remove(Sender)
        End If

        'If _UploadingAttachments.Count = 0 Then
        '    Me.Dispatcher.BeginInvoke(Function()
        '                                  Me.NavigationGrid.Visibility = Windows.Visibility.Collapsed
        '                                  Return Nothing
        '                              End Function)
        'End If

        If Id >= 0 Then
            Me.Dispatcher.BeginInvoke(Function()
                                          Dim discoFunctions = System.Windows.Browser.HtmlPage.Document.GetProperty("DiscoFunctions")
                                          If discoFunctions IsNot Nothing Then
                                              discoFunctions.addAttachment(Id)
                                          End If
                                          Return (Nothing)
                                      End Function)
        End If
    End Sub

End Class