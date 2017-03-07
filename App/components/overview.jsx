var React = require('react');
var CounterWidget = require('./counter-widget.jsx');
var Panel = require('./panel.jsx');

module.exports = React.createClass({
    render:function(){
        return <div>
            <div className="row">
                <div className="col-md-6">
                    <CounterWidget icon="cubes" counter={this.props.dashboardCounters.totalActivationCount} title="Total Activations" link="#/grains" />
                </div>
                <div className="col-md-6">
                    <CounterWidget icon="database" counter={this.props.dashboardCounters.totalActiveHostCount} title="Active Silos" link="#/silos" />
                </div>
            </div>
        </div>
    }
});
