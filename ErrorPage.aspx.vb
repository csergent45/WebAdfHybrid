
Partial Class ErrorPage
    Inherits System.Web.UI.Page

    'Before deploying application, set showTrace to false
    ' to prevent web application users from seeing error details	
    Private showTrace As Boolean = True

    Private Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load
        'get error message stored in session
        Dim message As String = CStr(Session("ErrorMessage"))

        'get details of error from exception stored in session
        Dim errorDetail As String = [String].Empty
        Dim exception As Exception = Session("Error") '
        If Not (exception Is Nothing) Then
            Select Case exception.GetType().ToString()
                Case "System.UnauthorizedAccessException"
                    Dim errorAccess As UnauthorizedAccessException = exception '
                    If errorAccess.StackTrace.ToUpper().IndexOf("SERVERCONNECTION.CONNECT") > 0 Then
                        errorDetail = "Unable to connect to server. <br>"
                    End If
            End Select
            errorDetail += exception.Message
        End If

        'create response and display it
        Dim response As String
        If Not (message Is Nothing) And message <> [String].Empty Then
            response = [String].Format("{0}<br>{1}", message, errorDetail.ToString())
        Else
            response = errorDetail
        End If
        lblError.Text = response
        If showTrace And Not (exception Is Nothing) Then lblExtendedMessage.Text = exception.StackTrace

    End Sub 'Page_Load 

End Class
