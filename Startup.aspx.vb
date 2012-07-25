Imports System.Data.OleDb.OleDbConnection
Imports System.Data

Partial Class Startup
    Inherits System.Web.UI.Page



    Public Sub UpdateData()

        '************************************************************************************************
        '                   STARTUP PAGE DESCRIPTION
        '************************************************************************************************
        '
        '   A desktop application populates input parameters which includes the IP address of the desktop
        '   application. This IP address is used as the unique identifier to determine what data is to be
        '   used.
        '
        '   With the IP address as the lone parameter, the web page extends the parameter to map the data
        '   requested, with the definitions listed below. IP_ADDRESS, RAND_PARAM, APPLICATION_DESC,
        '   FEATURE_ID, FEATURE_TYPE are used to uniqely identify the IP address, the unique query called
        '   from an IP address by assigning a random number, defining the application to identify the 
        '   application, the feature ID to query the feature graphically and via gridviews, and type of
        '   feature to determine how to graphically draw the feature; points, polygons or lines.
        '
        '   Since the IP address is the lone parameter for this page, the data extracted is transferred
        '   to a web table; GENINFO.GIS_MAP_WEB.
        '
        '   The web page performs a URL redirect and then loads the Default page.
        '
        '************************************************************************************************
        '   Variable               Definition
        '---------------        ----------------------------------------------------------------------
        '   userIP_Address      Returns the querying users IP Address
        '   qryRandParam        Returns value to set as parameter
        '   qryAppName          Returns the name of the application running the query
        '   qryFeatureId        Returns the unique ID of feature to be used parameter in geo-query
        '   qryFeatureType      Returns the type of geography to be queried 
        '   qryFeatureDesc      Returns the description of the geography to be queried
        '   qryMultiFeature     Returns multiple addresses from a multi-select query
        '
        '************************************************************************************************
        '                   PREPARING DATA EXTRACTION
        '************************************************************************************************
        Dim G, GW As String


        G = "GENINFO.GIS_MAP"
        GW = "GENINFO.GIS_MAP_WEB"

        Dim userIP_Address As String
        'userIP_Address = Server.HtmlEncode(Request.UserHostName)
        userIP_Address = "192.168.100.227"

        'Define database connection
        Dim conn As Data.OleDb.OleDbConnection
        Dim comm As Data.OleDb.OleDbCommand
        Dim comm2 As Data.OleDb.OleDbCommand


        'Defines the name of the database to connect to
        'Prodgen stores the desktop application data
        Dim S As String = "Prodgen"

        'Define method to read data
        'Data is stored in an IBM DB2 database
        Dim reader As Data.OleDb.OleDbDataReader

        'Define conection string parameters
        Dim connectionString As String = ConfigurationManager.ConnectionStrings(S).ConnectionString

        'Fields used to return data
        Dim IP, random, application, featureID, featureType, feature, mapable, identifier, caseType, caseQty, mapImageId, drawMap As String


        IP = "IP_ADDRESS"
        random = "RAND_PARAM"
        application = "APPLICATION_DESC"
        featureID = "FEATURE_ID"
        featureType = "FEATURE_TYPE"
        feature = "FEATURE_DESC"
        mapable = "MAPABLE"
        identifier = "REQ_SEQ_NUM"
        caseType = "CASE_TYPE"
        caseQty = "CASE_QUANTITY"
        mapImageId = "MAP_IMAGE_ID"
        drawMap = "DRAW_MAP"

        'Select data to be queried; returns data that can be mapped and data that can not be mapped as well
        'May 18, 2010 Added id and caseType as parameters for mapping
        Dim SelectData As String
        SelectData = IP & "," & random & "," & application & "," & featureID & "," & featureType & "," & feature & "," & mapable & "," & identifier & "," & caseType & "," & caseQty & "," & mapImageId & "," & drawMap

        '************************************************************************************************
        '           CONNECT AND READ DATA FROM DESKTOP APPLICATION TABLE
        '************************************************************************************************
        'Instantiate connection
        conn = New Data.OleDb.OleDbConnection(connectionString)

        'Instantiate command
        comm = New Data.OleDb.OleDbCommand( _
            "SELECT " & SelectData & " " & _
            "FROM " & G & " WHERE " & IP & "= '" & userIP_Address & "'", conn)

        'Open connection, execute reader and read data
        conn.Open()
        reader = comm.ExecuteReader()
        reader.Read()

        'Define the value of the variables
        'May 18, 2010 Added the variable qryId and qryCaseType to query parameters
        Dim qryIP_Address, qryAppName, qryFeatureId, qryFeatureType, qryFeatureDesc, qrymapable, qryId, qryCaseType, qryCaseQty, qryMapImageId, qryDrawMap As String
        Dim qryRandParam As Integer


        qryIP_Address = reader(IP).ToString
        qryRandParam = reader(random).ToString
        qryAppName = reader(application).ToString
        qryFeatureId = reader(featureID).ToString
        qryFeatureType = reader(featureType).ToString
        qryFeatureDesc = reader(feature).ToString
        qrymapable = reader(mapable).ToString
        qryId = reader(identifier).ToString
        qryCaseType = reader(caseType).ToString
        qryCaseQty = reader(caseQty).ToString
        qryMapImageId = reader(mapImageId).ToString
        qryDrawMap = reader(drawMap).ToString

        Dim qryRandParam1 As Integer
        qryRandParam1 = qryRandParam

        Title = "Generating Map for " & qryAppName

        conn.Close()

        '*********************************************************************************************
        '           COPY DATA FROM DESKTOP APPLICATION TABLE TO WEB APPLICATION TABLE
        '*********************************************************************************************
        'Copy data and set primary parameter for GIS query
        Dim connCopy As Data.OleDb.OleDbConnection
        Dim commCopy As Data.OleDb.OleDbCommand

        'Define data to copy into web table
        connCopy = New Data.OleDb.OleDbConnection(connectionString)
        commCopy = New Data.OleDb.OleDbCommand( _
            "INSERT INTO " & GW & " " & _
            "(" & SelectData & ")" & _
            "SELECT " & SelectData & " " & _
            "FROM " & G & " " & _
            "WHERE " & IP & "= '" & qryIP_Address & "' AND " & random & " = " & qryRandParam, connCopy)

        'Copying data to web table
        connCopy.Open()
        commCopy.ExecuteNonQuery()
        conn.Close()

        'Delete data if application is not in test mode

        '*********************************************************************************************
        '           DELETE DATA FROM DESKTOP APPLICATION TABLE
        '*********************************************************************************************
        'Delete original data
        Dim connDelete As Data.OleDb.OleDbConnection
        Dim commDelete As Data.OleDb.OleDbCommand

        'Define data to delete from original table
        connDelete = New Data.OleDb.OleDbConnection(connectionString)
        commDelete = New Data.OleDb.OleDbCommand( _
            "DELETE FROM " & G & " WHERE " & _
            IP & "= '" & qryIP_Address & "' AND " & random & " = " & qryRandParam, connDelete)

        'Delete original table data
        connDelete.Open()

        commDelete.ExecuteNonQuery()
        connDelete.Close()

        '*********************************************************************************************
        '           EVALUATE IF GEO-QUERY RETURNS MULTIPLE RECORDS FOR PAGE REDIRECT
        '*********************************************************************************************
        'Define record count command
        Dim commCount As Data.OleDb.OleDbCommand

        'Select records queried from source IP address based on assigned random number
        'Distinct has been removed to evaluate count of addresses; I think this was required, but do
        'no
        commCount = New Data.OleDb.OleDbCommand( _
            "SELECT COUNT(" & GW & "." & featureID & ") AS " & _
            "MySelection FROM " & GW & " WHERE " & mapable & " = 'Y' AND " & _
            IP & "= '" & qryIP_Address & "' AND " & random & " = " & qryRandParam, conn)

        conn.Open()
        reader = commCount.ExecuteReader()
        reader.Read()

        'If the count of the records equals one, then set the page redirect using
        'the FEATURE_ID AND FEATURE_DESC values
        If reader("MySelection") = 1 Then
            conn.Close()
            comm2 = New Data.OleDb.OleDbCommand( _
                "SELECT " & SelectData & " " & _
                "FROM " & GW & " WHERE " & IP & "= '" & userIP_Address & "' AND " & "MAPABLE= 'Y' AND " & random & " = " & qryRandParam1, conn)
            conn.Open()
            reader = comm2.ExecuteReader()
            reader.Read()
            qryIP_Address = reader(IP).ToString
            qryRandParam = reader(random).ToString
            qryAppName = reader(application).ToString
            qryFeatureId = reader(featureID).ToString
            qryFeatureType = reader(featureType).ToString
            qryFeatureDesc = reader(feature).ToString
            qrymapable = reader(mapable).ToString

            Response.Redirect( _
            "Default.aspx?sender=" + qryRandParam.ToString + _
            "&" & featureID & "=" & qryFeatureId.ToString + _
            "&" & application & "=" & qryAppName.ToString + _
            "&" & feature & "=" & qryFeatureDesc.ToString)
        Else
            'If the count of records is greater than one, then set the page redirect
            'FEATURE_ID AND FEATURE_DESC values to ALL
            Response.Redirect( _
            "Default.aspx?sender=" + qryRandParam.ToString + _
            "&" & featureID & "=ALL".ToString + _
            "&" & application & "=" + qryAppName + _
            "&" & feature & "=ALL".ToString)
        End If

        conn.Close()

    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        'Transfers data from query table to map query table
        UpdateData()
    End Sub

End Class
