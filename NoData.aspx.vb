
Partial Class NoData
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim beginTime As String
        Dim endTime As String
        Dim myMinutes As String
        Dim myMinute As String = Nothing

        beginTime = "2:00AM"
        endTime = "2:30AM"
        myMinutes = DateTime.Parse(endTime).Minute - DateTime.Now.Minute


        If DateTime.Now > DateTime.Parse(beginTime) And DateTime.Now < DateTime.Parse(endTime) Then
            Select Case myMinutes > 1
                Case True
                    myMinute = "minutes."
                Case False
                    myMinute = "minute."
            End Select

            lblUserMessage.Text = "The server is currently performing maintenance, please check again in " & myMinutes & " " & myMinute

        Else
            lblUserMessage.Text = "The application that was used to populate the map appears to be closed or the server may be running updates. If you believe that you received this message in error call somebody, anybody, but not us and we aren't telling you who we are."
        End If

    End Sub
End Class
