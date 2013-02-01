Imports System.Windows.Media.Effects

Partial Public Class WebCam
    Inherits Page

    Public Sub New()
        InitializeComponent()
    End Sub

    Private _CaptureSource As CaptureSource
    Private _VideoBrush As VideoBrush

    Private _CapturedImage As Imaging.WriteableBitmap

    'Executes when the user navigates to this page.
    Protected Overrides Sub OnNavigatedTo(ByVal e As System.Windows.Navigation.NavigationEventArgs)

        If _CaptureSource Is Nothing Then
            '' Init WebCam
            _CaptureSource = New CaptureSource
            _CaptureSource.VideoCaptureDevice = CaptureDeviceConfiguration.GetDefaultVideoCaptureDevice()

            '' Get Best Quality Image
            _CaptureSource.VideoCaptureDevice.DesiredFormat = (From f In _CaptureSource.VideoCaptureDevice.SupportedFormats Where f.PixelHeight <= 720
                                                                Order By (f.FramesPerSecond * f.PixelHeight * f.PixelWidth) Descending).FirstOrDefault()

            AddHandler _CaptureSource.CaptureImageCompleted, AddressOf CaptureSource_CaptureImageCompleted
            _VideoBrush = New VideoBrush()
            _VideoBrush.Stretch = Stretch.Uniform
            _VideoBrush.SetSource(_CaptureSource)
            WebCamHost.Fill = _VideoBrush
        End If

        If (CaptureDeviceConfiguration.AllowedDeviceAccess) Then
            ButtonStartWebCam_Click(Nothing, Nothing)
        Else
            WebCamAccessHost.Visibility = Windows.Visibility.Visible
            WebCamCaptured.Visibility = Windows.Visibility.Collapsed
            WebCamShow.Visibility = Windows.Visibility.Collapsed
        End If

    End Sub

    Protected Overrides Sub OnNavigatedFrom(e As System.Windows.Navigation.NavigationEventArgs)
        If _CaptureSource IsNot Nothing Then
            If _CaptureSource.State = CaptureState.Started Then
                _CaptureSource.Stop()
            End If
        End If

        MyBase.OnNavigatedFrom(e)
    End Sub

    Private Sub ButtonStartWebCam_Click(sender As Object, e As System.Windows.RoutedEventArgs) Handles ButtonStartWebCam.Click

        If (CaptureDeviceConfiguration.AllowedDeviceAccess OrElse CaptureDeviceConfiguration.RequestDeviceAccess()) Then
            _CapturedImage = Nothing
            Me.WebCamShow.Effect = Nothing

            WebCamAccessHost.Visibility = Windows.Visibility.Collapsed
            WebCamShow.Visibility = Windows.Visibility.Visible
            WebCamCaptured.Visibility = Windows.Visibility.Collapsed

            _CaptureSource.Start()
            Me.ButtonCapture.Focus()
        Else
            WebCamAccessHost.Visibility = Windows.Visibility.Visible
            WebCamCaptured.Visibility = Windows.Visibility.Collapsed
            WebCamShow.Visibility = Windows.Visibility.Collapsed
        End If

        Me.IsEnabled = True

    End Sub

    Private Sub ButtonCapture_Click(sender As Object, e As System.Windows.RoutedEventArgs) Handles ButtonCapture.Click
        _CaptureSource.CaptureImageAsync()
    End Sub

    Private Sub CaptureSource_CaptureImageCompleted(sender As Object, e As CaptureImageCompletedEventArgs)
        Me.Dispatcher.BeginInvoke(Function()
                                      ImageCaptured(e.Result)
                                      Return Nothing
                                  End Function)

    End Sub

    Private Sub ImageCaptured(image As Imaging.WriteableBitmap)

        _CapturedImage = image

        _CaptureSource.Stop()
        Dim effect As New BlurEffect() With {.Radius = 15}
        Me.WebCamShow.Effect = effect

        Dim capturedImageBrush = New ImageBrush() With {.ImageSource = _CapturedImage, .Stretch = Stretch.Uniform}
        Me.WebCamCapturedImage.Fill = capturedImageBrush

        Me.TextBoxComments.Text = String.Empty

        Me.WebCamCaptured.Visibility = Windows.Visibility.Visible
        Me.TextBoxComments.Focus()

    End Sub

    Private Sub ButtonCancel_Click(sender As Object, e As System.Windows.RoutedEventArgs) Handles ButtonCancel.Click
        ButtonStartWebCam_Click(Nothing, Nothing)
    End Sub

    Private Sub ButtonPost_Click(sender As Object, e As System.Windows.RoutedEventArgs) Handles ButtonPost.Click

        Dim comments As String = Me.TextBoxComments.Text

        If String.IsNullOrEmpty(comments) Then
            MessageBox.Show("Please provide a Comment")
            Me.TextBoxComments.Focus()
        Else
            Me.IsEnabled = False
            System.Threading.ThreadPool.QueueUserWorkItem(Function()
                                                              App.MainPage.UploadAttachment(_CapturedImage.ToJpgStream, String.Format("CapturedImage-{0}.jpg", Now.ToString("yyyyMMdd-HHmmss")), "image/jpeg", comments)
                                                              Me.Dispatcher.BeginInvoke(Function()
                                                                                            ButtonStartWebCam_Click(Nothing, Nothing)
                                                                                            Return Nothing
                                                                                        End Function)
                                                              Return Nothing
                                                          End Function)
        End If

    End Sub

    Private Sub TextBoxComments_KeyDown(sender As Object, e As System.Windows.Input.KeyEventArgs) Handles TextBoxComments.KeyDown

        If e.Key = Key.Enter Then
            ButtonPost_Click(Nothing, Nothing)
            Return
        End If
        If e.Key = Key.Escape Then
            ButtonCancel_Click(Nothing, Nothing)
            Return
        End If

    End Sub
End Class
