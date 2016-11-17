var React = require('react');


module.exports = React.createClass({

    renderStat:function(stat){
        var parts = stat.grainType.split('.');
        var grainClassName = parts[parts.length - 1];
        var systemGrain = stat.grainType.startsWith("Orleans.Runtime.");
        var dashboardGrain = stat.grainType.startsWith("OrleansDashboard.");
        return <tr key={stat.grainType}>
            <td style={{textOverflow: "ellipsis"}} title={stat.grainType}><a href={`#/grain/${stat.grainType}`}>{grainClassName}</a></td>
            <td>{systemGrain ? <span className="label label-primary">System Grain</span> : null}{dashboardGrain ? <span className="label label-primary">Dashboard Grain</span> : null}</td>
            <td><span className="pull-right"><strong>{stat.activationCount}</strong> <small>activation{stat.activationCount == 1 ? "" : "s"}</small></span></td>
            <td><span className="pull-right"><strong>{(stat.totalCalls / stat.totalSeconds).toFixed(2)}</strong> <small>req/sec</small></span></td>
            <td><span className="pull-right"><strong>{(stat.totalCalls === 0) ? "0" : (stat.totalAwaitTime / stat.totalCalls).toFixed(2)}</strong> <small>ms/req</small></span></td>
        </tr>
    },
    render:function(){
        var grainTypes = {};
        if (!this.props.data) return null;

        this.props.data.forEach(stat => {

            if (this.props.silo && stat.siloAddress !== this.props.silo) return;

            if (!grainTypes[stat.grainType]) {
                grainTypes[stat.grainType] = {
                    activationCount: 0,
                    totalSeconds: 0,
                    totalAwaitTime : 0,
                    totalCalls : 0
                };
            }

            var x = grainTypes[stat.grainType];
            x.activationCount += stat.activationCount;
            x.totalSeconds += stat.totalSeconds;
            x.totalAwaitTime += stat.totalAwaitTime;
            x.totalCalls += stat.totalCalls;
        });

        var values = Object.keys(grainTypes).map(function(key){
            var x = grainTypes[key];
            x.grainType = key;
            return x;
        }).sort(function(a,b){
            return b.activationCount - a.activationCount;
        });

        return <table className="table">
            <tbody>
                {values.map(this.renderStat)}
            </tbody>
        </table>
    }
});
