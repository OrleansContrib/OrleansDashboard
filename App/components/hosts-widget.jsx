var React = require('react');

module.exports = React.createClass({
    renderHost:function(host, status){

        var labelClassMapper = {
            Created : 'info',
            Joining : 'info',
            Active : 'success',
            ShuttingDown : 'warning',
            Stopping : 'warning',
            Dead : 'danger'
        }

        var subTotal = 0;
        this.props.dashboardCounters.simpleGrainStats.forEach(function(stat){
            if (stat.siloAddress.toLowerCase() === host.toLowerCase()) subTotal += stat.activationCount;
        });

        return <tr key={host}>
            <td><a href={"#/host/" + host}>{host}</a></td>
            <td><span className={"label label-" + labelClassMapper[status]}>{status}</span></td>
            <td><span className="pull-right"><strong>{subTotal}</strong> <small>activations</small></span></td>
        </tr>
    },
    render:function(){
        return <table className="table">
            <tbody>
                { Object.keys(this.props.dashboardCounters.hosts).sort().map(function(key){
                    return this.renderHost(key, this.props.dashboardCounters.hosts[key])
                }, this) }
            </tbody>
        </table>
    }
});