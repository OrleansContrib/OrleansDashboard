import React from 'react'
import './App.css'
import { getDashboardCounters } from './lib/api'
import routie from './lib/routie'
import setIntervalDebounced from './lib/setIntervalDebounced'
import { DashboardCounters } from './models/dashboardCounters'

interface IState { 
  renderMethod: () => JSX.Element
  dashboardCounters: DashboardCounters
}

export default class App extends React.Component<{}, IState> {

  state:IState = {
    renderMethod: () => <div>Loading...</div>,
    dashboardCounters: {
      simpleGrainStats: [],
      totalActivationCount: 0,
      totalActivationCountHistory: [],
      totalActiveHostCount: 0,
      totalActiveHostCountHistory: [],
      hosts: []
    }
  }

  cancel?: () => void

  componentDidMount() {
    this.cancel = setIntervalDebounced(this.loadDataOnSchedule, 1000)

    routie('', () => {
      const renderMethod = () => {
        return <div>Home <a href="#/grains">Grains</a></div>
      }
      
      this.setState({renderMethod})
    })

    routie('/grains', () => {
      const renderMethod = () => {
        return <div>grains</div>
      }
      
      this.setState({renderMethod})
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
    return this.state.renderMethod()
  }

}

