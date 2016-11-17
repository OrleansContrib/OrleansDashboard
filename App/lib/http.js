function makeRequest(method, uri, body, cb){
    var xhr = new XMLHttpRequest();
    xhr.open(method,uri,true);
    xhr.onreadystatechange = function(){
        if(xhr.readyState !== 4) return
        if(xhr.status < 400) return cb(null, JSON.parse(xhr.responseText || '{}'));
        console.log("error", xhr.status, xhr.responseText);
    	cb(xhr.status);
    };
    xhr.setRequestHeader('Content-Type','application/json');
    xhr.setRequestHeader('Accept','application/json');
    xhr.send(body);
}

module.exports.get = function(url, cb){
	makeRequest('GET', url, null, cb);
}
