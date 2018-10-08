const storage = require('./lib/storage');

describe("testing", () => {
    it("works", done => done());
});


describe("storage", () => {
    it("polyfills localstorage", done => {
        
        if (storage.get("foo")) return done("expected null")
        
        storage.put("foo", "bar");
        
        if ("bar" !== storage.get("foo")) return done("expected bar")
               
        storage.del("foo");
        
        if (storage.get("foo")) return done("expected value to be deleted");
        
        done();
    });
});
