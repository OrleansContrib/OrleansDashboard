import React from 'react'
import Page from '../components/page'
import DisplayGrainState from '../components/display-grain-state'
import Panel from '../components/panel'
import { getGrainState } from '../lib/api'

interface IProps {
  grainTypes: string[]
}

interface IState {
  grainId: string
  grainType: string
  grainState: any
}

export default class GrainDetails extends React.Component<IProps, IState> {
  state = { grainId: '', grainType: '', grainState: '' }

  handleGrainIdChange = (event: any) => {
    this.setState({ grainId: event.target.value })
  }
  handleGrainTypeChange = (event: any) => {
    this.setState({ grainType: event.target.value })
  }

  handleSubmit = async (event: any) => {
    event.preventDefault()
    const { grainId, grainType } = this.state
    const grainState = await getGrainState(grainType, grainId)
    this.setState({ grainState: grainState.value })
  }

  renderEmpty() {
    return <span>No state retrieved</span>
  }

  renderState() {
    let displayComponent

    if (this.state.grainState != '') {
      displayComponent = <DisplayGrainState code={this.state.grainState} />
    } else {
      displayComponent = <div></div>
    }

    return (
      <Page title="Grain Details" subTitle="">
        <div>
          <Panel title="Grain" subTitle="Only non generic grains are supported">
            <div className="row">
              <div className="col-md-6 col-lg-6 col-xl-6">
                <div className="input-group">
                  <select
                    value={this.state.grainType}
                    className="form-control"
                    onChange={this.handleGrainTypeChange}
                  >
                    <option disabled selected value="">
                      {' '}
                      -- Select an grain type --{' '}
                    </option>
                    {this.props.grainTypes.map(_item => (
                      <option key={_item} value={_item}>
                        {_item}
                      </option>
                    ))}
                  </select>
                </div>
              </div>
              <div className="col-md-4 col-lg-4 col-xl-5">
                <div className="input-group">
                  <input
                    type="text"
                    placeholder="Grain Id"
                    className="form-control"
                    value={this.state.grainId}
                    onChange={this.handleGrainIdChange}
                  />
                </div>
              </div>
              <div className="col-md-2 col-lg-2 col-xl-1">
                <div className="input-group">
                  <input
                    type="button"
                    value="Show Details"
                    className="btn btn-primary"
                    onClick={this.handleSubmit}
                  />
                </div>
              </div>
            </div>
          </Panel>

          <Panel title="State" /*bodyPadding="0px"*/>
            <div className="row">
              <div className="col-md-12">{displayComponent}</div>
            </div>
          </Panel>
        </div>
      </Page>
    )
  }

  render() {
    return this.renderState()
  }
}
