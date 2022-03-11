import React from 'react'

const labelClassMapper = {
  Created: 'info',
  Joining: 'info',
  Active: 'success',
  ShuttingDown: 'warning',
  Stopping: 'warning',
  Dead: 'danger'
}

export default class SiloStateLabel extends React.Component<{ status: string }> {
  render() {
    return (
      <span className={'label label-' + (labelClassMapper as any)[this.props.status] || ''}>
        {this.props.status || 'unknown'}
      </span>
    )
  }
}
