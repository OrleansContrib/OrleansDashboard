import React from 'react'

interface IValue {
  grain: string
  method: string
}

interface IProps {
  values: IValue[]
  valueFormatter: (value: any) => string
}

export default class GrainMethodTable extends React.Component<IProps> {
  renderRow = (value: IValue) => {
    return (
      <tr key={`${value.grain}.${value.method}`}>
        <td style={{ wordWrap: 'break-word' }}>
          <span className="pull-right">
            <strong>{this.props.valueFormatter(value)}</strong>
          </span>
          {value.method}
          <br />
          <small>
            <a href={`#/grain/${value.grain}`}>{value.grain}</a>
          </small>
        </td>
      </tr>
    )
  }
  render() {
    const values = this.props.values || []

    return (
      <table className="table" style={{ tableLayout: 'fixed', width: '100%' }}>
        <tbody>
          {values.map(this.renderRow)}
          {values.length === 0 && (
            <tr>
              <td>
                <i>No data</i>
              </td>
            </tr>
          )}
        </tbody>
      </table>
    )
  }
}
