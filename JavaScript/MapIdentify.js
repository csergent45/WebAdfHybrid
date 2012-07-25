
arcgisIdentifyTool = null;
var scrnpnt = null;
var identifyPosition = new Object();

ESRI.ADF.UI.MapIdentifyTool = function(element) {
	ESRI.ADF.UI.MapIdentifyTool.initializeBase(this);
	this.selectedIndex = 0;
	this.selectedValue = "";
	this._lastExpanded = false;
	this._addToResultsLink = "";
	this._layerInfo = "";
	this._dropDownList = "";
	this._identifyIcon = null;
	this._waitIcon = null;
	this._processing = false;
	this._onIdentify = Function.createDelegate(this,function(geom) {
		// don't continue if previous call is not finished
		if (!this._processing) {
			var map = $find("Map1");
			this._processing = true;
			// remove callout if it already exists
			var dropdown = $get("dropdown_" + this.get_id());
			if (dropdown!=null) dropdown.style.display = "none";
			if (this.identifyMapTips) this.identifyMapTips.dispose();
			if(this.identifyMapCallout) {
				map.removeGraphic(this.identifyMapCallout);
				this.identifyMapCallout.dispose();
			}
			// put the ajax activity indicator at cursor click and make visible
			var mappnt = new ESRI.ADF.Geometries.Point(geom.get_x(), geom.get_y());
			scrnpnt = this._map.toScreenPoint(mappnt);
			var wicon = $find(this.get_id()+'_waiticon');
			if(wicon==null) {
				this.waitIcon = $create(ESRI.ADF.Graphics.GraphicFeature,
				{"id":this.get_id()+'_waiticon',"geometry":geom,"symbol":new ESRI.ADF.Graphics.MarkerSymbol(this._waitIcon,8,8)
				});
				this.waitIcon.get_symbol().set_imageFormat("gif");
				this.waitIcon.get_symbol().set_width(16);
				this.waitIcon.get_symbol().set_height(16);
			}
			else
				this.waitIcon.set_geometry(geom);
			map.addGraphic(this.waitIcon);
			// make callback for identify
			var argument = "mode=identify&coords=" + geom.get_x() + ":" + geom.get_y();
	        if (document.documentElement.dir=="rtl") argument += "&dir=rtl";
			this.doCallback(argument,this);
		}
	});
	arcgisIdentifyTool = this;
};
ESRI.ADF.UI.MapIdentifyTool.prototype = {
	startIdentify : function() {
		arcgisIdentifyTool = this;
		this._processing = false;
		this._map.getGeometry(ESRI.ADF.Graphics.ShapeType.Point,this._onIdentify,null,'black','gray','pointer',true);
	},
	processCallbackResult : function(action,params) {
		if(action=="mappoint") {
			this.identifyLocation(this._map.get_id(), params[0], params[1], params[2]); 
		}
		else if(action=='error') {
			if(this.waitIcon) { map.removeGraphic(this.waitIcon); }
			alert("Error: " + params[0]);
			Sys.Debug.trace ('Error: '+params[0])
		}
	},
	doCallback : function(argument, context) {
		/// <summary>
		/// Performs a callback or partial postback depending on it's postback mode
		/// </summary>
		/// <param name="argument" type="String">The argument parsed back to the server control</param>
		/// <param name="context" type="string">The context of this callback</param>
		/// <returns />
		ESRI.ADF.System._doCallback(this._callbackFunctionString, this.get_uniqueID(), this.get_id(), argument, context);
	},
	get_uniqueID : function() {
		/// <value name="uniqueID" type="String">Gets or sets the unique ID used to identify the control serverside</value>
		return this._uniqueID;
	},
	set_uniqueID : function(value) {
		this._uniqueID = value;
	},
	get_callbackFunctionString : function() {
		/// <value name="callbackFunctionString" type="String">Gets or sets the callback function string used to call back to the serverside control.</value>
		/// <remarks>
		/// Executing the callbackfunctionstring will either generate a partial postback or a callback to the server, depending on the current AJAX mode.
		/// To perform a call to the servercontrol, use the <see cref="doCallback"/> method of this instance.
		/// </remarks>
		return this._callbackFunctionString;
	},
	set_callbackFunctionString : function(value) {
		this._callbackFunctionString = value;
	},
	get_map : function() {
		return this._map;
	},
	set_map : function(value) {
		this._map = value;
	} ,
	_getLayerName : function(index){
		if (this.identifyAttributes == null || this.identifyAttributes.length < (index + 1))
			return "";
		return this.identifyAttributes[index]["layer"];
	},
	_getLayerInfo : function(index){
  		if (this.identifyAttributes == null || this.identifyAttributes.length < (index + 1))
			return "";
		var mresource = this.identifyAttributes[index]["resource"];
		return ((mresource.length>0) ? (mresource +  " &gt; ") : "") + this._getLayerName(index); 
	},
	_getTitle : function(index){
		if (this.identifyAttributes == null || this.identifyAttributes.length < (index + 1))
			return "";
		var layerName = this._getLayerName(index);
		var title =  this.identifyAttributes[index]["title"];
		if (title==null || title.length==0)
			titleString = "<b>" + layerName + "</b>"; //if title is empty, use layer name     
		else
			titleString = title + "&nbsp; (" + layerName + ")";
		return titleString;
	},
	identifyLocation : function (mapID, pointX, pointY, pointAttributes) {
		this.identifyMapClientID = mapID;
		var newPt = new ESRI.ADF.Geometries.Point(pointX,pointY);
		var identifyMapPoint = newPt;
		this.identifyAttributes = pointAttributes;
		this.selectedIndex = 0;
		this.selectedValue = "";
		this._addedToResults = new Array();
		if (pointAttributes) {
			for (var a=0;a<pointAttributes.length;a++) {
				this._addedToResults.push(false);
			}
		}
		this.identifyComboBox = "<b>" + "There is no information available for this location." + "</b>";
		this._dropDownList = "";
		if (this.identifyAttributes.length==0)
		{
			this.identifyComboBox = "No Features Found";
			this._addToResultsLink = "";
			this._layerInfo = ""; 
		}
		else if (this.identifyAttributes.length == 1) {// One layer
			this.identifyComboBox = this._getTitle(0); 
			this.selectedValue =  this._getLayerName(0);
			this._addToResultsLink = '<a href="javascript: AddToTaskResults(0);">Add to Results</a>';
			this._layerInfo = this._getLayerInfo(0); 
		} else {// multiple layers
			var index = 0;
			for (var x = 0; x < this.identifyAttributes.length; x++) {	// get layer names
				if (this._getLayerName(x)==this.selectedValue) {
					index = x;
					this.selectedIndex = x;
				}
			}
			this.identifyComboBoxPopulate(index);
		}
		if (this.identifyComboBox.length==0) this.identifyComboBox = "<b>" + "No Features found at this location." + "</b>";
		
		this.identifyMapTips = new ESRI.ADF.UI.IdentifyMapTips(this);
		this.identifyMapTips.set_animate(false);
		this.identifyMapTips.set_hoverTemplate(this.identifyComboBox);
		this.identifyMapTips.set_maxheight(250);	
		this.identifyMapTips.set_width(275);
		this.identifyMapTips.initialize();
		var imcallout = $find(mapID+'_identifyicon');
		if (imcallout!=null) {
			this._map.removeGraphic(imcallout);
			imcallout.dispose();
			imcallout = null; 
		}
		this.identifyMapCallout = $create(ESRI.ADF.Graphics.GraphicFeature,
			{"id":mapID+'_identifyicon',"geometry":identifyMapPoint,"symbol":new ESRI.ADF.Graphics.MarkerSymbol(this._identifyIcon,12,24),
			"mapTips":this.identifyMapTips,
			"attributes":pointAttributes[0]});

		this.identifyMapCallout.get_symbol().set_imageFormat("png32");
		if(this.waitIcon) { this._map.removeGraphic(this.waitIcon); }	    
		this._map.addGraphic(this.identifyMapCallout);
		var callout = this.identifyMapTips.get_callout();
		// populate, position, and show the inital display
		if(this._lastExpanded && this.identifyAttributes.length>0) { this.identifyMapTips.expand(); }
		else { this.identifyMapTips.collapse(); }

		this.identifyMapTips._setContent(this.identifyMapCallout.get_attributes());
		this.identifyMapTips._dropDownBox.innerHTML = this._dropDownList;
		this.identifyMapTips.setPosition(this.identifyMapCallout.get_geometry());
		this.identifyMapTips.get_callout().show();
		
		if(!this.onMapZoom) {
			this._onMapZoom = Function.createDelegate(this, this.clearOutCallout );
			this._map.add_zoomStart(this._onMapZoom);
		}
		this._processing = false;
	},
	identifyComboBoxPopulate : function (index){
		var layerName = this.identifyMapClientID;
		var layerNameSelected = this.identifyMapClientID;
		var titleString = "";
		this._layerInfo = "";
		this._addToResultsLink = "";
		this._dropDownList = "";
		if (index==null || index=="undefined") index = 0;
		if (this.identifyAttributes.length==0) { 
			this.identifyComboBox = "No Features Found";
			this._addToResultsLink = ""; 
		} else if (this.identifyAttributes.length===1) {
			titleString = this._getTitle(0);
			this.identifyComboBox = titleString;
 			if (!this._addedToResults[0])
				this._addToResultsLink = '<a href="javascript: AddToTaskResults(0);">Add to Results</a>';
		} else {
			titleString = this._getTitle(index);
			this.identifyComboBox = '<div id="identifyTitleDiv" >';
			var ie = (Sys.Browser.agent == Sys.Browser.InternetExplorer);
			this.identifyComboBox += '<table id="identifyTitleText" style="border: solid 1px rgb(238, 238, 238);font-size: x-small; width: 225px;" onmouseover="showIdentifyDropdownButton()" cellspacing="0" cellpadding="0"><tr><td ><div id="identifyTitleTextHolder">&nbsp;' + titleString + '</div></td>';
            this.identifyComboBox += '<td align="' + (document.documentElement.dir=="rtl"?"left":"right") + '"><img id="identifyDropdownButton" src="images/dropdown-button.png" onmousedown="showIdentifyDropdown(\'' + this.get_id() + '\')" style="visibility: hidden" /></td></tr></table>';
			// populate identifyCombobox
			this._dropDownList = '<table id="identifyTitleDropdown" style="background-color: White; color: black; width: 100%; font-size: x-small;" cellspacing="0">';
			for (var x = 0; x < this.identifyAttributes.length; x++) {	// get layer names
				layerName = this._getLayerName(x);
				titleString =  this._getTitle(x);
				// generate ComboBox selection list
				this._dropDownList += '<tr><td onclick="selectIdentifyDropdownItem(\'' + this.get_id() + '\', ' + x + ', \'' + layerName + '\');" onmouseover="this.style.backgroundColor=\'#DDDDDD\'" onmouseout="this.style.backgroundColor=\'White\'" style="cursor: default">' + titleString + '</td></tr>';
			}
			this._dropDownList += '</table>';
			if (!this._addedToResults[index]) 
				this._addToResultsLink = '<a href="javascript: AddToTaskResults(' + index + ');">Add to Results</a>';
		}
		if (this.identifyComboBox.length==0) {
			this.identifyComboBox = "No Features found at this location.";
			this._layerInfo = ""; 
		} else {
			this._layerInfo = this._getLayerInfo(index); 
		}
		if (this.identifyMapCallout) this.identifyMapCallout.set_attributes(this.identifyAttributes[index]);
	},
	setToggleImage : function () {
		var toggleId = "expand_" + this.get_id();
		var toggleImage = $get(toggleId); 
		var maptip = this.identifyMapTips;
		if (maptip.get_isExpanded()) {
			toggleImage.src = "images/collapse.gif";
			toggleImage.alt = "Collapse";
		} else {
			toggleImage.src = "images/expand.gif";
			toggleImage.alt = "Expand";
		}
	},
	get_identifyIcon : function() {
		return this._identifyIcon;
	},
	set_identifyIcon : function(value) {
		this._identifyIcon = value;
	},
	get_waitIcon : function() {
		return this._waitIcon;
	},
	set_waitIcon : function(value) {
		this._waitIcon = value;
	},
	clearOutCallout : function() { 
		if (this.identifyMapTips) this.identifyMapTips.dispose();
		if(this.identifyMapCallout) {
			this._map.removeGraphic(this.identifyMapCallout);
			this.identifyMapCallout.dispose();
		}
	},
	 //get the page height
	getPageHeight : function() {
		var height = window.innerHeight;
		if (height == null) {
			if (document.documentElement && document.documentElement.clientHeight)
				height = document.documentElement.clientHeight;
			else
				height = document.body.clientHeight;
		}
		return height;	
		
	}
};
ESRI.ADF.UI.MapIdentifyTool.registerClass('ESRI.ADF.UI.MapIdentifyTool', Sys.Component);

ESRI.ADF.UI.IdentifyMapTips = function(idTool) {
	ESRI.ADF.UI.IdentifyMapTips.initializeBase(this);
	this._onMouseGfxOverHandler = Function.createDelegate(this,function() { }); 
	this._onZoomStartHandler = Function.createDelegate(this, function() { if(arcgisIdentifyTool.identifyMapTips.get_callout()) { arcgisIdentifyTool.identifyMapTips.get_callout().hide(); } window.clearTimeout(arcgisIdentifyTool.identifyMapTips._showDelayedTimer); });
	this.set_maxheight(300);
	this.set_autoHide(false);
	this.set_expandOnClick(false);
	this._idTool = idTool;
	var elmID = this._idTool.get_id();
	this._maptipExpandID = "expand_"+elmID;
	this._maptipCloseID = "close_"+elmID;
	this._maptipLayerInfoID = "layerinfo_"+elmID;
	this._maptipAddLinkID = "addlink_"+elmID;
	this._maptipDropdownID = "dropdown_"+elmID;	
	this._closeUrl = (isIE && ieVersion<7) ? "images/dismiss_solid.png" : "images/dismiss.png";
};

ESRI.ADF.UI.IdentifyMapTips.prototype = {
	initialize : function() {
		ESRI.ADF.UI.IdentifyMapTips.callBaseMethod(this, 'initialize');
		this._dropDownBox = this.createIdentifyDropdown();
	},
	createTemplate : function() {
		//overrides base.createTemplate
		var template = '<div style="position: relative"><table cellpadding="0" cellspacing="0" style="position: relative; width:275px; background-color:White;border: solid 1px #999;">';
		template +='<tr style="padding:2;margin:0"><td style="background-color:#eee;border-bottom: 1px solid #aaa;"><table cellpadding="0" cellspacing="0" style="width: 100%"><tr>';
		template += '<td style="color: Black;" id="'+ this.get_maptipTitleID() +'" >{@title}</td>';
		template +='<td align="' + (document.dir=="rtl" ? 'left' : 'right') + '"><img id="' + this._maptipExpandID + '" src="images/expand.gif" alt="Expand" onmousedown="expandCollapseIdentify(\''+this._idTool.get_id()+'\')"   /><img id="' + this._maptipCloseID + '" src="' + this._closeUrl + '" alt="Close" onmousedown="closeIdentifyPanel(this)"/></td></tr></table></td></tr>';
		template +='<tr id="identify_content_tr" ><td style="padding:0; background-color: #eee"><div id="'+this.get_maptipContentID()+'" style="overflow:auto; overflow-x: auto; margin: 1px 3px 5px 3px; background-color: White; display: '+(this.get_isExpanded()?'block':'none')+'; color: Black">{@content}</div>';
		template +='<div id="'+this._maptipLayerInfoID+'" style="display: '+(this.get_isExpanded()?'block':'none')+'; background-color: #eee; padding: 0px 0px 3px 6px; font-size: xx-small; color: Black;">{@layerinfo}</div>';
		template +='<div id="'+this._maptipAddLinkID+'" style="display: '+(this.get_isExpanded()?'block':'none')+'; background-color: #eee; padding: 0px 0px 3px 6px; font-size: xx-small; color: Black">{@link}</div>';
		template +='</td></tr></table>';
		template +='</div>';
		return template;
	},
    _setContent : function(attributes){
		var contents;
		if (attributes != null) 
			contents = attributes["contents"];
		else
			contents = ""; 
		this.get_callout().setContent({"title":this._idTool.identifyComboBox,"content":contents, "layerinfo": this._idTool._layerInfo, "link": this._idTool._addToResultsLink});
		this._updateLinks(this.get_isExpanded());
	},
	updateContent : function() {
		/// <summary>Sets the content of the maptip</summary>
		/// <param name="func" type="Object">Function to execute</param>
		var attributes = null;
		if (this._currentElement && this._currentElement.get_attributes()) {
			attributes = this._currentElement.get_attributes();
		} else {
			attributes = this._idTool.identifyAttributes[this._idTool.selectedIndex];
		}
		this._dropDownBox.innerHTML = this._idTool._dropDownList;
		this._setContent(attributes);
		this._setPngForIE6(this._maptipCloseID, "images/dismiss.png");
		this.adjustHeight();
	},
	_updateLinks : function(show) {
		var layerinfo = $get(this._maptipLayerInfoID);
		var addlink = $get(this._maptipAddLinkID);
		var toggleImage = $get(this._maptipExpandID);
		if(layerinfo && addlink && toggleImage) {
			if(show) {
				layerinfo.style.display='';
				addlink.style.display='';
				toggleImage.src = "images/collapse.gif";
				toggleImage.alt = "Collapse";
			} else {
				layerinfo.style.display='none';
				addlink.style.display='none';
				toggleImage.src = "images/expand.gif";
				toggleImage.alt = "Expand";
			}
		}
		this.adjustHeight();
	},
	expand : function() {
		this._updateLinks(true);
		ESRI.ADF.UI.IdentifyMapTips.callBaseMethod(this, 'expand');
	},
	collapse : function() {
		this._updateLinks(false);
		ESRI.ADF.UI.IdentifyMapTips.callBaseMethod(this, 'collapse');
	},
	_setPngForIE6 : function (imgId, newSource) {
		if (Sys.Browser.agent === Sys.Browser.InternetExplorer && Sys.Browser.version<7) {
			var imgElement = $get(imgId);
            ESRI.ADF.System.setIEPngTransparency(imgElement, newSource,true);
		}
	},
	createIdentifyDropdown : function() {
		var div = $get(this._maptipDropdownID);
		if (div===null) {
			div = document.createElement("div");
			div.id = this._maptipDropdownID;
			div.style.position= "absolute";
			div.style.zIndex="10005";
			div.style.left="-1000px";
			div.style.top= "100px";
			div.style.border =  "solid 1px rgb(77, 77, 77)";
			div.style.backgroundColor = "White";
			div.style.width = "223px";
			div.style.overflowY = "auto";
			div.style.height = "auto";
			div.style.display = "none";
			document.body.appendChild(div);
		}
		return div;
	}
};
ESRI.ADF.UI.IdentifyMapTips.registerClass('ESRI.ADF.UI.IdentifyMapTips', ESRI.ADF.UI.MapTips);

function identifyExpandDetails(idToolId) {
	var idTool = $find(idToolId); 
	idTool.identifyComboBoxPopulate(idTool.selectedIndex); 
	idTool.setToggleImage();
	idTool.identifyMapTips.expand();
	idTool.identifyMapTips.updateContent();
};

function expandCollapseIdentify(idToolId) {
	hideIdentifyDropdown(idToolId);
	var idTool = $find(idToolId);
	var maptip = idTool.identifyMapTips;
	if (maptip.get_isExpanded()) {
		maptip.collapse();
		idTool._lastExpanded = false;
	} else {
		identifyExpandDetails(idToolId);
		maptip.expand();
		idTool._lastExpanded = true;
	}
};

function closeIdentifyPanel() {
	var dropdown = $get("dropdown_" + arcgisIdentifyTool.get_id());
	if (dropdown) dropdown.style.display = "none";
	arcgisIdentifyTool.clearOutCallout();
};

function toggleIdentifyDropdown(visibility) {
	var dropdown = $get("identifyComboBox");
	var idtitle = $get("identifyTitleText");
	if (dropdown!=null && idtitle!=null) { 
		if (visibility=="visible") {
			dropdown.style.display = "";
			idtitle.style.display = "none";
		} else {
			dropdown.style.display = "none";
			idtitle.style.display = "";
		}
	}
};

function hideIdentifyDropdown(id) {
	var dropdown = $get("dropdown_" + id);
	var textbox = $get("identifyTitleText");
	var button = $get("identifyDropdownButton"); 
	dropdown.style.display = "none";
	if (button!=null) button.style.visibility = "hidden"; 
	if (textbox!=null) textbox.style.borderColor = "rgb(238, 238, 238)";
	return false;
};

function showIdentifyDropdown(id) {
	var textbox = $get("identifyTitleText");
	var dropdown = $get("dropdown_" + id);
	var rect = GetElementRectangle(textbox); 
	var leftpos = (Sys.Browser.agent === Sys.Browser.InternetExplorer) ? rect.left-2 : rect.left;
	var toppos = (Sys.Browser.agent === Sys.Browser.InternetExplorer) ? rect.bottom-2 : rect.bottom;  
	dropdown.style.left = leftpos + "px";
	dropdown.style.top = toppos + "px";
	dropdown.style.height = "auto";
	if (dropdown.style.display==="none") 
		dropdown.style.display = "";
	else
		dropdown.style.display="none";
	var bounds = Sys.UI.DomElement.getBounds(dropdown);
	var ddLength = bounds.height;
	var pHeight = arcgisIdentifyTool.getPageHeight();
	if (pHeight < ddLength + toppos) {
	    var ddHeight = pHeight - toppos;
	    if (ddHeight < 160) {
	        ddHeight = ddLength > toppos ? toppos : ddLength;
	        toppos -=ddHeight;
            dropdown.style.top = toppos + "px";
	    } 
	    dropdown.style.height = ddHeight + "px";
	}
	return false;
};

function showIdentifyDropdownButton() {
	var textbox = $get("identifyTitleText");
	textbox.style.borderColor = "rgb(77, 77, 77)";
	var button = $get("identifyDropdownButton");
	button.style.visibility = "visible";
	return false;
};

function selectIdentifyDropdownItem(idToolId, index,layer) {
	var idTool = $find(idToolId);
	var idText = $get("identifyTitleTextHolder"); 
	idTool.selectedIndex = index;
	idTool.selectedValue = layer;
	 var titleText = layer;
	if (titleText.length==0) titleText = idTool._getLayerName(index);
	if (idText!=null) idText.innerHTML = "&nbsp;" + titleText;
	identifyExpandDetails(idToolId);
	hideIdentifyDropdown(idToolId);
	return false; 
};


function AddToTaskResults(index) {
	var link = $get(arcgisIdentifyTool.identifyMapTips._maptipAddLinkID);
	if (link!=null) {
		link.innerHTML = ""; 
		arcgisIdentifyTool.identifyMapTips.adjustHeight();
	}
	arcgisIdentifyTool._addedToResults[index] = true; 
	var argument = "mode=addresults&index=" + index;
	arcgisIdentifyTool.doCallback(argument, arcgisIdentifyTool);
};