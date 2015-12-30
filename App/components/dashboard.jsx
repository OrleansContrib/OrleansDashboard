var React = require('react');

var SiloWidget = require('./silo-widget.jsx');
var ActivationWidget = require('./activation-widget.jsx');

module.exports = React.createClass({
    render:function(){
        return <div>
            <h2>Grains</h2>
            <ActivationWidget dashboardCounters={this.props.dashboardCounters}/>
            <h2>Silos</h2>
            <SiloWidget dashboardCounters={this.props.dashboardCounters} />
        </div>
    }
});