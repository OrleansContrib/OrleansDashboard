const http = require('./lib/http');
const React = require('react');
const ReactDom = require('react-dom');
const routie = require('./lib/routie');
const Silo = require('./silos/silo.jsx');
const target = document.getElementById('content');
const events = require('eventthing');
const ThemeButtons = require('./components/theme-buttons.jsx');
const Grain = require('./grains/grain.jsx');
const Page = require('./components/page.jsx');
const Loading = require('./components/loading.jsx');
const Menu = require('./components/menu.jsx');
const Grains = require('./grains/grains.jsx');
const Silos = require('./silos/silos.jsx');
const Overview = require('./overview/overview.jsx');
const SiloState = require('./silos/silo-state-label.jsx');
const Alert = require('./components/alert.jsx');
const LogStream = require('./logstream/log-stream.jsx');
const SiloCounters = require('./silos/silo-counters.jsx');
const Reminders = require('./reminders/reminders.jsx');
var timer;

var dashboardCounters = {};
var routeIndex = 0;

function scroll(){
    try{
        document.getElementsByClassName("wrapper")[0].scrollTo(0,0);
    } catch(e){}
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
    http.get('DashboardCounters', function(err, data){
        dashboardCounters = data;
        events.emit('dashboard-counters', data);
    });
}



function getVersion() {
    var version = '2';
    var renderVersion = function(){
        ReactDom.render(<span id="version">
            v.{version} 
            <i style={{marginLeft:"12px", marginRight:"5px"}} className="fa fa-github"></i>
            <a style={{color:"white", textDecoration:"underline"}} href="https://github.com/OrleansContrib/OrleansDashboard/">Source</a>
        </span>, document.getElementById('version-content'));    
    }

    var loadData = function(cb){
        http.get('version', function(err, data){
            version = data.version;
            renderVersion();
        });
    }
    loadData();
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
    renderLoading();

    var clusterStats = {};
    var grainMethodStats = [];
    var loadData = function(cb){
        http.get('ClusterStats', function(err, data){
            clusterStats = data;
            http.get('TopGrainMethods', function(err, grainMethodsData){
                grainMethodStats = grainMethodsData
                render();
            })
        });
    }

    render = function(){
        if (routeIndex != thisRouteIndex) return;
        renderPage(<Page title="Dashboard"><Overview dashboardCounters={dashboardCounters} clusterStats={clusterStats} grainMethodStats={grainMethodStats} /></Page>, "#/");
    }

    events.on('dashboard-counters', render);
    events.on('refresh', loadData);
    getVersion();
    loadDashboardCounters();
});

routie('/grains', function(){
    var thisRouteIndex = ++routeIndex;
    events.clearAll();
    scroll();
    renderLoading();

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
    renderLoading();

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
    renderLoading();

    var siloProperties = {};

    var siloData = [];
    var siloStats = [];
    var loadData = function(cb){
        http.get(`HistoricalStats/${host}`, (err, data) => {
            siloData = data;
            render();
        });
        http.get(`SiloStats/${host}`, (err, data) => {
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
        renderPage(<Page title={`Silo ${host}`} subTitle={subTitle}><Silo silo={host} data={siloData} siloProperties={siloProperties} dashboardCounters={dashboardCounters} siloStats={siloStats} /></Page>, "#/silos");
    }

    events.on('dashboard-counters', render);
    events.on('refresh', loadData);

    http.get('SiloProperties/' + host, function(err, data){
        siloProperties = data;
        loadData();
    });

});

routie('/host/:host/counters', function(host){
    var thisRouteIndex = ++routeIndex;
    events.clearAll();
    scroll();
    renderLoading();

    http.get(`SiloCounters/${host}`, (err, data) => {

        var subTitle = <a href={`#/host/${host}`}>Silo Details</a>
        renderPage(<Page title={`Silo ${host}`} subTitle={subTitle}><SiloCounters silo={host} dashboardCounters={dashboardCounters} counters={data}/></Page>, "#/silos")
    });
});


routie('/grain/:grainType', function(grainType){
    var thisRouteIndex = ++routeIndex;
    events.clearAll();
    scroll();
    renderLoading();

    var grainStats = {};
    var loadData = function(cb){
        http.get('GrainStats/' + grainType, function(err, data){
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

routie('/reminders/:page?', function(page){
    var thisRouteIndex = ++routeIndex;
    events.clearAll();
    scroll();
    renderLoading();

    var remindersData = [];
    if (page){
      page = parseInt(page);
    } else {
      page = 1;
    }

    var renderReminders = function(){
        if (routeIndex != thisRouteIndex) return;
        renderPage(<Page title="Reminders"><Reminders remindersData={remindersData} page={page} /></Page>, "#/reminders");
    }

    var rerouteToLastPage = function(lastPage){
        return document.location.hash = `/reminders/${lastPage}`
    }

    var loadData = function(cb){
        http.get(`Reminders/${page}`, function(err, data){
            remindersData = data;
            renderReminders();
        });
    }

    events.on('long-refresh', loadData);

    loadData();
});

routie('/trace', function(){
    var thisRouteIndex = ++routeIndex;
    events.clearAll();
    scroll();
    var xhr = http.stream("Trace");
    renderPage(<LogStream xhr={xhr} />, "#/trace");
});


setInterval(() => events.emit('refresh'), 1000);
setInterval(() => events.emit('long-refresh'), 10000);

routie.reload();



function getMenu(){
    var result = [
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
            name:"Reminders",
            path:"#/reminders",
            icon:"fa-circle"
        }
    ];

    if (!window.hideTrace) {
        result.push({
            name:"Log Stream",
            path:"#/trace",
            icon:"fa-file-text"
        });
    }

    return result;
}
