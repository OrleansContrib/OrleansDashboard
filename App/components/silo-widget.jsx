var React = require('react');

var CounterWidget = require('./counter-widget.jsx');
var ChartWidget = require('./multi-series-chart-widget.jsx');
var HostsWidget = require('./hosts-widget.jsx');

module.exports = React.createClass({
    render:function(){
        return <div className="well">
            <div className="row">
                <div className="col-md-3">
                    <CounterWidget counter={this.props.dashboardCounters.totalActiveHostCount} title="Active Silos" />
                </div>
                <div className="col-md-9">
                    <ChartWidget series={[this.props.dashboardCounters.totalActiveHostCountHistory]}/>
                </div>
            </div>
            <div>
                <h4>Silo Health</h4>
                <HostsWidget dashboardCounters={this.props.dashboardCounters}/>
            </div>
        </div>
    }
});
