' Copyright 2009 ESRI
' 
' All rights reserved under the copyright laws of the United States
' and applicable international laws, treaties, and conventions.
' 
' You may freely redistribute and use this sample code, with or
' without modification, provided you include the original copyright
' notice and use restrictions.
' 
' See use restrictions at <your ArcGIS install location>/developerkit/userestrictions.txt.
' 

Imports Microsoft.VisualBasic
Imports System
Namespace ESRI.ADF.Samples.Renderers
	''' <summary>
	''' Class containing various helper methods for rendering features
	''' </summary>
	Friend Class Utility
		''' <summary>
		''' Converts a transparency percentage to a 0-255 alpha value
		''' </summary>
		''' <param name="percentage">Percentage to convert</param>
		''' <returns>Corresponding alpha value, as a byte</returns>
		Public Shared Function TransparencyToAlpha(ByVal percentage As Double) As Byte
			Dim value As Double = 0
			value = 100 - percentage
			value = value / 100
			value = value * 255
			If value < 0 Then
			value = 0
			ElseIf value > 255 Then
			value = 255
			End If
			Return CByte(value)
		End Function

		''' <summary>
		''' Converts a Web ADF polygon to a GDI+ 2D graphics path
		''' </summary>
		''' <param name="adfPolygon">Polygon to convert</param>
		''' <returns>The corresponding graphics path</returns>
		Public Shared Function PolygonToPath(ByVal adfPolygon As ESRI.ArcGIS.ADF.Web.Geometry.Polygon) As System.Drawing.Drawing2D.GraphicsPath
			Return Utility.PolygonToPath(adfPolygon, 0, 0)
		End Function

		''' <summary>
		''' Converts a Web ADF polygon to a GDI+ 2D graphics path, offset by the specified amounts
		''' </summary>
		''' <param name="adfPolygon"></param>
		''' <param name="offsetX"></param>
		''' <param name="offsetY"></param>
		''' <returns>The corresponding graphics path</returns>
		Public Shared Function PolygonToPath(ByVal adfPolygon As ESRI.ArcGIS.ADF.Web.Geometry.Polygon, ByVal offsetX As Integer, ByVal offsetY As Integer) As System.Drawing.Drawing2D.GraphicsPath
			Dim graphicsPath As System.Drawing.Drawing2D.GraphicsPath = New System.Drawing.Drawing2D.GraphicsPath()

			' Loop through the polygon's rings, adding each to the graphics path
			For Each adfRing As ESRI.ArcGIS.ADF.Web.Geometry.Ring In adfPolygon.Rings
				' Add the current ring to the path
				graphicsPath.AddPolygon(pointCollectionToPointArray(adfRing.Points, offsetX, offsetY))

				' Add each of the current ring's holes to the path
				For Each ring As ESRI.ArcGIS.ADF.Web.Geometry.Hole In adfRing.Holes
					graphicsPath.AddPolygon(pointCollectionToPointArray(ring.Points, offsetX, offsetY))
				Next ring
			Next adfRing
			Return graphicsPath
		End Function

		''' <summary>
		''' Converts a Web ADF polyline to a GDI+ 2D graphics path
		''' </summary>
        ''' <param name="adfPolyline">The polyline to convert</param>
		''' <returns>The corresponding graphics path</returns>
		Public Shared Function PolylineToPath(ByVal adfPolyline As ESRI.ArcGIS.ADF.Web.Geometry.Polyline) As System.Drawing.Drawing2D.GraphicsPath
			Dim graphicsPath As System.Drawing.Drawing2D.GraphicsPath = New System.Drawing.Drawing2D.GraphicsPath()

			' Loop through the paths in the passed-in polyline, adding each to the graphics path
			For Each adfPath As ESRI.ArcGIS.ADF.Web.Geometry.Path In adfPolyline.Paths
				graphicsPath.AddLines(pointCollectionToPointArray(adfPath.Points, 0, 0))
			Next adfPath
			Return graphicsPath
		End Function

		''' <summary>
		''' Draws a GDI+ polygon on the specified surface corresponding to the specified Web ADF polygon.
		''' Applies a solid fill with the specified color and transparency and offsets the polygon by the
		''' specified amount.
		''' </summary>
		Public Shared Sub FillPolygon(ByVal graphics As System.Drawing.Graphics, ByVal adfPolygon As ESRI.ArcGIS.ADF.Web.Geometry.Polygon, ByVal color As System.Drawing.Color, ByVal transparency As Integer, ByVal offsetX As Integer, ByVal offsetY As Integer)
			If adfPolygon Is Nothing Then
			Return
			End If

			' Create a GDI+ graphics path from the passed-in polygon
			Using graphicsPath As System.Drawing.Drawing2D.GraphicsPath = Utility.PolygonToPath(adfPolygon, offsetX, offsetY)
				' Calculate the fill color after the specified transparency is applied
				Dim fillColor As System.Drawing.Color = System.Drawing.Color.FromArgb(Utility.TransparencyToAlpha(transparency), color)

				' Draw the polygon on the surface
				Using solidBrush As System.Drawing.SolidBrush = New System.Drawing.SolidBrush(fillColor)
					graphics.FillPath(solidBrush, graphicsPath)
				End Using
			End Using
		End Sub

		''' <summary>
		''' Renders a Web ADF polygon on the specified GDI+ surface with the specified color and outline width
		''' </summary>
		Public Shared Sub DrawPolygon(ByVal graphics As System.Drawing.Graphics, ByVal adfPolygon As ESRI.ArcGIS.ADF.Web.Geometry.Polygon, ByVal outlineColor As System.Drawing.Color, ByVal outlineWidth As Single)
			Utility.DrawPolygon(graphics, adfPolygon, outlineColor, outlineWidth, 0, 0)
		End Sub

		''' <summary>
		''' Renders a Web ADF polygon on the specified GDI+ surface with the specified color, outline width, and offset
		''' </summary>
		Public Shared Sub DrawPolygon(ByVal graphics As System.Drawing.Graphics, ByVal adfPolygon As ESRI.ArcGIS.ADF.Web.Geometry.Polygon, ByVal outlineColor As System.Drawing.Color, ByVal outlineWidth As Single, ByVal offsetX As Integer, ByVal offsetY As Integer)
			If adfPolygon Is Nothing Then
			Return
			End If

			' Create a GDI+ graphics path from the passed-in polygon
			Using graphicsPath As System.Drawing.Drawing2D.GraphicsPath = Utility.PolygonToPath(adfPolygon, offsetX, offsetY)
				' Draw the polygon on the surface
				Using pen As System.Drawing.Pen = New System.Drawing.Pen(outlineColor,outlineWidth)
					graphics.DrawPath(pen, graphicsPath)
				End Using
			End Using
		End Sub

		''' <summary>
		''' Renders a Web ADF polyline on the specified GDI+ surface with the specified color and thickness
		''' </summary>
		Public Shared Sub DrawPolyline(ByVal graphics As System.Drawing.Graphics, ByVal adfPolyline As ESRI.ArcGIS.ADF.Web.Geometry.Polyline, ByVal color As System.Drawing.Color, ByVal width As Single)
			If adfPolyline Is Nothing Then
			Return
			End If

			' Convert the polyline to a GDI+ graphics path
			Using path As System.Drawing.Drawing2D.GraphicsPath = Utility.PolylineToPath(adfPolyline)
				' Draw the polyline on the GDI+ surface
				Using pen As System.Drawing.Pen = New System.Drawing.Pen(color, width)
					graphics.DrawPath(pen, path)
				End Using
			End Using
		End Sub

		' Converts a Web ADF point collection to a screen point array, applying any specified offset
		' to each point
		Private Shared Function pointCollectionToPointArray(ByVal points As ESRI.ArcGIS.ADF.Web.Geometry.PointCollection, ByVal offsetX As Integer, ByVal offsetY As Integer) As System.Drawing.Point()
			' Loop through each point in the point collection, creating a corresponding screen
			' point in the point array for each
			Dim pointArray As System.Drawing.Point() = New System.Drawing.Point(points.Count - 1){}
			Dim i As Integer = 0
			Do While i < points.Count
				pointArray(i) = New System.Drawing.Point(System.Convert.ToInt32(points(i).X - offsetX), System.Convert.ToInt32(points(i).Y - offsetY))
				i += 1
			Loop
			Return pointArray
		End Function

	End Class
End Namespace
