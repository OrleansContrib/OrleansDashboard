import React from 'react'

import CounterWidget from '../components/counter-widget'
import ChartWidget from '../components/multi-series-chart-widget'
import { DashboardCounters } from '../models/dashboardCounters.js'
import HostsWidget from './host-table'
import SiloGrid from './silo-grid'
import Panel from '../components/panel'

interface IProps {
  dashboardCounters : DashboardCounters
}

export default class Silos extends React.Component<IProps> {
  render() {
    return (
      <div>
        <div className="row">
          <div className="col-md-4">
            <div className="info-box">
              <CounterWidget
                icon="database"
                counter={this.props.dashboardCounters.totalActiveHostCount}
                title="Active Silos"
              />
            </div>
          </div>
          <div className="col-md-8">
            <div className="info-box" style={{ padding: '5px' }}>
              <ChartWidget
                series={[
                  this.props.dashboardCounters.totalActiveHostCountHistory
                ]}
              />
            </div>
          </div>
        </div>

        <Panel title="Silo Health">
          <HostsWidget dashboardCounters={this.props.dashboardCounters} />
        </Panel>
        <Panel title="Silo Map">
          <SiloGrid dashboardCounters={this.props.dashboardCounters} />
        </Panel>
      </div>
    )
  }
}
