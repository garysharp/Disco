Imports System.Runtime.CompilerServices
#If Not SILVERLIGHT = 1 Then
Imports System.Web.Mvc
#End If

Namespace BI
    <Extension()>
    Public Module Utilities

        <Extension()>
        Public Function StreamToString(stream As IO.Stream) As String
            If (stream.Position <> 0) Then
                stream.Position = 0
            End If
            Using sr As IO.StreamReader = New IO.StreamReader(stream)
                Return sr.ReadToEnd
            End Using
        End Function

        <Extension()>
        Public Function ToFuzzy(d As DateTime) As String

            Dim n = DateTime.Now
            '' Today
            If d.Date = n.Date Then
                '' Earlier
                If d < n Then
                    '' Earlier
                    If d > n.AddMinutes(-1) Then
                        Return String.Format("A moment ago")
                    End If

                    If d > n.AddMinutes(-10) Then
                        Return String.Format("A few minutes ago")
                    End If
                Else
                    '' Later
                    If d < n.AddMinutes(1) Then
                        Return String.Format("In a moment")
                    End If

                    If d < n.AddMinutes(10) Then
                        Return String.Format("In a few minutes")
                    End If
                End If

                Return String.Format("Today at {0:h:mm tt}", d)
            End If

            '' Past
            If d.Date < n.Date Then
                '' Yesterday
                If d.Date = n.Date.AddDays(-1) Then
                    Return String.Format("Yesterday at {0:h:mm tt}", d)
                End If

                '' Last Week
                Dim weekStart = n.Date.AddDays(-7)
                If d > weekStart Then
                    Return String.Format("Last {0:dddd} at {0:h:mm tt}", d)
                End If

                '' This Year
                Dim yearStart = New Date(n.Year, 1, 1)
                If d > yearStart Then
                    '' Weeks
                    Dim weekNumber As Integer = ((n.DayOfYear - d.DayOfYear) / 7)
                    Return String.Format("{0} Weeks ago, {1:ddd, d MMM}", weekNumber, d)
                End If
            End If

            '' Future
            If d.Date > n.Date Then
                '' Tomorrow
                If d.Date = n.Date.AddDays(1) Then
                    Return String.Format("Tomorrow at {0:h:mm tt}", d)
                End If

                '' Next Week
                Dim weekEnd = n.Date.AddDays(7)
                If d < weekEnd Then
                    Return String.Format("Next {0:dddd} at {0:h:mm tt}", d)
                End If

                '' This Year
                Dim nextYearStart = New Date(n.Year + 1, 1, 1)
                If d < nextYearStart Then
                    '' Weeks
                    Dim weekNumber As Integer = ((d.DayOfYear - n.DayOfYear) / 7)
                    Return String.Format("In {0} Weeks, {1:ddd, d MMM}", weekNumber, d)
                End If
            End If

            Return d.ToString("ddd, d MMM yyyy")

        End Function
        <Extension()>
        Public Function ToFuzzy(d As DateTime?, Optional NullValue As String = "N/A") As String
            If d.HasValue Then
                Return ToFuzzy(d.Value)
            Else
                Return NullValue
            End If
        End Function

        <Extension()>
        Public Function ToFullDateTime(d As DateTime) As String
            Return d.ToString("ddd, d MMM yyyy @ h:mm:sstt")
        End Function
        <Extension()>
        Public Function ToFullDateTime(d As DateTime?, Optional NullValue As String = "N/A") As String
            If d.HasValue Then
                Return ToFullDateTime(d.Value)
            Else
                Return NullValue
            End If
        End Function

        <Extension()>
        Public Function ToJavascriptDate(d As DateTime?, Optional DefaultDate As DateTime? = Nothing) As String
            If d.HasValue Then
                Return ToJavascriptDate(d.Value)
            Else
                If DefaultDate.HasValue Then
                    Return ToJavascriptDate(DefaultDate.Value)
                Else
                    Return "null"
                End If
            End If
        End Function
        <Extension()>
        Public Function ToJavascriptDate(d As DateTime) As String
            Return String.Format("new Date({0}, {1}, {2}, {3}, {4}, {5})", d.Year, d.Month - 1, d.Day, d.Hour, d.Minute, d.Second)
        End Function

#If Not SILVERLIGHT = 1 Then
        <Extension()>
        Public Function ToSelectListItems(Items As List(Of String), Optional SelectedItem As String = Nothing) As List(Of SelectListItem)
            If SelectedItem Is Nothing Then
                Return Items.Select(Function(item) New SelectListItem() With {.Value = item,
                                                                              .Text = item}).ToList()
            Else
                Return Items.Select(Function(item) New SelectListItem() With {.Value = item,
                                                                              .Text = item,
                                                                              .Selected = (item = SelectedItem)}).ToList()
            End If
        End Function
#End If

    End Module
End Namespace