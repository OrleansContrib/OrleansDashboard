const React = require('react')

module.exports = class CheckboxFilter extends React.Component {
  constructor(props) {
    super(props)
    this.state = {
      hidden:
        this.props.preference === 'system'
          ? this.props.settings.systemGrainsHidden
          : this.props.settings.dashboardGrainsHidden
    }
    this.handleChangeFilter = this.handleChangeFilter.bind(this)
  }

  handleChangeFilter(e) {
    // Prevent link navigation.
    e.preventDefault()

    const hidden = e.target.name === 'hidden'
    const newSettings = {
      [this.props.preference === 'system'
        ? 'systemGrainsHidden'
        : 'dashboardGrainsHidden']: hidden
    }
    this.props.onChange(newSettings)
    this.setState({ hidden })
  }

  render() {
    return (
      <div className="btn-group btn-group-sm" role="group">
        <a
          href="#/"
          className={this.state.hidden ? 'btn btn-default' : 'btn btn-primary'}
          name="visible"
          onClick={this.handleChangeFilter}
        >
          Visible
        </a>
        <a
          href="#/"
          className={this.state.hidden ? 'btn btn-primary' : 'btn btn-default'}
          name="hidden"
          onClick={this.handleChangeFilter}
        >
          Hidden
        </a>
      </div>
    )
  }
}
