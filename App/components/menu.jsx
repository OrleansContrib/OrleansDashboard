const React = require('react')

const MenuSection = class extends React.Component {
  render() {
    return (
      <li
        style={this.props.style}
        className={(this.props.active ? 'active' : '') + ' treeview'}
      >
        <a href={this.props.path}>
          <i className={this.props.icon} />
          {this.props.name}
        </a>
      </li>
    )
  }
}

module.exports = class extends React.Component {
  render() {
    return (
      <ul className="sidebar-menu">
        {this.props.menu.map(x => (
          <MenuSection
            active={x.active}
            icon={x.icon}
            name={x.name}
            path={x.path}
            style={x.style}
          />
        ))}
      </ul>
    )
  }
}
