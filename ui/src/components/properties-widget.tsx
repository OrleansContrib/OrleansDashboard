import React from 'react'

interface IProps {
  data: any
}

export default class PropertiesWidget extends React.Component<IProps> {
  renderRow = (key: string) => {
    return (
      <tr key={key}>
        <td style={{ textOverflow: 'ellipsis' }}>{key}</td>
        <td style={{ textAlign: 'right' }}>
          <strong>{this.props.data[key]}</strong>
        </td>
      </tr>
    )
  }
  render() {
    return (
      <table className="table">
        <tbody>{Object.keys(this.props.data).map(this.renderRow)}</tbody>
      </table>
    )
  }
}
