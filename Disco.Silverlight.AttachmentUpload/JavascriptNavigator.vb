Imports System.Windows.Browser

<ScriptableType()>
Public Class JavascriptNavigator

    Private _contentFrame As Controls.Frame

    Public Sub New(contentFrame As Controls.Frame)
        _contentFrame = contentFrame
    End Sub

    <ScriptableMember()>
    Public Sub Navigate(uri As String)
        _contentFrame.Navigate(New Uri(uri, UriKind.Relative))
    End Sub

    <ScriptableMember()>
    Public ReadOnly Property Location As String
        Get
            Return _contentFrame.Source.ToString
        End Get
    End Property

End Class
