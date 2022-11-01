import React from 'react'
import Gauge from '../components/gauge-widget'
import PropertiesWidget from '../components/properties-widget'
import GrainBreakdown from '../components/grain-table'
import ChartWidget from '../components/multi-series-chart-widget'
import Panel from '../components/panel'
import Chart from '../components/time-series-chart'
import { DashboardCounters } from '../models/dashboardCounters'
import { Properties } from '../models/properties'
import { getSiloProperties } from '../lib/api'

interface ISiloGraphProps {
  stats: {
    [key: string]: {
      period: number
    }
  }
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
}

export default class Silo extends React.Component<IProps, IState> {
  state: IState = {
    siloProperties: {
      HostVersion: '',
      OrleansVersion: ''
    }
  }

  componentDidMount() {
    this.loadInitialData();
  }

  loadInitialData = async () => {
    const siloProperties = await getSiloProperties(this.props.silo)
    this.setState({ siloProperties })
  }

  hasData(value: []) {
    for (var i = 0; i < value.length; i++) {
      if (value[i] !== null) return true
    }
    return false
  }

  querySeries(lambda: (x: any) => number) {
    return this.props.data.map(function (x) {
      if (!x) return 0
      return lambda(x)
    })
  }

  hasSeries(lambda) {
    var hasValue = false

    for (var key in this.props.data) {
      var value = this.props.data[key]
      if (value && lambda(value)) {
        hasValue = true
      }
    }

    return hasValue
  }

  render() {
    if (!this.hasData(this.props.data)) {
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

    var last = this.props.data[this.props.data.length - 1]
    var properties = {
      Clients: last.clientCount || '0',
      'Messages recieved': last.receivedMessages || '0',
      'Messages sent': last.sentMessages || '0',
      'Receive queue': last.receiveQueueLength || '0',
      'Request queue': last.requestQueueLength || '0',
      'Send queue': last.sendQueueLength || '0'
    }

    var grainStats = (
      this.props.dashboardCounters.simpleGrainStats || []
    ).filter(x => x.siloAddress === this.props.silo)

    var status = (this.props.dashboardCounters.hosts || {})[this.props.silo]
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
          <ChartWidget series={[this.querySeries(x => x.cpuUsage)]} />
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
              )
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
                  this.querySeries(x => x.activationCount),
                  this.querySeries(x => x.recentlyUsedActivationCount)
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
            <SiloGraph stats={this.props.siloStats} />
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
/*

dateTime: "2015-12-30T17:02:32.6695724Z"

cpuUsage: 11.8330326
activationCount: 4
availableMemory: 4301320000
totalPhysicalMemory: 8589934592
memoryUsage: 8618116
recentlyUsedActivationCount: 2


clientCount: 0
isOverloaded: false

receiveQueueLength: 0
requestQueueLength: 0
sendQueueLength: 0

receivedMessages: 0
sentMessages: 0

*/
