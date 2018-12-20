var React = require('react')

module.exports = class extends React.Component {
  render() {
    return (
      <div>
        <section className="content-header">
          <h1>
            {this.props.title} <small> {this.props.subTitle}</small>
          </h1>
        </section>
        <section className="content" style={{ position: 'relative' }}>
          {this.props.children}
        </section>
      </div>
    )
  }
}
