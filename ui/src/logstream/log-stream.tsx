import React, { createRef } from 'react'

interface IProps {
  xhr: XMLHttpRequest
}

interface IState {
  log: string
  filter: string
  scrollEnabled: boolean
}

export default class LogStream extends React.Component<IProps, IState> {
  state: IState = {
    log: 'Connecting...',
    filter: '',
    scrollEnabled: true
  }

  scroll = () => {
    if (this.logRef.current) {
      this.logRef.current.scrollTop = this.logRef.current?.scrollHeight
    }
  }

  onProgress = () => {
    if (!this.state.scrollEnabled) return
    this.setState(
      {
        log: this.props.xhr.responseText
      },
      this.scroll
    )
  }

  componentWillMount() {
    this.props.xhr.onprogress = this.onProgress
  }

  componentWillUnmount() {
    this.props.xhr.abort()
  }

  toggle = () => {
    this.setState({
      scrollEnabled: !this.state.scrollEnabled
    })
  }

  filterChanged = (event: any) => {
    this.setState({
      filter: event.target.value,
    })
  }

  getFilteredLog = () => {
    if (!this.state.filter) return this.state.log

    const regex = new RegExp(
      `[^\s-]* (Trace|Debug|Information|Warning|Error):.*${this.state.filter
      }.*`,
      'gmi'
    )

    const matches = this.state.log.match(regex)
    return matches ? matches.join('\r\n') : ''
  }

  logRef = createRef<HTMLPreElement>()

  render() {
    return (
      <div
        style={{
          display: 'flex',
          flexDirection: 'column',
          overflow: 'hidden',
          maxHeight: '100%'
        }}
      >
        <input
          type="search"
          name="filter"
          className="text log-filter"
          style={{ width: '100%', height: '40px' }}
          value={this.state.filter}
          onChange={this.filterChanged}
          placeholder={'Regex Filter'}
        />
        <pre
          ref={this.logRef}
          className="log"
          style={{
            overflowY: 'auto',
            width: '100%',
            height: 'calc(100vh - 100px)',
            whiteSpace: 'pre-wrap'
          }}
        >
          {this.getFilteredLog()}
        </pre>
        <a
          href="javascript:void"
          onClick={this.toggle}
          className="btn btn-default"
          style={{
            marginLeft: '-80px',
            position: 'fixed',
            top: '95px',
            left: '100%'
          }}
        >
          {this.state.scrollEnabled ? 'Pause' : 'Resume'}
        </a>
      </div>
    )
  }
}
