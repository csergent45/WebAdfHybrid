Imports Microsoft.VisualBasic
Imports System
Imports System.Data
Imports System.Configuration
Imports System.Web
Imports System.Web.Security
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.UI.WebControls.WebParts
Imports System.Web.UI.HtmlControls
Imports ESRI.ArcGIS.ADF.Web
Imports ESRI.ArcGIS.ADF.Web.UI.WebControls
Imports ESRI.ArcGIS.ADF.Web.DataSources
Imports System.Collections.Specialized
Imports ESRI.ArcGIS.ADF.Web.Geometry
Imports ESRI.ArcGIS.ADF.Web.Display.Graphics

Namespace WebMapApp
    '/ <summary>
    '/ Summary description for MapIdentifyControl
    '/ </summary>
    <ToolboxData("<{0}:MapIdentify runat=server></{0}:MapIdentify>")> _
     Public Class MapIdentify
        Inherits ESRI.ArcGIS.ADF.Web.UI.WebControls.WebControl
        ' Column names to exclude from Identify.
        ' "Display Column", "GRAPHICS_ID", "IS_SELECTED" and columns of type ADF geometry are already set by the framework to not be visible.
        ' GraphicsLayer.GetContentsTemplate honors this visibility setting.
        Private m_excludedColumnNames As String() = {"OID", "ObjectID", "#ID#", "#SHAPE#"}
        'likely of type IMS geometry
        Private m_identifyIconUrl As String = "images/identify-map-icon.png"
        Private m_waitIconUrl As String = "images/callbackActivityIndicator2.gif"
        Private m_IdentifyTolerance As Integer = 2 ' tolerance used in identify request... may need to be adjusted to a specific resource type
        Private m_idOption As IdentifyOption = IdentifyOption.VisibleLayers
        Private m_mapBuddyId As String = "Map1"
        Private m_taskResultsId As String = "TaskResults1"
        Private m_numberDecimals As Integer = 3 'number of decimals in coordinate string
        Private m_resourceManger As MapResourceManager
        Public m_id As String '
        Private m_map As Map
        Private m_taskResults As TaskResults
        Private m_isRTL As Boolean = False

        Public Sub New() '
        End Sub 'New


        Protected Overrides Sub OnPreRender(ByVal e As EventArgs)
            MyBase.OnPreRender(e)

            m_id = Me.ClientID
            ' find the map, task results and map resource manager controls
            m_map = Page.FindControl(m_mapBuddyId)
            m_taskResults = FindControlRecursive(Page, m_taskResultsId)
            m_resourceManger = m_map.MapResourceManagerInstance

            Dim sm As ScriptManager = ScriptManager.GetCurrent(Me.Page)
            If Not (sm Is Nothing) Then
                sm.RegisterAsyncPostBackControl(Me) '
            End If
            Dim create As String = [String].Format(ControlChars.Lf + "Sys.Application.add_init(function() {{" + ControlChars.Lf + ControlChars.Tab + "$create(ESRI.ADF.UI.MapIdentifyTool,{{""id"":""{3}"",""uniqueID"":""{0}"",""callbackFunctionString"":""{1}"",""identifyIcon"":""{4}"",""waitIcon"":""{5}""}},null,{{""map"":""{2}""}});" + ControlChars.Lf + ControlChars.Tab + "MapIdentifyTool = function() {{ $find('{3}').startIdentify(); }};" + ControlChars.Lf + " }});" + ControlChars.Lf, Me.UniqueID, Me.CallbackFunctionString, m_map.ClientID, Me.ClientID, m_identifyIconUrl, m_waitIconUrl)
            Page.ClientScript.RegisterStartupScript(Me.GetType(), Me.Id + "_startup", create, True)
        End Sub 'OnPreRender

        Private Function FindControlRecursive(ByVal root As Control, ByVal id As String) As Control
            If root.ID = id Then
                Return root
            End If
            Dim c As Control
            For Each c In root.Controls
                Dim t As Control = FindControlRecursive(c, id)
                If Not (t Is Nothing) Then
                    Return t
                End If
            Next c
            Return Nothing
        End Function 'FindControlRecursive

        Protected Overrides Sub Render(ByVal writer As HtmlTextWriter)
            'support script file
            writer.WriteLine("<script type=""text/javascript"" src=""JavaScript/MapIdentify.js""></script>")
        End Sub 'Render


        '/ <summary>
        '/ Identify layers at Point.
        '/ </summary>
        '/ <param name="map">Map control</param>
        '/ <param name="mapPoint">Map point location</param>
        Public Function PointIdentify(ByVal map As Map, ByVal mapPoint As ESRI.ArcGIS.ADF.Web.Geometry.Point) As CallbackResult
            Dim identifyResults As New System.Collections.ArrayList() 'to send to client
			Dim tbList As System.Collections.Generic.List(Of System.Data.DataTable)
			tbList = New System.Collections.Generic.List(Of System.Data.DataTable)
            'to store on server for "Add to Results" behavior
            Dim identifyResultItem As System.Collections.Generic.Dictionary(Of String, String)
            Dim resource As ESRI.ArcGIS.ADF.Web.DataSources.IGISResource '
            Dim query As ESRI.ArcGIS.ADF.Web.DataSources.IQueryFunctionality
            Dim identifyDataTables As System.Data.DataTable() = Nothing
            Dim tableName As String
            Dim mapFunc As ESRI.ArcGIS.ADF.Web.DataSources.IMapFunctionality
            For Each mapFunc In map.GetFunctionalities() '
                If mapFunc.DisplaySettings.Visible Then
                    resource = mapFunc.Resource
                    query = resource.CreateFunctionality(GetType(ESRI.ArcGIS.ADF.Web.DataSources.IQueryFunctionality), "identify_")

                    If Not (query Is Nothing) AndAlso query.Supports("identify") Then
                        Try
                            identifyDataTables = query.Identify(mapFunc.Name, mapPoint, m_IdentifyTolerance, m_idOption, Nothing)
                        Catch
                            identifyDataTables = Nothing
                        End Try
                        'return new CallbackResult(this, "error", "Cannot identify");
                        If Not (identifyDataTables Is Nothing) Then
                            If identifyDataTables.Length > 0 Then
                                Dim index As Integer
                                For index = 0 To identifyDataTables.Length - 1
                                    Dim identifyTable As System.Data.DataTable = identifyDataTables(index)
                                    tableName = identifyTable.ExtendedProperties(ESRI.ArcGIS.ADF.Web.Constants.ADFLayerName)
                                    If String.IsNullOrEmpty(tableName) Then
                                        tableName = identifyDataTables(index).TableName
                                    End If
                                    Dim layerID As String = identifyTable.ExtendedProperties(ESRI.ArcGIS.ADF.Web.Constants.ADFLayerID)
                                    Dim layerFormat As LayerFormat = Nothing '
                                    Dim formattedTable As DataTable = identifyTable
                                    If Not String.IsNullOrEmpty(layerID) Then
                                        layerFormat = layerFormat.FromMapResourceManager(map.MapResourceManagerInstance, mapFunc.Resource.Name, layerID)
                                        If Not (layerFormat Is Nothing) Then
                                            Dim layer As ESRI.ArcGIS.ADF.Web.Display.Graphics.GraphicsLayer = ESRI.ArcGIS.ADF.Web.Converter.ToGraphicsLayer(identifyTable, System.Drawing.Color.Empty, System.Drawing.Color.Aqua, System.Drawing.Color.Red, True)
                                            If Not (layer Is Nothing) Then
                                                layerFormat.Apply(layer)
                                                formattedTable = layer
                                            End If
                                        End If
                                    End If
                                    Dim contentsTemplate As String = ESRI.ArcGIS.ADF.Web.Display.Graphics.GraphicsLayer.GetContentsTemplate(formattedTable, False, System.Drawing.Color.Empty, True, m_excludedColumnNames)
                                    Dim titleTemplate As String = ESRI.ArcGIS.ADF.Web.Display.Graphics.GraphicsLayer.GetTitleTemplate(formattedTable, False)
                                    If Not (layerFormat Is Nothing) Then
                                        If m_excludedColumnNames.Length > 0 Then
                                            layerFormat = layerFormat.Clone() 'clone layer format so that we do not change the layer format stored in the map resource manager
                                            'exclude column names and get template with field names instead of indices
                                            layerFormat.Contents = ESRI.ArcGIS.ADF.Web.Display.Graphics.GraphicsLayer.GetContentsTemplate(formattedTable, True, System.Drawing.Color.LightGray, True, m_excludedColumnNames)
                                        End If

                                    End If
                                    Dim rowCollection As System.Data.DataRowCollection = formattedTable.Rows
                                    If Not (rowCollection Is Nothing) Then
                                        If rowCollection.Count > 0 Then '
                                            Dim roundFactor As Double = Math.Pow(10, m_numberDecimals)
                                            Dim rtlString As String = ""
                                            If (m_isRTL) Then rtlString = "&lrm;"
                                            Dim pointXString As String = rtlString & Convert.ToString((Math.Round((mapPoint.X * roundFactor)) / roundFactor))
                                            Dim pointYString As String = rtlString & Convert.ToString((Math.Round((mapPoint.Y * roundFactor)) / roundFactor))
                                            Dim row As Integer
                                            For row = 0 To rowCollection.Count - 1
                                                Dim gds As New System.Data.DataSet()

                                                identifyResultItem = New System.Collections.Generic.Dictionary(Of String, String)
                                                Dim rowItems As Object() = formattedTable.Rows(row).ItemArray '

                                                Dim title As String = String.Format(titleTemplate, rowItems)
                                                Dim contents As String = String.Format(contentsTemplate, rowItems)

                                                Dim identifyTitle As String = title
                                                If Not identifyTitle.ToLower().Contains("<b>") AndAlso Not identifyTitle.ToLower().Contains("<strong>") Then '
                                                    identifyTitle = "<b>" + identifyTitle + "</b>"
                                                End If

                                                identifyResultItem("title") = identifyTitle
                                                identifyResultItem("contents") = contents
                                                identifyResultItem("layer") = tableName
                                                identifyResultItem("resource") = mapFunc.Resource.Name
                                                'table name is title + table name
                                                Dim rowName As String = title + " (" + tableName + ")" '
                                                Dim rowTable As DataTable = rowToTable(formattedTable.Rows(row), rowName)
                                                Dim layer As ESRI.ArcGIS.ADF.Web.Display.Graphics.GraphicsLayer = ESRI.ArcGIS.ADF.Web.Converter.ToGraphicsLayer(rowTable, System.Drawing.Color.Empty, System.Drawing.Color.Aqua, System.Drawing.Color.Red, True)
												Dim dsName As String = String.Format("{0}   {1}, {2}", rowName, pointXString, pointYString)

												'Add datatable for this result item to list of datatables to be stored in session
												If Not (layer Is Nothing) Then '
													layer.RenderOnClient = True
													' Add Display Name
													If (Not layer.ExtendedProperties.Contains("displayName")) Then
														layer.ExtendedProperties.Add("displayName", dsName)
													Else
														layer.ExtendedProperties("displayName") = dsName
													End If


													If Not (layerFormat Is Nothing) Then
														layerFormat.Apply(layer)
													End If
													tbList.Add(layer)
												Else
                                                    ' Add Display Name
                                                    If ((Not layer Is Nothing) AndAlso Not layer.ExtendedProperties.Contains("displayName")) Then
                                                        rowTable.ExtendedProperties.Add("displayName", dsName)
                                                    Else
                                                        rowTable.ExtendedProperties("displayName") = dsName
                                                    End If
													tbList.Add(rowTable)
												End If
													identifyResults.Add(identifyResultItem)	'Store each layer feature
                                            Next row '
                                        End If
                                    End If
                                Next index '
                            End If
                        End If '
                    End If
                End If '
            Next mapFunc '

			System.Web.HttpContext.Current.Session.Add("WebAppIdentifyDataTables", tbList.ToArray())

            'Note: arrays and dictionaries are handled natively by the JSON serialization framework
            Return New CallbackResult(Me, "mappoint", mapPoint.X, mapPoint.Y, identifyResults) '
        End Function 'PointIdentify

        Private Function rowToTable(ByVal row As DataRow, ByVal tableName As [String]) As System.Data.DataTable
            Dim table As New System.Data.DataTable(tableName)
            Dim t As DataTable = row.Table
            Dim i As Integer
            For i = 0 To t.Columns.Count - 1
                Dim c As DataColumn = t.Columns(i)
                Dim column As New DataColumn(c.ColumnName, c.DataType)
                table.Columns.Add(column)
            Next i

            Dim r As DataRow = table.NewRow()
            For i = 0 To table.Columns.Count - 1
                r(i) = row(i)
            Next i
            table.Rows.Add(r)
            Return table
        End Function 'rowToTable


        '/ <summary>
        '/ Whether map has at least one resource that supports identify
        '/ </summary>
        '/ <returns></returns>
        Public Function SupportsIdentify() As Boolean
            Dim resource As ESRI.ArcGIS.ADF.Web.DataSources.IGISResource
            Dim query As ESRI.ArcGIS.ADF.Web.DataSources.IQueryFunctionality
            Dim mapFunc As ESRI.ArcGIS.ADF.Web.DataSources.IMapFunctionality
            For Each mapFunc In m_map.GetFunctionalities()
                Try
                    resource = mapFunc.Resource
                    query = resource.CreateFunctionality(GetType(ESRI.ArcGIS.ADF.Web.DataSources.IQueryFunctionality), "identify_")
                    If Not (query Is Nothing) And query.Supports("Identify") Then
                        Return True
                    End If
                Catch
                End Try
            Next mapFunc
            Return False
        End Function 'SupportsIdentify


        Public Sub AddToTaskResults(ByVal index As Integer)
            If m_taskResults Is Nothing Then
                m_taskResults = FindControlRecursive(Page, m_taskResultsId)
			End If
			Dim dsName As String = "No Name Found"
            If Not (m_taskResults Is Nothing) Then
				Dim tbList As System.Data.DataTable() = System.Web.HttpContext.Current.Session("WebAppIdentifyDataTables")
				If Not (tbList Is Nothing) AndAlso tbList.Length > 0 Then
					Dim tb As System.Data.DataTable = tbList(index)
					If (tb.ExtendedProperties.Contains("displayName")) Then
						dsName = tb.ExtendedProperties("displayName").ToString()
					End If
					Dim tr As TaskResultNode = Nothing
					If (TypeOf tb Is GraphicsLayer) Then
						Dim gds As GraphicsDataSet = New GraphicsDataSet()
						gds.DataSetName = dsName
						gds.Tables.Add(tb)
						tr = m_taskResults.CreateTaskResultNode(Nothing, Nothing, Nothing, gds, False, True)
					Else
						Dim ds As DataSet = New DataSet()
						ds.Tables.Add(tb)
						ds.DataSetName = dsName
						tr = m_taskResults.CreateTaskResultNode(Nothing, Nothing, Nothing, ds, False, True)
					End If
					tr.Expanded = True
					m_taskResults.DisplayResults(Nothing, Nothing, Nothing, tr)
					Me.CallbackResults.CopyFrom(m_taskResults.CallbackResults)
				End If
			End If
        End Sub 'AddToTaskResults

        Public Property Id() As String '
            Get
                Return m_id
            End Get
            Set(ByVal value As String)
                m_id = value
            End Set
        End Property


        Private Property MapResourceManager() As MapResourceManager
            Get
                Return m_resourceManger
            End Get
            Set(ByVal value As MapResourceManager)
                m_resourceManger = value
            End Set
        End Property
        '/ <summary>
        '/ Id of Buddy MapControl
        '/ </summary>

        Public Property MapBuddyId() As String
            Get
                Return m_mapBuddyId
            End Get
            Set(ByVal value As String)
                m_mapBuddyId = value
            End Set
        End Property
        '/ <summary>
        '/ Id of TaskResults Control
        '/ </summary>

        Public Property TaskResultsId() As String
            Get
                Return m_taskResultsId
            End Get
            Set(ByVal value As String)
                m_taskResultsId = value
            End Set
        End Property

        Public Property NumberDecimals() As Integer
            Get
                Return m_numberDecimals
            End Get
            Set(ByVal value As Integer)
                m_numberDecimals = value
            End Set
        End Property

 

        Public Overrides Function GetCallbackResult() As String '
            Dim resultsString As String = ""
            Dim responseString As String = _callbackArg
            ' break out the responseString into a querystring
            Dim keyValuePairs As Array = responseString.Split("&".ToCharArray())
            Dim m_queryString As New NameValueCollection()
            Dim keyValue() As String
            Dim response As String = ""
            If keyValuePairs.Length > 0 Then
                Dim i As Integer
                For i = 0 To keyValuePairs.Length - 1
                    keyValue = keyValuePairs.GetValue(i).ToString().Split("=".ToCharArray())
                    m_queryString.Add(keyValue(0), keyValue(1))
                Next i
            Else
                keyValue = responseString.Split("=".ToCharArray())
                If keyValue.Length > 0 Then
                    m_queryString.Add(keyValue(0), keyValue(1))
                End If
            End If
            Dim map As Map = Page.FindControl(Me.m_mapBuddyId)
            Dim mode As String = m_queryString("mode")
            Dim rtlString As String = m_queryString("dir")
            If Not rtlString Is Nothing AndAlso rtlString = "rtl" Then m_isRTL = True
            Select Case mode
                Case "identify"
                    Dim xyString As String = m_queryString("coords")
                    Dim xy As String() = xyString.Split(Char.Parse(":"))
                    Dim mapPoint As Point = New Point(Convert.ToDouble(xy(0), System.Globalization.CultureInfo.InvariantCulture), Convert.ToDouble(xy(1), System.Globalization.CultureInfo.InvariantCulture))
                    mapPoint.SpatialReference = map.SpatialReference
                    Dim cResponse As CallbackResult = PointIdentify(map, mapPoint)
                    CallbackResults.Add(cResponse)
                Case "addresults"
                    Dim indexString As String = m_queryString("index")
                    Dim index As Integer = Convert.ToInt32(indexString)
                    AddToTaskResults(index)
            End Select

            Return MyBase.GetCallbackResult()
        End Function 'GetCallbackResult
    End Class 'MapIdentify '
End Namespace 'WebMapApp
