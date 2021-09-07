import React from 'react'

interface IProps {
  defaultTheme: 'light' | 'dark'
  light: () => void
  dark: () => void
}

interface IState {
  light: boolean
}

export default class ThemeButtons extends React.Component<IProps> {
  state: IState = {
    light: this.props.defaultTheme !== 'dark'
  }

  pickLight = (event: any) => {
    // Prevent link navigation.
    event.preventDefault()

    this.props.light()
    this.setState({ light: true })
  }

  pickDark = (event: any) => {
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
