var React = require('react')

module.exports = class extends React.Component {
  constructor(props) {
    super(props)
    this.state = {
      log: 'Connecting...',
      filter: '',
      scrollEnabled: true
    }
    this.scroll = this.scroll.bind(this)
    this.onProgress = this.onProgress.bind(this)
    this.toggle = this.toggle.bind(this)
    this.filterChanged = this.filterChanged.bind(this)
    this.getFilteredLog = this.getFilteredLog.bind(this)
  }

  scroll() {
    this.refs.log.scrollTop = this.refs.log.scrollHeight
  }

  onProgress() {
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

  toggle() {
    this.setState({
      scrollEnabled: !this.state.scrollEnabled
    })
  }

  filterChanged(event) {
    this.setState({
      filter: event.target.value,
      filterRegex: new RegExp(
        `[^\s-]* (Trace|Debug|Information|Warning|Error):.*${
          event.target.value
        }.*`,
        'gmi'
      )
    })
  }

  getFilteredLog() {
    if (!this.state.filter) return this.state.log

    const matches = this.state.log.match(this.state.filterRegex)
    return matches ? matches.join('\r\n') : ''
  }

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
          ref="log"
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
