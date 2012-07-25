
Partial Class ip_address
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim userIP_Address = Server.HtmlEncode(Request.UserHostName)
        Label1.Text = userIP_Address

    End Sub
End Class
