const http = require('./lib/http')
const React = require('react')
const ReactDom = require('react-dom')
const routie = require('./lib/routie')
const Silo = require('./silos/silo.jsx')
const events = require('eventthing')
const Grain = require('./grains/grain.jsx')
const Page = require('./components/page.jsx')
const Loading = require('./components/loading.jsx')
const Menu = require('./components/menu.jsx')
const Grains = require('./grains/grains.jsx')
const Silos = require('./silos/silos.jsx')
const Overview = require('./overview/overview.jsx')
const SiloState = require('./silos/silo-state-label.jsx')
const Alert = require('./components/alert.jsx')
const LogStream = require('./logstream/log-stream.jsx')
const SiloCounters = require('./silos/silo-counters.jsx')
const Reminders = require('./reminders/reminders.jsx')
const Preferences = require('./components/preferences.jsx')
const storage = require('./lib/storage')

const target = document.getElementById('content')

// Restore theme preference.
let defaultTheme = storage.get('theme')
defaultTheme === 'dark' ? dark() : light()

// Restore grain visibility preferences.
let settings = {
  dashboardGrainsHidden: storage.get('dashboardGrains') === 'hidden',
  systemGrainsHidden: storage.get('systemGrains') === 'hidden'
}

// Global state.
var dashboardCounters = {}
var unfilteredDashboardCounters = {}
var routeIndex = 0

function scroll() {
  try {
    document.getElementsByClassName('wrapper')[0].scrollTo(0, 0)
  } catch (e) {}
}

var errorTimer
function showError(message) {
  ReactDom.render(
    <Alert onClose={closeError}>{message}</Alert>,
    document.getElementById('error-message-content')
  )
  if (errorTimer) clearTimeout(errorTimer)
  errorTimer = setTimeout(closeError, 3000)
}

function closeError() {
  clearTimeout(errorTimer)
  errorTimer = null
  ReactDom.render(<span />, document.getElementById('error-message-content'))
}

http.onError(showError)

function setIntervalDebounced(action, interval) {
  Promise.resolve(action()).finally(() => {
    setTimeout(setIntervalDebounced.bind(this, action, interval), interval);
  });
}

// continually poll the dashboard counters
function loadDashboardCounters() {
  return http.get('DashboardCounters', function(err, data) {
    dashboardCounters = data
    unfilteredDashboardCounters = data
    dashboardCounters.simpleGrainStats = unfilteredDashboardCounters.simpleGrainStats.filter(
      getFilter(settings)
    )
    events.emit('dashboard-counters', dashboardCounters)
  })
}

function getVersion() {
  var version = '2'
  var renderVersion = function() {
    ReactDom.render(
      <span id="version" style={{marginLeft:40}}>
        v.{version}
        <i
          style={{ marginLeft: '12px', marginRight: '5px' }}
          className="fa fa-github"
        />
        <a
          style={{ color: '#b8c7ce', textDecoration: 'underline' }}
          href="https://github.com/OrleansContrib/OrleansDashboard/"
        >
          Source
        </a>
      </span>,
      document.getElementById('version-content')
    )
  }

  var loadData = function(cb) {
    http.get('version', function(err, data) {
      version = data.version
      renderVersion()
    })
  }
  loadData()
}

// we always want to refresh the dashboard counters
setIntervalDebounced(loadDashboardCounters, 1000)
loadDashboardCounters()
var render = () => {}

function renderLoading() {
  ReactDom.render(<Loading />, target)
}

var menuElement = document.getElementById('menu')

function renderPage(jsx, path) {
  ReactDom.render(jsx, target)
  var menu = getMenu()
  menu.forEach(x => {
    x.active = x.path === path
  })

  ReactDom.render(<Menu menu={menu} />, menuElement)
}

routie('', function() {
  var thisRouteIndex = ++routeIndex
  events.clearAll()
  scroll()
  renderLoading()

  var clusterStats = {}
  var grainMethodStats = []
  var unfiltedMethodStats = []
  var loadDataIsPending = false;
  var loadData = function(cb) {
    if (!loadDataIsPending) {
      loadDataIsPending = true;
      http.get('ClusterStats', function(err, data) {
        clusterStats = data
        http.get('TopGrainMethods', function(err, grainMethodsData) {
          loadDataIsPending = false;
          grainMethodStats = grainMethodsData
          unfiltedMethodStats = grainMethodsData
          grainMethodStats.calls = unfiltedMethodStats.calls.filter(
            getFilter(settings)
          )
          grainMethodStats.errors = unfiltedMethodStats.errors.filter(
            getFilter(settings)
          )
          grainMethodStats.latency = unfiltedMethodStats.latency.filter(
            getFilter(settings)
          )
          render()
        })
      })
    }
  }

  render = function() {
    if (routeIndex != thisRouteIndex) return
    renderPage(
      <Page title="Overview">
        <Overview
          dashboardCounters={dashboardCounters}
          clusterStats={clusterStats}
          grainMethodStats={grainMethodStats}
        />
      </Page>,
      '#/'
    )
  }

  events.on('dashboard-counters', render)
  events.on('refresh', loadData)
  loadDashboardCounters()
})

routie('/grains', function() {
  var thisRouteIndex = ++routeIndex
  events.clearAll()
  scroll()
  renderLoading()

  render = function() {
    if (routeIndex != thisRouteIndex) return
    renderPage(
      <Page title="Grains">
        <Grains dashboardCounters={dashboardCounters} />
      </Page>,
      '#/grains'
    )
  }

  events.on('dashboard-counters', render)
  events.on('refresh', render)

  loadDashboardCounters()
})

routie('/silos', function() {
  var thisRouteIndex = ++routeIndex
  events.clearAll()
  scroll()
  renderLoading()

  render = function() {
    if (routeIndex != thisRouteIndex) return
    renderPage(
      <Page title="Silos">
        <Silos dashboardCounters={dashboardCounters} />
      </Page>,
      '#/silos'
    )
  }

  events.on('dashboard-counters', render)
  events.on('refresh', render)

  loadDashboardCounters()
})

routie('/host/:host', function(host) {
  var thisRouteIndex = ++routeIndex
  events.clearAll()
  scroll()
  renderLoading()

  var siloProperties = {}

  var siloData = []
  var siloStats = []
  var loadData = function(cb) {
    http.get(`HistoricalStats/${host}`, (err, data) => {
      siloData = data
      render()
    })
    http.get(`SiloStats/${host}`, (err, data) => {
      siloStats = data
      render()
    })
  }

  var renderOverloaded = function() {
      if (!siloData.length) return null
      if (!siloData[siloData.length - 1]) return null
      if (!siloData[siloData.length - 1].isOverloaded) return null
      return (
        <small>
          <span className="label label-danger">OVERLOADED</span>
        </small>
      )
    },
    render = function() {
      if (routeIndex != thisRouteIndex) return
      var silo =
        (dashboardCounters.hosts || []).filter(
          x => x.siloAddress === host
        )[0] || {}
      var subTitle = (
        <span>
          <SiloState status={silo.status} /> {renderOverloaded()}
        </span>
      )
      renderPage(
        <Page title={`Silo ${host}`} subTitle={subTitle}>
          <Silo
            silo={host}
            data={siloData}
            siloProperties={siloProperties}
            dashboardCounters={dashboardCounters}
            siloStats={siloStats}
          />
        </Page>,
        '#/silos'
      )
    }

  events.on('dashboard-counters', render)
  events.on('refresh', loadData)

  http.get('SiloProperties/' + host, function(err, data) {
    siloProperties = data
    loadData()
  })
})

routie('/host/:host/counters', function(host) {
  var thisRouteIndex = ++routeIndex
  events.clearAll()
  scroll()
  renderLoading()

  http.get(`SiloCounters/${host}`, (err, data) => {
    if (routeIndex != thisRouteIndex) return
    var subTitle = <a href={`#/host/${host}`}>Silo Details</a>
    renderPage(
      <Page title={`Silo ${host}`} subTitle={subTitle}>
        <SiloCounters
          silo={host}
          dashboardCounters={dashboardCounters}
          counters={data}
        />
      </Page>,
      '#/silos'
    )
  })
})

routie('/grain/:grainType', function(grainType) {
  var thisRouteIndex = ++routeIndex
  events.clearAll()
  scroll()
  renderLoading()

  var grainStats = {}
  var loadDataIsPending = false;
  var loadData = function(cb) {
    if (!loadDataIsPending) {
      http.get('GrainStats/' + grainType, function(err, data) {
        loadDataIsPending = false;
        grainStats = data
        render()
      })
    }
  }

  render = function() {
    if (routeIndex != thisRouteIndex) return
    renderPage(
      <Grain
        grainType={grainType}
        dashboardCounters={dashboardCounters}
        grainStats={grainStats}
      />,
      '#/grains'
    )
  }

  events.on('dashboard-counters', render)
  events.on('refresh', loadData)

  loadData()
})

routie('/reminders/:page?', function(page) {
  var thisRouteIndex = ++routeIndex
  events.clearAll()
  scroll()
  renderLoading()

  var remindersData = []
  if (page) {
    page = parseInt(page)
  } else {
    page = 1
  }

  var renderReminders = function() {
    if (routeIndex != thisRouteIndex) return
    renderPage(
      <Page title="Reminders">
        <Reminders remindersData={remindersData} page={page} />
      </Page>,
      '#/reminders'
    )
  }

  var rerouteToLastPage = function(lastPage) {
    return (document.location.hash = `/reminders/${lastPage}`)
  }

  var loadDataIsPending = false;
  var loadData = function(cb) {
    if (!loadDataIsPending) {
      loadDataIsPending = true;
      http.get(`Reminders/${page}`, function(err, data) {
        loadDataIsPending = false;
        remindersData = data
        renderReminders()
      })
    }
  }

  events.on('long-refresh', loadData)

  loadData()
})

routie('/trace', function() {
  var thisRouteIndex = ++routeIndex
  events.clearAll()
  scroll()
  var xhr = http.stream('Trace')
  renderPage(<LogStream xhr={xhr} />, '#/trace')
})

routie('/preferences', function() {
  var thisRouteIndex = ++routeIndex
  events.clearAll()
  scroll()
  renderLoading()

  var changeSettings = newSettings => {
    settings = {
      ...settings
    }

    if (newSettings.hasOwnProperty('dashboardGrainsHidden')) {
      storage.put(
        'dashboardGrains',
        newSettings.dashboardGrainsHidden ? 'hidden' : 'visible'
      )
      settings.dashboardGrainsHidden = newSettings.dashboardGrainsHidden
    }

    if (newSettings.hasOwnProperty('systemGrainsHidden')) {
      storage.put(
        'systemGrains',
        newSettings.systemGrainsHidden ? 'hidden' : 'visible'
      )
      settings.systemGrainsHidden = newSettings.systemGrainsHidden
    }

    dashboardCounters.simpleGrainStats = unfilteredDashboardCounters.simpleGrainStats.filter(
      getFilter(settings)
    )
    events.emit('dashboard-counters', dashboardCounters)
  }

  render = function() {
    if (routeIndex != thisRouteIndex) return
    renderPage(
      <Page title="Preferences">
        <Preferences
          changeSettings={changeSettings}
          settings={settings}
          defaultTheme={defaultTheme}
          light={light}
          dark={dark}
        />
      </Page>,
      '#/preferences'
    )
  }
  loadDashboardCounters()

  render()
})

setInterval(() => events.emit('refresh'), 1000)
setInterval(() => events.emit('long-refresh'), 10000)

routie.reload()
getVersion()

function getMenu() {
  var result = [
    {
      name: 'Overview',
      path: '#/',
      icon: 'fa fa-tachometer-alt'
    },
    {
      name: 'Grains',
      path: '#/grains',
      icon: 'fa fa-cubes'
    },
    {
      name: 'Silos',
      path: '#/silos',
      icon: 'fa fa-database'
    },
    {
      name: 'Reminders',
      path: '#/reminders',
      icon: 'fa fa-calendar'
    }
  ]

  if (!window.hideTrace) {
    result.push({
      name: 'Log Stream',
      path: '#/trace',
      icon: 'fa fa-bars'
    })
  }

  result.push({
    name: 'Preferences',
    path: '#/preferences',
    icon: 'fa fa-gear',
    style: { position: 'absolute', bottom: 0, left: 0, right: 0 }
  })

  return result
}

function getFilter(settings) {
  let filter
  if (settings.dashboardGrainsHidden && settings.systemGrainsHidden) {
    filter = filterByBothDashSys
  } else if (settings.dashboardGrainsHidden) {
    filter = filterByDashboard
  } else if (settings.systemGrainsHidden) {
    filter = filterBySystem
  } else {
    filter = () => true
  }
  return filter
}

function filterByDashboard(x) {
  if (x.grainType == undefined) {
    var dashboardGrain = x.grain.startsWith('OrleansDashboard.')
    return !dashboardGrain
  } else {
    var dashboardGrain = x.grainType.startsWith('OrleansDashboard.')
    return !dashboardGrain
  }
}

function filterBySystem(x) {
  if (x.grainType == undefined) {
    var systemGrain = x.grain.startsWith('Orleans.')
    return !systemGrain
  } else {
    var systemGrain = x.grainType.startsWith('Orleans.')
    return !systemGrain
  }
}

function filterByBothDashSys(x) {
  if (x.grainType == undefined) {
    var systemGrain = x.grain.startsWith('Orleans.')
    var dashboardGrain = x.grain.startsWith('OrleansDashboard.')
    return !systemGrain && !dashboardGrain
  } else {
    var systemGrain = x.grainType.startsWith('Orleans.')
    var dashboardGrain = x.grainType.startsWith('OrleansDashboard.')
    return !systemGrain && !dashboardGrain
  }
}

function light() {
  // Save preference to localStorage.
  storage.put('theme', 'light')
  defaultTheme = 'light'

  // Disable dark theme (which falls back to light theme).
  const style = document.getElementById('dark-theme-style')
  style.setAttribute('media', 'none')
}

function dark() {
  // Save preference to localStorage.
  storage.put('theme', 'dark')
  defaultTheme = 'dark'

  // Enable dark theme.
  const style = document.getElementById('dark-theme-style')
  style.setAttribute('media', '')
}
