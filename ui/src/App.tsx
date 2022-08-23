import React from 'react'
import './App.css'
import Menu from './components/menu'
import { getDashboardCounters, getReminders } from './lib/api'
import routie from './lib/routie'
import setIntervalDebounced from './lib/setIntervalDebounced'
import { DashboardCounters } from './models/dashboardCounters'
import Overview from './overview/overview'
import Grains from './grains/grains'
import Grain from './grains/grain'
import Silos from './silos/silos'
import Reminders from './reminders/reminders'
import Loading from './components/loading'
import Silo from './silos/silo'
import Preferences from './components/preferences'
import LogStream from './logstream/log-stream'
import { stream } from './lib/http'

interface IState {
  renderMethod: () => JSX.Element
  dashboardCounters: DashboardCounters
  activeMenuItem: string
}

export default class App extends React.Component<{}, IState> {

  state: IState = {
    renderMethod: () => <div>Loading...</div>,
    dashboardCounters: {
      simpleGrainStats: [],
      totalActivationCount: 0,
      totalActivationCountHistory: [],
      totalActiveHostCount: 0,
      totalActiveHostCountHistory: [],
      hosts: []
    },
    activeMenuItem: '#/'
  }

  cancel?: () => void

  componentDidMount() {
    this.cancel = setIntervalDebounced(this.loadDataOnSchedule, 1000)

    routie('', () => {
      const renderMethod = () => {
        return <Overview dashboardCounters={this.state.dashboardCounters} />
      }

      this.setState({ renderMethod, activeMenuItem: '#/' })
    })

    routie('/grains', () => {
      const renderMethod = () => {
        return <Grains dashboardCounters={this.state.dashboardCounters} />
      }
      this.setState({ renderMethod, activeMenuItem: '#/grains' })
    })

    routie('/grain/:grainType', (grainType: string) => {
      const renderMethod = () => {
        return <Grain grainType={grainType} dashboardCounters={this.state.dashboardCounters} />
      }
      this.setState({ renderMethod, activeMenuItem: '#/grains' })
    })

    routie('/silos', () => {
      const renderMethod = () => {
        return <Silos dashboardCounters={this.state.dashboardCounters} />
      }
      this.setState({ renderMethod, activeMenuItem: '#/silos' })
    })

    routie('/host/:host', (host: string) => {
      const renderMethod = () => {
        return <Silo dashboardCounters={this.state.dashboardCounters} silo={host} />
      }
      this.setState({ renderMethod, activeMenuItem: '#/silos' })
    })

    routie('/reminders/:page?', async (page: string) => {
      const pageNumber = parseInt(page || '1')
      const activeMenuItem = '#/reminders'
      this.setState({ renderMethod: () => <Loading />, activeMenuItem })

      // TODO: move this api request into the state of the reminder page.

      const reminderData = await getReminders(pageNumber)

      const renderMethod = () => {
        return <Reminders page={pageNumber} remindersData={reminderData} />
      }

      this.setState({
        activeMenuItem,
        renderMethod
      })
    })

    routie('/trace', () => {
      var xhr = stream('Trace')

      const renderMethod = () => {
        return <LogStream xhr={xhr} />
      }
      this.setState({
        activeMenuItem: '#/trace',
        renderMethod
      })
    })

    routie('/preferences', () => {
      const renderMethod = () => {
        return <Preferences changeSettings={() => {}} settings={{ dashboardGrainsHidden: false, systemGrainsHidden: false}} />
      }

      this.setState({
        activeMenuItem: '#/preferences',
        renderMethod
      })
    })

    routie.reload()
  }

  componentWillUnmount() {
    if (this.cancel) this.cancel()
  }

  loadDataOnSchedule = async () => {
    const dashboardCounters = await getDashboardCounters()
    this.setState({ dashboardCounters })
  }

  render() {

    return <>
      <div id="error-message-content" className="error-container"></div>
      <div className="wrapper">
        <aside className="main-sidebar">
          <div style={{ padding: 10 }}>
            {/* eslint-disable-next-line jsx-a11y/anchor-is-valid */}
            <a href="#" >
              <h1 style={{ color: '#b8c7ce', fontWeight: 500, marginTop: 5, fontSize: 26 }}>
                OrleansDashboard
              </h1>
            </a>
            <div id="version-content" style={{ color: '#b8c7ce', marginTop: 5, marginBottom: 25 }}></div>
          </div>

          <section className="sidebar">
            <div id="menu">
              <Menu activeMenuItem={this.state.activeMenuItem} />
            </div>
          </section>
        </aside>

        <div className="content-wrapper" id="content">
          <section className="content" style={{ height: '100vh' }}>
            {this.state.renderMethod()}
          </section>
        </div>
        <div className="control-sidebar-bg"></div>
      </div>
    </>

  }

}

