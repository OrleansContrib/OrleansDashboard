var React = require('react');

var CounterWidget = require('../components/counter-widget.jsx');
var ChartWidget = require('../components/multi-series-chart-widget.jsx');
var GrainBreakdown = require('../components/grain-table.jsx');
var Panel = require('../components/panel.jsx');

module.exports = React.createClass({
    render:function(){
        
        var stats = { totalActivationCount: 0 };
        this.props.dashboardCounters.simpleGrainStats.forEach(stat => {
          stats.totalActivationCount += stat.activationCount;
        });

        return <div>
            <div className="row">
                <div className="col-md-4">
                    <CounterWidget icon="cubes" counter={stats.totalActivationCount} title="Total Activations" />
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
