// JScript File
var arcgisWebApp;
var isRTL = (document.documentElement.dir == "rtl"); // find out if the document has been set to right-to-left
var isIE = (Sys.Browser.agent == Sys.Browser.InternetExplorer);

/// <reference assembly="System.Web.Extensions" name="MicrosoftAjax.js"/>
Type.registerNamespace('ESRI.ADF.WebMappingApplication');
// function run at startup
function startUp() {
    if (isRTL) document.images["CollapseImage"].src = "images/expand_right.gif";
    arcgisWebApp.MapId = "Map1";
    arcgisWebApp.map = $find(arcgisWebApp.MapId);
    map = arcgisWebApp.map;
    // set up references to the console panels
    arcgisWebApp.ResultsPanelResize = $find("Results_Panel_ResizeBehavior");
    arcgisWebApp.TocPanelResize = $find("Toc_Panel_ResizeBehavior");
    // set up task results panel to auto open on update... 
    var taskResults = $find("TaskResults1");
    var adjustMapDisplay = false;
    taskResults.add_taskResultNodeInserted(function() {
        var panel = $get("Results_Panel_Collapse");
        if (panel && panel.style.display == "none") {
            expandConsolePanel("Results_Panel");
            arcgisWebApp.MapDisplay.style.left = getMapLeft(); +"'px";
            adjustMapDisplay = true;
        } else
            arcgisWebApp.adjustMapSize();
        if (!isRTL) adjustMapAndCopyrightPosition(adjustMapDisplay);
    });
    var panel = $get("Results_Panel_Collapse");
    if (panel != null) panel.style.display = "none";
    // because the toolbar is listed before the map control,
    //  make sure toolbar will be listening to changes in map extent
    var toolbar = $find("Toolbar1");
    if (map != null && toolbar != null) {
        map.remove_extentChanged(toolbar._toolbarExtentChangedHandler); // just in case it is already present
        map.add_extentChanged(toolbar._toolbarExtentChangedHandler);
        toolbar.add_onToolSelected(OnToolSelectHandler);
        window.setTimeout("resetMapHistory();", 1500);
    }
    // hide identify window when extent changes, especially when back or forward are clicked
    map.add_extentChanging(function() { if (arcgisIdentifyTool != null) closeIdentifyPanel(); }); // for start of continuous panning or zoom animation
    map.add_extentChanged(function() { if (arcgisIdentifyTool != null) closeIdentifyPanel(); });  // extent changes that do not trigger ExentChanging
    // set the copyright callout template 
    window.setTimeout("setupCopyrightText();", 100);
    arcgisWebApp.setTocHeight();
    if (arcgisWebApp.Toc == null)
        arcgisWebApp.TocPanel.style.display = "none";
    map.add_mouseMove(MapCoordsMouseMove);
    var ov = $find("OverviewMap1");
    if (ov != null) {
        toggleOverviewMap();
        if (isRTL) {
            var ovExt = $find("DockExtender4");
            ovExt.set_alignment(ESRI.ADF.System.ContentAlignment.TopLeft);
        }
    }
    if (!isRTL) {
        arcgisWebApp.TocPanelResize.add_resize(adjustMapAndCopyrightPosition);
        arcgisWebApp.ResultsPanelResize.add_resize(adjustMapAndCopyrightPosition);
    } else {
        adjustRTLElements();
    }
    // set window resize event handler
    window.setTimeout('$addHandler(window,"resize", arcgisWebApp.adjustMapSizeHandler);', 1000);
}

function resetMapHistory() {
    map = arcgisWebApp.map;
    var toolbar = $find("Toolbar1");
    map._currentExtentHistory = 0;
    var tbElem = Toolbars[toolbar._uniqueID];
    var backButton = tbElem.items[tbElem.btnMapBack];
    if (backButton) { backButton.disabled = true; }
    var forwardButton = tbElem.items[tbElem.btnMapForward];
    if (forwardButton) { forwardButton.disabled = true; }
    tbElem.refreshCommands();
}

// function to request closing of session items.... only called if at least one resource is local non-pooled
function CloseOut(logout) {
    var r = Math.random().toString();
    PageMethods.CleanUp(r, CloseOutResponse, CloseOutResponse);
}

// response function to close out browser ... request sent to server by CloseOut()
function CloseOutResponse(response) {
    window.close();
    // if user selects No/Cancel in close dialog, send to close page 
    document.location = response;
}

function LogOut() {
    var r = Math.random().toString();
    PageMethods.CleanUp(r, LogOutResponse, LogOutResponse);
}

function LogOutResponse(response) {
    // loginstatus should send back to login page
    document.location = "Login.aspx";
}

function toggleMagnifier() {
    var mag = $get("Magnifier1");
    if (mag != null) {
        toggleFloatingPanelVisibility('Magnifier1');
    } else
        alert("Magnifier is not available");

}

function toggleOverviewMap() {
    var ovm = $find("OverviewMap1");
    var toolbarobj = $find("Toolbar1");
    var toolbar = (toolbarobj != null) ? Toolbars[toolbarobj._uniqueID] : null;
    var imageTag = "Toolbar1OverviewMapToggleImage";
    var toolbarItemName = "OverviewMapToggle";
    var showImage = (isRTL ? "images/show-overview-map2.png" : "images/show-overview-map.png");
    var hideImage = (isRTL ? "images/hide-overview-map2.png" : "images/hide-overview-map.png");
    var img = document.images[imageTag];
    if (ovm) {
        if (ovm.isVisible()) {
            ovm.hide();
            if (toolbar != null) {
                toolbar.items[toolbarItemName].selectedImage = showImage;
                toolbar.items[toolbarItemName].defaultImage = showImage;
                toolbar.items[toolbarItemName].hoverImage = showImage;
                img.alt = "Show OverviewMap";
                img.title = "Show OverviewMap";
                switchImageSourceAndAlphaBlend(img, toolbar.items[toolbarItemName].defaultImage);
            }
        } else {
            ovm.show();
            if (toolbar != null) {
                toolbar.items[toolbarItemName].selectedImage = hideImage;
                toolbar.items[toolbarItemName].defaultImage = hideImage;
                toolbar.items[toolbarItemName].hoverImage = hideImage;
                img.alt = "Hide OverviewMap";
                img.title = "Hide OverviewMap";
                switchImageSourceAndAlphaBlend(img, toolbar.items[toolbarItemName].defaultImage);
            }
            if (isRTL) adjustRTLElements();
        }
    }
}

function toggleConsolePanel(panelName) {
    var panel = $get(panelName + "_Collapse");
    if (panel) {
        if (panel.style.display == "none") {
            expandConsolePanel(panelName);
        } else {
            collapseConsolePanel(panelName);
        }
        arcgisWebApp.setTocHeight();
        window.setTimeout("arcgisWebApp.MapDisplay.style.left = getMapLeft(); + 'px';", 500);
    }
}

function expandConsolePanel(panelName) {
    var panel = $get(panelName + "_Collapse");
    var image = (panelName + "_Image");
    panel.style.display = "";
    image.src = "images/collapse.gif";
    image.alt = "Collapse";
}

function collapseConsolePanel(panelName) {
    var panel = $get(panelName + "_Collapse");
    var image = $get(panelName + "_Image");
    panel.style.display = "none";
    image.src = "images/expand.gif";
    image.alt = "Expand";
}

function backForward(value) {
    map = $find("Map1");
    map.stepExtentHistory(value);
}

function setupCopyrightText() {
    var crBounds = Sys.UI.DomElement.getBounds(arcgisWebApp.CopyrightText);
    var pHeight = arcgisWebApp.getPageHeight();
    arcgisWebApp.CopyrightText.style.top = (pHeight - crBounds.height) + "px";
    var cr = $find("MapCopyrightText1");
    if (cr != null) {
        var template = '<div style="width: 300px;background-color:#eee;border: solid 1px #aaa;padding: 0px; margin:0;" ><div style="background:#eee; margin:0; height: 20px; color: black">';
        template += '<span style="float: ' + (isRTL ? 'right' : 'left') + '; padding: 0px 0px 0px 2px;"><b>{@title}</b></span>';
        template += '<span style="float: ' + (isRTL ? 'left' : 'right') + '; cursor: pointer; padding: 0px 2px 0px 0px;" onclick="ESRI.ADF.UI.MapCopyrightText.closeClick(\'' + cr.get_id() + '\');"><img id="CopyrightTextClose" src="images/dismiss.png" alt="Close" /></span>';
        template += '</div><div style="padding: 2px 2px 2px 2px; background-color: white; color: black; font-size:x-small;">{@content}</div></div>';
        cr.get_callout().set_template(template);
        cr.get_callout().set_animate(false);
    }
}

function MapCoordsMouseMove(sender, args) {
    var coords = args.coordinate;
    var roundFactor = Math.pow(10, arcgisWebApp.CoordsDecimals);
    window.status = (Math.round(coords.get_x() * roundFactor) / roundFactor) + ", " + (Math.round(coords.get_y() * roundFactor) / roundFactor);
}

function OnToolSelectHandler(sender, args) {
    if (args.name) {
        var mode = args.name;
        arcgisWebApp.currentMode = mode;
        if (mode != "Measure" && arcgisWebApp.lastMode == "Measure") closeMeasureToolbarTool();
        if (mode != "MapIdentify" && arcgisWebApp.lastMode == "MapIdentify") {
            if (arcgisIdentifyTool != null) closeIdentifyPanel();
        }
        arcgisWebApp.lastMode = mode;
    }
}

function GetElementRectangle(element) {
    var rect = null;
    if (isIE)
        rect = element.getBoundingClientRect();
    else {
        var bounds = Sys.UI.DomElement.getBounds(element);
        rect = { "left": bounds.x, "top": bounds.y, "right": bounds.x + bounds.width, "bottom": bounds.y + bounds.height };
    }
    return rect;
}

function setMapLeftPosition() {
    if (isRTL)
        setDividerLeftPosition();
    else
        setMapWidth();
}

function getMapLeft() {
    var bounds = Sys.UI.DomElement.getBounds(arcgisWebApp.ToggleDisplay);
    var mLeft = isRTL ? 0 : bounds.x + bounds.width;
    return mLeft;
}

function getMapWidth() {
    var bounds = Sys.UI.DomElement.getBounds(arcgisWebApp.ToggleDisplay);
    var mLeft = 0;
    var bounds2 = Sys.UI.DomElement.getBounds(arcgisWebApp.PanelDisplay);
    var pWidth = arcgisWebApp.getPageWidth();
    var mWidth = 5;
    if (isRTL) {
        if (arcgisWebApp.PanelDisplay.style.display == "none")
            mWidth = pWidth - bounds.width;
        else
            mWidth = pWidth - bounds2.width - 8;

    } else {
        if (arcgisWebApp.PanelDisplay.style.display == "none")
            mLeft = arcgisWebApp.PanelDisplayCell.clientWidth;
        else
            mLeft = arcgisWebApp.PanelDisplay.clientWidth + bounds.width;
        mWidth = arcgisWebApp.getPageWidth() - mLeft;
    }
    if (mWidth < 5) mWidth = 5;
    return mWidth;
}

function setMapWidth() {
    var mLeft = getMapLeft();
    arcgisWebApp.MapDisplay.style.left = mLeft + "px";
    var mWidth = getMapWidth();
    if (mWidth < 5) mWidth = 5;
    arcgisWebApp.MapDisplay.style.width = mWidth + "px";
    arcgisWebApp.CurrentMapWidth = mWidth;
}

function setDividerLeftPosition() {
    var mWidth = getMapWidth();
    arcgisWebApp.MapDisplay.style.left = "0px";
    arcgisWebApp.PanelDisplayCell.style.left = mWidth + "px";
    arcgisWebApp.MapDisplay.style.width = mWidth + "px";
    arcgisWebApp.CurrentMapWidth = mWidth;
}


function adjustMapAndCopyrightPosition(adjustMapDisplay) {
    if (adjustMapDisplay == null) adjustMapDisplay = false;
    var bounds = Sys.UI.DomElement.getBounds(arcgisWebApp.ToggleDisplay);
    var mLeft = bounds.x + bounds.width;
    if (arcgisWebApp.CopyrightText) {
        var crLeft = parseInt(arcgisWebApp.CopyrightText.style.left);
        arcgisWebApp.CopyrightText.style.left = mLeft + "px";
    }
    if (adjustMapDisplay)
        arcgisWebApp.MapDisplay.style.left = mLeft + "px";
}

function adjustRTLElements() {
    // these elements must be manually adjusted in right-to-left mode 
    if (arcgisWebApp.CopyrightText) {
        arcgisWebApp.CopyrightText.style.left = "0px";
    }
    var ovm = $get("OverviewMap1");
    if (ovm)
        ovm.style.left = "0px";
    if (!isIE) {
        var dbounds = Sys.UI.DomElement.getBounds(arcgisWebApp.ToggleDisplay);
        var navp = $get("Navigation_Panel");
        if (navp) {
            var nbounds = Sys.UI.DomElement.getBounds(navp);
            navp.style.left = (dbounds.x - nbounds.width) + "px";
        }
    }
    var tmenu = $get("TaskMenu");
    if (tmenu) tmenu.className = "appFloat1";
}

// Common webpage object for manipulating page elements
ESRI.ADF.WebMappingApplication.WebPage = function() {
    this.MapId = "Map1";
    this.map = null;
    this.MapDisplay = $get("Map_Panel");
    this.PanelDisplay = $get("LeftPanelCellDiv");
    this.PanelDisplayCell = $get("LeftPanelCell");
    this.PanelScrollDiv = $get("LeftPanelScrollDiv");
    this.ToggleDisplay = $get("ToggleCell");
    this.PanelSlider = $get("PanelSlider");
    this.PanelDisplayTableCell = $get("LeftPanelTableCell");
    this.PanelBottomSlider = $get("PanelSliderBottom");
    this.ResultsPanel = $get("Results");
    this.TocPanel = $get("Toc_Panel");
    this.Results = $get("TaskResults1");
    this.Toc = $get("Toc1");
    this.ResultsPanelContents = $get("Results_Panel_Body");
    this.TocPanelContents = $get("Toc_Panel_Body");
    this.ResultsPanelResize = $find("Results_Panel_ResizeBehavior");
    this.TocPanelResize = $find("Toc_Panel_ResizeBehavior");
    this.NavigationTool = $get("Navigation1");
    this.CopyrightText = $get("Copyright_Panel");
    this.ZoomLevel = $get("ZoomLevel1");

    this.LeftPanelWidth = 262;
    this.ToggleWidth = 10;
    this.TopBannerHeight = 80;
    this.WindowWidth = 500;
    this.LeftOffsetX = 0;
    this.RightOffsetX = 0
    this.NavigationLeft = 0;
    this.CopyrightTextLeft = 0;
    this.DefaultMinDockWidth = 125;
    this.MinDockWidth = this.DefaultMinDockWidth;
    this.MapLeft = 262;
    this.LastMapWidth = 512;
    this.LastMapHeight = 512;
    this.CurrentMapWidth = 512;
    this.CurrentMapHeight = 512;
    this.HasScroll = false;
    this.LastHasScroll = false;
    this.DockMoving = false;
    this.reloadTimer = null;
    this.hasMeasure = false;
    this.Measure = null;
    this.MapUnits = null;
    this.CoordsDecimals = 3;
    this.currentMode = "Pan";
    this.lastMode = "Pan";

    this.setPageElementSizes = function() {
        // set body style 
        if (document.documentElement) {
            document.documentElement.style.overflow = "hidden";
            document.documentElement.style.height = "100%";
            this.PanelDisplay.style.overflow = "hidden";
        } else {
            document.body.style.overflow = "hidden";
            document.body.style.height = "100%";
        }
        this.PanelScrollDiv.style.overflow = "hidden";
        var headerDisplay = $get("PageHeader");
        var linkDisplay = $get("LinkBar");
        // get the set widths and heights
        this.LeftPanelWidth = this.PanelDisplay.clientWidth;
        this.ToggleWidth = parseInt(this.ToggleDisplay.style.width);
        this.TopBannerHeight = headerDisplay.clientHeight + linkDisplay.clientHeight;
        // get browser window dimensions
        var sWidth = this.getPageWidth();
        var sHeight = this.getPageHeight();
        // set map display dimensions
        var mHeight = sHeight - this.TopBannerHeight;
        this.MapLeft = this.LeftPanelWidth + this.ToggleWidth;
        var mWidth = getMapWidth();
        this.MapDisplay.style.width = mWidth + "px";
        this.MapDisplay.style.height = mHeight + "px";
        this.CurrentMapWidth = mWidth;
        this.CurrentMapHeight = mHeight;
        // set heights of left panel and toggle bar
        this.ToggleDisplay.style.height = mHeight + "px";
        this.PanelScrollDiv.style.height = mHeight + "px";

        esriMaxFloatingPanelDragRight = sWidth - 15;
        esriMaxFloatingPanelDragBottom = sHeight - 15;

        $addHandler(document.images["CollapseImage"], "mousedown", this.togglePanelDock);
        $addHandler(this.PanelBottomSlider, "mousedown", this.startDockDrag);
        $addHandler(this.PanelSlider, "mousedown", this.startDockDrag);
        var widthString = this.LeftPanelWidth + "px";
        this.ResultsPanel.style.width = widthString;
        this.TocPanel.style.width = widthString;
        if (isRTL) {
            this.PanelDisplayCell.style.left = mWidth + "px";
        }
    }

    // function for adjusting element sizes when brower is resized
    this.adjustMapSize = function() {
        // set element widths 
        this.PanelDisplay.style.width = this.LeftPanelWidth + "px";
        this.ToggleDisplay.style.width = this.ToggleWidth + "px";
        // get browser window dimensions 
        var sWidth = this.getPageWidth();
        var sHeight = this.getPageHeight();
        var mHeight = sHeight - this.TopBannerHeight;
        this.LastMapWidth = this.CurrentMapWidth;
        this.LastMapHeight = this.CurrentMapHeight;
        this.MapDisplay.style.height = mHeight + "px";
        this.ToggleDisplay.style.height = mHeight + "px";
        this.PanelScrollDiv.style.height = mHeight + "px";
        this.CurrentMapHeight = mHeight;
        this.setTocHeight();
        // calc dimensions needed for map
        var mWidth = getMapWidth();
        if (mWidth < 5) mWidth = 5;
        if (mHeight < 5) mHeight = 5;
        setMapLeftPosition();
        // set heights on elements 
        var widthString = this.PanelDisplay.style.width;
        this.ResultsPanel.style.width = widthString;
        this.TocPanel.style.width = widthString;
        this.ResultsPanelResize.set_MinimumWidth(this.LeftPanelWidth);
        this.TocPanelResize.set_MinimumWidth(this.LeftPanelWidth);
        this.ResultsPanelResize.set_MaximumWidth(this.LeftPanelWidth);
        this.TocPanelResize.set_MaximumWidth(this.LeftPanelWidth);
        this.ResultsPanelResize._handle.style.width = this.LeftPanelWidth + "px";
        this.TocPanelResize._handle.style.width = this.LeftPanelWidth + "px";
        this.ResultsPanelContents.style.width = widthString;
        this.TocPanelContents.style.width = widthString;
        if (this.LastMapWidth != this.CurrentMapWidth || this.LastMapHeight != this.CurrentMapHeight) {
            var cr = $find("MapCopyrightText1");
            if (cr != null)
                cr.get_callout().hide();
            if (arcgisIdentifyTool != null) closeIdentifyPanel();
        }
        // refresh the map 
        var m = $find(this.MapId);
        if (this.map != null) {
            if (this.LastMapWidth != this.CurrentMapWidth || this.LastMapHeight != this.CurrentMapHeight) {
                this.map.checkMapsize();
            }
        } else
            window.setTimeout("arcgisWebApp.adjustMapSize();", 1000);
        if (isRTL) adjustRTLElements();
        return false;
    }

    // handler for window resize
    this.adjustMapSizeHandler = function(e) {
        window.clearTimeout(arcgisWebApp.reloadTimer);
        arcgisWebApp.reloadTimer = window.setTimeout("arcgisWebApp.adjustMapSize();", 1000);
    }

    // get the page width
    this.getPageWidth = function() {
        var width = window.innerWidth;
        if (width == null) {
            if (document.documentElement && document.documentElement.clientWidth)
                width = document.documentElement.clientWidth
            else
                width = document.body.clientWidth;
        }
        return width;
    }

    //get the page height
    this.getPageHeight = function() {
        var height = window.innerHeight;
        if (height == null) {
            if (document.documentElement && document.documentElement.clientHeight)
                height = document.documentElement.clientHeight;
            else
                height = document.body.clientHeight;
        }
        return height;

    }

    // sets the height of the Map Contents panel
    this.setTocHeight = function() {
        var tocLocation = Sys.UI.DomElement.getLocation(this.TocPanelContents);
        var leftLocation = Sys.UI.DomElement.getLocation(this.PanelDisplayCell);
        var pHeight = this.getPageHeight();
        var tHeight = pHeight - tocLocation.y - 13;
        if (isNaN(tHeight) || tHeight < 5) tHeight = 5;
        this.TocPanelContents.style.height = tHeight + "px";

    }

    // functions for Dock movement and sizing
    this.togglePanelDock = function() {
        if (arcgisIdentifyTool != null) {
            var dropdown = $get("dropdown_" + arcgisIdentifyTool.get_uniqueID());
            if (dropdown) dropdown.style.display = "none";
        }
        if (arcgisWebApp.PanelDisplay.style.display == "none") {
            arcgisWebApp.expandPanelDock();
        } else {
            arcgisWebApp.collapsePanelDock();
        }
    }

    this.expandPanelDock = function() {
        var image = document.images["CollapseImage"];
        var imgUrl = (isRTL) ? "images/expand_right.gif" : "images/collapse_left.gif";
        arcgisWebApp.PanelDisplay.style.display = "block";
        image.src = imgUrl;
        image.alt = "Collapse";
        arcgisWebApp.PanelSlider.style.cursor = "e-resize";
        arcgisWebApp.PanelBottomSlider.style.cursor = "e-resize";
        window.setTimeout('arcgisWebApp.adjustMapSize();', 500);

    }

    this.collapsePanelDock = function() {
        var image = document.images["CollapseImage"];
        var imgUrl = (isRTL) ? "images/collapse_left.gif" : "images/expand_right.gif";
        dockWidthString = arcgisWebApp.PanelDisplayCell.clientWidth + "px";
        arcgisWebApp.PanelDisplay.style.display = "none";
        image.src = imgUrl;
        image.alt = "Expand";
        arcgisWebApp.PanelSlider.style.cursor = "default";
        arcgisWebApp.PanelBottomSlider.style.cursor = "default";
        window.setTimeout('arcgisWebApp.adjustMapSize();', 500);

    }

    var startConsoleDragLeft = 0;
    this.startDockDrag = function(e) {
        if (!arcgisWebApp.DockMoving) {
            arcgisWebApp.MoveFunction = document.onmousemove;
            arcgisWebApp.UpFunction = document.onmouseup;
            arcgisWebApp.DockMoving = true;
            // hide the map addons while sliding the divider 
        }
        $addHandler(document, "mouseup", arcgisWebApp.stopDockDrag);
        if (arcgisWebApp.PanelDisplay.style.display != "none") {
            arcgisWebApp.WindowWidth = arcgisWebApp.getPageWidth();
            if (isRTL) {
                arcgisWebApp.LeftOffsetX = arcgisWebApp.PanelDisplay.clientWidth - e.clientX;
                var location = Sys.UI.DomElement.getLocation($get("LeftPanelTableCell"));
                arcgisWebApp.RightOffsetX = location.x - e.clientX;
                $addHandler(document, "mousemove", arcgisWebApp.moveDockDragRTL);
            } else {
                arcgisWebApp.LeftOffsetX = e.clientX - arcgisWebApp.PanelDisplay.clientWidth;
                var location = Sys.UI.DomElement.getLocation($get("Map_Panel"));
                arcgisWebApp.RightOffsetX = location.x - e.clientX;
                if (arcgisWebApp.CopyrightText) {
                    arcgisWebApp.CopyrightTextLeft = parseInt(arcgisWebApp.CopyrightText.style.left);
                    arcgisWebApp.CopyrightTextTop = parseInt(arcgisWebApp.CopyrightText.style.top);
                }
                $addHandler(document, "mousemove", arcgisWebApp.moveDockDrag);
            }
            startConsoleDragLeft = e.clientX;

        }
        e.preventDefault();
        e.stopPropagation();
    }

    this.moveDockDrag = function(e) {
        var theButton = (Sys.Browser.agent == Sys.Browser.InternetExplorer) ? event.button : e.which;
        if (theButton == 0) arcgisWebApp.stopDockDrag(e);
        arcgisWebApp.LeftPanelWidth = e.clientX - arcgisWebApp.LeftOffsetX;
        arcgisWebApp.DockMoving = true;
        var sWidth = arcgisWebApp.getPageWidth();
        var x = e.clientX;
        var y = e.clientY;
        var rightWidth = 2;
        if (arcgisWebApp.CopyrightText) rightWidth = arcgisWebApp.CopyrightText.clientWidth + 10;
        if (x >= sWidth - rightWidth || y >= arcgisWebApp.getPageHeight() - 2 || y < 1 || x < arcgisWebApp.MinDockWidth) arcgisWebApp.stopDockDrag(e);
        if (arcgisWebApp.LeftPanelWidth > sWidth - arcgisWebApp.ToggleDisplay.clientWidth - rightWidth) arcgisWebApp.LeftPanelWidth = sWidth - arcgisWebApp.ToggleDisplay.clientWidth - rightWidth;
        if (arcgisWebApp.LeftPanelWidth < arcgisWebApp.MinDockWidth) arcgisWebApp.LeftPanelWidth = arcgisWebApp.MinDockWidth;
        var mapLeftString = (arcgisWebApp.LeftPanelWidth + arcgisWebApp.ToggleDisplay.clientWidth) + "px";
        var widthString = arcgisWebApp.LeftPanelWidth + "px";
        arcgisWebApp.PanelDisplay.style.width = widthString;
        arcgisWebApp.ResultsPanel.style.width = widthString;
        arcgisWebApp.TocPanel.style.width = widthString;
        var pWidth = arcgisWebApp.LeftPanelWidth;
        arcgisWebApp.ResultsPanelResize.set_MinimumWidth(pWidth);
        arcgisWebApp.TocPanelResize.set_MinimumWidth(pWidth);
        arcgisWebApp.ResultsPanelResize.set_MaximumWidth(pWidth);
        arcgisWebApp.TocPanelResize.set_MaximumWidth(pWidth);
        arcgisWebApp.ResultsPanelResize._handleHolder.style.width = pWidth + "px";
        arcgisWebApp.TocPanelResize._handleHolder.style.width = pWidth + "px";
        var width = getMapWidth();
        if (width < 1) width = 1;
        arcgisWebApp.MapDisplay.style.width = width + "px";
        arcgisWebApp.MapDisplay.style.left = mapLeftString;
        var leftDiff = e.clientX - startConsoleDragLeft;
        if (arcgisWebApp.CopyrightText) {
            var crLeft = arcgisWebApp.CopyrightTextLeft + leftDiff;
            if (crLeft < sWidth - arcgisWebApp.CopyrightText.clientWidth) {
                if (crLeft < arcgisWebApp.LeftPanelWidth + arcgisWebApp.ToggleDisplay.clientWidth) crLeft = arcgisWebApp.LeftPanelWidth + arcgisWebApp.ToggleDisplay.clientWidth;
                arcgisWebApp.CopyrightText.style.left = crLeft + "px";
                if (!isIE) arcgisWebApp.CopyrightText.style.top = arcgisWebApp.CopyrightTextTop + "px";
            }
        }
        if (arcgisIdentifyTool != null) {
            var dropdown = $get("dropdown_" + arcgisIdentifyTool.get_uniqueID());
            if (dropdown) dropdown.style.display = "none";
        }
        e.preventDefault();
        e.stopPropagation();
    }

    this.moveDockDragRTL = function(e) {
        var theButton = (Sys.Browser.agent == Sys.Browser.InternetExplorer) ? event.button : e.which;
        if (theButton == 0) arcgisWebApp.stopDockDrag(e);
        var sWidth = arcgisWebApp.getPageWidth();
        arcgisWebApp.LeftPanelWidth = sWidth - (e.clientX + arcgisWebApp.RightOffsetX);
        arcgisWebApp.DockMoving = true;
        var x = e.clientX;
        var y = e.clientY;
        var rightWidth = 2;
        if (arcgisWebApp.CopyrightText) rightWidth = arcgisWebApp.CopyrightText.clientWidth + 10;
        if (x <= rightWidth || y >= arcgisWebApp.getPageHeight() - 2 || y < 1 || x > sWidth - arcgisWebApp.MinDockWidth) arcgisWebApp.stopDockDrag(e);
        if (arcgisWebApp.LeftPanelWidth > sWidth - arcgisWebApp.ToggleDisplay.clientWidth - rightWidth) arcgisWebApp.LeftPanelWidth = sWidth - arcgisWebApp.ToggleDisplay.clientWidth - rightWidth;
        if (arcgisWebApp.LeftPanelWidth < arcgisWebApp.MinDockWidth) arcgisWebApp.LeftPanelWidth = arcgisWebApp.MinDockWidth;
        var mapLeftString = (arcgisWebApp.LeftPanelWidth + arcgisWebApp.ToggleDisplay.clientWidth) + "px";
        var widthString = arcgisWebApp.LeftPanelWidth + "px";
        arcgisWebApp.PanelDisplay.style.width = widthString;
        arcgisWebApp.ResultsPanel.style.width = widthString;
        arcgisWebApp.TocPanel.style.width = widthString;
        var pWidth = arcgisWebApp.LeftPanelWidth;
        arcgisWebApp.ResultsPanelResize.set_MinimumWidth(pWidth);
        arcgisWebApp.TocPanelResize.set_MinimumWidth(pWidth);
        arcgisWebApp.ResultsPanelResize.set_MaximumWidth(pWidth);
        arcgisWebApp.TocPanelResize.set_MaximumWidth(pWidth);
        arcgisWebApp.ResultsPanelResize._handleHolder.style.width = pWidth + "px";
        arcgisWebApp.TocPanelResize._handleHolder.style.width = pWidth + "px";
        var width = sWidth - (pWidth + arcgisWebApp.ToggleDisplay.clientWidth);
        if (width < 5) width = 5;
        arcgisWebApp.MapDisplay.style.width = width + "px";
        arcgisWebApp.PanelDisplayCell.style.left = width + "px";
        var leftDiff = e.clientX - startConsoleDragLeft;
        if (arcgisIdentifyTool != null) {
            var dropdown = $get("dropdown_" + arcgisIdentifyTool.get_uniqueID());
            if (dropdown) dropdown.style.display = "none";
        }
        e.preventDefault();
        e.stopPropagation();
    }

    this.stopDockDrag = function(e) {
        try {
            $removeHandler(document, "mouseup", arcgisWebApp.stopDockDrag);
            if (isRTL)
                $removeHandler(document, "mousemove", arcgisWebApp.moveDockDragRTL);
            else
                $removeHandler(document, "mousemove", arcgisWebApp.moveDockDrag);
        } catch (ex) {
            var dummy = ex;
        }
        if (arcgisWebApp.CopyrightText && !isIE)
            arcgisWebApp.CopyrightText.style.top = arcgisWebApp.CopyrightTextTop + "px";
        arcgisWebApp.DockMoving = false;
        arcgisWebApp.adjustMapSize();
        e.preventDefault();
        e.stopPropagation();
    }

    ESRI.ADF.WebMappingApplication.WebPage.initializeBase(this);
}

ESRI.ADF.WebMappingApplication.WebPage.registerClass('ESRI.ADF.WebMappingApplication.WebPage');

arcgisWebApp = new ESRI.ADF.WebMappingApplication.WebPage();


if (typeof (Sys) !== 'undefined') { Sys.Application.notifyScriptLoaded(); }