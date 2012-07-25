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
	''' This class is used for generating point, line and polygon layers with a set of random features
	''' </summary>
	Public Class GenerateGraphicsHelper
        ' Random number generator
        Private Shared randomizer As System.Random = New System.Random(CInt(Fix(System.DateTime.Now.Ticks / 1000000000)))

		''' <summary>
		''' Creates the specified number of random polygons within the given extent
		''' </summary>
		''' <param name="layerName">Name of layer</param>
		''' <param name="adfEnvelope">Extent to create features within</param>
		''' <param name="count">Number of features to create</param>
		''' <returns>FeatureGraphicsLayer</returns>
		Public Shared Function CreatePolygonFeatures(ByVal layerName As String, ByVal adfEnvelope As ESRI.ArcGIS.ADF.Web.Geometry.Envelope, ByVal count As Integer) As ESRI.ArcGIS.ADF.Web.Display.Graphics.FeatureGraphicsLayer
			If adfEnvelope Is Nothing Then ' Cannot create features within a null extent
			Return Nothing
			End If

			' Create a feature graphics layer and give it columns for height, width, and color
			Dim featureGraphicsLayer As ESRI.ArcGIS.ADF.Web.Display.Graphics.FeatureGraphicsLayer = New ESRI.ArcGIS.ADF.Web.Display.Graphics.FeatureGraphicsLayer(ESRI.ArcGIS.ADF.Web.FeatureType.Polygon)
			featureGraphicsLayer.TableName = layerName
			featureGraphicsLayer.Columns.Add("Height", GetType(System.Double))
			featureGraphicsLayer.Columns.Add("Width", GetType(Integer))
			featureGraphicsLayer.Columns.Add("Color", GetType(System.Drawing.Color))

			' Calculate the maximum width/height of a polygon.  Specify that it not exceed 1/10th of the
			' width of the passed-in extent
			Dim maxSize As Double = adfEnvelope.Width / 10

			' Create the number of polygons specified by the passed in argument.  Add each to the feature graphics
			' layer.  The polygons created will be rectangles.
			Dim i As Integer = 0
			Do While i < count
				' Calculate an origin for the rectangle within the passed-in extent
				Dim xmin As Double = adfEnvelope.XMin + randomizer.NextDouble() * adfEnvelope.Width
				Dim ymin As Double = adfEnvelope.YMin + randomizer.NextDouble() * adfEnvelope.Height

				' Calculate rotation factors
				Dim rotation As Double = randomizer.NextDouble() * System.Math.PI
				Dim cosRot As Double = System.Math.Cos(rotation)
				Dim sinRot As Double = System.Math.Sin(rotation)

				' Calculate a width and height less than the maximum size
				Dim width As Double = randomizer.NextDouble() * maxSize
				Dim height As Double = randomizer.NextDouble() * maxSize

				' Create the rectangle
				Dim adfRing As ESRI.ArcGIS.ADF.Web.Geometry.Ring = New ESRI.ArcGIS.ADF.Web.Geometry.Ring()
				adfRing.Points.Add(New ESRI.ArcGIS.ADF.Web.Geometry.Point(xmin, ymin))
				adfRing.Points.Add(New ESRI.ArcGIS.ADF.Web.Geometry.Point(xmin + cosRot * width, ymin + sinRot * width))
				adfRing.Points.Add(New ESRI.ArcGIS.ADF.Web.Geometry.Point(xmin + cosRot * width + sinRot * height, ymin + sinRot * width - cosRot * height))
				adfRing.Points.Add(New ESRI.ArcGIS.ADF.Web.Geometry.Point(xmin + sinRot * height, ymin - cosRot * height))
				adfRing.Points.Add(New ESRI.ArcGIS.ADF.Web.Geometry.Point(xmin, ymin))
				Dim adfPolygon As ESRI.ArcGIS.ADF.Web.Geometry.Polygon = New ESRI.ArcGIS.ADF.Web.Geometry.Polygon()
				adfPolygon.Rings.Add(adfRing)

				' Add the rectangle to the graphics layer
				Dim row As System.Data.DataRow = featureGraphicsLayer.Add(adfPolygon)

				' Populate the color, height, and width fields of the graphic feature
				row("Color") = getRandomColor() ' Random color
				row("Height") = randomizer.NextDouble() ' Height between 0 and 1
				row("Width") = System.Convert.ToInt32(randomizer.NextDouble() * 5 + 1) ' Width between 1 and 6
				i += 1
			Loop
			Return featureGraphicsLayer
		End Function

		''' <summary>
		''' Creates the specified number of random polylines within the given extent
		''' </summary>
		''' <param name="layerName">Name of layer</param>
		''' <param name="adfEnvelope">Extent to create features within</param>
		''' <param name="count">Number of features to create</param>
		''' <returns>FeatureGraphicsLayer</returns>
		Public Shared Function CreatePolylineFeatures(ByVal layerName As String, ByVal adfEnvelope As ESRI.ArcGIS.ADF.Web.Geometry.Envelope, ByVal count As Integer) As ESRI.ArcGIS.ADF.Web.Display.Graphics.FeatureGraphicsLayer
			If adfEnvelope Is Nothing Then ' Cannot create features within a null extent
			Return Nothing
			End If

			' Create a feature graphics layer and give it columns for height, width, and color
			Dim featureGraphicsLayer As ESRI.ArcGIS.ADF.Web.Display.Graphics.FeatureGraphicsLayer = New ESRI.ArcGIS.ADF.Web.Display.Graphics.FeatureGraphicsLayer(ESRI.ArcGIS.ADF.Web.FeatureType.Line)
			featureGraphicsLayer.TableName = layerName
			featureGraphicsLayer.Columns.Add("Height", GetType(System.Double))
			featureGraphicsLayer.Columns.Add("Width", GetType(Integer))
			featureGraphicsLayer.Columns.Add("Color", GetType(System.Drawing.Color))

			Dim maxSize As Double = adfEnvelope.Width / 10 ' Maximum width/height of a polygon

			' Create the specified number of polylines, adding each to the graphics layer
			Dim i As Integer = 0
			Do While i < count
				' Calculate the origin of the line as a random point within the specified extent
				Dim x As Double = adfEnvelope.XMin + randomizer.NextDouble() * adfEnvelope.Width
				Dim y As Double = adfEnvelope.YMin + randomizer.NextDouble() * adfEnvelope.Height

				' Create a polyline with 3 - 10 randomly placed vertices
				Dim adfPath As ESRI.ArcGIS.ADF.Web.Geometry.Path = New ESRI.ArcGIS.ADF.Web.Geometry.Path()
				adfPath.Points.Add(New ESRI.ArcGIS.ADF.Web.Geometry.Point(x, y))
				' In the loop control statement, we add three to the random number to ensure the
				' polyline has at least three points
				Dim j As Integer = 0
				Do While j < randomizer.Next(7) + 3
					x += randomizer.NextDouble() * maxSize / 5
					y += randomizer.NextDouble() * maxSize / 5 - maxSize / 10
					adfPath.Points.Add(New ESRI.ArcGIS.ADF.Web.Geometry.Point(x, y))
					j += 1
				Loop
				Dim adfPolyline As ESRI.ArcGIS.ADF.Web.Geometry.Polyline = New ESRI.ArcGIS.ADF.Web.Geometry.Polyline()
				adfPolyline.Paths.Add(adfPath)

				' Add the polyline to the graphics layer
				Dim row As System.Data.DataRow = featureGraphicsLayer.Add(adfPolyline)

				' Populate the color, height, and width fields of the graphic feature
				row("Color") = getRandomColor() ' Random color
				row("Height") = randomizer.NextDouble() ' Height between 0 and 1
				row("Width") = System.Convert.ToInt32(randomizer.NextDouble() * 5 + 1) ' Width between 1 and 6
				i += 1
			Loop
			Return featureGraphicsLayer
		End Function

		''' <summary>
		''' Creates a number of random points within the given extent
		''' </summary>
		''' <param name="layerName">Name of layer</param>
        ''' <param name="adfEnvelope">Extent to create features within</param>
		''' <param name="count">Number of features to create</param>
		''' <returns>FeatureGraphicsLayer</returns>
		Public Shared Function CreatePointFeatures(ByVal layerName As String, ByVal adfEnvelope As ESRI.ArcGIS.ADF.Web.Geometry.Envelope, ByVal count As Integer) As ESRI.ArcGIS.ADF.Web.Display.Graphics.FeatureGraphicsLayer
			If adfEnvelope Is Nothing Then ' Cannot create features within a null extent
			Return Nothing
			End If

			' Create a graphics layer for the points
			Dim featureGraphicsLayer As ESRI.ArcGIS.ADF.Web.Display.Graphics.FeatureGraphicsLayer = New ESRI.ArcGIS.ADF.Web.Display.Graphics.FeatureGraphicsLayer(ESRI.ArcGIS.ADF.Web.FeatureType.Point)
			featureGraphicsLayer.TableName = layerName
			featureGraphicsLayer.Columns.Add("ImagePath", GetType(String))
			featureGraphicsLayer.Columns.Add("RandomName", GetType(String))

			' Create the specified number of points and add them to the graphics layer
			Dim i As Integer = 0
			Do While i < count
				' Create a point with a random geometry within the specified extent and add it to the
				' graphics layer
				Dim x As Double = adfEnvelope.XMin + randomizer.NextDouble() * adfEnvelope.Width
				Dim y As Double = adfEnvelope.YMin + randomizer.NextDouble() * adfEnvelope.Height
				Dim row As System.Data.DataRow = featureGraphicsLayer.Add(New ESRI.ArcGIS.ADF.Web.Geometry.Point(x,y))

				' Populate the ImagePath field with a path that points to 1.gif, 2.gif, or 3.gif.  
				row("ImagePath") = System.String.Format("~/images/{0}.gif", randomizer.Next(3) + 1)
				' Populate the name field with an arbitrary name
				row("RandomName") = RandomNames(randomizer.Next(RandomNames.Length)) 'Random name
				i += 1
			Loop
			Return featureGraphicsLayer
		End Function

		' Array of names to use in assigning a random name
		Private Shared RandomNames As String() = { "Antilocapra americana", "Euarctos americanus", "Cervus canadensis", "Felis concolor", "Pelecanus erythrorhynchos" }

		''' <summary>
		''' Creates a random color
		''' </summary>
		''' <returns>System.Drawing.Color</returns>
		Private Shared Function getRandomColor() As System.Drawing.Color
			Dim rgb As Byte() = New Byte(2){}
			randomizer.NextBytes(rgb)
			Return System.Drawing.Color.FromArgb(rgb(0), rgb(1), rgb(2))
		End Function
	End Class
End Namespace