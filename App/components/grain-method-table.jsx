const React = require('react')

module.exports = class extends React.Component {
  constructor(props) {
    super(props)
    this.renderRow = this.renderRow.bind(this)
  }
  renderRow(value) {
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
    const values = this.props.values || [];

    return (
      <table className="table" style={{ tableLayout: 'fixed', width: '100%' }}>
        <tbody>
          {values.map(this.renderRow)}
          {values.length === 0 &&
            <tr>
              <td>
                <i>No data</i>
              </td>
            </tr>
          }
        </tbody>
      </table>
    )
  }
}
