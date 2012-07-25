
Partial Class ServerStuff
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim name As String
        Dim hname As String
        Dim method As String
        name = Server.HtmlEncode(Request.UserHostAddress)
        hname = Server.HtmlEncode(Request.UserHostName)
        method = Server.HtmlEncode(Request.HttpMethod)


        TextBox1.Text = name
        TextBox2.Text = hname
        TextBox3.Text = method

    End Sub
End Class
