import React from 'react'

interface IReminder {
  period: string
  grainReference: string
  primaryKey: string
  activationCount: number
  name: string
  startAt: string
}

interface IProps {
  data: IReminder[]
}

interface IState {
  grain_reference: string
  primary_key: string
  name: string
  startAt: string
  period: string
}

export default class ReminderTable extends React.Component<IProps, IState> {
  state: IState = {
    grain_reference: '',
    primary_key: '',
    name: '',
    startAt: '',
    period: ''
  }

  handleChange = (e: any) => {
    const newState: Partial<IState> = {}
    newState[e.target.name as keyof IState] = e.target.value
    this.setState(newState as IState)
  }
  renderReminder(reminderData: IReminder, index: number) {
    return (
      <tr key={index}>
        <td>{reminderData.grainReference}</td>
        <td>{reminderData.primaryKey}</td>
        <td>
          <span className="pull-right">{reminderData.activationCount}</span>
        </td>
        <td>
          <span className="pull-right">{reminderData.name}</span>
        </td>
        <td>
          <span className="pull-right">
            {new Date(reminderData.startAt).toLocaleString()}
          </span>
        </td>
        <td>
          <span className="pull-right">{reminderData.period}</span>
        </td>
      </tr>
    )
  }
  filterData(data: IReminder[]) {
    return data
      .filter(x =>
        this.state.grain_reference
          ? x.grainReference.includes(this.state.grain_reference)
          : x
      )
      .filter(x =>
        this.state.primary_key
          ? x.primaryKey.includes(this.state.primary_key)
          : x
      )
      .filter(x => (this.state.name ? x.name.includes(this.state.name) : x))
      .filter(x =>
        this.state.startAt ? x.startAt.includes(this.state.startAt) : x
      )
      .filter(x =>
        this.state.period ? x.period.includes(this.state.period) : x
      )
  }
  render() {
    if (!this.props.data) return null
    var filteredData = this.filterData(this.props.data)
    return (
      <table className="table">
        <tbody>
          <tr>
            <th style={{ textAlign: 'left' }}>Grain Reference</th>
            <th>Primary Key</th>
            <th />
            <th style={{ textAlign: 'left' }}>Name</th>
            <th style={{ textAlign: 'left' }}>Start At</th>
            <th style={{ textAlign: 'right' }}>Period</th>
          </tr>
          <tr>
            <th style={{ textAlign: 'left' }}>
              <input
                onChange={this.handleChange}
                value={this.state['grain_reference']}
                type="text"
                name="grain_reference"
                className="form-control"
                placeholder="Filter by Grain Reference"
              />
            </th>
            <th style={{ textAlign: 'left' }}>
              <input
                onChange={this.handleChange}
                value={this.state['primary_key']}
                type="text"
                name="primary_key"
                className="form-control"
                placeholder="Filter by Primary Key"
              />
            </th>
            <th />
            <th style={{ textAlign: 'left' }}>
              <input
                onChange={this.handleChange}
                value={this.state['name']}
                type="text"
                name="name"
                className="form-control"
                placeholder="Filter by Name"
              />
            </th>
            <th style={{ textAlign: 'left' }}>
              <input
                onChange={this.handleChange}
                value={this.state['startAt']}
                type="text"
                name="startAt"
                className="form-control"
                placeholder="Filter by Start At"
              />
            </th>
            <th style={{ textAlign: 'right' }}>
              <input
                onChange={this.handleChange}
                value={this.state['period']}
                type="text"
                name="period"
                className="form-control"
                placeholder="Filter by Period"
              />
            </th>
          </tr>
          {filteredData.map(this.renderReminder)}
        </tbody>
      </table>
    )
  }
}
