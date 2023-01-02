import React from 'react'
import Panel from '../components/panel'

interface ICounter {
  name:string
  value:string
  delta: number
}

interface IProps {
  counters: ICounter[]
}

export default class SiloCounters extends React.Component<IProps> {
  renderItem(item:ICounter) {
    return (
      <tr key={item.name}>
        <td style={{ textOverflow: 'ellipsis' }}>{item.name}</td>
        <td>
          <strong>{item.value}</strong>
        </td>
        <td style={{ whiteSpace: 'nowrap' }}>
          {item.delta === null ? '' : <span>&Delta; {item.delta}</span>}
        </td>
      </tr>
    )
  }
  render() {
    return (
      <div>
        <Panel title="Silo Counters">
          <div>
            <table className="table">
              <tbody>{this.props.counters.map(this.renderItem)}</tbody>
            </table>
            {this.props.counters.length === 0 ? (
              <span>
                <p className="lead">No counters available.</p> It may take a few
                minutes for data to be published.
              </span>
            ) : null}
          </div>
        </Panel>
      </div>
    )
  }
}
