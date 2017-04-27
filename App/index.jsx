var http = require('./lib/http');
var React = require('react');
var ReactDom = require('react-dom');
var routie = require('./lib/routie');
var Silo = require('./silos/silo.jsx');
var target = document.getElementById('content');
var events = require('eventthing');
var ThemeButtons = require('./components/theme-buttons.jsx');
var Grain = require('./grains/grain.jsx');
var Page = require('./components/page.jsx');
var Loading = require('./components/loading.jsx');
var Menu = require('./components/menu.jsx');
var Grains = require('./grains/grains.jsx');
var Silos = require('./silos/silos.jsx');
var Overview = require('./overview/overview.jsx');
var SiloState = require('./silos/silo-state-label.jsx');
var Alert = require('./components/alert.jsx');
var LogStream = require('./logstream/log-stream.jsx');
var timer;

var dashboardCounters = {};
var routeIndex = 0;

function scroll(){
    window.scrollTo(0,0);
}

ReactDom.render(<ThemeButtons/>, document.getElementById('button-toggles-content'));

var errorTimer;
function showError(message){
    ReactDom.render(<Alert onClose={closeError}>{message}</Alert>, document.getElementById('error-message-content'));
    if (errorTimer) clearTimeout(errorTimer);
    errorTimer = setTimeout(closeError, 3000);
}

function closeError(){
    clearTimeout(errorTimer);
    errorTimer = null;
    ReactDom.render(<span></span>, document.getElementById('error-message-content'));
}

http.onError(showError);

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
    var thisRouteIndex = ++routeIndex;
    events.clearAll();
    scroll();

    var clusterStats = {};
    var loadData = function(cb){
        http.get('/ClusterStats', function(err, data){
            clusterStats = data;
            render();
        });
    }

    render = function(){
        if (routeIndex != thisRouteIndex) return;
        renderPage(<Page title="Dashboard"><Overview dashboardCounters={dashboardCounters} clusterStats={clusterStats} /></Page>, "#/");
    }

    events.on('dashboard-counters', render);
    events.on('refresh', loadData);

    loadDashboardCounters();
});

routie('/grains', function(){
    var thisRouteIndex = ++routeIndex;
    events.clearAll();
    scroll();

    render = function(){
        if (routeIndex != thisRouteIndex) return;
        renderPage(<Page title="Grains"><Grains dashboardCounters={dashboardCounters} /></Page>, "#/grains");
    }

    events.on('dashboard-counters', render);
    events.on('refresh', render);

    loadDashboardCounters();
});

routie('/silos', function(){
    var thisRouteIndex = ++routeIndex;
    events.clearAll();
    scroll();

    render = function(){
        if (routeIndex != thisRouteIndex) return;
        renderPage(<Page title="Silos"><Silos dashboardCounters={dashboardCounters} /></Page>, "#/silos");
    }

    events.on('dashboard-counters', render);
    events.on('refresh', render);

    loadDashboardCounters();
});



routie('/host/:host', function(host){
    var thisRouteIndex = ++routeIndex;
    events.clearAll();
    scroll();
    var siloProperties = {};

    var siloData = [];
    var siloStats = [];
    var loadData = function(cb){
        http.get(`/HistoricalStats/${host}`, (err, data) => {
            siloData = data;
            render();
        });
        http.get(`/SiloStats/${host}`, (err, data) => {
            siloStats = data;
            render();
        });
    }

    var renderOverloaded = function(){
        if (!siloData.length) return null;
        if (!siloData[siloData.length-1]) return null;
        if (!siloData[siloData.length-1].isOverloaded) return null;
        return <small><span className="label label-danger">OVERLOADED</span></small>
    },

    render = function(){
        if (routeIndex != thisRouteIndex) return;
        var silo = (dashboardCounters.hosts || []).filter(x => x.siloAddress === host)[0] || {};
        var subTitle = <span><SiloState status={silo.status}/> {renderOverloaded()}</span>
        renderPage(<Page title={"Silo " + host} subTitle={subTitle}><Silo silo={host} data={siloData} siloProperties={siloProperties} dashboardCounters={dashboardCounters} siloStats={siloStats} /></Page>, "#/silos");
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
    var thisRouteIndex = ++routeIndex;
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
        if (routeIndex != thisRouteIndex) return;
        renderPage(<Grain grainType={grainType} dashboardCounters={dashboardCounters} grainStats={grainStats} />, "#/grains");
    }

    events.on('dashboard-counters', render);
    events.on('refresh', loadData);

    loadData();

});

routie('/trace', function(){
    var thisRouteIndex = ++routeIndex;
    events.clearAll();
    scroll();
    var xhr = http.stream("/Trace");
    renderPage(<LogStream xhr={xhr} />, "#/trace");
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
        },
        {
            name:"Log Stream",
            path:"#/trace",
            icon:"fa-file-text"
        }
    ];
}
