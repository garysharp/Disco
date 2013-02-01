Partial Public Class App
    Inherits Application

    Public Shared Property UploadUrl As Uri
    Public Shared Property MainPage As MainPage

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub Application_Startup(ByVal o As Object, ByVal e As StartupEventArgs) Handles Me.Startup
        MainPage = New MainPage

        Me.RootVisual = MainPage

        UploadUrl = New Uri(Application.Current.Host.Source.AbsoluteUri.Substring(0, Application.Current.Host.Source.AbsoluteUri.IndexOf("/", 8)) & e.InitParams.Item("UploadUrl"))

    End Sub

    Private Sub Application_UnhandledException(ByVal sender As Object, ByVal e As ApplicationUnhandledExceptionEventArgs) Handles Me.UnhandledException

        ' If the app is running outside of the debugger then report the exception using
        ' the browser's exception mechanism. On IE this will display it a yellow alert 
        ' icon in the status bar and Firefox will display a script error.
        If Not System.Diagnostics.Debugger.IsAttached Then

            ' NOTE: This will allow the application to continue running after an exception has been thrown
            ' but not handled. 
            ' For production applications this error handling should be replaced with something that will 
            ' report the error to the website and stop the application.
            e.Handled = True
            Dim errorWindow As ChildWindow = New ErrorWindow(e.ExceptionObject)
            errorWindow.Show()
        End If
    End Sub

End Class
