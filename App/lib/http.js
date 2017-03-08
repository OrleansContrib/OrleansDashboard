var events = require('eventthing');

function makeRequest(method, uri, body, cb){
    var xhr = new XMLHttpRequest();
    xhr.open(method,uri,true);
    xhr.onreadystatechange = function(){
        if(xhr.readyState !== 4) return
        if(xhr.status < 400 && xhr.status > 0) return cb(null, JSON.parse(xhr.responseText || '{}'));
        var errorMessage = 'Error connecting to Orleans Silo. Status code: ' + (xhr.status || "NO_CONNECTION");
        errorHandlers.forEach(x => x(errorMessage));
    };
    xhr.setRequestHeader('Content-Type','application/json');
    xhr.setRequestHeader('Accept','application/json');
    xhr.send(body);
}

module.exports.get = function(url, cb){
	makeRequest('GET', url, null, cb);
}

var errorHandlers = [];

module.exports.onError = function(handler){
    errorHandlers.push(handler);
}
