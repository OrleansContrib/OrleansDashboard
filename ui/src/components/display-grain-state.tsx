import React from 'react'

interface IProps {
  code: string
}

export default class DisplayGrainState extends React.Component<IProps> {
  render() {
    return <pre style={{ margin: '0 0' }}>{this.props.code}</pre>
  }
}
