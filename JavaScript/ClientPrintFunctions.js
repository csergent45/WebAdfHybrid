function PrintTaskSendRequest(button, callbackFunctionString, errorMessage) {
	var pre = getControlPrefix(button.id);
	var argument = "";
	var gotOutput = false;

	// Results only checkbox
	var printMap = document.getElementById(pre + "_resultsOnly");
	if (printMap !== null) {
		argument = addArgument (argument, "resultsOnly", printMap.checked);
		if (printMap.checked === false)
		{
			gotOutput = true;
		}
	}

	// Task Results. If the map is not to be printed, then check to make sure at least one task result
	// has been checked for printing.
	if (!gotOutput) {
		var cblResults = document.getElementById(pre + "_pnlResults_cblResults");
		if (cblResults !== null)
		{
			var checkBoxes = cblResults.getElementsByTagName('input');
			if (checkBoxes !== null && checkBoxes.length > 0) {
				for (var i = 0; i < checkBoxes.length; ++i) {
					if (checkBoxes[i].type === "checkbox" && checkBoxes[i].checked) {
						gotOutput = true;
						break;
					}
				}
			}
		}
	}

	// If no map and no results are to be printed, inform the user and abort
	if (gotOutput === false)
	{
		alert(decode(errorMessage));
		return;
	}

	// Map Title
	var printTitle = "";
	var printBox = document.getElementById(pre + "_mapTitle");
	if (printBox !== null) {
		// replace "&"
		printTitle = printBox.value + "Hi Chris";
		printTitle = printTitle.replace(/&/, "##amp##");
		argument = addArgument (argument, "mapTitle", printTitle);
	}

	// Size drop down list
	var printWidth = document.getElementById(pre + "_width");
	if (printWidth !== null)
	{
		argument = addArgument (argument, "width", printWidth.selectedIndex);
	}

	// Quality drop down list
	var printQuality = document.getElementById(pre + "_quality");
	if (printQuality !== null)
	{
		argument = addArgument (argument, "quality", printQuality.selectedIndex);
	}

	// Include ScaleBar checkbox
	var printScaleBar = document.getElementById(pre + "_includeScaleBar");
	if (printScaleBar !== null)
	{
		argument = addArgument (argument, "includeScaleBar", printScaleBar.checked);
	}

	// Include North Arrow checkbox
	var printNorthArrow = document.getElementById(pre + "_includeNorthArrow");
	if (printNorthArrow !== null)
	{
		argument = addArgument (argument, "includeNorthArrow", printNorthArrow.checked);
	}

	// Include Legend checkbox
	var printLegend = document.getElementById(pre + "_includeLegend");
	if (printLegend !== null)
	{
		argument = addArgument (argument, "includeLegend", printLegend.checked);
	}

	var context=null;
	doHourglass();
	executeTask(argument, callbackFunctionString);
}

// Called from server-generated function after print request
function PrintTaskDisplayPage(printContent, exceptionMessage, blockerMessage) {

	var regexp = /xxrnxx/gi;

	// If exceptions occurred during rendering, inform the user before displaying the preview   
	if (exceptionMessage !== "") {
		exceptionMessage = exceptionMessage.replace(regexp, "\n");
		exceptionMessage = decode(exceptionMessage);
		alert (exceptionMessage);
	}

	var windowOptions = "menubar=yes,toolbar=yes,width=700,height=600,top=100,left=200,scrollbars=yes,resizable=yes";
	var printWin = window.open("", "_blank", windowOptions);

	if (printWin !== null)
	{
		// restore line breaks (encoded as xxrnxx at server)
		printContent = printContent.replace(regexp, "\n");

		// "unescape" the content
		printContent = decode(printContent);

		printWin.document.write(printContent);

		printWin.document.close();
		printWin.focus();
	}
	else
	{
		alert(blockerMessage);
	}

	undoHourglass();
}

function getControlPrefix (fullControlId) {
	var lastUnderscore = fullControlId.lastIndexOf("_");
	return fullControlId.substring(0, lastUnderscore);
}

function doHourglass() {
	document.body.style.cursor = 'wait';
}
function undoHourglass() {
	document.body.style.cursor = 'auto';
}

var conversionMap = {
	"amp" : "&",
	"lt" : "<",
	"gt" : ">",
	"apos" : "'",
	"quot" : '"'
};

function decode(entityString) {
	return entityString.replace(/&(\w+);/g, function(m,g) {
		return conversionMap[g]||m;
	});
}

function addArgument (output, name, value) {
	if (output !== "")
	{
		output += "&";
	}

	output += name + "=" + value;
	return output;
}
