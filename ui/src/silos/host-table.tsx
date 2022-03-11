import React from 'react'
import SiloState from './silo-state-label'
import humanize from 'humanize-duration'
import { DashboardCounters, Host } from '../models/dashboardCounters'

function sortFunction(a: Host, b: Host) {
  var nameA = a.siloAddress.toUpperCase() // ignore upper and lowercase
  var nameB = b.siloAddress.toUpperCase() // ignore upper and lowercase
  if (nameA < nameB) return -1
  if (nameA > nameB) return 1
  return 0
}

interface IProps {
  dashboardCounters:DashboardCounters
}

export default class HostTable extends React.Component<IProps> {

  renderHost = (host: string, silo: Host) => {
    var subTotal = 0
    this.props.dashboardCounters.simpleGrainStats.forEach(function (stat) {
      if (stat.siloAddress.toLowerCase() === host.toLowerCase())
        subTotal += stat.activationCount
    })

    return (
      <tr key={host}>
        <td>
          <a href={'#/host/' + host}>{host}</a>
        </td>
        <td>
          <SiloState status={silo.status} />
        </td>
        <td>
          {silo.startTime ? (
            <span>
              up for{' '}
              {humanize(
                new Date().getTime() - new Date(silo.startTime).getTime(),
                { round: true, largest: 2 }
              )}
            </span>
          ) : (
            <span>uptime is not available</span>
          )}
        </td>
        <td>
          <span className="pull-right">
            <strong>{subTotal}</strong> <small>activations</small>
          </span>
        </td>
      </tr>
    )
  }
  render() {
    if (!this.props.dashboardCounters.hosts) return null
    return (
      <table className="table">
        <tbody>
          {this.props.dashboardCounters.hosts
            .sort(sortFunction)
            .map((silo:Host) => this.renderHost(silo.siloAddress, silo))}
        </tbody>
      </table>
    )
  }
}
