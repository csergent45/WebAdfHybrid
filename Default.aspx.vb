Imports ESRI.ArcGIS.ADF.Web.Converter

'Imports For Working With ESRI Web ADF DataScources
Imports ESRI.ArcGIS.ADF.Web.DataSources
Imports ESRI.ArcGIS.ADF.Web.DataSources.Graphics

'Imports For Displaying Swatch Symbols In The ESRI Web ADF Table Of Contents
Imports ESRI.ArcGIS.ADF.Web.Display.Swatch
Imports ESRI.ArcGIS.ADF.Web.Display.Swatch.SwatchCollection
Imports ESRI.ArcGIS.ADF.Web.Display.Swatch.SwatchUtility


'Imports For Displaying Symbology On The ESRI Web ADF Map
Imports ESRI.ArcGIS.ADF.Web.Display.Graphics
Imports ESRI.ArcGIS.ADF.Web.Display.Graphics.FeatureGraphicsLayer
Imports ESRI.ArcGIS.ADF.Web.Display.Renderer 'IRenderer Interface For Renderering ESRI Web ADF Custom Graphics 
Imports ESRI.ArcGIS.ADF.Web.Display.Symbol
Imports ESRI.ArcGIS.ADF.Web.Display.Symbol.RasterMarkerSymbol

'Imports For Using ESRI Web ADF Geometry Coordinates
Imports ESRI.ArcGIS.ADF.Web.Geometry
Imports ESRI.ArcGIS.ADF.Web.Geometry.Point

'Imports For Creating ESRI Web ADF Web Controls
Imports ESRI.ArcGIS.ADF.Web.UI.WebControls
Imports ESRI.ArcGIS.ADF.Web.UI.WebControls.Toc
Imports ESRI.ArcGIS.ADF.Web.UI.WebControls.TreeViewPlusNode
Imports ESRI.ArcGIS.ADF.Web.CartoImage


Imports System.Collections.Generic
Imports System.Configuration

'Imports For Querying In Db2
Imports System.Data
Imports System.Data.OleDb.OleDbConnection
Imports System.Data.SqlClient

Imports System.Diagnostics
Imports System.Drawing
Imports System.Drawing.Graphics
Imports System.Exception
Imports System.Linq
Imports System.Object
Imports System.SystemException
Imports System.Text

Imports System.Web.Handlers.ScriptResourceHandler
Imports System.Web.HttpContext
Imports System.Uri
Imports System.Net
Imports System.IO


Imports CityApps.DecaturIl.Mapping

Imports System.Collections
Imports System.ComponentModel
Imports System.Web
Imports System.Web.SessionState
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.UI.HtmlControls
Imports ESRI.ArcGIS.ADF.Web

Imports System.Xml




Partial Class WebMapApplication
    Inherits System.Web.UI.Page

    Private _rowResultsGraphicsLayer, _crimeStats, _resultsGraphicsLayer As FeatureGraphicsLayer
    Private _featureGraphicsLayer As ESRI.ArcGIS.ADF.Web.Display.Graphics.FeatureGraphicsLayer

    Private _contentsText, _param, _appPath, _legendImage As String
    Private SelectInvalid, qryMap, qryRandParam, qryIP_Address As String
    Private P, IP, userIP_Address, FEATURE_ID As String
    Private mapable, param, random, identifier, caseType As String
    Private type, crimeType, crimeName, crimeLevel, crimeInfo As String
    Private _layerName, _imgPath, _applicationDesc As String
    Private _featureId, _isSelected As String
    Private _objectId, _ipAddress, _mapable, _randParam As String

    Private _qryStart As String


    Private _crimeResultsDataTable, _resultsDataTable As System.Data.DataTable

    ' Potential inital load. Check if graphics resource has the graphics layer, and if not, create it
    Private mapResourceItem As ESRI.ArcGIS.ADF.Web.UI.WebControls.MapResourceItem
    Private graphicsMapResource As ESRI.ArcGIS.ADF.Web.DataSources.Graphics.MapResource

    Private crimeStatsColumn As Integer

    Private _appStatus As String


    Private _reqSeqNum As String
    Private _featureDesc, _caseQuantity, _featureType, _caseType As String

    Private _pt As New ESRI.ArcGIS.ADF.Web.Geometry.Point

    Private qty As Integer

    Private typeflag As Boolean = True



    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load

        Dim crimeStats As New ESRI.ArcGIS.ADF.Web.Display.Graphics.FeatureGraphicsLayer(ESRI.ArcGIS.ADF.Web.FeatureType.Point)

        mapResourceItem = MapResourceManager1.ResourceItems.Find("Selection")
        graphicsMapResource = TryCast(mapResourceItem.Resource, ESRI.ArcGIS.ADF.Web.DataSources.Graphics.MapResource)


        If Not Page.IsCallback And Not Page.IsPostBack Then
            If Map1.MapResourceManager Is Nothing Or Map1.MapResourceManager.Length = 0 Then
                callErrorPage("No MapResourceManager defined for the map.", Nothing)
            End If
            If MapResourceManager1.ResourceItems.Count = 0 Then
                callErrorPage("The MapResourceManager does not have a valid ResouceItem Definition.", Nothing)
            ElseIf MapResourceManager1.ResourceItems(0) Is Nothing Then
                callErrorPage("The MapResourceManager does not have a valid ResouceItem Definition.", Nothing)
            End If
        End If


        If Map1.Extent Is Nothing Then
            ' Forces earlier initialization of map and will set map extent
            Dim primaryMapResource As ESRI.ArcGIS.ADF.Web.DataSources.IMapResource = Map1.PrimaryMapResourceInstance
        End If

        '   Variable               Definition
        '---------------        ----------------------------------------------------------------------
        '   userIP_Address      Returns the querying users IP Address
        '   qryRandParam        Returns value to set as parameter
        '   qryAppName          Returns the name of the application running the query
        '   qryFEATURE_ID        Returns the unique ID of feature to be used parameter in geo-query
        '   qryFeatureType      Returns the type of geography to be queried
        '   qryFeatureDesc      Returns the description of the geography to be queried
        '   qryMultiFeature     Returns multiple addresses from a multi-select query
        '
        '*********************************************************************************************
        '                   PREPARING DATA EXTRACTION
        '*********************************************************************************************

        'Get the users IP Address for use as query parameter
        'userIP_Address = Server.HtmlEncode(Request.UserHostName)
        userIP_Address = "192.168.100.227"

        Select Case Server.MachineName
            Case "CVCMS116"
                _appPath = "http://cvcms116/pbtest/Images/"
            Case "UNIBEAST"
                _appPath = "http://unibeast/cityapps/Images/"
            Case Else
                Exit Select
        End Select

        'Set the parameter based on the random number
        param = Request.QueryString("sender")


        'Define database connection
        Dim conn As Data.OleDb.OleDbConnection
        Dim comm As Data.OleDb.OleDbCommand


        'Define database
        P = "PRODGEN"


        'Define query table
        'August 4, 2010
        'Change query to look at view to display data on map.
        'qryMap = "GENINFO.GIS_MAP_WEB_VIEW"  This view may be used later if graphics are stored in the database.
        qryMap = "GENINFO.GIS_MAP_WEB"

        'Fields used to return data
        Dim application, feature As String

        Dim Valid, Invalid As String
        Dim drawMap As String

        'Defines valid records that can be displayed on the map
        Valid = "MAPABLE = 'Y'"
        'Defines invalid records that can not be displayed on the map
        Invalid = "MAPABLE ='N'"

        IP = "IP_ADDRESS"
        application = "APPLICATION_DESC"
        feature = "FEATURE_DESC"
        FEATURE_ID = "FEATURE_ID"
        random = "RAND_PARAM"
        mapable = "MAPABLE"
        identifier = "REQ_SEQ_NUM"
        drawMap = "DRAW_MAP"
        _qryStart = "QRY_START"


        'Select data to be queried; returns data that can be mapped and data that can not be mapped as well
        Dim SelectData As String
        SelectData = IP & "," & application & "," & feature & "," & FEATURE_ID & "," & random & "," & mapable & "," & identifier & "," & drawMap & "," & _qryStart

        'Select invalid data that can not be mapped
        'Dim SelectInvalid As String
        SelectInvalid = "SELECT " & SelectData & " FROM " & qryMap & " WHERE " & Invalid & " AND "

        'Select valid data that is can be mapped
        Dim SelectValid As String
        SelectValid = "SELECT " & SelectData & " FROM " & qryMap & " WHERE " & Valid & " AND "

        'Define method to read data
        Dim reader As Data.OleDb.OleDbDataReader


        'Define conection string parameters
        Dim connectionString As String = ConfigurationManager.ConnectionStrings(P).ConnectionString

        'Instantiate connection
        conn = New Data.OleDb.OleDbConnection(connectionString)

        'Instantiate command
        comm = New Data.OleDb.OleDbCommand(SelectValid & IP & "= '" & userIP_Address & "' AND " & random & "= " & param, conn)

        'Open connection, execute reader and read data
        conn.Open()
        reader = comm.ExecuteReader()
        reader.Read()

        'Define the value of the variables
        Dim qryAppName, qryFEATURE_ID, qryFeatureDesc, qryMapable, qryIdentifier, qryDrawMap As String

        qryIP_Address = reader(IP).ToString
        qryRandParam = reader(random)
        qryAppName = reader(application).ToString
        qryFEATURE_ID = reader(FEATURE_ID).ToString
        qryFeatureDesc = reader(feature).ToString
        qryMapable = reader(mapable).ToString
        qryIdentifier = reader(identifier)
        qryDrawMap = reader(drawMap)
        txtOriginalQuery.Text = reader(_qryStart)

        lbl99.Text = reader(drawMap).ToString

        Title = "Generating Map for " & qryAppName

        'Set application icon
        Dim appIcon As String

        'START INVALID ADDRESS CHECK
        Select Case qryAppName

            Case "City Owned Property"
                appIcon = "tree.ico"
                Crime_Legend_Panel.Visible = False
                RunParcel()
                Exit Select
            Case "Address Manager"
                appIcon = "envelope.ico"
                Crime_Legend_Panel.Visible = False
                RunParcel()
                Exit Select
            Case "Code Enforcement"
                appIcon = "House2.ico"
                Crime_Legend_Panel.Visible = False
                RunParcel()
                Exit Select
            Case "Weed Tracking"
                appIcon = "Weed.ico"
                Crime_Legend_Panel.Visible = False
                RunParcel()
                Exit Select
            Case "Citizen Response"
                appIcon = "CAL4.ico"
                Crime_Legend_Panel.Visible = False
                RunParcel()
                Exit Select
            Case "Building Permits"
                appIcon = "HAMMER.ico"
                Crime_Legend_Panel.Visible = False
                RunParcel()
                Exit Select
            Case "SOT"
                appIcon = "map.ico"
                Crime_Legend_Panel.Visible = False
                RunParcel()
                Exit Select
            Case "Work Orders"
                appIcon = "buldozer.ico"
                Crime_Legend_Panel.Visible = False
                RunParcel()
                Exit Select
            Case "Crime Analysis"
                appIcon = "hand_cuffs.ico"
                Crime_Legend_Panel.Visible = True

                'Define database connection
                Dim conn2 As Data.OleDb.OleDbConnection
                Dim comm2 As Data.OleDb.OleDbCommand

                'Define method to read data
                Dim reader2 As Data.OleDb.OleDbDataReader

                'Define conection string parameters
                Dim connectionString2 As String = ConfigurationManager.ConnectionStrings(P).ConnectionString

                'Instantiate connection
                conn2 = New Data.OleDb.OleDbConnection(connectionString2)

                'Instantiate command
                comm2 = New Data.OleDb.OleDbCommand(SelectInvalid & IP & "= '" & userIP_Address & "' AND " & random & "= " & param, conn2)

                Dim dataSet2 As New DataSet
                Dim adapter2 As New Data.OleDb.OleDbDataAdapter

                adapter2 = New Data.OleDb.OleDbDataAdapter(SelectInvalid & IP & "= '" & qryIP_Address & "' AND " & random & "= " & qryRandParam, conn2)
                adapter2.Fill(dataSet2, qryMap)


                'Change this to count only the invalid addresses
                Dim commCount2 As Data.OleDb.OleDbCommand
                commCount2 = New Data.OleDb.OleDbCommand("SELECT DISTINCT COUNT(" & qryMap & "." & FEATURE_ID & ") AS MySelection FROM " & qryMap & " WHERE " & IP & "= '" & qryIP_Address & "' AND " & random & " = " & qryRandParam & " AND " & mapable & " = 'N'", conn2)
                conn2.Open()

                reader2 = commCount2.ExecuteReader
                reader2.Read()


                If reader2("MySelection") > 0 Then
                    AddressPanelNone.Visible = True
                Else
                    AddressPanelNone.Visible = False
                End If

                If reader2("MySelection") > 15 Then
                    Panel1.Height = 150
                End If
                conn2.Close()
                gridAddressesNoMap.DataSource = dataSet2
                gridAddressesNoMap.DataBind()
                Exit Select
            Case "Investigation Reports"
                appIcon = "hand_cuffs.ico"

                'Define database connection
                Dim conn2 As Data.OleDb.OleDbConnection
                Dim comm2 As Data.OleDb.OleDbCommand

                'Define method to read data
                Dim reader2 As Data.OleDb.OleDbDataReader

                'Define conection string parameters
                Dim connectionString2 As String = ConfigurationManager.ConnectionStrings(P).ConnectionString

                'Instantiate connection
                conn2 = New Data.OleDb.OleDbConnection(connectionString2)

                'Instantiate command
                comm2 = New Data.OleDb.OleDbCommand(SelectInvalid & IP & "= '" & userIP_Address & "' AND " & random & "= " & param, conn2)

                Dim dataSet2 As New DataSet
                Dim adapter2 As New Data.OleDb.OleDbDataAdapter

                adapter2 = New Data.OleDb.OleDbDataAdapter(SelectInvalid & IP & "= '" & qryIP_Address & "' AND " & random & "= " & qryRandParam, conn2)
                adapter2.Fill(dataSet2, qryMap)

                'Change this to count only the invalid addresses
                Dim commCount2 As Data.OleDb.OleDbCommand
                commCount2 = New Data.OleDb.OleDbCommand("SELECT DISTINCT COUNT(" & qryMap & "." & FEATURE_ID & ") AS MySelection FROM " & qryMap & " WHERE " & IP & "= '" & qryIP_Address & "' AND " & random & " = " & qryRandParam & " AND " & mapable & " = 'N'", conn2)
                conn2.Open()

                reader2 = commCount2.ExecuteReader
                reader2.Read()


                If reader2("MySelection") > 0 Then
                    AddressPanelNone.Visible = True
                Else
                    AddressPanelNone.Visible = False
                End If

                If reader2("MySelection") > 15 Then
                    Panel1.Height = 150
                End If
                conn2.Close()
                gridAddressesNoMap.DataSource = dataSet2
                gridAddressesNoMap.DataBind()
                Exit Select
            Case Else
                appIcon = "globe.ico"
                Exit Select
        End Select
        'END INVALID ADDRESS CHECK

        'MAP VALID GRAPHICS
        With LinkFavIcon
            .Attributes.Add("rel", "SHORTCUT ICON")
            .Attributes.Add("href", _appPath & appIcon)
        End With

        qryRandParam = CInt(reader(random))

        Dim dataSet As New DataSet

        Dim adapter As New Data.OleDb.OleDbDataAdapter

        conn.Close()


        adapter = New Data.OleDb.OleDbDataAdapter("SELECT IP_ADDRESS, RAND_PARAM, APPLICATION_DESC, FEATURE_ID, FEATURE_TYPE,SUM(CASE_QUANTITY) AS Cases, MIN(FEATURE_DESC) AS FEATURE_DESC FROM PRODGEN.GENINFO.GIS_MAP_WEB WHERE (MAPABLE = 'Y') AND (" & qryRandParam & " = " & random & ") AND (IP_ADDRESS = '" & qryIP_Address & "') GROUP BY IP_ADDRESS,RAND_PARAM, APPLICATION_DESC, FEATURE_ID, FEATURE_TYPE", conn)

        adapter.Fill(dataSet, mapable)


        gridAddresses.DataSource = dataSet

        gridAddresses.DataBind()

        'Change this to count only the valid addresses
        'Distinct has been removed to evaluate count of addresses; I think this was required, but do
        'not recall why.
        Dim commCount As Data.OleDb.OleDbCommand
        commCount = New Data.OleDb.OleDbCommand("SELECT COUNT(" & qryMap & "." & FEATURE_ID & ") AS MySelection FROM " & qryMap & " WHERE " & IP & "= '" & qryIP_Address & "' AND " & random & " = " & qryRandParam & " AND " & mapable & " = 'Y'", conn)

        conn.Open()

        reader = commCount.ExecuteReader()
        reader.Read()


        If reader("MySelection") > 25 Then
            MyAddressPanel.Height = 250
        End If

        'Evaluating if only one address has been requested
        If Request.QueryString(FEATURE_ID) <> "ALL" Then
            If reader("MySelection") > 1 Then
                lnkAllAddresses.Visible = True
            Else
                lnkAllAddresses.Visible = False
            End If

            reader = comm.ExecuteReader()

            reader.Read()

            'Initial PIN
            Dim myFeatureDesc As String

            myIP_Address.Text = reader(IP).ToString()

            txtFeature.Text = Request.QueryString(feature).ToString

            txtAppName.Text = reader(application).ToString()


            txtRandParam.Text = reader(random).ToString()

            Title = reader(application.ToString()) & " Search Results For " & Request.QueryString(feature).ToString

            With PrintTask1.TitleSettings
                .DisplayInDialog = True
                .DefaultValue = reader(application.ToString()) & " Search Results For " & Request.QueryString(feature).ToString
            End With


            'If the application is Crime Analysis, check the Address Code.
            If txtAppName.Text = "Crime Analysis" Or txtAppName.Text = "Investigation Reports" Then
                myFeatureDesc = "FEATURE_ID= '" & Request.QueryString(FEATURE_ID).ToString() & "' AND " & "RAND_PARAM= " & reader(random).ToString
                _legendImage = "Images/Cuffs.png"
            Else
                myFeatureDesc = "PIN= '" & Request.QueryString(FEATURE_ID).ToString() & "'"
                _legendImage = "Images/parcel.gif"
            End If


            myParameter.Text = myFeatureDesc.ToString



            _contentsText = txtFeature.Text

        Else

            reader = comm.ExecuteReader()
            reader.Read()

            myIP_Address.Text = reader(IP).ToString()

            txtReqSeqNum.Text = reader(identifier)
            txtFeature.Text = reader(feature).ToString()

            txtAppName.Text = "Selected " & reader(application).ToString()

            txtRandParam.Text = reader(random).ToString()

            Title = reader(application.ToString())

            With PrintTask1.TitleSettings
                .DisplayInDialog = True
                .DefaultValue = reader(application.ToString()) & " Search Results"
            End With


            'Initial PIN
            Dim myFeatureDesc As String
            If txtAppName.Text = "Selected Crime Analysis" Or txtAppName.Text = "Selected Investigation Reports" Then
                myFeatureDesc = "FEATURE_ID= '" & reader(FEATURE_ID).ToString() & "' AND " & "REQ_SEQ_NUM= " & reader(identifier) & " AND " & "RAND_PARAM= " & reader(random).ToString
                _legendImage = "Images/Cuffs.png"
            Else
                myFeatureDesc = "PIN= '" & reader(FEATURE_ID).ToString() & "'"
                _legendImage = "Images/parcel.gif"
            End If

            'Concatenates the rest of the query for the Where Clause
            While reader.Read()
                If txtAppName.Text = "Selected Crime Analysis" Or txtAppName.Text = "Selected Investigation Reports" Then
                    txtMultiFeature.Text &= " OR FEATURE_ID= '" & reader(FEATURE_ID).ToString() & "' AND " & "REQ_SEQ_NUM= " & reader(identifier) & " AND " & "RAND_PARAM= " & reader(random).ToString
                Else
                    txtMultiFeature.Text &= " OR PIN= '" & reader.Item(FEATURE_ID) & "'"

                End If
            End While

            Dim qryMultiFeatures As String

            qryMultiFeatures = txtMultiFeature.Text

            myParameter.Text = myFeatureDesc.ToString & " " & qryMultiFeatures

            _contentsText = txtAppName.Text


        End If



        reader.Close()
        conn.Close()

        txtQuery.Text = myParameter.Text


        If Not Page.IsPostBack Then
            Query(Map1.Extent)
        End If



        'Error Page not currently used
        'Catch ex As Exception
        '    Page.Response.Redirect("NoData.aspx")
        'End Try


    End Sub 'Page_Load

    'Checks invalid Parcels
    Private Sub RunParcel()

        'Define database connection
        Dim conn2 As Data.OleDb.OleDbConnection
        Dim comm2 As Data.OleDb.OleDbCommand


        'Define method to read data
        Dim reader2 As Data.OleDb.OleDbDataReader

        'Define conection string parameters
        Dim connectionString2 As String = ConfigurationManager.ConnectionStrings(P).ConnectionString

        'Instantiate connection
        conn2 = New Data.OleDb.OleDbConnection(connectionString2)


        'Instantiate command
        comm2 = New Data.OleDb.OleDbCommand(SelectInvalid & IP & "= '" & userIP_Address & "' AND " & random & "= " & param, conn2)

        Dim dataSet2 As New DataSet
        Dim adapter2 As New Data.OleDb.OleDbDataAdapter

        Dim commCount2 As Data.OleDb.OleDbCommand
        commCount2 = New Data.OleDb.OleDbCommand("SELECT DISTINCT COUNT(" & qryMap & "." & FEATURE_ID & ") AS MySelection FROM " & qryMap & " WHERE " & IP & "= '" & qryIP_Address & "' AND " & random & " = " & qryRandParam & " AND " & mapable & " = 'N'", conn2)
        conn2.Open()

        reader2 = commCount2.ExecuteReader
        reader2.Read()

        If reader2("MySelection") > 0 Then
            AddressPanelNone.Visible = True
            adapter2 = New Data.OleDb.OleDbDataAdapter(SelectInvalid & IP & "= '" & qryIP_Address & "' AND " & random & "= " & qryRandParam, conn2)
            adapter2.Fill(dataSet2, qryMap)
            gridAddressesNoMap.DataSource = dataSet2
            gridAddressesNoMap.DataBind()
        Else
            AddressPanelNone.Visible = False
            conn2.Close()
            Exit Sub
        End If

        If reader2("MySelection") > 15 Then
            Panel1.Height = 150
        End If
        conn2.Close()

    End Sub
    Protected Sub Query(ByVal mapExtent As ESRI.ArcGIS.ADF.Web.Geometry.Geometry)

        'Declare mapFunc to hold the map functionality
        'Get the map functionality for the resource item and
        'store the result in the variable mapFunc
        If Not MapResourceManager1.Initialized Then
            MapResourceManager1.Initialize()
        End If

        Dim mapFunc As ESRI.ArcGIS.ADF.Web.DataSources.IMapFunctionality
        mapFunc = Map1.GetFunctionality("IntranetVector")

        'Get the resource property of the functionality
        'Store the result in gisResource
        Dim gisResource As ESRI.ArcGIS.ADF.Web.DataSources.IGISResource
        gisResource = mapFunc.Resource()

        'Determine whether the functionality is supported
        Dim supported As Boolean
        supported = gisResource.SupportsFunctionality( _
            GetType(ESRI.ArcGIS.ADF.Web.DataSources.IQueryFunctionality))

        If Not supported Then
            Exit Sub
        End If

        Dim qFunc As ESRI.ArcGIS.ADF.Web.DataSources.IQueryFunctionality
        qFunc = _
          CType(gisResource.CreateFunctionality( _
            GetType(ESRI.ArcGIS.ADF.Web.DataSources.IQueryFunctionality), _
              Nothing),  _
                ESRI.ArcGIS.ADF.Web.DataSources.IQueryFunctionality)



        Dim ids() As String = Nothing 'Layer IDs
        Dim names() As String = Nothing 'Layer names

        'Return the layers that can be queried
        'in the variable qFunc
        qFunc.GetQueryableLayers(Nothing, ids, names)


        'Evaluate application that opened web page
        Dim queryLayer As String = Nothing

        'Determines if it should query the address or parcel layer.
        If txtAppName.Text = "Selected Crime Analysis" Or txtAppName.Text = "Crime Analysis" Or txtAppName.Text = "Selected Investigation Reports" Or txtAppName.Text = "Investigation Reports" Then
            queryLayer = "CrimeLocs_view"
        Else
            queryLayer = "Macon Co. Tax Parcels"
        End If

        'Identify all layers for use in query
        'Check which layer is the crime layer
        Dim crimes As Integer = 0

        Do While crimes < ids.Length
            If names(crimes) = "CrimeLocs_view" Then
                ids(crimes).ToString()
                Exit Do
            Else
                crimes += 1
            End If
        Loop

        'Check which layer is the Addresses layer
        Dim a As Integer = 0
        Try
            Do While a < ids.Length
                If names(a) = "Addresses" Then
                    ids(a).ToString()
                    Exit Do
                Else
                    a += 1
                End If
            Loop
        Catch ex As Exception
            'Just exit if there is no address layer
            Exit Sub
        End Try


        'Check which layer is the Macon Co. Tax Parcels layer
        Dim p As Integer = 0
        Try
            Do While p < ids.Length
                If names(p) = "Macon Co. Tax Parcels" Then
                    ids(p).ToString()
                    Exit Do
                Else
                    p += 1
                End If
            Loop
        Catch ex As Exception
            'Just exit if there is no parcel layer
            Exit Sub
        End Try


        Dim i As Integer = 0


        'Dynamically evaluate query layer
        Do While i < ids.Length

            If names(i) = queryLayer Then

                ids(i).ToString()

                Exit Do
            Else
                i += 1
            End If

        Loop


        Dim l As Integer
        Do While l < ids.Length
            Dim strlayername As String = names(l).ToString
            If names(l) = "Lake Decatur" Then
                mapFunc.SetLayerVisibility(ids(l), False)
                Exit Do
            Else
                l += 1
            End If
        Loop


        Dim spatialFilter As New ESRI.ArcGIS.ADF.Web.SpatialFilter()
        'Assign SpatialFilter properties
        spatialFilter.ReturnADFGeometries = True
        spatialFilter.MaxRecords = 500000
        'Defines Query Based on Application the Called it
        spatialFilter.WhereClause = txtQuery.Text



        Dim resultsDataTable As New System.Data.DataTable


        'Initialize the datatable
        resultsDataTable = qFunc.Query(Nothing, ids(i), spatialFilter)

        Dim resultsGraphicsLayer As New FeatureGraphicsLayer


        resultsGraphicsLayer = ESRI.ArcGIS.ADF.Web.Converter.ToGraphicsLayer(resultsDataTable, Color.Transparent, Color.Cyan)
        _resultsGraphicsLayer = ESRI.ArcGIS.ADF.Web.Converter.ToGraphicsLayer(resultsDataTable, Color.Transparent, Color.Cyan)

        'Check if application is Crime Analysis or Investigation Reports
        If txtAppName.Text = "Selected Crime Analysis" Or txtAppName.Text = "Selected Investigation Reports" Then
            Dim env As New Envelope

            '//////////////////////////////////////////////////////////////////////////////////////
            '//
            '//         DRAW MULTIPLE POINTS START
            '//
            '//////////////////////////////////////////////////////////////////////////////////////
            'run code to query multiple addresses
            'insert line to clear graphics and then draw if required
            'Parse X and Y value for each address.
            _crimeStats = New ESRI.ArcGIS.ADF.Web.Display.Graphics.FeatureGraphicsLayer(ESRI.ArcGIS.ADF.Web.FeatureType.Point)
            _layerName = "Selection"
            _crimeStats.TableName = _layerName

            With _crimeStats.Columns
                .Add("CrimeId", GetType(String))
                .Add("FEATURE_DESC", GetType(String))
                .Add("ImagePath", GetType(String))
                .Add("CRIME_TYPE", GetType(String))
                .Add("CASE_QUANTITY", GetType(String))
                .Add("FEATURE_ID", GetType(String))
                .Add("FEATURE_TYPE", GetType(String))
            End With

            splitCrimeData()

            _crimeResultsDataTable = _crimeStats

            If Not graphicsMapResource Is Nothing Then
                ' Check whether the map extent is null, meaning the map has not yet been initialized
                If Map1.Extent Is Nothing Then
                    ' Forces earlier initialization of map and will set map extent
                    Dim primaryMapResource As ESRI.ArcGIS.ADF.Web.DataSources.IMapResource = Map1.PrimaryMapResourceInstance
                End If
            End If
            ' Call helper method in App_Code which generates a random graphics layer.  In real-world situations,
            ' the random layer could be replaced with, for instance, the result of a query.  Once the layer is
            ' created, apply the renderer and add the layer to the graphics resource.

            _featureGraphicsLayer = _crimeResultsDataTable

            If Not _featureGraphicsLayer Is Nothing Then
                Dim simplePointRenderer As ESRI.ADF.Samples.Renderers.SimplePointRenderer = New ESRI.ADF.Samples.Renderers.SimplePointRenderer()
                ' Name of column that contains the relative path to the image that will be used to 
                ' symbolize the feature
                simplePointRenderer.ImagePathColumn = "ImagePath"
                _featureGraphicsLayer.Renderer = simplePointRenderer 'Apply the renderer

                'Retrieve the graphics map functionality for the MapResourceItem named Selection
                Dim crimeGraphicsFunctionality As MapFunctionality
                crimeGraphicsFunctionality = CType(Map1.GetFunctionality("Selection"), MapFunctionality)


                Dim _graphicsDataSet = crimeGraphicsFunctionality.GraphicsDataSet
                _graphicsDataSet = crimeGraphicsFunctionality.GraphicsDataSet
                _graphicsDataSet.Tables.Clear()

                'Add the layer to the graphics resource
                graphicsMapResource.Graphics.Tables.Add(_featureGraphicsLayer)

                env = _featureGraphicsLayer.FullExtent

                With Map1
                    .Extent = env
                    'Lower the number to make the envelope larger or so more of the map will display.
                    'Consider .2 to be 20% of the full extent and 1.0 to be full extent.
                    .Zoom(0.2)
                    .CallbackResults.CopyFrom(Map1.CallbackResults)
                    .RefreshResource(crimeGraphicsFunctionality.Resource.Name)

                End With

            End If

            '//////////////////////////////////////////////////////////////////////////////////////
            '//
            '//         DRAW MULTIPLE POINTS FINISH
            '//
            '//////////////////////////////////////////////////////////////////////////////////////

        ElseIf txtAppName.Text = "Crime Analysis" And txtOriginalQuery.Text <> "ALL" Or txtAppName.Text = "Investigation Reports" And txtOriginalQuery.Text <> "ALL" Then

            Dim env As New Envelope
            Dim RowSelection As Data.DataRow = resultsDataTable.Rows(0)
            Dim pt As New ESRI.ArcGIS.ADF.Web.Geometry.Point
            pt = resultsGraphicsLayer.GeometryFromRow(RowSelection)
            env = resultsGraphicsLayer.FullExtent
            env = New ESRI.ArcGIS.ADF.Web.Geometry.Envelope(pt.X - 0.2, pt.Y - 0.2, pt.X + 0.2, pt.Y + 0.2)


            'Create symbols for all crimes in query
            '//////////////////////////////////////////////////////////////////////////////////////
            '//
            '//         DRAW SINGLE POINTS START
            '//
            '//////////////////////////////////////////////////////////////////////////////////////
            'Parse X and Y value for each address.
            _crimeStats = New ESRI.ArcGIS.ADF.Web.Display.Graphics.FeatureGraphicsLayer(ESRI.ArcGIS.ADF.Web.FeatureType.Point)
            _layerName = "Selection"
            _crimeStats.TableName = _layerName

            With _crimeStats.Columns
                .Add("CrimeId", GetType(String))
                .Add("FEATURE_DESC", GetType(String))
                .Add("ImagePath", GetType(String))
                .Add("CRIME_TYPE", GetType(String))
                .Add("CASE_QUANTITY", GetType(String))
                .Add("FEATURE_ID", GetType(String))
                .Add("FEATURE_TYPE", GetType(String))
            End With

            splitCrimeData()

            _crimeResultsDataTable = _crimeStats

            If Not graphicsMapResource Is Nothing Then
                ' Check whether the map extent is null, meaning the map has not yet been initialized
                If Map1.Extent Is Nothing Then
                    ' Forces earlier initialization of map and will set map extent
                    Dim primaryMapResource As ESRI.ArcGIS.ADF.Web.DataSources.IMapResource = Map1.PrimaryMapResourceInstance
                End If
            End If
            ' Call helper method in App_Code which generates a random graphics layer.  In real-world situations,
            ' the random layer could be replaced with, for instance, the result of a query.  Once the layer is
            ' created, apply the renderer and add the layer to the graphics resource.

            _featureGraphicsLayer = _crimeResultsDataTable

            If Not graphicsMapResource Is Nothing Then
                ' Check whether the map extent is null, meaning the map has not yet been initialized
                If Map1.Extent Is Nothing Then
                    ' Forces earlier initialization of map and will set map extent
                    Dim primaryMapResource As ESRI.ArcGIS.ADF.Web.DataSources.IMapResource = Map1.PrimaryMapResourceInstance
                End If
            End If
            ' Call helper method in App_Code which generates a random graphics layer.  In real-world situations,
            ' the random layer could be replaced with, for instance, the result of a query.  Once the layer is
            ' created, apply the renderer and add the layer to the graphics resource.

            _featureGraphicsLayer = _crimeResultsDataTable

            If Not _featureGraphicsLayer Is Nothing Then
                Dim simplePointRenderer As ESRI.ADF.Samples.Renderers.SimplePointRenderer = New ESRI.ADF.Samples.Renderers.SimplePointRenderer()
                ' Name of column that contains the relative path to the image that will be used to 
                ' symbolize the feature
                simplePointRenderer.ImagePathColumn = "ImagePath"
                _featureGraphicsLayer.Renderer = simplePointRenderer 'Apply the renderer

                'Retrieve the graphics map functionality for the MapResourceItem named Selection
                Dim crimeGraphicsFunctionality As MapFunctionality
                crimeGraphicsFunctionality = CType(Map1.GetFunctionality("Selection"), MapFunctionality)



                'Clear and Draw the Graphics if this is a new browser window
                '-----------------------------------------------------------

                Dim _graphicsDataSet = crimeGraphicsFunctionality.GraphicsDataSet
                _graphicsDataSet = crimeGraphicsFunctionality.GraphicsDataSet
                _graphicsDataSet.Tables.Clear()

                'Add the layer to the graphics resource
                graphicsMapResource.Graphics.Tables.Add(_featureGraphicsLayer)


                'run code to query a single address
                'insert line to clear graphics and then draw if required
                RowSelection = resultsDataTable.Rows(0)
                _pt = _resultsGraphicsLayer.GeometryFromRow(RowSelection)
                env = _resultsGraphicsLayer.FullExtent
                env = New ESRI.ArcGIS.ADF.Web.Geometry.Envelope(_pt.X - 20, _pt.Y - 20, _pt.X + 20, _pt.Y + 20)

                With Map1
                    .Extent = env
                    'Lower the number to make the envelope larger or so more of the map will display.
                    'Consider .2 to be 20% of the full extent and 1.0 to be full extent.
                    .Zoom(0.2)
                    .CallbackResults.CopyFrom(Map1.CallbackResults)
                    .RefreshResource(crimeGraphicsFunctionality.Resource.Name)

                End With
            End If

            '//////////////////////////////////////////////////////////////////////////////////////
            '//
            '//         DRAW SINGLE POINT FINISH
            '//
            '//////////////////////////////////////////////////////////////////////////////////////

        ElseIf txtOriginalQuery.Text <> "ALL" Then

            '//////////////////////////////////////////////////////////////////////////////////////
            '//
            '//         DRAW POLYGONS START
            '//
            '//////////////////////////////////////////////////////////////////////////////////////
            Dim env As Envelope

            'Starts Hollow Selection for Polygons
            Dim adfSelectedSimpleFillSymbol As ESRI.ArcGIS.ADF.Web.Display.Symbol.SimpleFillSymbol = New ESRI.ArcGIS.ADF.Web.Display.Symbol.SimpleFillSymbol()

            With adfSelectedSimpleFillSymbol
                'Define polygon's colors
                .Color = Drawing.Color.Empty
                .BoundaryWidth = 3
                .BoundaryColor = Drawing.Color.Cyan
            End With

            'Renders Polygon as a Polygon with a Cyan Border and Hollow Background
            Dim adfSimpleRenderer As ESRI.ArcGIS.ADF.Web.Display.Renderer.SimpleRenderer = New ESRI.ArcGIS.ADF.Web.Display.Renderer.SimpleRenderer()
            adfSimpleRenderer.Symbol = adfSelectedSimpleFillSymbol

            resultsGraphicsLayer.Renderer = adfSimpleRenderer
            _legendImage = "Images/parcel.gif"

            'Retrieve the graphics map functionality for the MapResourceItem named Selection
            Dim graphicsFunctionality As New MapFunctionality
            graphicsFunctionality = CType(Map1.GetFunctionality("Selection"), MapFunctionality)


            Dim graphicsDataSet As New GraphicsDataSet

            graphicsDataSet = graphicsFunctionality.GraphicsDataSet
            graphicsDataSet.Tables.Clear()

            'Add the selection graphics layer to the resource
            graphicsDataSet.Tables.Add(resultsGraphicsLayer)


            Dim selectionMapResourceItem As MapResourceItem = Map1.MapResourceManagerInstance.ResourceItems.Find(graphicsFunctionality.Resource.Name)

            'Set the transparency of the selection graphics resource map item
            selectionMapResourceItem.DisplaySettings.Transparency = 0

            env = resultsGraphicsLayer.FullExtent

            With Map1
                .Extent = env
                'Lower the number to make the envelope larger or so more of the map will display.
                'Consider .2 to be 20% of the full extent and 1.0 to be full extent.
                .Zoom(0.2)
                .CallbackResults.CopyFrom(Map1.CallbackResults)
                .RefreshResource(graphicsFunctionality.Resource.Name)
            End With



            '//////////////////////////////////////////////////////////////////////////////////////
            '//
            '//         DRAW POLYGONS FINISH
            '//
            '//////////////////////////////////////////////////////////////////////////////////////
        Else
            'This section was going to be used to draw graphics that were cleared.
            Exit Sub


        End If

    End Sub


    Protected Sub Page_PreRenderComplete(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.PreRenderComplete
        ' check to see if any of the resource items are non-pooled
        If Not Page.IsCallback Or Not Page.IsPostBack Then
            If TaskMenu.Items.Count > 1 Then
                Dim i As Integer
                For i = 0 To TaskMenu.Items.Count - 2
                    TaskMenu.Items(i).SeparatorImageUrl = "images/separator.gif"
                Next
            End If
            CloseHyperLink.Visible = (ArcGISServer.GISDataSourceLocal.HasNonPooledServices(MapResourceManager1) Or ArcGISServer.GISDataSourceLocal.HasNonPooledServices(GeocodeResourceManager1) Or ArcGISServer.GISDataSourceLocal.HasNonPooledServices(GeoprocessingResourceManager1))

            If User.Identity.AuthenticationType = "Forms" AndAlso User.Identity.IsAuthenticated Then
                'Set visibility using style instead of the Visible property because using the Visible property corrupts ViewState under certain circumstances 
                LoginStatus1.Style(HtmlTextWriterStyle.Visibility) = "visible"
                CloseHyperLink.Visible = False
            Else
                LoginStatus1.Style(HtmlTextWriterStyle.Visibility) = "hidden"
            End If

            ' Remove the overview toggle it overviewmap doesn't exist, and identify if none of the resources support it.
            Dim ov As OverviewMap = Page.FindControl("OverviewMap1")
            Dim supportsIdentify As Boolean = MapIdentify1.SupportsIdentify()
            Dim tb As Toolbar = Page.FindControl("Toolbar1")
            If Not (tb Is Nothing) Then
                Dim t As Integer
                For t = tb.ToolbarItems.Count - 1 To 0 Step -1
                    Dim item As ToolbarItem = tb.ToolbarItems(t)
                    If item.Name = "OverviewMapToggle" And ov Is Nothing Then
                        tb.ToolbarItems.Remove(item)
                    End If
                    If item.Name = "MapIdentify" And Not supportsIdentify Then
                        tb.ToolbarItems.Remove(item)
                    End If
                Next t
            End If
        End If

        Toc1.Nodes.Count.ToString()


        With Toc1

            .Nodes(0).Nodes(0).Expanded = False
            .Nodes(0).Nodes(0).Text = _contentsText
            .Nodes(0).Nodes(0).LegendCollapsedImage = _legendImage

            .ExpandDepth = 1
            .Nodes(0).HideNodeShowChildren = True
            .Nodes(1).HideNodeShowChildren = True
            .Nodes(2).HideNodeShowChildren = True
        End With



        For B As Integer = 25 To 0 Step -1

            On Error Resume Next

            Dim RemoveLayerB As String
            RemoveLayerB = Toc1.Nodes(1).Nodes(B).Text

            'Remove the following layers
            If RemoveLayerB = "CrimeLocs_view" Or _
                RemoveLayerB = "Street Signs" Or _
                RemoveLayerB = "City Trees" Or _
                RemoveLayerB = "Parking Spaces" Or _
                RemoveLayerB = "City Flower Beds" Or _
                RemoveLayerB = "Active Weed Cases" Or _
                RemoveLayerB = "Flood Zones" Or _
                RemoveLayerB = "Fire Hydrants" Or _
                RemoveLayerB = "Water Infrastructure" Or _
                RemoveLayerB = "Sewer Infrastructure" Or _
                RemoveLayerB = "Coal Mine Maps" Or _
                RemoveLayerB = "Street Jurisdiction" Or _
                RemoveLayerB = "Employee On-Street Parking" Or _
                RemoveLayerB = "Dredge" Then

                Toc1.Nodes(1).Nodes.RemoveAt(B)

                If RemoveLayerB = "Lake Decatur" Then
                    Toc1.Nodes(1).Nodes(B).Checked = False
                End If
            End If

        Next



        For C As Integer = 19 To 0 Step -1

            On Error Resume Next

            Dim RemoveLayerC As String
            RemoveLayerC = Toc1.Nodes(2).Nodes(C).Text

            'Remove the following layers
            If RemoveLayerC = "WaterServiceAreas" Or _
                RemoveLayerC = "Fire Demand Zones" Or _
                RemoveLayerC = "Annexations" Or _
                RemoveLayerC = "Wells Prohibited" Or _
                RemoveLayerC = "Fire Still Alarm Territories" Or _
                RemoveLayerC = "Garbage Haulers" Or _
                RemoveLayerC = "TIF Districts" Or _
                RemoveLayerC = "Historic Districts" Or _
                RemoveLayerC = "DHA Property" Or _
                RemoveLayerC = "Redevelopment Districts" Or _
                RemoveLayerC = "Enterprise Zone" Or _
                RemoveLayerC = "Neighborhoods" Or _
                RemoveLayerC = "Macon Co. Municipalities" Or _
                RemoveLayerC = "Macon Co. School Districts" Or _
                RemoveLayerC = "Zipcodes" Or _
                RemoveLayerC = "Police Districts" Or _
                RemoveLayerC = "Macon Co. Fire Districts" Or _
                RemoveLayerC = "Recycling Pickup" Or _
                RemoveLayerC = "Townships" Or _
                RemoveLayerC = "Police Reporting Areas" Then

                Toc1.Nodes(2).Nodes.RemoveAt(C)

            End If

        Next

        With Toc1
            .Nodes(0).Nodes(0).Nodes(0).Remove()
            .Nodes(1).Nodes(0).Nodes(0).ShowCheckBox = False
            .Nodes(1).Nodes(0).Nodes(1).ShowCheckBox = False
            .Nodes(1).Nodes(0).Nodes(2).ShowCheckBox = False


            .Nodes(3).Nodes(0).Remove()
            .Nodes(3).Nodes(0).Remove()

        End With


    End Sub 'Page_PreRenderComplete

    Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
        ' Enforce SSL requirement.
        Dim requireSSL As Boolean
        If (Not Page.IsPostBack() And ConfigurationManager.AppSettings("RequireSSL") <> Nothing) Then
            Boolean.TryParse(ConfigurationManager.AppSettings("RequireSSL"), requireSSL)
            If (requireSSL And Not Request.IsSecureConnection) Then
                Response.Redirect(Request.Url.ToString().Replace("http://", "https://"))
                Return
            End If
        End If

        Dim gisfunctionality As ESRI.ArcGIS.ADF.Web.DataSources.IGISFunctionality = Nothing
        gisfunctionality = Map1.GetFunctionality("IntranetVector")

        If TypeOf gisfunctionality Is ESRI.ArcGIS.ADF.Web.DataSources.IMapFunctionality Then
            Dim mf As ESRI.ArcGIS.ADF.Web.DataSources.IMapFunctionality
            mf = CType(gisfunctionality, ESRI.ArcGIS.ADF.Web.DataSources.IMapFunctionality)

            Dim layerids() As String = Nothing
            Dim layernames() As String = Nothing

            mf.GetLayers(layerids, layernames)

            Dim i As Integer
            For i = 0 To layerids.Length - 1
                Dim strlayername As String = layernames(i).ToString

                If strlayername = "Redevelopment Districts" Then
                    mf.SetLayerVisibility(layerids(i), True)

                End If
            Next

        End If

    End Sub

    '/ <summary>
    '/ Default method for catching errors that have no programmed catch point
    '/ </summary>
    Private Sub Page_Error(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Error
        Dim exception As Exception = Server.GetLastError()
        Server.ClearError()
        callErrorPage("Page_Error", exception)

    End Sub

    '/ <summary>
    '/ Common method for calling error page, passing specific parameters and messages
    '/ </summary>
    Private Sub callErrorPage(ByVal errorMessage As String, ByVal exception As Exception)

        Session("ErrorMessage") = errorMessage
        Session("Error") = exception
        Session("Message") = errorMessage.ToString
        If exception.Message = "No data exists for the row/column." Then
            Page.Response.Redirect("NoData.aspx")
        End If
        Page.Response.Redirect("ErrorPage.aspx", True)

    End Sub 'callErrorPage

    Protected Sub ResourceManager_ResourcesInit(ByVal sender As Object, ByVal e As EventArgs) Handles MapResourceManager1.ResourcesInit, GeocodeResourceManager1.ResourcesInit, GeoprocessingResourceManager1.ResourcesInit, MapResourceManager2.ResourceInit
        If DesignMode Then
            Return
        End If
        Dim manager As ResourceManager = sender '
        If Not manager.FailureOnInitialize Then
            Return
        End If
        If TypeOf manager Is MapResourceManager Then
            Dim mapManager As MapResourceManager = manager '
            Dim i As Integer
            For i = 0 To mapManager.ResourceItems.Count - 1
                Dim item As MapResourceItem = mapManager.ResourceItems(i)
                If Not (item Is Nothing) Then
                    If item.FailedToInitialize Then
                        mapManager.ResourceItems(i) = Nothing
                    End If
                End If
            Next i
        Else
            If TypeOf manager Is GeocodeResourceManager Then
                Dim gcManager As GeocodeResourceManager = manager '
                Dim i As Integer
                For i = 0 To gcManager.ResourceItems.Count - 1
                    Dim item As GeocodeResourceItem = gcManager.ResourceItems(i)
                    If Not (item Is Nothing) Then
                        If item.FailedToInitialize Then
                            gcManager.ResourceItems(i) = Nothing
                        End If
                    End If
                Next i
            Else
                If TypeOf manager Is GeoprocessingResourceManager Then
                    Dim gpManager As GeoprocessingResourceManager = manager '
                    Dim i As Integer
                    For i = 0 To gpManager.ResourceItems.Count - 1
                        Dim item As GeoprocessingResourceItem = gpManager.ResourceItems(i)
                        If Not (item Is Nothing) Then
                            If item.FailedToInitialize Then
                                gpManager.ResourceItems(i) = Nothing
                            End If
                        End If
                    Next i
                End If
            End If
        End If
    End Sub 'ResourceManager_ResourcesInit 

    '/ <summary>
    '/ Handles call from client to clean up session.
    '/ </summary>
    <System.Web.Services.WebMethod()> Public Shared Function CleanUp(ByVal randomNumber As String) As String
        Dim cleanUpResponse As String = ConfigurationManager.AppSettings("CloseOutUrl")
        If cleanUpResponse Is Nothing Then
            cleanUpResponse = "ApplicationClosed.aspx"
        ElseIf cleanUpResponse.Length = 0 Then
            cleanUpResponse = "ApplicationClosed.aspx"
        End If
        Try
            'GISDataSourceLocal.ReleaseNonPooledContexts(HttpContext.Current.Session)
            HttpContext.Current.Session.RemoveAll()
        Catch
        End Try
        Return cleanUpResponse
    End Function 'CleanUp

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub

    Protected Sub lnkAllAddresses_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lnkAllAddresses.Click
        Dim param As String
        'Set the text box to the query string value from the URL
        param = Request.QueryString("sender")
        Response.Redirect("Default.aspx?sender=" + param + "&FEATURE_ID=ALL".ToString + "&FEATURE_DESC=ALL" + "&APPLICATION_DESC=" + txtAppName.Text)
    End Sub

    Protected Sub GridView1_RowDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewRowEventArgs) Handles gridAddresses.RowDataBound

        If Not e.Row.RowType = DataControlRowType.Footer _
           And Not e.Row.RowType = DataControlRowType.Header _
           And Not e.Row.RowType = DataControlRowType.Pager Then
            e.Row.Cells(1).Attributes.Add("onmouseover", "this.style.backgroundColor='#00FFFF';this.style.color='#00FFFF';")

        End If

        If Not e.Row.RowType = DataControlRowType.Footer _
           And Not e.Row.RowType = DataControlRowType.Header _
           And Not e.Row.RowType = DataControlRowType.Pager Then
            If e.Row.RowState = DataControlRowState.Normal Then
                e.Row.Cells(1).Attributes.Add("onmouseout", "this.style.backgroundColor='#FFFFFF';this.style.color='';")

            ElseIf e.Row.RowState = DataControlRowState.Alternate Then
                e.Row.Cells(1).Attributes.Add("onmouseout", "this.style.backgroundColor='#FFFFFF';this.style.color='';")

            End If
        End If




    End Sub

    Private Sub splitCrimeData()
        Dim crimeStatsRow As Integer
        Dim Selection As Data.DataRow
        Dim seqNum As String
        Dim featDesc As String
        Dim caseType As String
        Dim featId As String
        Dim caseQty As String
        Dim featType As String

        For crimeStatsRow = 0 To _resultsGraphicsLayer.Rows.Count.ToString - 1
            Selection = _resultsGraphicsLayer.Rows(crimeStatsRow)

            crimeStatsColumn = 0
            Do Until _resultsGraphicsLayer.Columns(crimeStatsColumn).ToString = "REQ_SEQ_NUM"
                crimeStatsColumn = crimeStatsColumn + 1
                If _resultsGraphicsLayer.Columns(crimeStatsColumn).ToString = "REQ_SEQ_NUM" Then
                    _reqSeqNum = _resultsGraphicsLayer.Rows.Item(crimeStatsRow).ItemArray(crimeStatsColumn).ToString
                    Exit Do
                End If
            Loop

            crimeStatsColumn = 0
            Do Until _resultsGraphicsLayer.Columns(crimeStatsColumn).ToString = "FEATURE_DESC"
                crimeStatsColumn = crimeStatsColumn + 1
                If _resultsGraphicsLayer.Columns(crimeStatsColumn).ToString = "FEATURE_DESC" Then
                    _featureDesc = _resultsGraphicsLayer.Rows.Item(crimeStatsRow).ItemArray(crimeStatsColumn).ToString
                    Exit Do
                End If
            Loop


            crimeStatsColumn = 0
            Do Until _resultsGraphicsLayer.Columns(crimeStatsColumn).ToString = "CASE_TYPE"
                crimeStatsColumn = crimeStatsColumn + 1
                If _resultsGraphicsLayer.Columns(crimeStatsColumn).ToString = "CASE_TYPE" Then
                    _caseType = _resultsGraphicsLayer.Rows.Item(crimeStatsRow).ItemArray(crimeStatsColumn).ToString
                    Exit Do
                End If
            Loop


            crimeStatsColumn = 0
            Do Until _resultsGraphicsLayer.Columns(crimeStatsColumn).ToString = "FEATURE_ID"
                crimeStatsColumn = crimeStatsColumn + 1
                If _resultsGraphicsLayer.Columns(crimeStatsColumn).ToString = "FEATURE_ID" Then
                    _featureId = _resultsGraphicsLayer.Rows.Item(crimeStatsRow).ItemArray(crimeStatsColumn).ToString
                    Exit Do
                End If
            Loop


            crimeStatsColumn = 0
            Do Until _resultsGraphicsLayer.Columns(crimeStatsColumn).ToString = "CASE_QUANTITY"
                crimeStatsColumn = crimeStatsColumn + 1
                If _resultsGraphicsLayer.Columns(crimeStatsColumn).ToString = "CASE_QUANTITY" Then
                    _caseQuantity = _resultsGraphicsLayer.Rows.Item(crimeStatsRow).ItemArray(crimeStatsColumn).ToString
                    Exit Do
                End If
            Loop


            crimeStatsColumn = 0
            Do Until _resultsGraphicsLayer.Columns(crimeStatsColumn).ToString = "FEATURE_TYPE"
                crimeStatsColumn = crimeStatsColumn + 1
                If _resultsGraphicsLayer.Columns(crimeStatsColumn).ToString = "FEATURE_TYPE" Then
                    _featureType = _resultsGraphicsLayer.Rows.Item(crimeStatsRow).ItemArray(crimeStatsColumn).ToString
                    Exit Do
                End If
            Loop


            seqNum = _reqSeqNum
            featDesc = _featureDesc
            caseType = _caseType
            featId = _featureId
            caseQty = _caseQuantity
            featType = _featureType

            _pt = _resultsGraphicsLayer.GeometryFromRow(Selection)

            Dim s As String
            Dim r As String
            Dim xCount As Integer
            xCount = 0
            'Split the Crime Type and Quantity For Each Address
            For Each r In caseType.Split(":"c)
                For Each s In r.Split(","c)
                    If typeflag Then
                        type = s
                        crimeType = s
                        typeflag = False
                        Continue For
                    Else
                        qty = s
                        typeflag = True
                    End If
                    xCount = xCount + 1


                    '/////////////////////////////////////////////////////////////////////////////////////////
                    '//
                    '// Next Step - Populate Feature Table
                    '//
                    '/////////////////////////////////////////////////////////////////////////////////////////
                    Dim row As System.Data.DataRow = _crimeStats.Add(New ESRI.ArcGIS.ADF.Web.Geometry.Point(_pt.X, _pt.Y)) 'This will need to be modified based on how many incidents are at a given location
                    'Sets the location of the point
                    _pt.Y = _pt.Y - xCount - 20

                    '   Variable               Definition
                    '---------------        ----------------------------------------------------------------------
                    '   CODE_STATUS         Number Representing a crime type defined in Crime Types Below
                    '   MISC_TYP_DESC       Description of a given crime
                    '   CRIME LEVEL VALUE   Three colors that represent the quantity of crimes
                    '   CRIME LEVEL COLOR   Symbol that represents a crime
                    '   
                    '
                    '   
                    '
                    '   CODE_STATUS VALUES          MISC_TYP_DESC()
                    '   (Value representing crime)  (Description of the crime)
                    '
                    '   VALUE                       DESCRIPTION         SYMBOL
                    '----------------------------------------------------------------------------------------------
                    '       0 -------------------   INVESTIGATION
                    '       1 -------------------   SHOOTING
                    '       2 -------------------   SEXUAL ASSAULT
                    '       3 -------------------   HOME INVASION
                    '       4 -------------------   VEHICLE INVASION
                    '       5 -------------------   STREET ROBBERY
                    '       6 -------------------   BUSINESS ROBBERY
                    '       7 -------------------   BURGLARY
                    '       8 -------------------   VEHICLE THEFT
                    '       9 -------------------   AUTO BURGLARY
                    '      10 -------------------   N/A
                    '----------------------------------------------------------------------------------------------
                    '
                    '   CRIME LEVEL
                    '
                    '       VALUE                       COLOR
                    '----------------------------------------------------------------------------------------------
                    '       1 ------------------------  GREEN
                    '       2-5 ----------------------  YELLOW
                    '       6 OR MORE ----------------  RED
                    'Evaluates the name of the crime

                    Select Case crimeType
                        Case 0
                            crimeName = "Investigation"
                            Crime_Legend_Panel.Visible = False
                        Case 1
                            crimeName = "Shooting"
                            Crime_Legend_Panel.Visible = True
                        Case 2
                            crimeName = "SexualAssault"
                            Crime_Legend_Panel.Visible = True
                        Case 3
                            crimeName = "HomeInvasion"
                            Crime_Legend_Panel.Visible = True
                        Case 4
                            crimeName = "VehicleInvasion"
                            Crime_Legend_Panel.Visible = True
                        Case 5
                            crimeName = "StreetRobbery"
                            Crime_Legend_Panel.Visible = True
                        Case 6
                            crimeName = "BusinessRobbery"
                            Crime_Legend_Panel.Visible = True
                        Case 7
                            crimeName = "Burglary"
                            Crime_Legend_Panel.Visible = True
                        Case 8
                            crimeName = "VehicleTheft"
                            Crime_Legend_Panel.Visible = True
                        Case 9
                            crimeName = "AutoBurglary"
                            Crime_Legend_Panel.Visible = True
                        Case 10
                            crimeName = "NA"
                            Crime_Legend_Panel.Visible = True
                        Case Else
                            crimeName = "NotAssigned"
                    End Select

                    'Crime Intensity
                    Select Case qty
                        Case 1
                            crimeLevel = "Green"
                        Case 2
                            crimeLevel = "Yellow"
                        Case Is >= 3
                            crimeLevel = "Red"
                    End Select

                    If crimeName = "NotAssigned" Then
                        _imgPath = System.String.Format("Images/" & crimeName & ".png")
                    Else
                        _imgPath = System.String.Format("Images/" & crimeLevel + crimeName & ".png")
                    End If

                    row("CRIME_TYPE") = crimeType
                    row("ImagePath") = _imgPath
                    row("CASE_QUANTITY") = qty
                    row("CrimeId") = seqNum
                    row("FEATURE_DESC") = featDesc
                    row("FEATURE_TYPE") = featType
                    row("FEATURE_ID") = featId

                Next
            Next

        Next
    End Sub



End Class