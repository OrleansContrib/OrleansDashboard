var React = require('react');
var SiloState = require('./silo-state.jsx');

module.exports = React.createClass({
    renderHost:function(host, status){

        var subTotal = 0;
        this.props.dashboardCounters.simpleGrainStats.forEach(function(stat){
            if (stat.siloAddress.toLowerCase() === host.toLowerCase()) subTotal += stat.activationCount;
        });

        return <tr key={host}>
            <td><a href={"#/host/" + host}>{host}</a></td>
            <td><SiloState status={status}/></td>
            <td><span className="pull-right"><strong>{subTotal}</strong> <small>activations</small></span></td>
        </tr>
    },
    render:function(){
        if (!this.props.dashboardCounters.hosts) return null;
        return <table className="table">
            <tbody>
                { Object.keys(this.props.dashboardCounters.hosts).sort().map(function(key){
                    return this.renderHost(key, this.props.dashboardCounters.hosts[key])
                }, this) }
            </tbody>
        </table>
    }
});