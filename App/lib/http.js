var events = require('eventthing')

function makeRequest(method, uri, body, cb) {
  return new Promise((resolve, reject) => {
    var xhr = new XMLHttpRequest()
    xhr.open(method, uri, true)
    xhr.onreadystatechange = function() {
      if (xhr.readyState !== 4) return
      if (xhr.status < 400 && xhr.status > 0) {
        const result = JSON.parse(xhr.responseText || '{}');
        resolve(result);
        return cb(null, result);
      }
      var errorMessage =
        'Error connecting to Orleans Silo. Status code: ' +
        (xhr.status || 'NO_CONNECTION')
      errorHandlers.forEach(x => x(errorMessage))
      reject(errorMessage);
    }
    xhr.setRequestHeader('Content-Type', 'application/json')
    xhr.setRequestHeader('Accept', 'application/json')
    xhr.send(body)
  });
}

module.exports.get = function(url, cb) {
  return makeRequest('GET', url, null, cb)
}

module.exports.stream = function(url) {
  var xhr = new XMLHttpRequest()
  xhr.open('GET', url, true)
  xhr.send()
  return xhr
}

var errorHandlers = []

module.exports.onError = function(handler) {
  errorHandlers.push(handler)
}
