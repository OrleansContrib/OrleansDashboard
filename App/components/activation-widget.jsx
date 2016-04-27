var React = require('react');

var CounterWidget = require('./counter-widget.jsx');
var ChartWidget = require('./multi-series-chart-widget.jsx');
var GrainBreakdown = require('./grain-breakdown.jsx');

module.exports = React.createClass({
    render:function(){
        return <div className="well">
            <div className="row">
                <div className="col-md-3">
                    <CounterWidget counter={this.props.dashboardCounters.totalActivationCount} title="Total Activations" />
                </div>
                <div className="col-md-9">
                    <ChartWidget series={[this.props.dashboardCounters.totalActivationCountHistory]}/>
                </div>
            </div>
            <div>
                <h4>Activations by Type</h4>
                <GrainBreakdown data={this.props.dashboardCounters.simpleGrainStats}/>
            </div>
        </div>
    }
});
