<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Startup.aspx.vb" Inherits="Startup" Debug="true" %>

<!-- Declaring XHTML Strict to ensure the page is well formed. -->
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">

<!-- HTML Document Begin -->
<html xmlns="http://www.w3.org/1999/xhtml">

<!-- Document Information Begin -->
<head id="headStartUp" runat="server">
    <!-- Set your application title -->
    <title>Map Generator</title>
    
    
    <!-- Notes Section Start -->
			<!-- 
					NOTES
					
					  Title:    Startup
					 Author:    Chris Sergent
					Created:    February 4, 2011
					Summary:    This is the startup page for the Hybrid application.
				  
			-->
    <!-- Notes Section Finish -->
</head>
<!-- Document Information End -->

<!-- Page Content Begin -->
<body id="startUpContent" runat="server">
    <!-- .Net Content Begin -->
    <form id="frmStartup" runat="server">
        <!-- Startup Section Begin -->
        <div id="startUp" runat="server">
            <!-- Generating map graphic will display for slow load times -->
            <img src="Images/generatingMap.gif" alt="Generating Map" />
        </div>
        <!-- Startup Section End -->
    </form>
    <!-- .Net Content End -->
</body>
<!-- Page Content End -->
</html>
<!-- HTML Document End -->