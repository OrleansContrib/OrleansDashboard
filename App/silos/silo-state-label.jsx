const React = require('react')

const labelClassMapper = {
  Created: 'info',
  Joining: 'info',
  Active: 'success',
  ShuttingDown: 'warning',
  Stopping: 'warning',
  Dead: 'danger'
}

module.exports = class extends React.Component {
  render() {
    return (
      <span className={'label label-' + labelClassMapper[this.props.status]}>
        {this.props.status || 'unknown'}
      </span>
    )
  }
}
