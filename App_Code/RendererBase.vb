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
	''' Base Renderer for simplifying creating a new renderer
	''' You only need to implement the Render method, unless you want support for legends.
	''' </summary>
	<System.Serializable> _
	Public MustInherit Class RendererBase
        Implements ESRI.ArcGIS.ADF.Web.Display.Renderer.IRenderer
		#Region "IRenderer Members"

        Public Overridable Function GenerateSwatches(ByVal swatchInfo As ESRI.ArcGIS.ADF.Web.Display.Swatch.SwatchInfo, ByVal fileName As String, ByVal minScale As String, ByVal maxScale As String) As ESRI.ArcGIS.ADF.Web.Display.Swatch.SwatchCollection Implements ESRI.ArcGIS.ADF.Web.Display.Renderer.IRenderer.GenerateSwatches
            'Override if needed
            Return New ESRI.ArcGIS.ADF.Web.Display.Swatch.SwatchCollection()
        End Function

        Public Overridable Sub GetAllSymbols(ByVal symbols As System.Collections.Generic.List(Of ESRI.ArcGIS.ADF.Web.Display.Symbol.FeatureSymbol)) Implements ESRI.ArcGIS.ADF.Web.Display.Renderer.IRenderer.GetAllSymbols
            'Override if needed
        End Sub

        Public Overridable Sub GetMaxSwatchDimensions(ByRef width As Integer, ByRef height As Integer) Implements ESRI.ArcGIS.ADF.Web.Display.Renderer.IRenderer.GetMaxSwatchDimensions
            'Override if needed
        End Sub

        Public MustOverride Sub Render(ByVal row As System.Data.DataRow, ByVal graphics As System.Drawing.Graphics, ByVal geometryColumn As System.Data.DataColumn) Implements ESRI.ArcGIS.ADF.Web.Display.Renderer.IRenderer.Render

		#End Region

		#Region "ICloneable Members"

        Public Overridable Function Clone() As Object Implements ESRI.ArcGIS.ADF.Web.Display.Renderer.IRenderer.Clone
            Return TryCast(Me.MemberwiseClone(), RendererBase)
        End Function

		#End Region
	End Class
End Namespace