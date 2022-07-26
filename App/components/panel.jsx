var React = require('react')

module.exports = class extends React.Component {
  render() {
    if (this.props.children.length) {
      var body = this.props.children[0]
      var footer = (
        <div className="box-footer clearfix">{this.props.children[1]}</div>
      )
    } else {
      var body = this.props.children
      var footer = null
    }

    const bodyStyle = { };
    if(this.props.bodyPadding){
      bodyStyle.padding = this.props.bodyPadding;
    }
    return (
      <div className="box">
        <div className="box-header with-border">
          <h3 className="box-title">
            {this.props.title}
            <small style={{ marginLeft: '10px' }}>{this.props.subTitle}</small>
          </h3>
        </div>
        <div className="box-body" style={bodyStyle}>{body}</div>
        {footer}
      </div>
    )
  }
}
