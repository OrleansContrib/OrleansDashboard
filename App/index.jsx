var http = require('./lib/http');
var React = require('react');
var ReactDom = require('react-dom');
var Dashboard = require('./components/dashboard.jsx');
var routie = require('./lib/routie');
var SiloDrilldown = require('./components/silo-drilldown.jsx');
var target = document.getElementById('content');
var events = require('eventthing');
var ThemeButtons = require('./components/theme-buttons.jsx');
var Grain = require('./components/grain.jsx');
var timer;

var dashboardCounters = {};

ReactDom.render(<ThemeButtons/>, document.getElementById('button-toggles-content'));

// continually poll the dashboard counters
function loadDashboardCounters(){
    http.get('/DashboardCounters', function(err, data){
        dashboardCounters = data;
        events.emit('dashboard-counters', data);
    });
}

// we always want to refresh the dashboard counters
setInterval(loadDashboardCounters, 5000);
loadDashboardCounters();


function renderLoading(){
    ReactDom.render(<span>Loading...</span>, target);
}

routie('', function(){
    events.clearAll();

    var render = function(){
        ReactDom.render(<Dashboard dashboardCounters={dashboardCounters} />, target);
    }

    events.on('dashboard-counters', render);
    events.on('refresh', render);

    loadDashboardCounters();
});



routie('/host/:host', function(host){
    events.clearAll();

    var siloData = [];
    var loadData = function(cb){
        http.get('/HistoricalStats/' + host, function(err, data){
            siloData = data;
            render();
        });
    }

    var render = function(){
        ReactDom.render(<SiloDrilldown silo={host} data={siloData} dashboardCounters={dashboardCounters}  />, target);
    }

    events.on('dashboard-counters', render);
    events.on('refresh', loadData);

    loadData();
});


routie('/grain/:grainType', function(grainType){
    events.clearAll();

    var grainStats = {};
    var loadData = function(cb){
        http.get('/GrainStats/' + grainType, function(err, data){
            grainStats = data;
            render();
        });
    }

    var render = function(){
        ReactDom.render(<Grain grainType={grainType} dashboardCounters={dashboardCounters} grainStats={grainStats} />, target);
    }

    events.on('dashboard-counters', render);
    events.on('refresh', loadData);

    loadData();

});

setInterval(() => events.emit('refresh'), 5000);
