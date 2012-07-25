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
	''' Renders point graphic features with the image specified by the features' attribute data
	''' </summary>
	<System.Serializable> _
	Public Class SimplePointRenderer
		Inherits ESRI.ADF.Samples.Renderers.RendererBase
		Private imagePathColumn_Renamed As String = "ImagePath"

		''' <summary>
		''' Name of the column containing the path to the image that will be used to symbolize the feature
		''' </summary>
		Public Property ImagePathColumn() As String
			Get
				Return imagePathColumn_Renamed
			End Get
			Set
				imagePathColumn_Renamed = Value
			End Set
		End Property

		#Region "IRenderer Members"

		''' <summary>
		''' Main part of the IRenderer interface, within which a feature encapsulating the specified DataRow is to be 
		''' rendered on the specified graphics surface. The geometry instance has already been transformed to screen 
		''' coordinate, so we don't have to worry about that here.
		''' </summary>
		''' <param name="row">row containing the feature's data</param>
		''' <param name="graphics">GDI+ surface on which to render the feature</param>
		''' <param name="geometryColumn">column containing the feature's geometry</param>
		Public Overrides Sub Render(ByVal row As System.Data.DataRow, ByVal graphics As System.Drawing.Graphics, ByVal geometryColumn As System.Data.DataColumn)
			' Validate method input
			If row Is Nothing OrElse graphics Is Nothing OrElse geometryColumn Is Nothing Then
				Return
			End If

			' Validate input geometry.  This renderer only supports points.
			Dim geometry As ESRI.ArcGIS.ADF.Web.Geometry.Geometry = TryCast(row(geometryColumn), ESRI.ArcGIS.ADF.Web.Geometry.Geometry)
			If geometry Is Nothing OrElse Not(TypeOf geometry Is ESRI.ArcGIS.ADF.Web.Geometry.Point) Then
				Return
			End If

			' Get the input point
            Dim p As ESRI.ArcGIS.ADF.Web.Geometry.Point = TryCast(geometry, ESRI.ArcGIS.ADF.Web.Geometry.Point)

			' Make sure the feature contains the image path column specified on the renderer
			If row.Table.Columns.Contains(Me.ImagePathColumn) Then
				' Get the path to the image
				Dim imagePath As String = row(Me.ImagePathColumn).ToString()

				' Convert the relative path to its absolute equivalent
				imagePath = System.Web.HttpContext.Current.Server.MapPath(imagePath)

				' Make sure the image exists
				If (Not System.IO.File.Exists(imagePath)) Then
					System.Diagnostics.Debug.Fail("Image path '" & imagePath & "' not found")
					Return
				End If

				' Get the image and draw it at the location specified by the input point
				Using image As System.Drawing.Image = System.Drawing.Image.FromFile(imagePath)
                    graphics.DrawImageUnscaled(image, System.Convert.ToInt32(p.X - 16), System.Convert.ToInt32(p.Y - 16))
                End Using


            End If
		End Sub

		''' <summary>
		''' Creates swatches used for the Table of Contents / Legend.
		''' This is automatically called by IMapTocFunctionality when generating the TOC.
		''' </summary>
		''' <param name="swatchInfo"></param>
		''' <param name="fileName"></param>
		''' <param name="minScale"></param>
		''' <param name="maxScale"></param>
		''' <returns></returns>
		Public Overrides Function GenerateSwatches(ByVal swatchInfo As ESRI.ArcGIS.ADF.Web.Display.Swatch.SwatchInfo, ByVal fileName As String, ByVal minScale As String, ByVal maxScale As String) As ESRI.ArcGIS.ADF.Web.Display.Swatch.SwatchCollection
            Return Nothing

            'Dim swatches As ESRI.ArcGIS.ADF.Web.Display.Swatch.SwatchCollection = New ESRI.ArcGIS.ADF.Web.Display.Swatch.SwatchCollection()
            'Dim swatchUtility As ESRI.ArcGIS.ADF.Web.Display.Swatch.SwatchUtility = New ESRI.ArcGIS.ADF.Web.Display.Swatch.SwatchUtility(swatchInfo)

            '' Create the swatches and add them to the swatch collection.  The code assumes three images placed in a
            '' website folder named "images" and named 1.gif, 2.gif, and 3.gif.
            'For i As Integer = 1 To 3
            '	' Get the absolute path to the current image
            '	Dim swatchPath As String = String.Format("~/images/{0}.gif", i)
            '	swatchPath = System.Web.HttpContext.Current.Server.MapPath(swatchPath)

            '	' Create a symbol from the image
            '	Dim swatchSymbol As ESRI.ArcGIS.ADF.Web.Display.Symbol.RasterMarkerSymbol = New ESRI.ArcGIS.ADF.Web.Display.Symbol.RasterMarkerSymbol(swatchPath)

            '	' Generate the swatch image and add it to the collection
            '	Dim swatchImage As ESRI.ArcGIS.ADF.Web.CartoImage = swatchUtility.DrawNewSwatch(swatchSymbol, Nothing)
            '	swatches.Add(New ESRI.ArcGIS.ADF.Web.Display.Swatch.Swatch(swatchImage, "Marker #" & i.ToString(), Nothing, Nothing))
            'Next i
            'Return swatches
		End Function

		#End Region
	End Class
End Namespace