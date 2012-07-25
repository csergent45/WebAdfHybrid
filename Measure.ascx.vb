Imports System
Imports System.Data
Imports System.Configuration
Imports System.Collections
Imports System.Collections.Specialized
Imports System.Web
Imports System.Web.Security
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.UI.WebControls.WebParts
Imports System.Web.UI.HtmlControls
Imports ESRI.ArcGIS.ADF.Web
Imports ESRI.ArcGIS.ADF.Web.UI.WebControls
Imports ESRI.ArcGIS.ADF.Web.DataSources
Imports ESRI.ArcGIS.ADF.Web.Geometry

Public Enum MapUnit
	Resource_Default
	Degrees
	Feet
	Meters
End Enum 'MapUnit

Public Enum MeasureUnit
	Feet
	Kilometers
	Meters
	Miles
End Enum 'MeasureUnit

Public Enum AreaUnit
	Acres
	Sq_Feet
	Sq_Kilometers
	Sq_Meters
	Sq_Miles
End Enum 'AreaUnit


Partial Class Measure
	Inherits System.Web.UI.UserControl
	Implements ICallbackEventHandler
	Private m_resourceManger As MapResourceManager
	Private m_mapFunctionality As IMapFunctionality
	Private m_page As Page
	Public m_callbackInvocation As String = ""
	Private m_mapBuddyId As String = "Map1"
	Public m_id As String
	Private m_map As Map
	Private m_mapUnits As MapUnit = MapUnit.Degrees
	Private m_startMapUnits As MapUnit = MapUnit.Degrees
	Private m_FallbackMapUnits As MapUnit = MapUnit.Degrees
	Public m_measureUnits As MeasureUnit = MeasureUnit.Miles
	Public m_areaUnits As AreaUnit = AreaUnit.Sq_Miles
    Private m_numberDecimals As Double = 4



	Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
		m_id = Me.ClientID
		m_page = Me.Page
		' find the map control
		If (m_mapBuddyId Is Nothing Or m_mapBuddyId.Length = 0) Then
			m_mapBuddyId = "Map1"
		End If
		m_map = m_page.FindControl(m_mapBuddyId)
		' find the map resource manager
		m_resourceManger = m_page.FindControl(m_map.MapResourceManager)
		m_callbackInvocation = CallbackFunctionString
	End Sub
	Protected ReadOnly Property CallbackFunctionString() As String
		Get
            'If ScriptManager.GetCurrent(Me.Page) Is Nothing Then
            Return Page.ClientScript.GetCallbackEventReference(Me, "argument", "ESRI.ADF.System.processCallbackResult", "context", "postBackError", False)
            'Else
            '         Return String.Format("__esriDoPostBack('{0}','{1}', argument, ESRI.ADF.System.ProcessMSAjaxCallbackResult, context);", Me.UniqueID, Me.ClientID)
            'End If
		End Get
	End Property

	Protected Sub Page_PreRender(ByVal sender As Object, ByVal e As EventArgs)
		GetMeasureResource()
	End Sub

	Private Sub GetMeasureResource()
		' use the primary resouce, if defined
		Dim primeResource As String = m_map.PrimaryMapResource
		Dim mapResources As IEnumerable = m_resourceManger.GetResources()
		Dim resEnum As IEnumerator = mapResources.GetEnumerator()
		resEnum.MoveNext()
		Dim resource As IGISResource
		If Not primeResource Is Nothing And primeResource.Length > 0 Then
			resource = m_resourceManger.GetResource(primeResource)
		Else
			resource = resEnum.Current
		End If
		If Not (resource Is Nothing) Then
			m_mapFunctionality = CType(resource.CreateFunctionality(GetType(IMapFunctionality), "mapFunctionality"), IMapFunctionality)
		End If
	End Sub

	Public Function ProcessMeasureRequest(ByVal queryString As NameValueCollection) As String
		If m_mapFunctionality Is Nothing Then GetMeasureResource()

		Dim o As Object = Session("MeasureMapUnits")
		If Not (o Is Nothing) Then
			m_mapUnits = CType([Enum].Parse(GetType(MapUnit), o.ToString()), MapUnit)
		ElseIf m_startMapUnits = MapUnit.Resource_Default Then
			m_mapUnits = GetResourceDefaultMapUnit()
		Else
			m_mapUnits = m_startMapUnits
		End If
		Dim eventArg As String = queryString("EventArg").ToLower()
		Dim vectorAction As String = queryString("VectorMode").ToLower()
		Dim coordPairs(), xys() As String
		Dim coordString As String = queryString("coords")
		If coordString Is Nothing And coordString.Length = 0 Then
			coordString = ""
		End If

		coordPairs = coordString.Split(Char.Parse("|"))
        Dim mapUnitString As String = queryString("MapUnits")
        Dim forceRefresh As Boolean = (queryString("refresh") = "true")
		If mapUnitString Is Nothing Then mapUnitString = ""
		If mapUnitString.Length > 0 Then
			m_mapUnits = CType([Enum].Parse(GetType(MapUnit), mapUnitString), MapUnit)
		End If
		Session("MeasureMapUnits") = m_mapUnits
		Dim measureUnitString As String = queryString("MeasureUnits")
		If measureUnitString Is Nothing Then measureUnitString = ""
		If measureUnitString.Length > 0 Then m_measureUnits = CType([Enum].Parse(GetType(MeasureUnit), measureUnitString), MeasureUnit)
		Dim areaUnitstring As String = queryString("AreaUnits")
		If areaUnitstring Is Nothing Then areaUnitstring = ""
		If areaUnitstring.Length > 0 Then m_areaUnits = CType([Enum].Parse(GetType(AreaUnit), areaUnitstring), AreaUnit)
		Dim response As String = Nothing
		Dim points As New PointCollection()
		Dim dPoints As New PointCollection()
		Dim distances As New ArrayList()
		Dim totalDistance As Double = 0
		Dim segmentDistance As Double = 0
		Dim area As Double = 0
		Dim perimeter As Double = 0
		Dim roundFactor As Double = Math.Pow(10, m_numberDecimals)
		Dim xD, yD, tempDist, tempDist2, tempArea, x1, x2, y1, y2 As Double
		Dim transformationParameters As TransformationParams = m_map.GetTransformationParams(TransformationDirection.ToMap)
		If (vectorAction = "measure") Then

			If Not coordPairs Is Nothing And coordPairs.Length > 1 Then
                For cp As Integer = 0 To coordPairs.Length - 1
                    xys = coordPairs(cp).Split(Char.Parse(":"))
                    points.Add(New Point(Convert.ToDouble(xys(0), System.Globalization.CultureInfo.InvariantCulture), Convert.ToDouble(xys(1), System.Globalization.CultureInfo.InvariantCulture)))
                    If cp > 0 Then
                        ' check for duplicate points from double click.... Firefox will send coords for both clicks, causing segmentDistance to be zero.
                        If (Not points(cp - 1).X = points(cp).X OrElse Not points(cp - 1).Y = points(cp).Y) Then
                            If m_mapUnits = MapUnit.Degrees Then
                                ' use great circle formula
                                tempDist = DegreeToFeetDistance(points(cp - 1).X, points(cp - 1).Y, points(cp).X, points(cp).Y)
                                y1 = DegreeToFeetDistance(points(cp).X, points(cp).Y, points(cp).X, 0)
                                x1 = DegreeToFeetDistance(points(cp).X, points(cp).Y, 0, points(cp).Y)
                                dPoints.Add(New Point(x1, y1))
                                segmentDistance = ConvertUnits(tempDist, MapUnit.Feet, m_measureUnits)
                            Else
                                ' get third side of triangle for distance
                                xD = Math.Abs(points(cp).X - points(cp - 1).X)
                                yD = Math.Abs(points(cp).Y - points(cp - 1).Y)
                                tempDist = Math.Sqrt(Math.Pow(xD, 2) + Math.Pow(yD, 2))
                                segmentDistance = ConvertUnits(tempDist, m_mapUnits, m_measureUnits)
                            End If
                            distances.Add(segmentDistance)
                            totalDistance += segmentDistance
                            segmentDistance = Math.Round(segmentDistance * roundFactor) / roundFactor
                            totalDistance = Math.Round(totalDistance * roundFactor) / roundFactor
                        End If
                    Else
                        If (m_mapUnits = MapUnit.Degrees) Then
                            y1 = DegreeToFeetDistance(points(cp).X, points(cp).Y, points(cp).X, 0)
                            x1 = DegreeToFeetDistance(points(cp).X, points(cp).Y, 0, points(cp).Y)
                            dPoints.Add(New Point(x1, y1))
                        End If
                    End If
                Next
			End If
			If (eventArg = "polygon") Then
                If (points.Count > 2 OrElse forceRefresh) Then
                    If (m_mapUnits = MapUnit.Degrees) Then
                        tempDist = DegreeToFeetDistance(points(points.Count - 1).X, points(points.Count - 1).Y, points(0).X, points(0).Y)
                        tempDist2 = ConvertUnits(tempDist, MapUnit.Feet, m_measureUnits)
                        distances.Add(tempDist2)
                        dPoints.Add(dPoints(0))
                    Else
                        xD = Math.Abs(points(points.Count - 1).X - points(0).X)
                        yD = Math.Abs(points(points.Count - 1).Y - points(0).Y)
                        tempDist = Math.Sqrt(Math.Pow(xD, 2) + Math.Pow(yD, 2))
                        tempDist2 = ConvertUnits(tempDist, m_mapUnits, m_measureUnits)
                        distances.Add(tempDist2)
                    End If
                    points.Add(points(0))
                    perimeter = totalDistance + tempDist2
                    ' add area calculation
                    tempArea = 0
                    Dim mUnits As MapUnit = m_mapUnits
                    Dim xDiff As Double = 0
                    Dim yDiff As Double = 0
                    If (m_mapUnits = MapUnit.Degrees) Then
                        points = dPoints
                        mUnits = MapUnit.Feet
                    End If
                    For j As Integer = 0 To points.Count - 2
                        x1 = Convert.ToDouble(points(j).X, System.Globalization.CultureInfo.InvariantCulture)
                        x2 = Convert.ToDouble(points(j + 1).X, System.Globalization.CultureInfo.InvariantCulture)
                        y1 = Convert.ToDouble(points(j).Y, System.Globalization.CultureInfo.InvariantCulture)
                        y2 = Convert.ToDouble(points(j + 1).Y, System.Globalization.CultureInfo.InvariantCulture)
                        xDiff = x2 - x1
                        yDiff = y2 - y1
                        tempArea += x1 * yDiff - y1 * xDiff
                    Next
                    tempArea = Math.Abs(tempArea) / 2
                    area = ConvertAreaUnits(tempArea, mUnits, m_areaUnits)
                    perimeter = Math.Round(perimeter * roundFactor) / roundFactor
                    area = Math.Round(area * roundFactor) / roundFactor
                Else
                    response = String.Format("<table cellspacing='0' ><tr><td>Perimeter: </td><td align='right' id='tdperimiter'> 0</td><td >{0}</td></tr><tr><td>Area:</td><td align='right' id='tdarea'>0 </td><td>{1}</td></tr></table>", WriteMeasureUnitDropdown(), WriteAreaUnitDropdown())
                End If

			ElseIf (eventArg = "polyline") Then
                If points.Count < 3 OrElse forceRefresh Then
                    response = String.Format("<table cellspacing='0' ><tr><td>Segment: </td><td align='right' id='tdsegment'>{0} </td><td>{1}</td></tr><tr><td>Total Length:</td><td align='right' id='tdtotaldistance'>{2} </td><td>{3}</td></tr></table>", segmentDistance, m_measureUnits.ToString(), totalDistance, WriteMeasureUnitDropdown())
                End If
            ElseIf (eventArg = "point" And coordPairs.Length > 0) Then
                xys = coordPairs(0).Split(Char.Parse(":"))
                response = String.Format("<table cellspacing='0' ><tr><td>X Coordinate:</td><td align='right' dir='ltr'>{0}</td></tr><tr><td>Y Coordinate:</td><td align='right' dir='ltr'>{1}</td></tr></table>", (Math.Round(Convert.ToDouble(xys(0), System.Globalization.CultureInfo.InvariantCulture) * roundFactor) / roundFactor).ToString(), (Math.Round(Convert.ToDouble(xys(1), System.Globalization.CultureInfo.InvariantCulture) * roundFactor) / roundFactor).ToString())


            End If
        End If
        Dim coll As CallbackResultCollection = New CallbackResultCollection()
        coll.Add(New CallbackResult("", "", "invoke", "measureComplete", _
         New Object() {response, m_id, area, perimeter, segmentDistance, totalDistance}))
        Return coll.ToString()
    End Function

	Public Function CheckFormMeasureUnits(ByVal unit As String) As String
		Dim response As String = ""
		If unit = m_measureUnits.ToString() Then
			response = "selected=""selected"""
		End If
		Return response
	End Function 'CheckFormMeasureUnits

	Public Function CheckFormAreaUnits(ByVal unit As String) As String
		Dim response As String = ""
		If unit = m_areaUnits.ToString() Then
			response = "selected=""selected"""
		End If
		Return response
	End Function 'CheckFormAreaUnits

	Public Function WriteMeasureUnitDropdown() As String
		Dim sb As New System.Text.StringBuilder()
		sb.Append("<select id=""MeasureUnits2"" onchange=""changeMeasureUnits()"" style=""font: normal 7pt Verdana; width: 100px;"">")
		Dim mArray As Array = [Enum].GetValues(GetType(MeasureUnit))
		Dim mu As MeasureUnit
		For Each mu In mArray
			sb.AppendFormat("<option value=""{0}"" {1}>{0}</option>", mu.ToString(), CheckFormMeasureUnits(mu.ToString()))
		Next mu
		sb.Append("</select>")

		Return sb.ToString()
	End Function 'WriteMeasureUnitDropdown

	Public Function WriteAreaUnitDropdown() As String
		Dim sb As New System.Text.StringBuilder()
		sb.Append("<select id=""AreaUnits2"" onchange=""changeAreaUnits()"" style=""font: normal 7pt Verdana; width: 100px;"">")
		Dim aArray As Array = [Enum].GetValues(GetType(AreaUnit))
		Dim au As AreaUnit
		For Each au In aArray
			sb.AppendFormat("<option value=""{0}"" {1}>{0}</option>", au.ToString(), CheckFormAreaUnits(au.ToString()))
		Next au
		sb.Append("</select>")

		Return sb.ToString()
	End Function 'WriteAreaUnitDropdown

	Public Function ConvertUnits(ByVal distance As Double, ByVal fromUnits As MapUnit, ByVal toUnits As MeasureUnit) As Double
		Dim mDistance As Double = distance
		If fromUnits = MapUnit.Feet Then
			If toUnits = MeasureUnit.Miles Then
				mDistance = distance / 5280
			Else
				If toUnits = MeasureUnit.Meters Then
					mDistance = distance * 0.304800609601
				Else
					If toUnits = MeasureUnit.Kilometers Then
						mDistance = distance * 0.0003048
					End If
				End If
			End If
		Else
			If toUnits = MeasureUnit.Miles Then
				mDistance = distance * 0.0006213700922
			Else
				If toUnits = MeasureUnit.Feet Then
					mDistance = distance * 3.280839895
				Else
					If toUnits = MeasureUnit.Kilometers Then
						mDistance = distance / 1000
					End If
				End If
			End If
		End If
		Return mDistance
	End Function 'ConvertUnits

	Private Function ConvertAreaUnits(ByVal area As Double, ByVal baseUnits As MapUnit, ByVal toUnits As AreaUnit) As Double
		Dim mArea As Double = area
		If baseUnits = MapUnit.Feet Then
			If toUnits = AreaUnit.Acres Then
				mArea = area * 0.000022956
			Else
				If toUnits = AreaUnit.Sq_Meters Then
					mArea = area * 0.09290304
				Else
					If toUnits = AreaUnit.Sq_Miles Then
						mArea = area * 0.00000003587
					Else
						If toUnits = AreaUnit.Sq_Kilometers Then
							mArea = area * 0.09290304 / 1000000
						End If
					End If
				End If
			End If
		Else
			If baseUnits = MapUnit.Meters Then
				If toUnits = AreaUnit.Acres Then
					mArea = area * 0.0002471054
				Else
					If toUnits = AreaUnit.Sq_Miles Then
						mArea = area * 0.0000003861003
					Else
						If toUnits = AreaUnit.Sq_Kilometers Then
							mArea = area * 0.000001
						Else
							If toUnits = AreaUnit.Sq_Feet Then
								mArea = area * 10.76391042
							End If
						End If
					End If
				End If
			End If
		End If
		Return mArea
	End Function 'ConvertAreaUnits

	Private Function DegreeToFeetDistance(ByVal x1 As Double, ByVal y1 As Double, ByVal x2 As Double, ByVal y2 As Double) As Double
		' use great circle formula
		Dim Lat1 As Double = DegToRad(y1)
		Dim Lat2 As Double = DegToRad(y2)
		Dim Lon1 As Double = DegToRad(x1)
		Dim Lon2 As Double = DegToRad(x2)
		Dim LonDist As Double = Lon1 - Lon2
		Dim LatDist As Double = Lat1 - Lat2
		Dim x As Double = Math.Pow(Math.Sin((LatDist / 2)), 2) + Math.Cos(Lat1) * Math.Cos(Lat2) * Math.Pow(Math.Sin((LonDist / 2)), 2)
		x = 2 * Math.Asin(Math.Min(1, Math.Sqrt(x)))
		x = (3963 - 13 * Math.Sin(((Lat1 + Lat2) / 2))) * x
		' in miles... convert to feet and use that as base
		Return x * 5280
	End Function 'DegreeToFeetDistance

	Private Function DegToRad(ByVal degrees As Double) As Double
		Return Convert.ToDouble((degrees * Math.PI / 180))
	End Function 'DegToRad

	Private Function GetResourceDefaultMapUnit() As MapUnit
		Dim mUnit As MapUnit = MapUnit.Degrees
		Try
			Dim mu As Units = m_mapFunctionality.Units
			If mu = Units.DecimalDegrees Then
				mUnit = MapUnit.Degrees
			Else
				If mu = Units.Feet Then
					mUnit = MapUnit.Feet
				Else
					If mu = Units.Meters Then
						mUnit = MapUnit.Meters
					End If
				End If
			End If
		Catch
			' cannot get units from resource... default to fallback value set in declaration
			mUnit = m_FallbackMapUnits
		End Try
		Return mUnit
	End Function 'GetResourceDefaultMapUnit 

	Private _callbackArg As String

	Public Overridable Function RaiseCallbackEvent2(ByVal requestString As String) As String
		' break out the responseString into a querystring

		Dim keyValuePairs As Array = requestString.Split("&".ToCharArray())
		Dim m_queryString As New NameValueCollection()
		m_page = Me.Page
		Dim map As Map = m_page.FindControl(Me.m_mapBuddyId)
		Dim keyValue() As String
		Dim response As String = ""
		If keyValuePairs.Length > 0 Then
			Dim i As Integer
			For i = 0 To keyValuePairs.Length - 1
				keyValue = keyValuePairs.GetValue(i).ToString().Split("=".ToCharArray())
				m_queryString.Add(keyValue(0), keyValue(1))
			Next i

		Else
			keyValue = requestString.Split("=".ToCharArray())
			If keyValue.Length > 0 Then
				m_queryString.Add(keyValue(0), keyValue(1))
			End If
		End If ' isolate control type and mode

		' isolate control type and mode
		Dim controlType As String = m_queryString("ControlType")
		Dim eventArg As String = m_queryString("EventArg")
		If controlType Is Nothing Then
			controlType = "Map"
		End If

		Select Case controlType
			Case "Map"
				' request is for the map control
				Dim vectorMode As String = m_queryString("VectorMode")
				If Not (vectorMode Is Nothing) And vectorMode.ToLower() = "measure" Then
					response = ProcessMeasureRequest(m_queryString)
				End If
			Case Else
		End Select '
		Return response

	End Function

	Public Function GetCallbackResult() As String Implements System.Web.UI.ICallbackEventHandler.GetCallbackResult
		Return RaiseCallbackEvent2(_callbackArg)
	End Function

	Public Sub RaiseCallbackEvent(ByVal eventArgument As String) Implements System.Web.UI.ICallbackEventHandler.RaiseCallbackEvent
		_callbackArg = eventArgument
	End Sub
	Public Overrides Property Id() As String '
		Get
			m_id = MyBase.ID
			Return MyBase.ID
		End Get
		Set(ByVal value As String)
			m_id = value
			MyBase.ID = value
		End Set
	End Property

	Private Property ClientCallbackInvocation() As String
		Get
			Return m_callbackInvocation
		End Get
		Set(ByVal value As String)
			m_callbackInvocation = value
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

	Public Property MapBuddyId() As String
		Get
			Return m_mapBuddyId
		End Get
		Set(ByVal value As String)
			m_mapBuddyId = value
		End Set
	End Property

	Public Property MapUnits() As MapUnit
		Get
			Return m_startMapUnits
		End Get
		Set(ByVal value As MapUnit)
			m_startMapUnits = value
		End Set
	End Property

	Public Property MeasureUnits() As MeasureUnit
		Get
			Return m_measureUnits
		End Get
		Set(ByVal value As MeasureUnit)
			m_measureUnits = value
		End Set
	End Property

	Public Property AreaUnits() As AreaUnit
		Get
			Return m_areaUnits
		End Get
		Set(ByVal value As AreaUnit)
			m_areaUnits = value
		End Set
	End Property

	Public Property NumberDecimals() As Double
		Get
			Return m_numberDecimals
		End Get
		Set(ByVal value As Double)
			m_numberDecimals = value
		End Set
	End Property


End Class
