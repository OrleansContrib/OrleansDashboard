const React = require('react')

module.exports = class extends React.Component {
  constructor(props) {
    super(props)
    this.handleClick = this.handleClick.bind(this)
  }
  handleClick() {
    if (this.props.onClose) {
      this.props.onClose()
    }
  }
  render() {
    return (
      <div className="alert alert-danger alert-dismissible">
        <button
          type="button"
          className="close"
          onClick={this.handleClick}
          ariaHidden="true"
        >
          ×
        </button>
        <h4>
          <i className="icon fa fa-ban" /> {this.props.title || 'Error'}
        </h4>
        {this.props.children}.
      </div>
    )
  }
}
