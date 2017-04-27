var React = require('react');

var CounterWidget = require('../components/counter-widget.jsx');
var ChartWidget = require('../components/multi-series-chart-widget.jsx');
var HostsWidget = require('./host-table.jsx');
var SiloGrid = require('./silo-grid.jsx');
var Panel = require('../components/panel.jsx');

module.exports = React.createClass({
    render:function(){
        return <div>

            <div className="row">
                <div className="col-md-4">
                    <div className="info-box"><CounterWidget icon="database" counter={this.props.dashboardCounters.totalActiveHostCount} title="Active Silos" /></div>
                </div>
                <div className="col-md-8">
                    <div className="info-box" style={{padding:"5px"}}>
                        <ChartWidget series={[this.props.dashboardCounters.totalActiveHostCountHistory]}/>
                    </div>
                </div>
            </div>

            <Panel title="Silo Health">
                <HostsWidget dashboardCounters={this.props.dashboardCounters}/>
            </Panel>
            <Panel title="Silo Map">
                <SiloGrid dashboardCounters={this.props.dashboardCounters}/>
            </Panel>
        </div>
    }
});
