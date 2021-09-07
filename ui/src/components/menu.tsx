import React from 'react'

interface ISectionProps {
  style: React.CSSProperties
  active: boolean
  path: string
  icon: string
  name: string
}

const MenuSection = class extends React.Component<ISectionProps> {
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

interface IMenuItem {
  name: string
  active: boolean
  icon: string
  path: string
  style: React.CSSProperties
}

interface IProps {
  menu: IMenuItem[]
}

export default class Menu extends React.Component<IProps> {
  render() {
    return (
      <ul className="sidebar-menu">
        {this.props.menu.map(x => (
          <MenuSection
            key={x.name}
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
