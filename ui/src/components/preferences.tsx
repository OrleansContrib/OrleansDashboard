import React from 'react'
import ThemeButtons from './theme-buttons'
import CheckboxFilter, { ISettings } from './checkbox-filter'
import Panel from './panel'

interface IProps {
  light: () => void
  dark: () => void
  changeSettings: (newSettings: Partial<ISettings>) => void
  settings: ISettings
  defaultTheme: 'light' | 'dark'
}

export default class Preferences extends React.Component<IProps> {
  render() {
    const props = this.props
    return (
      <Panel title="Preferences">
        <div>
          <p>
            The following preferences can be used to customize the Orleans
            Dashboard. The selected preferences are saved locally in this
            browser. The selected preferences do not affect other browsers or
            users.
          </p>
          <p>
            Selecting the "Hidden" option for System Grains or Dashboard Grains
            will exclude the corresponding types from counters, graphs, and
            tables with the exception of the Cluster Profiling graph on the
            Overview page and the Silo Profiling graph on a silo overview page.
          </p>
          <div
            style={{
              alignItems: 'center',
              display: 'grid',
              grid: '1fr 1fr / auto 1fr',
              gridGap: '0px 30px'
            }}
          >
            <div>
              <h4>Dashboard Grains</h4>
            </div>
            <div>
              <CheckboxFilter
                onChange={props.changeSettings}
                settings={props.settings}
                preference="dashboard"
              />
            </div>
            <div>
              <h4>System Grains</h4>
            </div>
            <div>
              <CheckboxFilter
                onChange={props.changeSettings}
                settings={props.settings}
                preference="system"
              />
            </div>
            <div>
              <h4>Theme</h4>
            </div>
            <div>
              <ThemeButtons
                defaultTheme={props.defaultTheme}
                light={props.light}
                dark={props.dark}
              />
            </div>
          </div>
        </div>
      </Panel>
    )
  }
}
