import React from 'react'

export interface ISettings {
  systemGrainsHidden: boolean
  dashboardGrainsHidden: boolean
}

interface IProps {
  preference: string
  settings: ISettings
  onChange: (newSettings: Partial<ISettings>) => void
}

interface IState {
  hidden: boolean
}

export default class CheckboxFilter extends React.Component<IProps, IState> {
  state: IState = {
    hidden:
      this.props.preference === 'system'
        ? this.props.settings.systemGrainsHidden
        : this.props.settings.dashboardGrainsHidden
  }

  handleChangeFilter = (e: any) => {
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
          onClick={this.handleChangeFilter}
        >
          Visible
        </a>
        <a
          href="#/"
          className={this.state.hidden ? 'btn btn-primary' : 'btn btn-default'}
          onClick={this.handleChangeFilter}
        >
          Hidden
        </a>
      </div>
    )
  }
}
