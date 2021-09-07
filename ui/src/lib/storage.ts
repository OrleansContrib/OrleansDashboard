// polyfill localStorage with temp store
var store: any = {}

try {
  if (localStorage) {
    store = localStorage
  }
} catch (e) {
  // noop
}

export const put = (key: string, value: string) => (store[key] = value)

export const get = (key: string) => store[key]

export const del = (key: string) => {
  if (store.removeItem) {
    store.removeItem(key)
  } else {
    delete store[key]
  }
}
