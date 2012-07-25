<%@ Control Language="VB" AutoEventWireup="true" CodeFile="Measure.ascx.vb" Inherits="Measure" %>

    <script language="javascript" type="text/javascript" src="JavaScript/display_measure.js"></script>
    
<table id="MeasureToolbar" cellpadding="0" cellspacing="0" style="color:Black;background-color:White;border-color:Black; border-width:1px;border-style:Outset;width:275px;display:none;position: absolute;  left: 285px; top: 298px; z-index:11000; ">
		<tr id="MeasureToolbar_Title"  onmouseover="this.style.cursor='move'" style="background-image:url(images/blank.gif); cursor:move; ">
		    <td style="height:24px;" colspan="2" class="MapViewer_WindowTitleBarStyle">
		        <table cellpadding="0" cellspacing="0" width="100%" >
		        <tr>
			        <td style="padding-left:5px;">Measure<div id="MeasureTitleGlass" style="position: absolute; left: 0px; top: 0px; width: 200px; height: 24px; "><img src="images/blank.gif" alt="" width="200px" height="24px" /></div></td>
			       <script language="javascript" type="text/javascript">
			            document.writeln('<td align="' + (document.documentElement.dir=="rtl" ? 'left' : 'right') + '">');
			            document.writeln('<img id="MeasureToolbar_CloseButton" src="images/dismiss.png" onclick="hideMeasureToolbarTool(\'MeasureToolbar\')" style="cursor:pointer;" alt="Close" hspace="0" vspace="0" /></td>');
			       </script> 
			        <td  style="width:5px;height:24px;"></td></tr></table></td></tr>
		        <tr>
					<td align="left" style="padding-left: 5px; padding-top: 5px;">
				        <table cellpadding="0" cellspacing="0" ><tr>
					    <td id="MeasureToolbarButton_point" style="border: solid White 1px; background-color: White;" onmouseover="this.style.cursor='pointer'; this.style.borderColor='Black';" onmouseout="checkMeasureToolbarBorder(this, 'point')" onmousedown="setMeasureToolbarTool('point')"><img id="ToolbarImage_point" src="images/measure-point.png" align="middle" alt="Point - Coordinates" title="Point - Coordinates" style="padding: 0px 0px 0px 0px" /></td>
					    <td id="MeasureToolbarButton_polyline" style="border: solid Black 1px; background-color: #EEEEEE;" onmouseover="this.style.cursor='pointer';this.style.borderColor='Black';" onmouseout="checkMeasureToolbarBorder(this, 'polyline')" onmousedown="setMeasureToolbarTool('polyline')"><img id="ToolbarImage_polyline" src="images/measure-line.png" align="middle" alt="Line - Distance" title="Line - Distance" style="padding: 0px 0px 0px 0px" /></td>
					    <td id="MeasureToolbarButton_polygon" style="border: solid White 1px; background-color: White;" onmouseover="this.style.cursor='pointer';this.style.borderColor='Black';" onmouseout="checkMeasureToolbarBorder(this, 'polygon')" onmousedown="setMeasureToolbarTool('polygon')"><img id="ToolbarImage_polygon" src="images/measure-poly.png" align="middle" alt="Polygon - Area" title="Polygon - Area" style="padding: 0px 0px 0px 0px" />	</td>
					    </tr></table>
                        <input id="MeasureUnits" type="hidden" value="<%=MeasureUnits %>"/>
                        <input id="AreaUnits" type="hidden" value="<%=AreaUnits %>"/>
			        </td>

		</tr>
		<tr id="MeasureToolbar_BodyRow">
			<td  id="MeasureToolbar_BodyCell" style="background-image:url(images/blank.gif);vertical-align:top;padding-left:5px;padding-top:5px;">
    
	            <table id="MeasureToolbarTable" cellspacing="2" cellpadding="1" style=" width: 100%;font: normal 7pt Verdana; ">
		            <tr><td style="background-color: #ffffff" id="MeasureDisplay" colspan="2"  valign="top">
		                Click on the map and draw a line. Double-click to end line.
		            </td></tr>
	            </table>

			</td>
			<td id="MeasureToolbar_SideResizeCell" ><img width="5px" height="100%" src="images/blank.gif" alt="" /></td>
		</tr>
		<tr id="MeasureToolbar_ResizeRow">
			<td ><img height="5px" width="100%" src="images/blank.gif" alt="" /></td>
			<td><img width="5px" src="images/blank.gif" alt="" /></td>
		</tr>
	</table>

    <script language="javascript" type="text/javascript">
        var measureVectorCallbackFunctionString = "<%=m_callbackInvocation %>";
        if (isIE && ieVersion<7)  setIE6MeasureToolbarImages(); 
        var measureHandle = $get("MeasureToolbar_Title");
        $addHandler(measureHandle, "mousedown", dragMeasureToolbarStart);
        if (document.documentElement.dir=="rtl") $get("MeasureTitleGlass").style.left = "30px";
    </script>
