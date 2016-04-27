var http = require('./lib/http');
var React = require('react');
var Dashboard = require('./components/dashboard.jsx');
var routie = require('./lib/routie');
var SiloDrilldown = require('./components/silo-drilldown.jsx');
var target = document.getElementById('content');
var events = require('eventthing');
var ThemeButtons = require('./components/theme-buttons.jsx');
var timer;

var dashboardCounters = {};

var x = () => console.log("test");
x();

React.render(<ThemeButtons/>, document.getElementById('button-toggles-content'));

// continually poll the dashboard counters
function loadDashboardCounters(){
    http.get('/DashboardCounters', function(err, data){
        dashboardCounters = data;
        events.emit('dashboard-counters', data);
    });
}
setInterval(loadDashboardCounters, 5000);
loadDashboardCounters();


function renderLoading(){
    React.render(<span>Loading...</span>, target);
}

routie('', function(){
    events.clearAll();
    clearInterval(timer);

    var render = function(){
        React.render(<Dashboard dashboardCounters={dashboardCounters} />, target);
    }

    events.on('dashboard-counters', render);

    loadDashboardCounters();


});



routie('/host/:host', function(host){
    events.clearAll();
    clearInterval(timer);

    var siloData = [];
    var loadData = function(cb){
        http.get('/HistoricalStats/' + host, function(err, data){
            siloData = data;
            render();
        });
    }


    var render = function(){
        React.render(<SiloDrilldown silo={host} data={siloData} dashboardCounters={dashboardCounters}  />, target);
    }

    events.on('dashboard-counters', render);

    timer = setInterval(loadData, 5000);

    loadData();
});
