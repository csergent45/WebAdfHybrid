// display_measure.js

var m_currentMeasureToolbarTool = "polyline";
var m_measureToolbarImagePath = "images/";
var m_measureToolbarImageExtension = ".gif";
var m_measureDisplay = "MeasureDisplay";
var m_measureToolbarId = "MeasureToolbar";
var m_measureLengthsTotal = 0.0;
var m_measureAreasTotal = 0.0;
var m_measureXOffset = 0;
var m_measureYOffset = 0;
var m_MeasureTypes = new Array();
m_MeasureTypes[0] = "point";
m_MeasureTypes[1] = "polyline";
m_MeasureTypes[2] = "polygon";
var m_measureMoveFunction = null;
var m_measureCoords = "";
var m_measureLastCoords = "";
var m_measureMouseUpSet = false;

var m_measureToolbar = null;
var m_measureGraphicFeature = null;

function checkMeasureToolbarBorder(cell, type) {
    if (type.toLowerCase()==m_currentMeasureToolbarTool)
        cell.style.borderColor = "Black";
    else
        cell.style.borderColor = "White";  
}

// set current measure tool
function setMeasureToolbarTool(type) {
	m_currentMeasureToolbarTool = type.toLowerCase();
	var cellObj;
	var buttonId = "";
	for (var i=0; i<m_MeasureTypes.length; i++) {
		buttonId = "MeasureToolbarButton_" + m_MeasureTypes[i];
		cellObj = document.getElementById(buttonId);
		if (cellObj!=null) {
			if (m_MeasureTypes[i]==m_currentMeasureToolbarTool) {
				cellObj.style.borderColor = "Black";
				cellObj.style.backgroundColor = "#EEEEEE";
				startMeasure();
			}
			else {
				cellObj.style.borderColor = "White";
				cellObj.style.backgroundColor = "White";
			}
		}
	}
}


// Polyline Measure action ... for distances
function MeasurePolyline(map) {
	if (map!=null) {
		map.getGeometry(ESRI.ADF.Graphics.ShapeType.Path,MapCoordsClick,null,'black','blue','crosshair', true);
        vectortoolbar = "MeasureToolbar";
	}
}

// Polygon Measure action ... for areas
function MeasurePolygon(map) {
	if (map!=null) {
		map.getGeometry(ESRI.ADF.Graphics.ShapeType.Ring,MapCoordsClick,null,'black','black','crosshair', true);        
        vectortoolbar = "MeasureToolbar";
	}
}

// Point Measure action ... for location coordinates
function MeasurePoint(map) {
	if (map!=null) {
		map.getGeometry(ESRI.ADF.Graphics.ShapeType.Point,MapCoordsClick,null,'black',null,'pointer', true);        
	}    
}

// Handler for MeasurePoint clicks
function MapCoordsClick(geom, evtArgs) {
	var geomString = '';
	var type = '';
    removeMeasureGraphic();
    var style = null; 
	if(ESRI.ADF.Geometries.Point.isInstanceOfType(geom)) {
		geomString = geom.toString(':');
		type = 'point';
		style = new ESRI.ADF.Graphics.MarkerSymbol("images/crosshair.png",6,6);
	}
	else if(ESRI.ADF.Geometries.Polyline.isInstanceOfType(geom)) {
		geomString = geom.getPath(0).toString('|',':');
		type = 'polyline';
		style = new ESRI.ADF.Graphics.LineSymbol("black",2);
	}
	else if(ESRI.ADF.Geometries.Polygon.isInstanceOfType(geom)) {
		geomString = geom.getRing(0).toString('|',':');
		type = 'polygon';
		style = new ESRI.ADF.Graphics.FillSymbol("black","black",2);	
		style.set_opacity(0.2);	
	}
	m_measureGraphicFeature = $create(ESRI.ADF.Graphics.GraphicFeature,
		        {"id": "MeasurePointIcon","geometry":geom,"symbol":style});
	map.addGraphic(m_measureGraphicFeature);        
	coordString = geomString;
	  	
	m_measureLastCoords = m_measureCoords;
	m_measureCoords = "";
	var argument = 'ControldID='+map.get_id()+'&EventArg='+type+'&coords='+geomString+'&VectorMode=measure';	
    if (checkForFormElement(document, 0, "MeasureUnits")) argument += "&MeasureUnits=" + document.forms[0].MeasureUnits.value;
    if (checkForFormElement(document, 0, "AreaUnits")) argument += "&AreaUnits=" + document.forms[0].AreaUnits.value;
    if (checkForFormElement(document, 0, "MapUnits")) argument += "&MapUnits=" + document.forms[0].MapUnits.options[document.forms[0].MapUnits.selectedIndex].value;

	var context = this;
	eval(measureVectorCallbackFunctionString);
}

function MeasureCoordsMouseUp(sender, args) {
    m_measureToolbar = $get(m_measureToolbarId);
    if (m_measureToolbar!=null) {
        m_measureToolbar.style.display = "";
    }
    if (m_currentMeasureToolbarTool!="point") {
        var coords = args.coordinate;
        if (coords!=null && !isNaN(coords.get_x()) && !isNaN(coords.get_y())) {
            // ignore null or non-numeric input
            if (m_measureCoords.length>0) {
                if (args.button==Sys.UI.MouseButton.rightButton) {
                    var pos = m_measureCoords.lastIndexOf("|");
                    m_measureCoords =  m_measureCoords.substring(0,pos);
                } else if (args.button==Sys.UI.MouseButton.leftButton){  
                    m_measureCoords += (m_measureCoords.length>0 ? "|" : "") + coords.get_x() + ":" + coords.get_y();
                } else 
                    return;
	            var argument = 'ControldID='+map.get_id()+'&EventArg='+m_currentMeasureToolbarTool+'&coords='+m_measureCoords+'&VectorMode=measure';	
                if (checkForFormElement(document, 0, "MeasureUnits")) argument += "&MeasureUnits=" + document.forms[0].MeasureUnits.value;
                if (checkForFormElement(document, 0, "AreaUnits")) argument += "&AreaUnits=" + document.forms[0].AreaUnits.value;
                if (checkForFormElement(document, 0, "MapUnits")) argument += "&MapUnits=" + document.forms[0].MapUnits.options[document.forms[0].MapUnits.selectedIndex].value;

	            var context = this;
	            eval(measureVectorCallbackFunctionString);
	        } else {
                removeMeasureGraphic();
                if (args.button==Sys.UI.MouseButton.rightButton)
                    m_measureCoords = "";
                else if (args.button==Sys.UI.MouseButton.leftButton)
	                m_measureCoords = coords.get_x() + ":" + coords.get_y(); 
	        }
	        m_measureLastCoords = m_measureCoords;
//	    } else {
//	        window.status = "no coords";
	    }
	}
}

function removeMeasureGraphic() {
    if (m_measureGraphicFeature!=null) {
        map.removeGraphic(m_measureGraphicFeature);
        m_measureGraphicFeature.dispose();
        m_measureGraphicFeature = null;
    }

}

// measure tool is selected... call current type (polyline for distance, polygon for area)
function startMeasure() {
    var md;
    if (m_measureDisplay!=null) {
        md = $get(m_measureDisplay);
        m_measureToolbar = $get(m_measureToolbarId);
        m_measureToolbar.style.display = "";
    }
	if (m_currentMeasureToolbarTool=="point") {
        if (md!=null) md.innerHTML = "Click on the map to return the coordinate location of the point.<br />";
		MeasurePoint(map);
	} else if (m_currentMeasureToolbarTool=="polyline") {
        if (md!=null) md.innerHTML = "Click on the map and draw a line. Double-click to end the line.<br />";
		MeasurePolyline(map);
	} else {
        if (md!=null) md.innerHTML = "Click on the map and draw a polygon. Double-click to end the polygon.<br />";
		MeasurePolygon(map);
	}
	if (!m_measureMouseUpSet) {
	    map.add_mouseUp(MeasureCoordsMouseUp);
	    m_measureMouseUpSet = true;
	}
}
function measureComplete(result,id,area,perimeter,segment, totaldistance) {
	var md = $get(m_measureDisplay);
	if(result) {
		md.innerHTML = result;
	}
	else {
		//just replace values
		var tdperimeter = $get("tdperimiter");
		var tdarea = $get("tdarea");
		var tdsegment = $get("tdsegment");
		var tdtotaldistance = $get("tdtotaldistance");
		
		if(tdarea) { tdarea.innerHTML = area; }
		if(tdperimeter) { tdperimeter.innerHTML = perimeter; }
		if(tdsegment && segment) { tdsegment.innerHTML = segment; }
		if(tdtotaldistance && totaldistance) { tdtotaldistance.innerHTML = totaldistance; }
	}
}
function closeMeasureToolbarTool(id) {
    hideMeasureToolbarTool(id);
    map.cancelGetGeometry();
    map.remove_mouseUp(MeasureCoordsMouseUp); 
    m_measureMouseUpSet = false; 
 }

function hideMeasureToolbarTool(id) {
    m_measureToolbar = $get(m_measureToolbarId);
    if (m_measureToolbar!=null) {
        m_measureToolbar.style.display = "none";
        removeMeasureGraphic();

    }
} 

// update distance unit settings... request new totals from server
function changeMeasureUnits() {
    var f = document.forms[docFormID];
    var i = f.MeasureUnits2.selectedIndex;
    var m = f.MeasureUnits2.options[i].value;
    f.MeasureUnits.value = m; 
    if (coordString==null) coordString="";
    var argument = "ControlID=" + map.get_id() + "&EventArg=" + m_currentMeasureToolbarTool + "&ControlType=Map&coords=" + m_measureLastCoords + "&VectorMode=measure&VectorAction=AddPoint&MeasureUnits=" + m + "&refresh=true";
   if (checkForFormElement(document, 0, "AreaUnits")) {
        argument += "&AreaUnits=" + f.AreaUnits.value ;
   } 
    var context = map.get_id() + "," + m_currentMeasureToolbarTool;

    eval(measureVectorCallbackFunctionString);    
}

// update area unit settings... request new totals from server
function changeAreaUnits() {
    var f = document.forms[docFormID];
    var i = f.AreaUnits2.selectedIndex;
    var a = f.AreaUnits2.options[i].value;
    f.AreaUnits.value = a
    coordString = map.coords;
    if (coordString==null) coordString="";
    var argument = "ControlID=" + map.get_id() + "&EventArg=" + m_currentMeasureToolbarTool + "&ControlType=Map&coords=" + m_measureLastCoords + "&VectorMode=measure&VectorAction=AddPoint&AreaUnits=" + a + "&refresh=true";
   if (checkForFormElement(document, 0, "MeasureUnits")) {
        argument += "&MeasureUnits=" + f.MeasureUnits.value;
   } 
    var context = map.get_id() + "," + m_currentMeasureToolbarTool;

	eval(measureVectorCallbackFunctionString);    
}

// event handler for starting to drag toolbar around... mouse down
function dragMeasureToolbarStart(e) {
    m_measureToolbar = $get("MeasureToolbar");
    if (m_measureToolbar!=null) {
        var box = calcElementPosition(m_measureToolbar.id);
        m_measureXOffset =e.clientX - box.left;
        m_measureYOffset = e.clientY - box.top;
    }
    $addHandler(document, "mousemove", dragMeasureToolbarMove);
    $addHandler(document, "mouseup", dragMeasureToolbarStop);  
    e.preventDefault();
    e.stopPropagation();
}

// event handler for toolbar drag movement... mousemove
function dragMeasureToolbarMove(e) {
    m_measureToolbar.style.left = (e.clientX-m_measureXOffset) + "px";;
    m_measureToolbar.style.top = (e.clientY-m_measureYOffset) + "px";
    e.preventDefault();
    e.stopPropagation();
}

// event handler for end of toolbar drag movement... mouseup
function dragMeasureToolbarStop(e) {
    $removeHandler(document, "mousemove", dragMeasureToolbarMove);
    $removeHandler(document, "mouseup", dragMeasureToolbarStop);  
    e.preventDefault();
    e.stopPropagation();
}

// set up the images for transparency in IE6
function setIE6MeasureToolbarImages() {
    var imageId = "";
    var imgSrc = ""; 
    var imgObj = document.images["MeasureToolbar_CloseButton"];
    if (imgObj!=null) {
        imgObj.src = "images/blank.gif";
        imgObj.style.filter =  "progid:DXImageTransform.Microsoft.AlphaImageLoader(src=/aspnet_client/ESRI/WebADF/images/dismiss.png)";
    }
    for (var i=0; i<m_MeasureTypes.length; i++) {
	    imageId = "ToolbarImage_" + m_MeasureTypes[i];
	    imgObj = document.images[imageId];
	    if (imgObj!=null) {
	        imgSrc = imgObj.src;
            imgObj.src = "images/blank.gif";
            imgObj.style.filter =  "progid:DXImageTransform.Microsoft.AlphaImageLoader(src=" + imgSrc + ")";
	    }
    }
}



