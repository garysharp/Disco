Partial Public Class FileWindow
    Inherits ChildWindow

    Public Property File As IO.FileInfo
    Public Property Filename As String
    Public Property Comments As String

    Private _InitialFocus As Boolean = False

    Public Sub New(File As IO.FileInfo)
        InitializeComponent()

        Me.File = File

        Me.Filename = File.Name
        Me.Comments = String.Empty
        Me.DataContext = Me

    End Sub

    Private Sub ButtonOK_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)

        If String.IsNullOrEmpty(Me.textBlockComments.Text) Then
            MessageBox.Show("Please provide a Comment")
            Me.textBlockComments.Focus()
        Else
            Me.Comments = textBlockComments.Text
            Me.DialogResult = True
        End If

    End Sub
    Private Sub ButtonCancel_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Me.DialogResult = False
    End Sub

    Private Sub textBlockComments_KeyDown(sender As Object, e As System.Windows.Input.KeyEventArgs) Handles textBlockComments.KeyDown

        If e.Key = Key.Enter Then
            ButtonOK_Click(Nothing, Nothing)
            Return
        End If
        If e.Key = Key.Escape Then
            ButtonCancel_Click(Nothing, Nothing)
            Return
        End If

    End Sub

    Private Sub FileWindow_GotFocus(sender As Object, e As System.Windows.RoutedEventArgs) Handles Me.GotFocus
        If Not _InitialFocus Then
            Me.textBlockComments.Focus()
            _InitialFocus = True
        End If
    End Sub
End Class
