import React from 'react'

import CounterWidget from '../components/counter-widget'
import ReminderTable from '../components/reminder-table'
import Panel from '../components/panel'
import { ReminderData } from '../models/reminder'

interface IProps {
  remindersData: ReminderData,
  page: number
}

export default class Reminders extends React.Component<IProps> {
  render() {
    var totalPages = Math.ceil(this.props.remindersData.count / 25)
    var showFirst = this.props.page > 2
    var showPrevious = this.props.page > 1
    var showNext = totalPages > this.props.page
    var showLast = totalPages > this.props.page + 1
    return (
      <div>
        <div className="row">
          <div className="col-md-12">
            <CounterWidget
              icon="calendar"
              counter={`${this.props.remindersData.count}`}
              title="Reminders Count"
            />
          </div>
        </div>
        <Panel title="Reminders" subTitle={`Page ${this.props.page}`}>
          <div>
            <ReminderTable data={this.props.remindersData.reminders} />
            <div style={{ textAlign: 'center' }}>
              {showFirst ? (
                <a className="btn btn-default bg-purple" href={'#/reminders/1'}>
                  <i className="fa fa-arrow-circle-left" /> First
                </a>
              ) : null}
              <span> </span>
              {showPrevious ? (
                <a
                  className="btn btn-default bg-purple"
                  href={`#/reminders/${this.props.page - 1}`}
                >
                  <i className="fa fa-arrow-circle-left" /> Previous
                </a>
              ) : null}
              <span> </span>
              {showNext ? (
                <a
                  className="btn btn-default bg-purple"
                  href={`#/reminders/${this.props.page + 1}`}
                >
                  Next <i className="fa fa-arrow-circle-right" />
                </a>
              ) : null}
              <span> </span>
              {showLast ? (
                <a
                  className="btn btn-default bg-purple"
                  href={`#/reminders/${totalPages}`}
                >
                  Last <i className="fa fa-arrow-circle-right" />
                </a>
              ) : null}
            </div>
          </div>
        </Panel>
      </div>
    )
  }
}
