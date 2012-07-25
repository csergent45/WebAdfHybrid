Imports System
Imports System.Data
Imports System.Configuration
Imports System.Collections
Imports System.Web
Imports System.Web.Security
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.UI.WebControls.WebParts
Imports System.Web.UI.HtmlControls

Partial Public Class Login
    Inherits System.Web.UI.Page
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load
        Dim provider As String = Membership.Provider.Name

        If Not [String].IsNullOrEmpty(provider) Then
            ' Use the default membership provider in web.config if not set separately 
            If [String].IsNullOrEmpty(PasswordRecovery1.MembershipProvider) Then
                PasswordRecovery1.MembershipProvider = provider
            End If
            If [String].IsNullOrEmpty(ChangePassword1.MembershipProvider) Then
                ChangePassword1.MembershipProvider = provider
            End If
        End If

        ' Show elements depending on whether initial load or after password change/recovery 
        Dim mode As String = "login"

        If Not [String].IsNullOrEmpty(Request.QueryString("mode")) Then
            mode = Request.QueryString("mode")
        ElseIf ViewState("mode") IsNot Nothing Then
            mode = DirectCast(ViewState("mode"), String)
        End If
        If Not Page.IsPostBack Then
            ' if user is authenticated but a return URL set, then assume user is not authorize 
            If User.Identity.IsAuthenticated AndAlso Request.QueryString("ReturnUrl") IsNot Nothing Then
                lblNotAuthorized.Text = [String].Concat(User.Identity.Name, " is not authorized to access the site. Please log in with an authorized account.")
                lblNotAuthorized.Visible = True
            End If
        Else
            lblNotAuthorized.Visible = False
        End If

        setMode(mode)
    End Sub

    Protected Sub btnLostPassword_Click(ByVal sender As Object, ByVal e As EventArgs)
        setMode("recoverPassword")
    End Sub

    Protected Sub btnChangePassword_Click(ByVal sender As Object, ByVal e As EventArgs)
        setMode("changePassword")
    End Sub

    Private Sub setMode(ByVal mode As String)
        ' hide everything 
        Login1.Visible = False
        btnLostPassword.Visible = False
        btnChangePassword.Visible = False
        lblLinksSeparator.Visible = False
        lblPasswordRecovery.Visible = False
        lblPasswordChange.Visible = False
        PasswordRecovery1.Visible = False
        ChangePassword1.Visible = False

        Select Case mode
            Case "login"
                setLoginVisible(True)
                Exit Select
            Case "pwdRecov"

                setLoginVisible(True)
                lblPasswordRecovery.Visible = True
                Exit Select
            Case "recoverPassword"

                PasswordRecovery1.Visible = True
                ' use ViewState flag to keep password-recovery control visible since 
                ' querystring flag gets cleared on 2nd step of recovery 
                ViewState("mode") = "recoverPassword"
                Exit Select
            Case "changePassword"

                ChangePassword1.Visible = True
                ViewState("mode") = "changePassword"
                Exit Select
        End Select
    End Sub


    Private Sub setLoginVisible(ByVal visible As Boolean)
        Login1.Visible = visible
        ' Get the web.config for the web application 
        Dim config As System.Configuration.Configuration = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~/")
        ' Get the web.config's <membership> section (will return a valid section, even 
        ' if <membership> not actually added to the web.config) 
        Dim membSection As System.Web.Configuration.MembershipSection = DirectCast(config.GetSection("system.web/membership"), System.Web.Configuration.MembershipSection)
        ' See whether membership section has a provider element 
        For Each provider As System.Configuration.ProviderSettings In membSection.Providers
            If provider.ElementInformation.IsPresent Then
                ' Local web.config has a membership provider - show password-change links 
                ' Test whether password reset/retrieval supported 
                If Membership.Provider.EnablePasswordReset OrElse Membership.Provider.EnablePasswordRetrieval Then
                    ' See whether smtp server is set 
                    Dim mailSection As System.Net.Configuration.SmtpSection = TryCast(config.GetSection("system.net/mailSettings/smtp"), System.Net.Configuration.SmtpSection)

                    If Not [String].IsNullOrEmpty(ChangePassword1.MembershipProvider) Then
                        btnChangePassword.Visible = visible
                    End If
                    If mailSection IsNot Nothing Then
                        If Not [String].IsNullOrEmpty(PasswordRecovery1.MembershipProvider) AndAlso (Membership.Provider.EnablePasswordReset OrElse Membership.Provider.EnablePasswordRetrieval) AndAlso mailSection.Network.Host IsNot Nothing Then
                            btnLostPassword.Visible = visible
                        End If
                    End If
                    If btnChangePassword.Visible AndAlso btnLostPassword.Visible Then
                        lblLinksSeparator.Visible = True
                    End If
                End If
            End If
        Next
    End Sub





    Protected Sub Login1_LoginError(ByVal sender As Object, ByVal e As EventArgs)
        Dim user As MembershipUser = Membership.GetUser(Login1.UserName)
        ' Check user exists; if so, was failure due to locked out? 
        If user IsNot Nothing Then
            If user.IsLockedOut Then
                Login1.FailureText = [String].Format("The account {0} has been locked out. Please contact your system administrator.", Login1.UserName)

            End If
        End If
    End Sub
End Class