import React from 'react'

export interface ISiloStat {
  grainType: string
  activationCount: number
  totalCalls: number
  totalExceptions: number
  totalAwaitTime: number
  siloAddress: string
  totalSeconds: number
}

interface IProps {
  data:ISiloStat[]
  grainType: string
}

export default class SiloTable extends React.Component<IProps> {
  renderStat(stat:ISiloStat) {
    return (
      <tr key={stat.siloAddress}>
        <td style={{ textOverflow: 'ellipsis' }} title={stat.siloAddress}>
          <a href={`#/host/${stat.siloAddress}`}>{stat.siloAddress}</a>
        </td>
        <td>
          <span className="pull-right">
            <strong>{stat.activationCount}</strong>
          </span>
        </td>
        <td>
          <span className="pull-right">
            <strong>
              {stat.totalCalls === 0
                ? '0.00'
                : ((100 * stat.totalExceptions) / stat.totalCalls).toFixed(2)}
            </strong>{' '}
            <small>%</small>
          </span>
        </td>
        <td>
          <span className="pull-right">
            <strong>
              {stat.totalSeconds === 0
                ? '0'
                : (stat.totalCalls / 100).toFixed(2)}
            </strong>{' '}
            <small>req/sec</small>
          </span>
        </td>
        <td>
          <span className="pull-right">
            <strong>
              {stat.totalCalls === 0
                ? '0'
                : (stat.totalAwaitTime / stat.totalCalls).toFixed(2)}
            </strong>{' '}
            <small>ms/req</small>
          </span>
        </td>
      </tr>
    )
  }
  render() {
    var silos:any = {}
    if (!this.props.data) return null

    this.props.data.forEach((stat:ISiloStat) => {
      if (!silos[stat.siloAddress]) {
        silos[stat.siloAddress] = {
          activationCount: 0,
          totalSeconds: 0,
          totalAwaitTime: 0,
          totalCalls: 0,
          totalExceptions: 0
        }
      }

      if (this.props.grainType && stat.grainType !== this.props.grainType)
        return

      var x = silos[stat.siloAddress]
      x.activationCount += stat.activationCount
      x.totalSeconds += stat.totalSeconds
      x.totalAwaitTime += stat.totalAwaitTime
      x.totalCalls += stat.totalCalls
      x.totalExceptions += stat.totalExceptions
    })

    var values = Object.keys(silos)
      .map(function(key) {
        var x = silos[key]
        x.siloAddress = key
        return x
      })
      .sort(function(a, b) {
        return b.activationCount - a.activationCount
      })

    return (
      <table className="table">
        <tbody>
          <tr>
            <th style={{ textAlign: 'left' }}>Silo</th>
            <th style={{ textAlign: 'right' }}>Activations</th>
            <th style={{ textAlign: 'right' }}>Exception rate</th>
            <th style={{ textAlign: 'right' }}>Throughput</th>
            <th style={{ textAlign: 'right' }}>Latency</th>
          </tr>
          {values.map(this.renderStat)}
        </tbody>
      </table>
    )
  }
}
