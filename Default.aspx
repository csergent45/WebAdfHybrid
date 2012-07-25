<%@ Page Language="VB" AutoEventWireup="true" Debug="true" Trace="true" CodeFile="Default.aspx.vb" Inherits="WebMapApplication" %>

<%@ Register Assembly="ESRI.ArcGIS.ADF.Web.UI.WebControls, Version=9.3.1.3000, Culture=neutral, PublicKeyToken=8fc3cc631e44ad86"
    Namespace="ESRI.ArcGIS.ADF.Web.UI.WebControls" TagPrefix="esri" %>
    
<%@ Register Src="Measure.ascx" TagName="Measure" TagPrefix="uc2" %>

<%@ Register Assembly="App_Code" Namespace="WebMapApp" TagPrefix="uc1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<!-- HTML Content Begin -->
<html xmlns="http://www.w3.org/1999/xhtml">

<!-- Head Section Begin -->
<head id="Head1" runat="server">
    
    <!-- Custom URL Address Icons Begin -->
    <link id="LinkFavIcon" runat="server" />
    <link id="LinkFavIconAni" runat="server" />
    <!-- Custom URL Address Icons End -->
    
    <title></title>

    <%--  For LTR. . . use these styles if dir="ltr" in html tag   --%>   
    <!-- ESRI Styles Begin -->
    <style type="text/css">
        .appFloat1 {float: left;} 
        .appFloat2 {float: right;}
        .appAlign1 {text-align: left;}
        .appAlign2 {text-align: right;}
        .combined1 {float: left; text-align: left;}
        .combined2 {float: right; text-align: right;}
        .mapPosition {left: 260px;} 
        .dockPosition {left: 0px;} 
   </style>
    <%--  For RTL. . . use these styles if dir="rtl" in html tag
    <style type="text/css">
        .appFloat1 {float: right;} 
        .appFloat2 {float: left;} 
        .appAlign1 {text-align: right;} 
        .appAlign2 {text-align: left;} 
        .combined1 {float: right; text-align: right;}
        .combined2 {float: left; text-align: left;}
        .mapPosition {left: 0px;} 
        .dockPosition {left: 512px;} 
    </style>  --%> 
    <!-- ESRI Styles End -->
</head>
<!-- Head Section End -->

<!-- Body Section Begin -->
<body style="margin: 0px 0px 0px 0px; background-color: white;  width: 100%; font-family: Verdana; font-size:8pt; ">
    <!-- .NET Content Begin -->
    <form id="form1" name="refreshForm" runat="server" enableviewstate="True">
        
        <!-- Set Values For ScriptManager -->
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />
        
        <input type="hidden" name="visited" value="" />
        
            <%-- Page Header --%>
            <!-- Web Page Heading Begin -->
            <asp:Panel runat="server"
                ID="PageHeader"
                CssClass="MapViewer_TitleBannerStyle"
                Width="100%"
                Height="50px"
                style="position: relative; ">
                
                <!-- Title Menu Section Begin -->
                <div id="TitleMenuDiv" style="top: 0px;" class="combined2">
                        <!-- Menu Begin -->
                        <asp:Menu ID="TitleMenu"
                                runat="server"
                                BackColor="Transparent"
                                Orientation="Horizontal"
                                Font-Size="8pt">
                                    <%-- Menu Items Begin --%>
                                    <Items>
                                        <asp:MenuItem Enabled="True" ImageUrl="" NavigateUrl="http://www.esri.com" PopOutImageUrl="" Selectable="True" Selected="False" SeparatorImageUrl="~/images/separator.gif" Target="_Blank" Text="ESRI" ToolTip="" Value="ESRI" />

                                        <asp:MenuItem Enabled="True" ImageUrl="" NavigateUrl="http://support.esri.com" PopOutImageUrl="" Selectable="True" Selected="False" SeparatorImageUrl="~/images/separator.gif" Target="_Blank" Text="ESRI  Support Center" ToolTip="" Value="ESRI  Support Center" />

                                        <asp:MenuItem Enabled="True" ImageUrl="" NavigateUrl="Help/Default.htm" PopOutImageUrl="" Selectable="True" 
                                            Selected="False" SeparatorImageUrl="~/Images/separator.gif" Target="_Blank" 
                                            Text="Help" ToolTip="" Value="Help" />
                                    </Items>
                                    <%-- Menu Items End --%>
                        </asp:Menu>
                        <!-- Menu End -->
                        
                        <asp:LoginName ID="LoginName1" runat="server" ForeColor="White" Font-Size="8pt" FormatString="Logged in as {0}" />&nbsp;&nbsp;
                        
                        <asp:LoginStatus ID="LoginStatus1" runat="server" Font-Size="8pt" ForeColor="White" LogoutAction="RedirectToLoginPage" Style="text-decoration: none" onmousedown="LogOut()" onmouseover="this.style.color='Black'" onmouseout="this.style.color='White'" />&nbsp;&nbsp;
                    
                        <asp:TextBox ID="txtPrint" runat="server" Width="74px" Visible="False"></asp:TextBox>
                        
                        <asp:HyperLink ID="CloseHyperLink" runat="server" Style="color: White; font-family: Verdana; font-size: 8pt; text-decoration: none" NavigateUrl="JavaScript: CloseOut()" Visible="False" ToolTip="Close Application" onmouseover="this.style.color='Black'" onmouseout="this.style.color='White'">Close&nbsp;&nbsp;</asp:HyperLink>
                        
                        <asp:TextBox ID="txtOriginalQuery"
                                runat="server"
                                BackColor="#99CCFF" 
                                ForeColor="Black"
                                Width="107px"
                                Visible="False">
                        </asp:TextBox>
                        
                        <asp:Label ID="lbl99"
                                runat="server"
                                BackColor="#CCFF33"
                                Font-Size="Large" 
                                Height="30px"
                                Width="50px" 
                                style="z-index: 1"
                                Visible="False">1
                        </asp:Label>
                             
                </div>
                <!-- Title Menu Section End -->
                
                &nbsp;&nbsp;&nbsp;<asp:Label ID="PageTitle" runat="server" Text="City of Decatur" Font-Size="12pt" Font-Names="Verdana" ForeColor="White" Font-Bold="True" style="z-index: 100; padding: 0px 5px 0px 5px" CssClass="appFloat1"></asp:Label>
           </asp:Panel>
            <!-- Web Page Heading End -->
            
            <%-- Link and Tool bar --%>
            <!-- Link Bar Begin -->
            <asp:Panel runat="server"
                    ID="LinkBar"
                    CssClass="MapViewer_TaskbarStyle"
                    Width="100%"
                    Height="30px">
                        <%-- Link Table Begin --%>
                        <table cellpadding="0"
                            cellspacing="0"
                            style="width: 100%; font-family: Verdana; font-size: 8pt;">
                            <%-- Link Row Begin --%>
                            <tr>
                                <%-- Task Menu Cell Begin --%>
                                <td id="TaskMenuCell"
                                        style="height: 30px; padding-left: 5px; "
                                        valign="middle"
                                        class="appAlign1">
                                            <%-- Task Menu Begin --%>
                                            <asp:Menu ID="TaskMenu"
                                                runat="server"
                                                Style="left: 0px; position: relative;top: 0px"
                                                Orientation="Horizontal"
                                                BackColor="Transparent"
                                                DynamicHorizontalOffset="2"
                                                Font-Names="Verdana"
                                                Font-Size="8pt"
                                                ForeColor="White"
                                                StaticSubMenuIndent="10px"
                                                Height="12px"
                                                CssClass="appFloat1"
                                                DynamicPopOutImageUrl="~/images/expand_right2.gif"
                                                StaticPopOutImageUrl="~/images/expand.gif">
                                            </asp:Menu>
                                            <%-- Task Menu End --%>
                                </td> 
                                <%-- Task Menu Cell End --%>
                                
                                <%-- Toolbar Cell Begin --%>
                                <td id="ToolbarCell"
                                        style="height: 30px;"
                                        class="appAlign2">
                                            <%-- ESRI Toolbar Begin --%>
                                            <esri:Toolbar ID="Toolbar1"
                                                    runat="server"
                                                    BuddyControlType="Map"
                                                    Group="Toolbar1_Group"
                                                    Height="28px"
                                                    ToolbarItemDefaultStyle-BackColor="Transparent"
                                                    ToolbarItemDefaultStyle-Font-Names="Arial"
                                                    ToolbarItemDefaultStyle-Font-Size="Smaller"
                                                    ToolbarItemDisabledStyle-BackColor="Transparent"
                                                    ToolbarItemDisabledStyle-Font-Names="Arial"
                                                    ToolbarItemDisabledStyle-Font-Size="Smaller"
                                                    ToolbarItemDisabledStyle-ForeColor="Gray"
                                                    ToolbarItemHoverStyle-Font-Bold="True"
                                                    ToolbarItemHoverStyle-Font-Italic="True"
                                                    ToolbarItemHoverStyle-Font-Names="Arial"
                                                    ToolbarItemHoverStyle-Font-Size="Smaller"
                                                    ToolbarItemSelectedStyle-BackColor="WhiteSmoke"
                                                    ToolbarItemSelectedStyle-Font-Bold="True"
                                                    ToolbarItemSelectedStyle-Font-Names="Arial"
                                                    ToolbarItemSelectedStyle-Font-Size="Smaller"
                                                    ToolbarStyle="ImageOnly"
                                                    WebResourceLocation="/aspnet_client/ESRI/WebADF/"
                                                    Width="280px"
                                                    ToolbarItemHoverStyle-BorderColor="Black"
                                                    ToolbarItemSelectedStyle-BorderColor="Black"
                                                    CurrentTool="MapZoomIn"
                                                    Alignment="Right"
                                                    ToolbarItemDefaultStyle-BorderColor="Transparent"
                                                    CssClass="appFloat2"
                                                    ToolbarItemHoverStyle-BackColor="White">
                                                        
                                                        <%-- Toolbar Items Begin --%>
                                                        <ToolbarItems>
                                                            <%-- ESRI Zoom In Control Begin --%>
                                                            <esri:Tool Text="Zoom In"
                                                                    DefaultImage="esriZoomIn.png"
                                                                    HoverImage="esriZoomIn.png"
                                                                    SelectedImage="esriZoomIn.png"
                                                                    Name="MapZoomIn"
                                                                    ToolTip="Zoom In"
                                                                    ServerActionAssembly="ESRI.ArcGIS.ADF.Web.UI.WebControls"
                                                                    ServerActionClass="ESRI.ArcGIS.ADF.Web.UI.WebControls.Tools.MapZoomIn"
                                                                    ClientAction="DragRectangle"
                                                                    JavaScriptFile="" />
                                                            <%-- ESRI Zoom In Control End --%>
                                                            
                                                            <%-- ESRI Zoom Out Control Begin --%>
                                                            <esri:Tool Text="Zoom Out"
                                                                    DefaultImage="esriZoomOut.png"
                                                                    HoverImage="esriZoomOut.png"
                                                                    SelectedImage="esriZoomOut.png"
                                                                    Name="MapZoomOut"
                                                                    ToolTip="Zoom Out"
                                                                    ServerActionAssembly="ESRI.ArcGIS.ADF.Web.UI.WebControls"
                                                                    ServerActionClass="ESRI.ArcGIS.ADF.Web.UI.WebControls.Tools.MapZoomOut"
                                                                    ClientAction="DragRectangle"
                                                                    JavaScriptFile="" />
                                                            <%-- ESRI Zoom Out Control End --%>
                                                            
                                                            <%-- ESRI Pan Control Begin --%>
                                                            <esri:Tool Text="Pan"
                                                                    DefaultImage="esriPan.png"
                                                                    HoverImage="esriPan.png"
                                                                    SelectedImage="esriPan.png"
                                                                    Name="MapPan"
                                                                    ToolTip="Pan"
                                                                    ServerActionAssembly="ESRI.ArcGIS.ADF.Web.UI.WebControls"
                                                                    ServerActionClass="ESRI.ArcGIS.ADF.Web.UI.WebControls.Tools.MapPan"
                                                                    ClientAction="DragImage"
                                                                    JavaScriptFile="" />
                                                            <%-- ESRI Pan Control End --%>
                                                            
                                                            <%-- ESRI Full Extent Begin --%>
                                                            <esri:Command Text="Full Extent"
                                                                    DefaultImage="esriZoomFullExtent.png"
                                                                    HoverImage="esriZoomFullExtent.png"
                                                                    SelectedImage="esriZoomFullExtent.png"
                                                                    Name="MapFullExtent"
                                                                    ToolTip="Full Extent"
                                                                    ServerActionAssembly="ESRI.ArcGIS.ADF.Web.UI.WebControls"
                                                                    ServerActionClass="ESRI.ArcGIS.ADF.Web.UI.WebControls.Tools.MapFullExtent"
                                                                    ClientAction=""
                                                                    JavaScriptFile="" />
                                                            <%-- ESRI Full Extent End --%>
                                                            
                                                            <%-- ESRI Back Command Begin --%>
                                                            <esri:Command Text="Back"
                                                                    DefaultImage="~/images/backward.png"
                                                                    HoverImage="~/images/backward.png"
                                                                    SelectedImage="~/images/backward.png"
                                                                    DisabledImage="~/images/backward_disabled.png"
                                                                    Name="MapBack"
                                                                    Disabled="True"
                                                                    BuddyItem="MapForward"
                                                                    ToolTip="Back Extent"
                                                                    ClientAction="ToolbarMapBack"
                                                                    JavaScriptFile="" />
                                                            <%-- ESRI Back Command End --%>
                                                            
                                                            <%-- ESRI Forward Command Begin --%>
                                                            <esri:Command Text="Forward"
                                                                    DefaultImage="~/images/forward.png"
                                                                    HoverImage="~/images/forward.png"
                                                                    SelectedImage="~/images/forward.png"
                                                                    DisabledImage="~/images/forward_disabled.png"
                                                                    Name="MapForward"
                                                                    Disabled="True"
                                                                    BuddyItem="MapBack"
                                                                    ToolTip="Forward Extent"
                                                                    ClientAction="ToolbarMapForward"
                                                                    JavaScriptFile="" />
                                                            <%-- ESRI Forward Command End --%>
                                                            
                                                            <%-- ESRI Magnify Command Begin --%>
                                                            <esri:Command DefaultImage="~/images/show-magnify.png"
                                                                    HoverImage="~/images/show-magnify.png"
                                                                    SelectedImage="~/images/show-magnify.png"
                                                                    Name="Magnifier"
                                                                    ToolTip="Magnifier"
                                                                    ClientAction="toggleMagnifier();"
                                                                    JavaScriptFile="" />
                                                            <%-- ESRI Magnify Command End --%>
                                                            
                                                            <%-- ESRI Identify Command Begin --%>
                                                            <esri:Tool Cursor="pointer"
                                                                    Text="Map Identify"
                                                                    DefaultImage="~/images/identify.png"
                                                                    HoverImage="~/images/identify.png"
                                                                    SelectedImage="~/images/identify.png"
                                                                    Name="MapIdentify"
                                                                    ToolTip="Map Identify"
                                                                    ClientAction="MapIdentifyTool()"
                                                                    JavaScriptFile="" />
                                                            <%-- ESRI Identify Command End --%>
                                                            
                                                            <%-- ESRI Measure Command Begin --%>
                                                            <esri:Tool DefaultImage="~/images/measure.png"
                                                                    HoverImage="~/images/measure.png"
                                                                    SelectedImage="~/images/measure.png"
                                                                    Name="Measure"
                                                                    ToolTip="Measure"
                                                                    ClientAction="startMeasure()"
                                                                    JavaScriptFile="" />
                                                            <%-- ESRI Measure Command End --%>

                                                            <%-- ESRI Overview Map Command Begin --%>
                                                            <esri:Command DefaultImage="~/images/show-overview-map.png"
                                                                    HoverImage="~/images/show-overview-map.png"
                                                                    SelectedImage="~/images/show-overview-map.png"
                                                                    Name="OverviewMapToggle"
                                                                    ToolTip="Show OverviewMap"
                                                                    ClientAction="toggleOverviewMap()"
                                                                    JavaScriptFile="" />
                                                            <%-- ESRI Overview Map Command End --%>
                                                        </ToolbarItems>         
                                                        <%-- Toolbar Items End --%>
                                                        
                                                        <BuddyControls>
                                                            <esri:BuddyControl Name="Map1" />
                                                        </BuddyControls>
                                            </esri:Toolbar>
                                            <%-- ESRI Toolbar End --%>
                                </td>       
                                <%-- Toolbar Cell End --%>
                            </tr>
                            <%-- Link Row End --%>
                        </table>
                        <%-- Link Table End --%>          
            </asp:Panel>
            <!-- Link Bar End --> 
            
            <%-- Page content area ..... Left panel and Map display --%>
            <!-- Page Content Begin -->            
            <div id="PageContent" 
                    style="width: 100%; position: relative; top: 0px; float: left; left: 0px; height: 100%;">
            
                <%-- Map Display --%>
                <!-- Map Display Begin -->
                <div id="Map_Panel"
                        style="width: 512px; height: 512px; position: absolute; top: 0px; overflow: hidden; background-color: White;"
                        class="mapPosition">
                    <!-- ESRI Map Begin -->
                    <esri:Map ID="Map1"
                            runat="server"
                            MapResourceManager="MapResourceManager1" 
                            Height="100%"
                            Width="100%" 
                            Extent="780590.66666666674,1144218.0555555555,847257.33333333326,1183106.9444444445" 
                            PrimaryMapResource="IntranetVector"
                            BorderColor="Black"
                            BorderStyle="Solid" 
                            BorderWidth="1px">
                    </esri:Map>
                    <!-- ESRI Map End -->
                      
                    <%--Navigation--%>
                    <!-- ESRI Navigation Begin -->        
                    <table id="Navigation_Panel"
                            style=" position: absolute; top: 5px; width: auto; height: auto; background-color: Transparent; font-family: Verdana; font-size: x-small; "
                            class="appFloat1">
                        <!-- North Arrow Row Begin -->
                        <tr>
                            <!-- North Arrow Row Cell Begin -->
                            <td>
                                <!-- North Arrow Graphic Begin -->
                                <esri:Navigation ID="Navigation1"
                                        runat="server"
                                        Map="Map1"
                                        DisplayImageUrl="~/North_Arrow/directional_arrows_N.gif"
                                        Height="52px"
                                        Width="52px"
                                        Size="44">
                                    <DisplayCharacter CharacterIndex="58" FontName="ESRI North" />
                                </esri:Navigation>
                                <!-- North Arrow Graphic End -->
                            </td>
                            <!-- North Arrow Row Cell End -->
                        </tr>
                        <!-- North Arrow Row End -->
                        
                        <!-- Zoom Level Row Begin -->
                        <tr>
                            <!-- Zoom Level Cell Data Begin -->
                            <td align="center">
                                <!-- Zoom Level Table Begin -->
                                <table>
                                    <!-- Zoom Level Graphic Row Begin -->
                                    <tr>
                                        <!-- Zoom Level Graphic Begin -->
                                        <td align="left">
                                            <esri:ZoomLevel ID="ZoomLevel1" runat="server" Map="Map1" />       
                                        </td>
                                        <!-- Zoom Level Graphic End -->
                                    </tr>
                                    <!-- Zoom Level Graphic Row End -->
                                </table>
                                <!-- Zoom Level Table End --> 
                            </td>
                            <!-- Zoom Level Cell Data End -->
                        </tr>
                        <!-- Zoom Level Row End -->
                         
                    </table>
                    <!-- ESRI Navigation End -->
                </div>
                <!-- Map Display End -->
                
                <%-- Left Panel ..... for tasks, results, toc, overview map, etc --%> 
                <!-- Left Panel Map Tools and Information Begin -->
                <table id="LeftPanelCell"
                        cellpadding="0"
                        cellspacing="0"
                        class="dockPosition"
                        style="position: absolute; top: 0px; background-color: White;">
                    <!-- Left Panel Map Tools and Information Row Begin -->
                    <tr>
                        <!-- Left Panel Map Tools and Information Cell Begin -->
                        <td id="LeftPanelTableCell" style="position: relative;">
                            
                            <!-- Left Panel Map Tool and Information Scroll Begin -->
                            <div id="LeftPanelScrollDiv" style="position: relative; width: auto;">
                                
                                <!-- Left Panel Map Tool and Information Cell Begin -->
                                <div id="LeftPanelCellDiv" style="width: 250px;  border: solid 1px #999999; position: relative; ">
                               
                                    <!-- Application Parameters Begin -->
                                        <!-- Random Parameter Value Begin -->
                                        <asp:TextBox ID="txtRandParam" runat="server"
                                            ForeColor="Black"
                                            Visible="False">
                                        </asp:TextBox>
                                        <!-- Random Parameter Value End -->
                                    
                                        <!-- Selection Parameter Begin -->
                                        <asp:TextBox ID="txtSelection"
                                            runat="server"
                                            Visible="False">
                                        </asp:TextBox>
                                        <!-- Selection Parameter End -->
                                        
                                    <!-- Application Parameters End -->
                                               
                                    <!-- Addresses Section Begin -->
                                    <div id="addresses" style="width: 100%; border: solid 1px #666666; height: auto">
                                        
                                        <!-- Valid Addresses Panel Begin -->                                
                                        <esri:FloatingPanel ID="AddressPanel"
                                                runat="server"
                                                BackColor="White"
                                                BorderColor="Gray"
                                                BorderStyle="Solid"
                                                BorderWidth="1px"
                                                Font-Names="Verdana"
                                                Font-Size="8pt" 
                                                ForeColor="Black"
                                                Title="Address Selection"
                                                TitleBarColor="WhiteSmoke"
                                                TitleBarHeight="20px"
                                                TitleBarSeparatorLine="False"
                                                Transparency="35"
                                                Width="100%" 
                                                CloseButton="False"
                                                Draggable="False"
                                                WidthResizable="False" 
                                                EnableViewState="False">
                                                    <!-- Valid Address Panel Scroll Bars Begin -->
                                                    <asp:Panel ID="MyAddressPanel"
                                                        runat="server" 
                                                        ScrollBars="Vertical"
                                                        HorizontalAlign="Left"
                                                        Width="100%" 
                                                        EnableViewState="False">
                                                            <!-- Valid Addresses Table Begin -->
                                                            <table cellpadding="4">
                                                                <!-- Valid Addresses Row Begin -->
                                                                <tr>
                                                                    <!-- Valid Addresses Data Begin -->
                                                                    <td>
                                                                        <!-- Valid Addresses Display All Link Begin -->
                                                                        <asp:LinkButton ID="lnkAllAddresses"
                                                                            runat="server"
                                                                            Visible="False">Zoom To All
                                                                        </asp:LinkButton>
                                                                        <!-- Valid Addresses Display All Link End -->
                                                                        
                                                                          <!-- Valid Addresses GridView Begin -->
                                                                        <!--
                                                                                RAND_PARAM - Uniquely identifies a given query
                                                                                FEATURE_ID - Unique identifier for spatial query
                                                                                FEATURE_DESC -
                                                                                APPLICATION_DESC -
                                                                                    
                                                                        -->
                                                                            
                                                                        <asp:GridView ID="gridAddresses"
                                                                            runat="server"
                                                                            AutoGenerateColumns="False" 
                                                                            CellPadding="4"
                                                                            DataKeyNames="FEATURE_ID" 
                                                                            EmptyDataText="No Addresses From Selection Can Be Mapped"
                                                                            GridLines="None" 
                                                                            ShowHeader="False">
                                                                                <Columns>
                                                                                    <asp:HyperLinkField DataNavigateUrlFields="RAND_PARAM,FEATURE_ID,FEATURE_DESC,APPLICATION_DESC" 
                                                                                        DataNavigateUrlFormatString="Default.aspx?sender={0}&amp;FEATURE_ID={1}&amp;FEATURE_DESC={2}&amp;APPLICATIION_DESC={3}" 
                                                                                        DataTextField="FEATURE_DESC"
                                                                                        Visible="false" />
                                                                                    <asp:TemplateField>
                                                                                        <ItemTemplate>
                                                                                            <asp:HyperLink ID="lblAddress"
                                                                                                runat="server" 
                                                                                                NavigateUrl='<%# String.Format("Default.aspx?sender={0}&amp;FEATURE_ID={1}&amp;FEATURE_DESC={2}&amp;APPLICATION_DESC={3}",Eval("RAND_PARAM"),Eval("FEATURE_ID"),Eval("FEATURE_DESC"),Eval("APPLICATION_DESC"))%>' 
                                                                                                Text='<%#Bind("FEATURE_DESC")%>'
                                                                                                ToolTip='<%#Bind("FEATURE_ID")%>'>
                                                                                            </asp:HyperLink>
                                                                                        </ItemTemplate>
                                                                                    </asp:TemplateField>
                                                                                    <asp:BoundField DataField="CASES" />
                                                                                </Columns>
                                                                        </asp:GridView>
                                                                        <!-- Valid Addresses GridView End -->
                                                                    </td>
                                                                    <!-- Valid Addresses Data End -->           
                                                                </tr>
                                                                <!-- Valid Addresses Row End -->                                                                               
                                                            </table>
                                                            <!-- Valid Addresses Table End -->
                                                    </asp:Panel>
                                                    <!-- Valid Address Panel Scroll Bars End -->
                                        </esri:FloatingPanel>
                                        <!-- Valid Addresses Panel End -->
                                        
                                        <!-- Invalid Addresses Panel Begin -->
                                        <esri:FloatingPanel ID="AddressPanelNone"
                                                runat="server"
                                                BackColor="White"
                                                BorderColor="Gray"
                                                BorderStyle="Solid"
                                                BorderWidth="1px"
                                                Font-Names="Verdana"
                                                Font-Size="8pt" 
                                                ForeColor="Black"
                                                Title="Unable to Map Address"
                                                TitleBarColor="WhiteSmoke"
                                                TitleBarHeight="20px"
                                                TitleBarSeparatorLine="False"
                                                Transparency="35"
                                                Width="100%" 
                                                Draggable="False"
                                                WidthResizable="False" 
                                                EnableViewState="False"
                                                Visible="False">
                                                    <!-- Unknown Panel Begin -->
                                                    <!-- 
                                                            You guessed it; I don't know what it's for
                                                            but it may be needed so it's staying.
                                                    -->
                                                    <asp:Panel ID="Panel1"
                                                            runat="server" 
                                                            ScrollBars="Vertical"
                                                            HorizontalAlign="Left"
                                                            Width="100%" 
                                                            EnableViewState="False">
                                                    </asp:Panel>
                                                    <!-- Unknown Panel End -->
                                            </esri:FloatingPanel>
                                        <!-- Invalid Addresses Panel End -->
                                            
                                    </div>
                                    <!-- Addresses Section End -->            
                                        
                                    <%--Results--%>
                                    <!-- Results Section Begin -->
                                    <div id="Results" style="width: 100%; border: solid 1px #666666; height: auto">
                                            
                                            <!-- Results Panel Header Begin -->
                                            <asp:Panel ID="Results_Panel_Header"
                                                    runat="server"
                                                    CssClass="MapViewer_WindowTitleBarStyle"
                                                    Height="24px">
                                                    
                                                <!-- Results Style Begin -->
                                                <div style="padding:4px; cursor: default; vertical-align: middle;">
                                                    <!-- Results Panel Title Begin -->
                                                    <div id="ResultsPanelTitle"
                                                            class="appFloat1"
                                                            style="vertical-align: middle;">Results
                                                    </div>
                                                    <!-- Result Panel Title End -->
                                                    
                                                    <!-- Results Panel Expand Button Begin -->
                                                    <div id="ResultsPanelExpandButton"
                                                            class="appFloat2"
                                                            style="vertical-align: middle;">
                                                            
                                                            <!-- Results Panel Expand Button Image Begin -->
                                                            <img id="Results_Panel_Image"
                                                                    src="images/expand.gif"
                                                                    alt="Expand"
                                                                    onmousedown="toggleConsolePanel('Results_Panel')"
                                                                    style="cursor: pointer"
                                                                    width="20"
                                                                    height="20" />
                                                            <!-- Results Panel Expand Button Image End -->
                                                    </div>
                                                    <!-- Results Panel Expand Button End -->
                                                </div>
                                                <!-- Results Style End -->
                                                
                                            </asp:Panel>
                                            <!-- Results Panel Header End -->
                                            
                                            <!-- Results Panel Collapse Begin -->
                                            <asp:Panel ID="Results_Panel_Collapse"
                                                    runat="server"
                                                    CssClass="MapViewer_CollapsePanel"
                                                    style="display: block;">
                                                 
                                                <!-- Results Task Begin -->
                                                <esri:TaskResults ID="TaskResults1"
                                                        runat="server"
                                                        BackColor="White" 
                                                        ExpandDepth="2"
                                                        Font-Bold="False"
                                                        Font-Names="Verdana"
                                                        Font-Size="8pt" 
                                                        ForeColor="Black"
                                                        GraphicsTransparency="0"
                                                        Height="100%"
                                                        Map="Map1" 
                                                        ShowClearAllButton="True" 
                                                        Style="width: 100%; height: 100%; cursor: default; margin-bottom: 1px; overflow: auto; float: left;" 
                                                        Width="100%" />
                                                <!-- Results Task End -->
                                                
                                                <!-- Results Panel Body Begin --> 
                                                <asp:Panel ID="Results_Panel_Body"
                                                        runat="server"
                                                        Height="150" 
                                                        style="padding: 0px 0px 10px 0px; background-color: White; width: 100% ">
                                                     
                                                            <div id="TaskResults1Holder" style="width: 100%; height: 100%;"></div>
                                                 
                                                 </asp:Panel>
                                                <!-- Results Panel Body End -->
                                                
                                            </asp:Panel>
                                            <!-- Results Panel Collapse End -->
                                           
                                    </div>
                                    <!-- Results Section End -->
                                        
                                    <!-- Crime Legend Section Begin -->
                                    <div id="CrimeLegend">
                                                <asp:Panel ID="Crime_Legend_Panel"
                                                        runat="server"
                                                        Visible="False">
                                                        
                                                    <esri:FloatingPanel ID="CrimeLegendPanel"
                                                            runat="server"
                                                            BackColor="White"
                                                            BorderColor="Gray"
                                                            BorderStyle="Solid"
                                                            BorderWidth="1px"
                                                            Font-Names="Verdana"
                                                            Font-Size="8pt"
                                                            ForeColor="Black"
                                                            Height="150px"
                                                            Title="Crime Legend"
                                                            TitleBarColor="WhiteSmoke"
                                                            TitleBarHeight="20px"
                                                            TitleBarSeparatorLine="False"
                                                            Transparency="35"
                                                            Width="100%" 
                                                            CloseButton="False">
                                                            
                                                                <asp:Panel ID="CrimeContentLegend"
                                                                        runat="server"
                                                                        Height="100%" 
                                                                        ScrollBars="Vertical"
                                                                        Width="100%">
                                                           
                                                           <br />&nbsp;
                                                                    <img src="Images/GreenCrimesLegend.png" alt="One Crime" />&nbsp;
                                                                    
                                                                    <asp:Label ID="Label4" runat="server" Text="One Crime"></asp:Label>
                                                                        
                                                                        <br /><br />&nbsp;
                                                                        
                                                                        <img src="Images/YellowCrimesLegend.png" alt="Two Crimes" />&nbsp;
                                                                        
                                                                        <asp:Label ID="Label5" runat="server" Text="Two Crimes"></asp:Label>
                                                                        
                                                                        <br /><br />&nbsp;
                                                                        
                                                                        <img src="Images/RedCrimesLegend.png" alt="Three or More Crimes" />&nbsp;
                                                                        
                                                                        <asp:Label ID="Label15" runat="server" Text="Three or More Crimes"></asp:Label><br />
                                                           
                                                           <br />&nbsp;
                                                           <img src="Images/AutoBurglary.png" alt="Auto Burglary" />&nbsp;<asp:Label ID="lblBurglary" runat="server" Text="Label"> Auto Burglary</asp:Label><br />
                                                           &nbsp;<img src="Images/AutoTheft.png" alt="Vehicle Theft" />&nbsp;<asp:Label ID="Label3" 
                                                               runat="server" Text="Vehicle Theft"></asp:Label><br />
                                                           &nbsp;<img src="Images/Burglary.png" alt="Burglary" />&nbsp;<asp:Label ID="Label7" runat="server" Text="Burglary"></asp:Label><br />
                                                           &nbsp;<img src="Images/BusinessRobber.png" alt="Business Robber" />&nbsp;<asp:Label 
                                                                        ID="Label8" runat="server" Text="Business Robbery"></asp:Label><br />
                                                           &nbsp;<img src="Images/HomeInvasion.png" alt="Home Invasion" />&nbsp;<asp:Label ID="Label9" runat="server" Text="Home Invasion"></asp:Label><br />
                                                           &nbsp;<img src="Images/SexualAssault.png" alt="Sexual Assault" />&nbsp;<asp:Label ID="Label10" runat="server" Text="Sexual Assault"></asp:Label><br />
                                                           &nbsp;<img src="Images/Shooting.png" alt="Shooting" />&nbsp;<asp:Label ID="Label11" runat="server" Text="Shooting"></asp:Label><br />
                                                           &nbsp;<img src="Images/StreetRobbery.png" alt="Street Robbery" />&nbsp;<asp:Label ID="Label12" runat="server" Text="Street Robbery"></asp:Label><br />
                                                           &nbsp;<img src="Images/NA.png" alt="N/A" />&nbsp;<asp:Label ID="Label13" runat="server" Text="N/A"></asp:Label><br />
                                                           &nbsp;<img src="Images/VehicleInvasion.png" alt="Vehicle Invasion" />&nbsp;<asp:Label ID="Label6" runat="server"
                                                               Text="Vehicle Invasion"></asp:Label><br />
                                                           &nbsp;<img src="Images/UndeterminedNotAssigned.png" alt="No Assigned Crime" />&nbsp;<asp:Label ID="Label14" runat="server" Text="No Assigned Crime"></asp:Label>
                                                    </asp:Panel>
                                                   </esri:FloatingPanel>
                                                    
                                               </asp:Panel>
                                    </div>
                                    <!-- Crime Legend Section End -->               
                                                              
                                    <%--TOC ... Map Contents--%>
                                    <div id="Toc_Panel" style="width: 100%; border: solid 1px #666666; height: auto">
                                            <asp:Panel ID="Toc_Panel_Header" runat="server" CssClass="MapViewer_WindowTitleBarStyle" Height="24px">
                                                <div style="padding:4px; cursor: default; vertical-align: middle;">
                                                    <div id="TocPanelTitle" class="appFloat1" style="vertical-align: middle;">Map Contents</div>
                                                    <div id="TocPanelExpandButton" class="appFloat2" style="vertical-align: middle">
                                                        <img id="Toc_Panel_Image" src="images/collapse.gif" alt="Collapse" onmousedown="toggleConsolePanel('Toc_Panel')" style="cursor: pointer" width="20" height="20" />
                                                    </div>
                                                </div>
                                            </asp:Panel>
                                            
                                             <asp:Panel ID="Toc_Panel_Collapse" runat="server" CssClass="MapViewer_CollapsePanel" style="display: block">
                                                    
                                                    <asp:TextBox ID="myParameter" runat="server" Visible="False"></asp:TextBox>
                                                    <asp:TextBox ID="txtReqSeqNum" runat="server" Visible="false"></asp:TextBox>
                                                    <asp:TextBox ID="txtFeature" runat="server" Visible="False"></asp:TextBox>
                                                    <asp:ListBox ID="lstSession" runat="server" Visible="False"></asp:ListBox>
                                                    <asp:TextBox ID="txtMultiFeature" runat="server" Visible="False"></asp:TextBox>
                                                    <asp:TextBox ID="txtQuery" runat="server" Visible="False"></asp:TextBox>
                                                    <asp:TextBox ID="txtStart" runat="server" Visible="False">0</asp:TextBox>
                                                 
                                                    <!--<asp:Label ID="Label1" runat="server"></asp:Label>-->
                                                    <asp:Panel ID="Toc_Panel_Body" runat="server" Height="250" style="padding: 0px 0px 10px 0px; background-color: White; width: 100% ">
                                                    <esri:Toc ID="Toc1" runat="server" BuddyControl="Map1" Height="100%" 
                                                        Width="100%" style="cursor: default; float: left;" ForeColor="Black" 
                                                        ExpandDepth="1" />
                                                </asp:Panel>
                                                
                                                 <asp:TextBox ID="txtAppName" runat="server" Visible="False"></asp:TextBox>
                                            </asp:Panel>
                                            
                                           </div>

                                </div>
                                <!-- Left Panel Map Tool and Information Cell End -->
                            </div>
                            <!-- Left Panel Map Tool and Information Scroll End -->
                        </td>
                        <!-- Left Panel Map Tools and Information Cell End -->
                        
                        <!-- Left Panel Toggle Cell Begin -->         
                        <td> 
                        <%-- Toggle Bar ..... toggles left panel visibility --%>
                            <!-- Toggle Cell Section Begin -->
                            <div id="ToggleCell"
                                    style="overflow: hidden; width: 10px; border-style: solid; border-color: #999999; border-bottom-width: 0px; border-left-width: 1px; border-right-width: 1px; border-top-width: 0px; background-color: White; position: relative ">
                                        <!-- Toggle Table Begin -->
                                        <table id="ToggleCellTable"
                                                cellpadding="0"
                                                cellspacing="0"
                                                style="position: relative; height: 100%; width: 100%;">
                                                    <tr>
                                                        <td id="PanelSlider"
                                                                style="cursor: e-resize; background-color: White; height: 45%">
                                                        </td>
                                                    </tr> 
                                                    <tr>
                                                        <td style="height: 24px;"><img id="CollapseImage" src="images/collapse_left.gif" alt="Collapse" style="cursor: pointer" height="20px" /></td>
                                                    </tr> 
                                                    <tr>
                                                        <td id="PanelSliderBottom" style="cursor: e-resize; background-color: White; height: 50%"></td>
                                                    </tr> 
                                        </table>
                                        <!-- Toggle Table End -->
                            </div>
                            <!-- Toggle Cell Section End -->                                    
                        </td>
                    </tr>
                    <!-- left Panel Map Tools and Information Row End -->
                </table>  
                <!-- Left Panel Map Tools and Information End -->        
            </div>
            <!-- Page Content End -->
            
            
            <%--Overview Map--%>
            <!-- Overview Map Begin -->
            <esri:OverviewMap ID="OverviewMap1"
                    runat="server"
                    Map="Map1"
                    Height="150px"
                    MapResourceManager="MapResourceManager1"
                    OverviewMapResource="IntranetVector"
                    Style="z-index: 1001"
                    Width="150px"
                    Font-Bold="False"
                    BorderStyle="Solid"
                    BorderWidth="3px"
                    ForeColor="Black"/>
            <!-- Overview Map End -->
        
            <%--Magnifier--%>
            <!-- Magnifier Begin -->   
            <esri:Magnifier ID="Magnifier1"
                    runat="server"
                    MagnifierMapResource="IntranetVector"
                    Map="Map1"
                    MapResourceManager="MapResourceManager1"
                    Style="left: 335px;position: absolute; top:190px"
                    Visible="False"
                    BackColor="White"
                    Transparency="15">
            </esri:Magnifier>
            <!-- Magnifier End -->
 
            <%--ScaleBar / MapCopyrightText --%>
            <!-- ScaleBar Map Copyright Text Begin -->        
            <table id="Copyright_Panel"
                    runat="server"
                    border="0"
                    cellpadding="0"
                    cellspacing="0"
                    style="width: auto; height: auto; background-color: Transparent; font-family: Verdana; font-size: x-small; color: Black "
                    dir="ltr">
                    <tr><td><img src="images/blank.gif" style="width: 5px; height: 35px; padding: 0px 0px 0px 0px;" alt="" /></td><td align="left" valign="middle">
                        <esri:ScaleBar ID="ScaleBar1" runat="server" BarHeight="8" Height="35px" Map="Map1" Style="z-index: 102; width: auto;" Width="165px" />
                    </td>
                    <td align="left" valign="middle">
                       <%--MAP_ELEMENT_COMMENT<esri:MapCopyrightText ID="MapCopyrightText1" runat="server" Height="15px" Font-Names="Verdana" Font-Size="8pt" Font-Bold="true" ForeColor="#333333" Map="Map1" style="z-index: 1011; cursor: pointer;" CalloutWindowTitleText="Copyright" Text="Copyright"></esri:MapCopyrightText>--%>
                    </td></tr> 
            </table>
            <!-- ScaleBar Map Copyright Text End -->
        

            <!-- GridView With Invalid Addresses Begin -->
            <asp:GridView ID="gridAddressesNoMap" 
                    runat="server"
                    AutoGenerateColumns="False" 
                    CellPadding="4" 
                    DataKeyNames="FEATURE_ID"
                    GridLines="None"
                    ShowHeader="False">
                            <Columns>
                                <asp:BoundField DataField="FEATURE_DESC" ReadOnly="True" />
                            </Columns>
            </asp:GridView>
            <!-- GridView With Invalid Addresses End -->
            
            <%--MapResourceManager--%>                                 
            <esri:MapResourceManager ID="MapResourceManager1" runat="server" 
            style="position: absolute; left: 344px; top: 129px; z-index: 503;">
        
<ResourceItems>
    <esri:MapResourceItem Definition="&lt;Definition DataSourceDefinition=&quot;In Memory&quot; DataSourceType=&quot;GraphicsLayer&quot; Identity=&quot;&quot; ResourceDefinition=&quot;&quot; /&gt;" 
        DisplaySettings="visible=True:transparency=0:mime=True:imgFormat=PNG32:height=100:width=100:dpi=96:color=-32513:transbg=True:displayInToc=False:dynamicTiling=" 
        LayerDefinitions="" Name="RowSelection" />
    <esri:MapResourceItem Definition="&lt;Definition DataSourceDefinition=&quot;In Memory&quot; DataSourceType=&quot;GraphicsLayer&quot; Identity=&quot;&quot; ResourceDefinition=&quot;&quot; /&gt;" 
        DisplaySettings="visible=True:transparency=0:mime=True:imgFormat=PNG8:height=100:width=100:dpi=96:color=-32576:transbg=True:displayInToc=True:dynamicTiling=" 
        LayerDefinitions="" Name="Selection" />
    <esri:MapResourceItem Definition="&lt;Definition DataSourceDefinition=&quot;In Memory&quot; DataSourceType=&quot;GraphicsLayer&quot; Identity=&quot;&quot; ResourceDefinition=&quot;&quot; /&gt;" 
        DisplaySettings="visible=True:transparency=0:mime=True:imgFormat=PNG8:height=100:width=100:dpi=96:color=-32576:transbg=True:displayInToc=False:dynamicTiling=" 
        LayerDefinitions="" Name="Selection2" />
    <esri:MapResourceItem DisplaySettings="visible=True:transparency=0:mime=True:imgFormat=PNG8:height=100:width=100:dpi=96:color=-32576:transbg=True:displayInToc=True:dynamicTiling=False" 
        LayerDefinitions="" 
        Definition="&lt;Definition DataSourceDefinition=&quot;http://unibeast2/arcgis/services&quot; DataSourceType=&quot;ArcGIS Server Internet&quot; Identity=&quot;&quot; ResourceDefinition=&quot;Layers@IntranetVector&quot; /&gt;" 
        Name="IntranetVector" />

    <esri:MapResourceItem Definition="&lt;Definition DataSourceDefinition=&quot;http://unibeast2/arcgis/services&quot; DataSourceType=&quot;ArcGIS Server Internet&quot; Identity=&quot;&quot; ResourceDefinition=&quot;Layers@IntranetVectorTransparent&quot; /&gt;" 
        DisplaySettings="visible=True:transparency=0:mime=True:imgFormat=PNG8:height=100:width=100:dpi=96:color=-32576:transbg=True:displayInToc=True:dynamicTiling=" 
        LayerDefinitions="" Name="IntranetVectorTransparent" />

    <esri:MapResourceItem Definition="&lt;Definition DataSourceDefinition=&quot;http://unibeast/arcgis/services&quot; DataSourceType=&quot;ArcGIS Server Internet&quot; Identity=&quot;&quot; ResourceDefinition=&quot;Layers@RasterDataset&quot; /&gt;" 
        DisplaySettings="visible=True:transparency=0:mime=True:imgFormat=JPG:height=100:width=100:dpi=96:color=-32576:transbg=True:displayInToc=True:dynamicTiling=False" 
        LayerDefinitions="" Name="Imagery" />

</ResourceItems>
</esri:MapResourceManager>
            
            <%--MapResourceManager for overview Map--%>
		    <esri:MapResourceManager ID="MapResourceManager2" runat="server" 
            style="left: 496px; position: absolute; top: 605px">

            </esri:MapResourceManager>
            
            <%--GeocodeResourceManager--%>
            <esri:GeocodeResourceManager ID="GeocodeResourceManager1" runat="server" Style="z-index: 104;left: 490px; position: absolute; top: 681px">
        
<ResourceItems>
            <esri:GeocodeResourceItem ShowAllCandidates="False" MinCandidateScore="30" 
                    MinMatchScore="65" 
                    Definition="&lt;Definition DataSourceDefinition=&quot;http://unibeast/arcgis/services&quot; DataSourceType=&quot;ArcGIS Server Internet&quot; Identity=&quot;&quot; ResourceDefinition=&quot;DefaultAddressLocator&quot; /&gt;" 
                    Name="DefaultAddressLocator" />
</ResourceItems>
</esri:GeocodeResourceManager>

            <%--GeoprocessingResourceManager--%>        
            <esri:GeoprocessingResourceManager ID="GeoprocessingResourceManager1" runat="server" Style="z-index: 105; left: 493px; position: absolute; top: 775px">
        

</esri:GeoprocessingResourceManager>       
            
            <%--TaskManager--%>
            <esri:TaskManager ID="TaskManager1" runat="server" Font-Names="Verdana" Font-Size="8pt" ForeColor="Black" Height="100px" Style="z-index: 106; left: 279px; position: absolute;top: 605px" Width="200px" BuddyControl="TaskMenu">

<esriTasks:FindAddressTask ID="FindAddressTask1" runat="server" 
                Style="z-index: 10000; left: 100px; position: absolute; top: 100px" 
                Width="200px" Visible="False" Title="Locate Address" 
                GeocodeResourceManager="GeocodeResourceManager1" NavigationPath="" 
                GeocodeResource="DefaultAddressLocator" ShowFieldAttributes="False" 
                Renderer="{&quot;type&quot;:&quot;simple&quot;,&quot;symbol&quot;:{&quot;type&quot;:&quot;marker&quot;,&quot;format&quot;:&quot;png32&quot;,&quot;url&quot;:&quot;%MarkerSymbolDir%\\pin-red-20x20.png&quot;,&quot;size&quot;:[20,20],&quot;center&quot;:[0,19]}}" 
                HighlightRenderer="{&quot;type&quot;:&quot;simple&quot;,&quot;symbol&quot;:{&quot;type&quot;:&quot;marker&quot;,&quot;format&quot;:&quot;png32&quot;,&quot;url&quot;:&quot;%MarkerSymbolDir%\\pin-red-28x28.png&quot;,&quot;size&quot;:[28,28],&quot;center&quot;:[0,27]}}" 
                ActivityIndicatorImage="" DockingContainerElementID="" 
                FeatureHighlightColor="Coral" FeatureSelectionColor="Aqua" HelpUrl="" 
                TitleBarBackgroundImage="" TitleBarCssClass="" TitleBarForeColor="Black" 
                TitleBarHeight="20px" WebResourceLocation="/aspnet_client/ESRI/WebADF/"><TaskResultsContainers><esri:BuddyControl Name="TaskResults1" /></TaskResultsContainers></esriTasks:FindAddressTask>
<esriTasks:SearchAttributesTask ID="SearchAttributesTask1" runat="server" Style="z-index: 10000; left: 100px; position: absolute; top: 100px" Width="200px" Visible="False" ButtonText="Find" MaxRecords="50" FindOption="AllLayers" DefaultValue="00-00-000-000" LabelText="Search for:" SearchFields="MapResourceManager1:::IntranetVector$$$-1780026885:::18:::PIN" Title="Locate Parcel" ToolTip="" NavigationPath="" GroupResultsByTable="True" ShowFieldAttributes="True" FeatureSelectionColor="Aqua" ShowResultsAsMapTips="True" CustomLayerFormats="" UseCustomLayerFormat="False"><TaskResultsContainers><esri:BuddyControl Name="TaskResults1" /></TaskResultsContainers></esriTasks:SearchAttributesTask>
<esriTasks:PrintTask ID="PrintTask1" runat="server" bordercolor="LightSteelBlue" 
                borderstyle="Outset" borderwidth="1px" font-names="Verdana" font-size="8pt" 
                forecolor="Black" titlebarcolor="WhiteSmoke" 
                Style="z-index: 10000; left: 200px; position: absolute; top: 200px" 
                Width="200px" Visible="False" BackColor="White" Map="Map1" 
                MapCopyrightText="MapCopyrightText1" Title="Print" 
                ButtonText="Create Print Page" TitleSettings-DisplayInDialog="True" 
                TitleSettings-DefaultValue="Map" MapSettings-DisplayInDialog="True" 
                MapSettings-DefaultValue="False" ResultsVisible="True" 
                WidthSettings-DisplayInDialog="True" WidthSettings-DefaultValue="0" 
                WidthSettings-WidthList="SizeHeight=3;SizeWidth=3;Name=Small;Unit=Inches:::SizeHeight=5;SizeWidth=5;Name=Medium;Unit=Inches:::SizeHeight=7;SizeWidth=7;Name=Large;Unit=Inches" 
                QualitySettings-DisplayInDialog="False" QualitySettings-DefaultValue="0" 
                ScaleBarSettings-DisplayInDialog="False" ScaleBarSettings-DefaultValue="True" 
                NorthArrowSettings-DisplayInDialog="False" 
                NorthArrowSettings-DefaultValue="False" NorthArrowSettings-FontName="ESRI North" 
                NorthArrowSettings-FontCharacter="176" LegendSettings-DisplayInDialog="False" 
                LegendSettings-DefaultValue="True" 
                LegendSettings-HiddenLayers="RowSelection:::Selection:::Selection2:::IntranetVector:::IntranetVectorTrans:::Imagery" 
                WebResourceLocation="/asp/net_client/ESRI/WebADF/" 
                NorthArrowSettings-DisplayCharacter="ESRI North:176"><TaskResultsContainers><esri:BuddyControl Name="TaskResults1" /></TaskResultsContainers></esriTasks:PrintTask>
</esri:TaskManager>
 
            <asp:TextBox ID="myMachineName" runat="server" Visible="False"></asp:TextBox>
            <asp:TextBox ID="myIP_Address" runat="server" Visible="false"></asp:TextBox>
            
            <%-- Extenders--%>        
            <esri:DockExtender ID="DockExtender4" runat="server" Alignment="TopRight" DockControlId="Map1" TargetControlID="OverviewMap1" SkinID="" Enabled="True" BehaviorID="DockExtender4" EnableViewState="True">
            </esri:DockExtender>        
        
            <esri:DockExtender ID="DockExtender3" runat="server" Alignment="BottomLeft" DockControlId="Map1" TargetControlID="Copyright_Panel" SkinID="" Enabled="True" BehaviorID="DockExtender3" EnableViewState="True">
            </esri:DockExtender>        
        
            <ajaxToolkit:ResizableControlExtender ID="Toc_Panel_ResizeExtender" runat="server" TargetControlID="Toc_Panel_Body" BehaviorID="Toc_Panel_ResizeBehavior" HandleCssClass="MapViewer_ResizeHandleStyle" MinimumWidth="250" MinimumHeight="20" MaximumWidth="250" MaximumHeight="1200" HandleOffsetY="10" />

            <ajaxToolkit:ResizableControlExtender ID="Results_Panel_ResizeExtender" runat="server" TargetControlID="Results_Panel_Body" BehaviorID="Results_Panel_ResizeBehavior" HandleCssClass="MapViewer_ResizeHandleStyle" MinimumWidth="250" MinimumHeight="20" MaximumWidth="250" MaximumHeight="1200" HandleOffsetY="10" />
            
            <uc2:Measure ID="Measure1" runat="server" AreaUnits="Sq_Miles" MapBuddyId="Map1" MapUnits="Resource_Default" MeasureUnits="Miles" NumberDecimals="3" />
                    
            <script language="javascript" type="text/javascript" src="javascript/WebMapApp.js"></script> 
            
            <script language="javascript" type="text/javascript">
            arcgisWebApp.setPageElementSizes();
            if (isRTL) {
                var tmenu = $get("TaskMenu");
                if (tmenu)
                    tmenu.style.visibility = "hidden";
            }
       </script>
            
            <uc1:MapIdentify ID="MapIdentify1" runat="server" MapBuddyId="Map1" /> 
   
    </form>
    <!-- .NET Content End -->

    <script language="javascript" type="text/javascript">
         Sys.Application.add_init(startUp);
    </script> 
 
    <%-- <script type="text/jscript" language="jscript">
    $('table[id*=gridAddresses] > tr').mouseover(function() {
$(this).children('td').eq(0).text(); // this will get 1 or to, as it targets the 1st td of the current tr the mouse is over.
});

</script>--%>

</body>
<!-- Body Section End -->
</html>
<!-- HTML Content End -->