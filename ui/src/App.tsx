import React from 'react'
import './App.css'
import { getDashboardCounters } from './lib/api'
import setIntervalDebounced from './lib/setIntervalDebounced'

interface IState { }

export default class App extends React.Component<{}, IState> {
  cancel?: () => void

  componentDidMount() {
    this.cancel = setIntervalDebounced(this.loadDataOnSchedule, 1000)
  }

  componentWillUnmount() {
    if (this.cancel) this.cancel()
  }

  loadDataOnSchedule = async () => {
    const dashboardCounters = await getDashboardCounters()
    this.setState({ dashboardCounters })
  }

  render() {
    return <div>Loading...</div>
  }

}

