import { get, put, del }  from './storage'

describe('testing', () => {
  it('works', done => done())
})

describe('storage', () => {
  it('polyfills localstorage', done => {
    if (get('foo')) return done('expected null')

    put('foo', 'bar')

    if ('bar' !== get('foo')) return done('expected bar')

    del('foo')

    if (get('foo')) return done('expected value to be deleted')

    done()
  })
})
