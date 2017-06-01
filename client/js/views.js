function getCouchDBViewByURL(param_url, callbackFunction) {
	$.ajax({
		url:param_url,
		dataType: "jsonp",
		success: callbackFunction
	});
}
viewSafer = {
	firstViewValue : null,
	secondViewValue : null,
	thirdViewValue : null
}
function valuateAndPrint() {
	if (viewSafer.firstViewValue != null && viewSafer.secondViewValue != null &&viewSafer.thirdViewValue != null) {
		var output = "";
		for (professor in viewSafer.firstViewValue.rows) {
			numberOfStudents=0;
			numberOfSWS=0;
			for(students in viewSafer.secondViewValue.rows) {
				if(viewSafer.firstViewValue.rows[professor].value == viewSafer.secondViewValue.rows[students].key) {
					numberOfStudents = viewSafer.secondViewValue.rows[students].value;
				}
			}
			for(sws in viewSafer.thirdViewValue.rows) {
				if(viewSafer.firstViewValue.rows[professor].value == viewSafer.thirdViewValue.rows[sws].key) {
					numberOfSWS = viewSafer.thirdViewValue.rows[sws].value;
				}
			}
			output = output + viewSafer.firstViewValue.rows[professor].key + "; Anzahl Studenten: " + numberOfStudents + ", Anzahl SWS: " + numberOfSWS + "<br/>" ;
		}
		$('#views').html(output);
	}else {
		$('#views').html("ERROR");
	}
}
function getThridView(jsonValue) {
	viewSafer.thirdViewValue = jsonValue;
	valuateAndPrint();
}
function getSecondView(jsonValue) {
	viewSafer.secondViewValue = jsonValue;
	getCouchDBViewByURL("http://localhost:5984/uni-db/_design/Abfragen/_view/Anzahl_SWS_pro_Professor?limit=20&reduce=true&group=true", getThridView);
}
function getFirstView(jsonValue) {
	viewSafer.firstViewValue = jsonValue;
	getCouchDBViewByURL("http://localhost:5984/uni-db/_design/Abfragen/_view/Anzahl_Studenten_pro_Professor?limit=20&reduce=true&group=true", getSecondView);
}
getCouchDBViewByURL("http://localhost:5984/uni-db/_design/Abfragen/_view/Alle_Professornamen_mit_zugehoeriger_ID?limit=20&reduce=false", getFirstView);