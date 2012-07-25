<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Login.aspx.vb" Inherits="Login" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>Login to Web Mapping Application</title>
</head>
<body style="margin: 0px 0px 0px 0px; background-color: white;  width: 100%; font-family: Verdana; font-size:8pt;">
    <form id="form1" runat="server">
    <div style="position: absolute; left: 0px; top: 0px; width: 100%; height: 100%; overflow: hidden;">
  <%-- Page Header --%>
            <div id="PageHeader"  class="MapViewer_TitleBannerStyle" style="height: 50px; width: 100%; position: relative;" >
                &nbsp;
            </div>
 <%-- Link and Tool bar --%>
           <div id="LinkBar" class="MapViewer_TaskbarStyle" style="height: 30px; width: 100%">
                &nbsp;
           </div>  
          
          <table style="width: 100%; font-size: medium; color: Black;">
            <tr><td align="center">
                <br />
                <br />
         <asp:Login ID="Login1" runat="server" BackColor="Gainsboro" BorderColor="Black" BorderPadding="4"
            BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana" Font-Size="0.8em"
            ForeColor="#333333" DestinationPageUrl="~/Default.aspx" DisplayRememberMe="False" 
                    Width="250px" onloginerror="Login1_LoginError">
            <TitleTextStyle BackColor="White" Font-Bold="True" Font-Size="0.9em" ForeColor="Black" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px" />
            <InstructionTextStyle Font-Italic="True" ForeColor="Black" />
            <TextBoxStyle Font-Size="0.8em" />
            <LoginButtonStyle BackColor="White" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px"
                Font-Names="Verdana" Font-Size="0.8em" ForeColor="Black" />
        </asp:Login>
                              
                <br />
                <asp:Label ID="lblNotAuthorized" runat="server" ForeColor="Red" Font-Names="Verdana" Font-Size="8pt" Text="This account is not authorized to access the site. Please log in with an authorized account." Visible="false"></asp:Label>
                <br />
                <asp:LinkButton ID="btnLostPassword" runat="server" OnClick="btnLostPassword_Click" Font-Names="Verdana" Font-Size="8pt">Lost password?</asp:LinkButton>
                <asp:Label ID="lblLinksSeparator" runat="server" Text="|"></asp:Label>
                <asp:LinkButton ID="btnChangePassword" runat="server" OnClick="btnChangePassword_Click" Font-Names="Verdana" Font-Size="8pt">Change your password</asp:LinkButton>
                <asp:Panel ID="PasswordRecoverPanel" runat="server" Visible="true" Style="width: auto; height: auto;" >
                    <asp:PasswordRecovery ID="PasswordRecovery1" runat="server" BackColor="Gainsboro" BorderColor="Black"
                      BorderPadding="4" BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana"
                      Font-Size="0.8em" Style="" Height="100px" Width="250px" MailDefinition-From="bbaker@esri.com"
                      MailDefinition-Subject="Important information" QuestionLabelText="Security question:" SuccessPageUrl="~/Login.aspx?mode=pwdRecov" SuccessText="Your password information has been sent to you." Visible="False">
                      <InstructionTextStyle Font-Italic="True" ForeColor="Black" Font-Bold="False" Font-Size="Smaller" />
                      <SuccessTextStyle Font-Bold="True" ForeColor="#5D7B9D" />
                      <TextBoxStyle Font-Size="0.8em" />
                      <TitleTextStyle BackColor="White" Font-Bold="True" Font-Size="0.9em" ForeColor="Black" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px" />
                      <SubmitButtonStyle BackColor="White" BorderColor="Black" BorderStyle="Solid"
                          BorderWidth="1px" Font-Names="Verdana" Font-Size="0.8em" ForeColor="Black" />
                        <MailDefinition Subject="Information about your account" BodyFileName="~/PasswordRecoveryMail.txt" From="username@yourdomain.com">
                        </MailDefinition>
                  </asp:PasswordRecovery>
              </asp:Panel>
              <asp:Label ID="lblPasswordRecovery" runat="server" Font-Names="Verdana" 
                Text="Your password information has been sent. Please check your e-mail for further details." Visible="False" Font-Size="8pt"></asp:Label>
                
              <br />
              <asp:ChangePassword ID="ChangePassword1" runat="server" DisplayUserName="True" MembershipProvider=""
                 Visible="False" BackColor="Gainsboro" BorderColor="Black" BorderPadding="4"
                 BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana" Font-Size="0.8em"
                 ForeColor="#333333" CancelDestinationPageUrl="~/Login.aspx" ContinueDestinationPageUrl="~/Login.aspx">
              </asp:ChangePassword>
              <br />
              <asp:Label ID="lblPasswordChange" runat="server" 
                Text="Your password has been changed. Please log in with your new password."
                 Font-Names="Verdana" Font-Size="8pt" Visible="False"></asp:Label>
          </td></tr></table>
    </div>      
    </form>
</body>
</html>