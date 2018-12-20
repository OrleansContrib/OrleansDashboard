var React = require('react')

module.exports = class ThemeButtons extends React.Component {
  constructor(props) {
    super(props)
    this.state = {
      light: this.props.defaultTheme !== 'dark'
    }
    this.pickLight = this.pickLight.bind(this)
    this.pickDark = this.pickDark.bind(this)
  }

  pickLight(event) {
    // Prevent link navigation.
    event.preventDefault()

    this.props.light()
    this.setState({ light: true })
  }

  pickDark(event) {
    // Prevent link navigation.
    event.preventDefault()

    this.props.dark()
    this.setState({ light: false })
  }

  render() {
    return (
      <div className="btn-group btn-group-sm" role="group">
        <a
          href="#/"
          className={this.state.light ? 'btn btn-primary' : 'btn btn-default'}
          onClick={this.pickLight}
        >
          Light
        </a>
        <a
          href="#/"
          className={this.state.light ? 'btn btn-default' : 'btn btn-primary'}
          onClick={this.pickDark}
        >
          Dark
        </a>
      </div>
    )
  }
}
