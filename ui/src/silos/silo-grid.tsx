import React from 'react'
import { DashboardCounters, Host } from '../models/dashboardCounters'
import SiloState from './silo-state-label'

interface IProps {
  dashboardCounters: DashboardCounters
}

export default class SiloGrid extends React.Component<IProps> {

  renderSilo = (silo: Host) => {
    return (
      <div key={silo.siloAddress} className="well well-sm">
        <a href={'#/host/' + silo.siloAddress}>{silo.siloAddress}</a>{' '}
        <small>
          <SiloState status={silo.status} />
        </small>
      </div>
    )
  }

  renderZone = (updateZone:number, faultZone:number) => {
    var matchingSilos = (this.props.dashboardCounters.hosts || []).filter(
      x => x.updateZone === updateZone && x.faultZone === faultZone
    )
    return <span>{matchingSilos.map(this.renderSilo)}</span>
  }

  render() {
    var hosts = this.props.dashboardCounters.hosts || []

    if (hosts.length === 0) return <span>no data</span>

    var updateZones = hosts
      .map(x => x.updateZone)
      .sort()
      .filter((v, i, a) => a.indexOf(v) === i)
    var faultZones = hosts
      .map(x => x.faultZone)
      .sort()
      .filter((v, i, a) => a.indexOf(v) === i)

    return (
      <div>
        <table className="table table-bordered table-hovered">
          <tbody>
            <tr>
              <td />
              {faultZones.map(faultZone => {
                return <th key={faultZone}>Fault Zone {faultZone}</th>
              })}
            </tr>
            {updateZones.map(updateZone => {
              return (
                <tr key={updateZone}>
                  <th>Update Zone {updateZone}</th>
                  {faultZones.map(faultZone => {
                    return (
                      <td key={faultZone}>
                        {this.renderZone(updateZone, faultZone)}
                      </td>
                    )
                  })}
                </tr>
              )
            })}
          </tbody>
        </table>
      </div>
    )
  }
}
