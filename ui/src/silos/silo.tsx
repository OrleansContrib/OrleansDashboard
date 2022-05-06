import React from 'react'
import Gauge from '../components/gauge-widget'
import PropertiesWidget from '../components/properties-widget'
import GrainBreakdown from '../components/grain-table'
import ChartWidget from '../components/multi-series-chart-widget'
import Panel from '../components/panel'
import Chart from '../components/time-series-chart'
import { DashboardCounters } from '../models/dashboardCounters'
import { Properties } from '../models/properties'
import { getHistoricalStats, getSiloProperties, getSiloStats } from '../lib/api'
import setIntervalDebounced from '../lib/setIntervalDebounced'
import { HistoricalStat } from '../models/historicalStat'
import { Stats } from '../models/stats'

interface ISiloGraphProps {
  stats: Stats
}

const SiloGraph = (props: ISiloGraphProps) => {
  const values = []
  const timepoints = []
  Object.keys(props.stats).forEach(key => {
    values.push(props.stats[key])
    timepoints.push(props.stats[key].period)
  })

  if (!values.length) {
    return null
  }

  while (values.length < 100) {
    values.unshift({ count: 0, elapsedTime: 0, period: 0, exceptionCount: 0 })
    timepoints.unshift('')
  }

  return (
    <div>
      <Chart
        timepoints={timepoints}
        series={[
          values.map(z => z.exceptionCount),
          values.map(z => z.count),
          values.map(z => (z.count === 0 ? 0 : z.elapsedTime / z.count))
        ]}
      />
    </div>
  )
}

interface IProps {
  dashboardCounters: DashboardCounters
  silo: string
}

interface IState {
  siloProperties: Properties
  historicalStats: HistoricalStat[]
  stats: Stats
}

export default class Silo extends React.Component<IProps, IState> {
  state: IState = {
    siloProperties: {
      HostVersion: '',
      OrleansVersion: ''
    },
    historicalStats: [],
    stats: {}
  }

  cancel?: () => void

  componentDidMount() {
    this.loadInitialData();
    this.cancel = setIntervalDebounced(this.loadDataOnSchedule, 1000)
  }

  loadDataOnSchedule = async () => {
    const stats = await getSiloStats(this.props.silo)
    const historicalStats = await getHistoricalStats(this.props.silo)
    this.setState({ stats, historicalStats })

  }

  loadInitialData = async () => {
    const siloProperties = await getSiloProperties(this.props.silo)
    this.setState({ siloProperties })
  }

  hasData = (value: any[]) => {
    for (var i = 0; i < value.length; i++) {
      if (value[i] !== null) return true
    }
    return false
  }

  querySeries = (lambda: (x: any) => number) => {
    return this.state.historicalStats.map(x => {
      if (!x) return 0
      return `${lambda(x)}`
    })
  }

  hasSeries = (lambda: (value: any) => boolean) => {
    for (var key in this.state.stats) {
      var value = this.state.stats[key]
      if (value && lambda(value)) {
        return true
      }
    }
    return false
  }

  render() {
    if (!this.hasData(this.state.historicalStats)) {
      return (
        <Panel title="Error">
          <div>
            <p className="lead">No data available for this silo</p>
            <p>
              <a href="#/silos">Show all silos</a>
            </p>
          </div>
        </Panel>
      )
    }

    var last = this.state.historicalStats[this.state.historicalStats.length - 1]
    var properties = {
      Clients: last.clientCount || '0',
      'Messages received': last.receivedMessages || '0',
      'Messages sent': last.sentMessages || '0',
      'Receive queue': last.receiveQueueLength || '0',
      'Request queue': last.requestQueueLength || '0',
      'Send queue': last.sendQueueLength || '0'
    }

    var grainStats = (
      this.props.dashboardCounters.simpleGrainStats || []
    ).filter(x => x.siloAddress === this.props.silo)

    var silo =
      this.props.dashboardCounters.hosts.filter(
        x => x.siloAddress === this.props.silo
      )[0] || {}

    var configuration: any = {
      'Host name': silo.hostName,
      'Role name': silo.roleName,
      'Silo name': silo.siloName,
      'Proxy port': silo.proxyPort,
      'Update zone': silo.updateZone,
      'Fault zone': silo.faultZone
    }

    if (this.state.siloProperties.OrleansVersion) {
      configuration[
        'Orleans version'
      ] = this.state.siloProperties.OrleansVersion
    }

    if (this.state.siloProperties.HostVersion) {
      configuration['Host version'] = this.state.siloProperties.HostVersion
    }

    var cpuGauge
    var memGauge

    if (this.hasSeries(x => x.cpuUsage > 0)) {
      cpuGauge = (
        <div>
          <Gauge
            value={last.cpuUsage}
            max={100}
            title="CPU Usage"
            description={Math.floor(last.cpuUsage) + '% utilisation'}
          />
          <ChartWidget series={[this.querySeries(x => x.cpuUsage).map(x => `${x}`)]} />
        </div>
      )
    } else {
      cpuGauge = (
        <div style={{ textAlign: 'center' }}>
          <h4>CPU Usage</h4>

          <div style={{ lineHeight: '40px' }}>No data available</div>
        </div>
      )
    }

    if (this.hasSeries(x => x.totalPhysicalMemory - x.availableMemory > 0)) {
      memGauge = (
        <div>
          <Gauge
            value={
              (last.totalPhysicalMemory || 0) - (last.availableMemory || 0)
            }
            max={last.totalPhysicalMemory || 1}
            title="Memory Usage"
            description={
              Math.floor((last.availableMemory || 0) / (1024 * 1024)) +
              ' MB free'
            }
          />
          <ChartWidget
            series={[
              this.querySeries(
                x => (x.totalPhysicalMemory - x.availableMemory) / (1024 * 1024)
              ).map(x => `${x}`)
            ]}
          />
        </div>
      )
    } else {
      memGauge = (
        <div style={{ textAlign: 'center' }}>
          <h4>Memory Usage</h4>

          <div style={{ lineHeight: '40px' }}>No data available</div>
        </div>
      )
    }

    return (
      <div>
        <Panel title="Overview">
          <div className="row">
            <div className="col-md-4">{cpuGauge}</div>
            <div className="col-md-4">{memGauge}</div>
            <div className="col-md-4">
              <Gauge
                value={last.recentlyUsedActivationCount}
                max={last.activationCount}
                title="Grain Usage"
                description={
                  last.activationCount +
                  ' activations, ' +
                  Math.floor(
                    (last.recentlyUsedActivationCount * 100) /
                    last.activationCount
                  ) +
                  '% recently used'
                }
              />
              
              <ChartWidget
                series={[
                  this.querySeries(x => x.activationCount).map(x => `${x}`),
                  this.querySeries(x => x.recentlyUsedActivationCount).map(x => `${x}`)
                ]}
              />
              
            </div>
          </div>
        </Panel>

        <Panel title="Silo Profiling">
          <div>
            <span>
              <strong style={{ color: '#783988', fontSize: '25px' }}>/</strong>{' '}
              number of requests per second
              <br />
              <strong style={{ color: '#EC1F1F', fontSize: '25px' }}>
                /
              </strong>{' '}
              failed requests
            </span>
            <span className="pull-right">
              <strong style={{ color: '#EC971F', fontSize: '25px' }}>/</strong>{' '}
              average latency in milliseconds
            </span>
            <SiloGraph stats={this.state.stats} />
          </div>
        </Panel>

        <div className="row">
          <div className="col-md-6">
            <Panel title="Silo Counters">
              <div>
                <PropertiesWidget data={properties} />
                <a href={`#/host/${this.props.silo}/counters`}>
                  View all counters
                </a>
              </div>
            </Panel>
          </div>
          <div className="col-md-6">
            <Panel title="Silo Properties">
              <PropertiesWidget data={configuration} />
            </Panel>
          </div>
        </div>

        <Panel title="Activations by Type">
          <GrainBreakdown data={grainStats} silo={this.props.silo} />
        </Panel>
      </div>
    )
  }
}
