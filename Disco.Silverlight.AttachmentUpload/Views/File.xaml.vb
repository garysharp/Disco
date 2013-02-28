Partial Public Class File
    Inherits Page

    Public Sub New()
        InitializeComponent()
    End Sub

    Private _FileWindow As FileWindow
    Private _ProcessFile As IO.FileInfo
    Private _ProcessFiles As Queue(Of IO.FileInfo)

    'Executes when the user navigates to this page.
    Protected Overrides Sub OnNavigatedTo(ByVal e As System.Windows.Navigation.NavigationEventArgs)

        _ProcessFiles = New Queue(Of IO.FileInfo)

    End Sub

    Private Sub ButtonBrowseForFile_Click(sender As Object, e As System.Windows.RoutedEventArgs) Handles ButtonBrowseForFile.Click

        Dim ofd As New OpenFileDialog() With {.Multiselect = True}
        If ofd.ShowDialog() Then
            For Each f In ofd.Files
                _ProcessFiles.Enqueue(f)
            Next
            StartProcessing()
        End If

    End Sub


    Private Sub StartProcessing()

        If _ProcessFiles.Count > 0 Then
            _ProcessFile = _ProcessFiles.Dequeue

            _FileWindow = New FileWindow(_ProcessFile)
            AddHandler _FileWindow.Closed, AddressOf FileWindow_Closed
            _FileWindow.Show()
        End If

    End Sub

    Private Sub FileWindow_Closed(sender As Object, e As EventArgs)
        If _FileWindow.DialogResult Then
            Me.IsEnabled = False
            Dim fs = _FileWindow.File.OpenRead
            App.MainPage.UploadAttachment(fs, _FileWindow.Filename, "unknown/unknown", _FileWindow.Comments)
            Me.IsEnabled = True
        End If
        StartProcessing()
    End Sub

    Private Sub DropTarget_Drop(sender As Object, e As System.Windows.DragEventArgs) Handles DropTarget.Drop

        For Each f As IO.FileInfo In e.Data.GetData(DataFormats.FileDrop)
            _ProcessFiles.Enqueue(f)
        Next
        StartProcessing()

    End Sub
End Class
