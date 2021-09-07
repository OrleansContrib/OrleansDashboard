function makeRequest<T>(method: 'GET' | 'POST', uri: string, body?: any) {
  return new Promise<T>((resolve, reject) => {
    const xhr = new XMLHttpRequest()
    xhr.open(method, uri, true)
    xhr.onreadystatechange = function () {
      if (xhr.readyState !== 4) return
      if (xhr.status < 400 && xhr.status > 0)
        return resolve(JSON.parse(xhr.responseText || '{}') as T)
      var errorMessage =
        'Error connecting to Orleans Silo. Status code: ' +
        (xhr.status || 'NO_CONNECTION')
      reject(errorMessage)
    }
    xhr.setRequestHeader('Content-Type', 'application/json')
    xhr.setRequestHeader('Accept', 'application/json')
    xhr.send(body)
  })
}

export function get<T>(url: string) {
  return makeRequest<T>('GET', url, null)
}

export const stream = (url: string) => {
  var xhr = new XMLHttpRequest()
  xhr.open('GET', url, true)
  xhr.send()
  return xhr
}
