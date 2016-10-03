var React = require('react');
var SiloState = require('./silo-state.jsx');
var humanize = require('humanize-duration');

function sortFunction(a, b) {
    var nameA = a.siloAddress.toUpperCase(); // ignore upper and lowercase
    var nameB = b.siloAddress.toUpperCase(); // ignore upper and lowercase
    if (nameA < nameB) return -1;
    if (nameA > nameB) return 1;
    return 0;
}

module.exports = React.createClass({
    renderHost:function(host, silo){
        console.log(silo);
        var subTotal = 0;
        this.props.dashboardCounters.simpleGrainStats.forEach(function(stat){
            if (stat.siloAddress.toLowerCase() === host.toLowerCase()) subTotal += stat.activationCount;
        });

        return <tr key={host}>
            <td><a href={"#/host/" + host}>{host}</a></td>
            <td><SiloState status={silo.status}/></td>
            <td>up for {humanize(new Date().getTime() - new Date(silo.startTime).getTime(), { round: true, largest: 2  })}</td>
            <td><span className="pull-right"><strong>{subTotal}</strong> <small>activations</small></span></td>
        </tr>
    },
    render:function(){
        if (!this.props.dashboardCounters.hosts) return null;
        return <table className="table">
            <tbody>
                { this.props.dashboardCounters.hosts.sort(sortFunction).map(function(silo){
                    return this.renderHost(silo.siloAddress, silo)
                }, this) }
            </tbody>
        </table>
    }
});
