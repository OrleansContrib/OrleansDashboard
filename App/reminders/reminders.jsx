var React = require('react');

var CounterWidget = require('../components/counter-widget.jsx');
var ReminderTable = require('../components/reminder-table.jsx');
var Panel = require('../components/panel.jsx');

module.exports = React.createClass({
    render:function(){
        return <div>
            <div className="row">
                <div className="col-md-12">
                    <CounterWidget icon="cubes" counter={this.props.remindersData.length} title="Reminders Count" />
                </div>
            </div>
            <Panel title="Reminders">
                <ReminderTable data={this.props.remindersData}/>
            </Panel>
        </div>
    }
});
