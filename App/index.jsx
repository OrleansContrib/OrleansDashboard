var http = require('./lib/http');
var React = require('react');
var ReactDom = require('react-dom');
var routie = require('./lib/routie');
var SiloDrilldown = require('./components/silo-drilldown.jsx');
var target = document.getElementById('content');
var events = require('eventthing');
var ThemeButtons = require('./components/theme-buttons.jsx');
var Grain = require('./components/grain.jsx');
var Page = require('./components/page.jsx');
var Loading = require('./components/loading.jsx');
var Menu = require('./components/menu.jsx');
var Grains = require('./components/grains.jsx');
var Silos = require('./components/silos.jsx');
var Overview = require('./components/overview.jsx');
var timer;

var dashboardCounters = {};

function scroll(){
    window.scrollTo(0,0);
}

//ReactDom.render(<ThemeButtons/>, document.getElementById('button-toggles-content'));

// continually poll the dashboard counters
function loadDashboardCounters(){
    http.get('/DashboardCounters', function(err, data){
        dashboardCounters = data;
        events.emit('dashboard-counters', data);
    });
}

// we always want to refresh the dashboard counters
setInterval(loadDashboardCounters, 1000);
loadDashboardCounters();
var render = () => {};

function renderLoading(){
    ReactDom.render(<Loading />, target);
}

var menuElement = document.getElementById('menu');

function renderPage(jsx, path){
    ReactDom.render(jsx, target);
    var menu = getMenu();
    menu.forEach(x => {
        x.active = (x.path === path);
    });

    ReactDom.render(<Menu menu={menu} />, menuElement);
}


routie('', function(){
    events.clearAll();
    scroll();

    render = function(){
        renderPage(<Page title="Dashboard"><Overview dashboardCounters={dashboardCounters} /></Page>, "#/");
    }

    events.on('dashboard-counters', render);
    events.on('refresh', render);

    loadDashboardCounters();
});

routie('/grains', function(){
    events.clearAll();
    scroll();

    render = function(){
        renderPage(<Page title="Grains"><Grains dashboardCounters={dashboardCounters} /></Page>, "#/grains");
    }

    events.on('dashboard-counters', render);
    events.on('refresh', render);

    loadDashboardCounters();
});

routie('/silos', function(){
    events.clearAll();
    scroll();

    render = function(){
        renderPage(<Page title="Silos"><Silos dashboardCounters={dashboardCounters} /></Page>, "#/silos");
    }

    events.on('dashboard-counters', render);
    events.on('refresh', render);

    loadDashboardCounters();
});



routie('/host/:host', function(host){
    events.clearAll();
    scroll();
    var siloProperties = {};

    var siloData = [];
    var loadData = function(cb){
        http.get('/HistoricalStats/' + host, function(err, data){
            siloData = data;
            render();
        });
    }

    render = function(){
        renderPage(<Page title="Silo"><SiloDrilldown silo={host} data={siloData} siloProperties={siloProperties} dashboardCounters={dashboardCounters}  /></Page>, "#/silos");
    }

    events.on('dashboard-counters', render);
    events.on('refresh', loadData);

    loadData();

    http.get('/SiloProperties/' + host, function(err, data){
        siloProperties = data;
        render();
    });

});


routie('/grain/:grainType', function(grainType){
    events.clearAll();
    scroll();

    var grainStats = {};
    var loadData = function(cb){
        http.get('/GrainStats/' + grainType, function(err, data){
            grainStats = data;
            render();
        });
    }

    render = function(){
        renderPage(<Grain grainType={grainType} dashboardCounters={dashboardCounters} grainStats={grainStats} />, "#/grains");
    }

    events.on('dashboard-counters', render);
    events.on('refresh', loadData);

    loadData();

});

setInterval(() => events.emit('refresh'), 1000);

routie.reload();



function getMenu(){
    return [
        {
            name:"Overview",
            path:"#/",
            icon:"fa-circle"
        },
        {
            name:"Grains",
            path:"#/grains",
            icon:"fa-circle"
        },
        {
            name:"Silos",
            path:"#/silos",
            icon:"fa-circle"
        }
    ];
}
