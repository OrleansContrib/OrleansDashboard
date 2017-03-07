var React = require('react');

var CounterWidget = require('./counter-widget.jsx');
var ChartWidget = require('./multi-series-chart-widget.jsx');
var GrainBreakdown = require('./grain-breakdown.jsx');
var Panel = require('./panel.jsx');

module.exports = React.createClass({
    render:function(){
        return <div>
            <div className="row">
                <div className="col-md-4">
                    <CounterWidget icon="cubes" counter={this.props.dashboardCounters.totalActivationCount} title="Total Activations" />
                </div>
                <div className="col-md-8">
                    <div className="info-box" style={{padding:"5px"}}><ChartWidget series={[this.props.dashboardCounters.totalActivationCountHistory]}/></div>
                </div>
            </div>
            <Panel title="Activations by Type">
                <GrainBreakdown data={this.props.dashboardCounters.simpleGrainStats}/>
            </Panel>
        </div>
    }
});
