function Professor() {
	this._id = "";
	this.type = "Professor";
	this.PersNr = "";
	this.Rang = "";
	this.Raum = "";
	this.Name = "";
}

function Vorlesung() {
	this._id = "";
	this.type = "Vorlesung";
	this.VorlNr = "";
	this.Titel = "";
	this.gelesenVon = "";
	this.besuchtVon = [];
	this.setztVoraus = [];
}
function openQueries() {
	this.numberOfQueries = 0;
	this.increaseNumberOfQueries = function(){
		this.numberOfQueries++;
	};
	this.decreaseNumberOfQueries = function(){
		this.numberOfQueries--;
	};
	this.done = function() {
		return this.numberOfQueries==0
	}
}

// Example Without Relation //

var Client = require('mariasql');
var najax = $ = require('najax');
var connection = new Client({
  host: '127.0.0.1',
  user: 'root',
  password: 'root',
  db: "uni"
});

connection.query('SELECT * FROM professoren', function(err, rows) {
  if (err)
    throw err;
	var prof = new Professor();
	for (i in rows) {
		if(i!="info") {
			prof._id = rows[i].PersNr;
			prof.PersNr = rows[i].PersNr;
			prof.Name = rows[i].Name;
			prof.Raum = rows[i].Raum;
			prof.Rang = rows[i].Rang;
			najax({url:'http://localhost:5984/uni-db', type: 'POST', data: prof, contentType: "application/json"});
		}
	}
	console.log("success migrating a lot");
});

//   EXAMPLE with relation //
gelesen = [];
voraussetzung = [];
vorlesungen = [];
queries = new openQueries();

function mergeEverything() {
	if (queries.done()) {
		for(i in vorlesungen){
			vorlesungen[i].besuchtVon = gelesen[i];
			vorlesungen[i].setztVoraus = voraussetzung[i];
			najax({url:'http://localhost:5984/uni-db', type: 'POST', data:vorlesungen[i], contentType: "application/json"});
		}
		console.log("success migrating a lot");
		return;
	}else{
		setTimeout(mergeEverything, 3000);
		return;
	}
}
connection.query('SELECT * FROM vorlesungen', function(err, rows) {
  if (err)
    throw err;
	for (i in rows) {
		if(i!="info") {
			done=false;
			var vorlesung = new Vorlesung();
			vorlesung._id = rows[i]._id;
			vorlesung.VorlNr = rows[i].VorlNr;
			vorlesung.Titel = rows[i].Titel;
			vorlesung.SWS = rows[i].SWS;
			vorlesung.gelesenVon = rows[i].gelesenVon;
			
			queries.increaseNumberOfQueries();
			connection.query('SELECT * FROM hoeren WHERE VorlNr='+rows[i].VorlNr, function(err, rows2) {
				gel = [];
				for(i2 in rows2) {
					if(i2!="info") {
						gel[i2] = rows2[i2].MatrNr;
					}
				}
				gelesen[gelesen.length] = gel;
				queries.decreaseNumberOfQueries();
			});
			
			queries.increaseNumberOfQueries();
			connection.query('SELECT * FROM uni.voraussetzen WHERE `Nachfolger`='+rows[i].VorlNr, function(err, rows3) {
				console.log(rows3);
				vor = [];
				for(i3 in rows3) {
					if(i3!="info") {
						vor[i3] = rows3[i3].Vorgaenger;
					}
				
				}
				voraussetzung[voraussetzung.length] = vor;
				queries.decreaseNumberOfQueries();
			});
			
			vorlesungen[i] = vorlesung;
		}
	}
	mergeEverything();
});


connection.end();