// polyfill localStorage with temp store
var store = {}

try {
  if (localStorage) {
    store = localStorage
  }
} catch (e) {
  // noop
}

module.exports.put = (key, value) => {
  store[key] = value
}

module.exports.get = key => {
  return store[key]
}

module.exports.del = key => {
  if (store.removeItem) {
    store.removeItem(key)
  } else {
    delete store[key]
  }
}
