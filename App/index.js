var http = require('./lib/http');
var React = require('react');
var Dashboard = require('./components/dashboard.jsx');
var routie = require('./lib/routie');

var target = document.getElementById('content');

var timer;

/*
routie('',function(){
    routie('/');
});
*/
routie('/', function(){
    clearInterval(timer);

    var loadData = function(cb){
        http.get('/DashboardCounters', cb);
    }

    var render = function(err, data){
        React.render(<Dashboard dashboardCounters={data} />, target);
    }

    loadData(render);

    timer = setInterval(function(){
        loadData(render);
    }, 5000);
});


routie('/host/:host', function(host){
    clearInterval(timer);

    var loadData = function(cb){
        http.get('/RuntimeStats/' + host, cb);
    }

    var render = function(err, data){
        console.log(data);
        React.render(<h1>Silo</h1>, target);
    }

    loadData(render);

    timer = setInterval(function(){
        loadData(render);
    }, 5000);

});