import React from 'react'


function getMenu() {
  var result:IMenuItem[] = [
    {
      name: 'Overview',
      path: '#/',
      icon: 'fa fa-tachometer-alt'
    },
    {
      name: 'Grains',
      path: '#/grains',
      icon: 'fa fa-cubes'
    },
    {
      name: 'Silos',
      path: '#/silos',
      icon: 'fa fa-database'
    },
    {
      name: 'Reminders',
      path: '#/reminders',
      icon: 'fa fa-calendar'
    }
  ]

  if (!(window as any).hideTrace) {
    result.push({
      name: 'Log Stream',
      path: '#/trace',
      icon: 'fa fa-bars'
    })
  }

  result.push({
    name: 'Preferences',
    path: '#/preferences',
    icon: 'fa fa-gear',
    style: { position: 'absolute', bottom: 0, left: 0, right: 0 }
  })

  return result
}


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
  icon: string
  path: string
  style?: React.CSSProperties
}

interface IProps {
  activeMenuItem: string
}

export default class Menu extends React.Component<IProps> {
  render() {
    return (
      <ul className="sidebar-menu">
        {getMenu().map(x => (
          <MenuSection
            key={x.name}
            active={x.path === this.props.activeMenuItem}
            icon={x.icon}
            name={x.name}
            path={x.path}
            style={x.style || {}}
          />
        ))}
      </ul>
    )
  }
}
