<%@ Page Language="VB" AutoEventWireup="false" CodeFile="ErrorPage.aspx.vb" Inherits="ErrorPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Application Error</title>
</head>
<body>
    <form id="form1" runat="server">
			<asp:label id="lblError" style="LEFT: 16px; POSITION: absolute; TOP: 72px" runat="server"
				Font-Size="Medium" Font-Names="Verdana" Width="921" Height="120px"></asp:label>
			<asp:label id="lblTitle" style="LEFT: 16px; POSITION: absolute; TOP: 16px" runat="server"
				Font-Size="Large" Font-Names="Verdana" Width="921" Height="40px" ForeColor="Navy" Font-Bold="True">Application Error</asp:label>
			<asp:Label id="lblExtendedMessage" style="LEFT: 16px; POSITION: absolute; TOP: 208px"
				runat="server" Font-Size="X-Small" Font-Names="Verdana" Width="921px"
				Height="104px"></asp:Label>
    </form>
</body>
</html>
