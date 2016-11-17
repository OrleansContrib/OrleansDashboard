var React = require('react');


module.exports = React.createClass({

    renderStat:function(stat){
        return <tr key={stat.siloAddress}>
            <td style={{textOverflow: "ellipsis"}} title={stat.siloAddress}><a href={`#/host/${stat.siloAddress}`}>{stat.siloAddress}</a></td>
            <td><span className="pull-right"><strong>{stat.activationCount}</strong> <small>activation{stat.activationCount == 1 ? "" : "s"}</small></span></td>
            <td><span className="pull-right"><strong>{(stat.totalSeconds === 0) ? "0" : (stat.totalCalls / stat.totalSeconds).toFixed(2)}</strong> <small>req/sec</small></span></td>
            <td><span className="pull-right"><strong>{(stat.totalCalls === 0) ? "0" : (stat.totalAwaitTime / stat.totalCalls).toFixed(2)}</strong> <small>ms/req</small></span></td>
        </tr>
    },
    render:function(){
        var silos = {};
        if (!this.props.data) return null;

        this.props.data.forEach(stat => {

            if (!silos[stat.siloAddress]) {
                silos[stat.siloAddress] = {
                    activationCount: 0,
                    totalSeconds: 0,
                    totalAwaitTime : 0,
                    totalCalls : 0
                };
            }

            if (this.props.grainType && stat.grainType !== this.props.grainType) return;

            var x = silos[stat.siloAddress];
            x.activationCount += stat.activationCount;
            x.totalSeconds += stat.totalSeconds;
            x.totalAwaitTime += stat.totalAwaitTime;
            x.totalCalls += stat.totalCalls;
        });

        var values = Object.keys(silos).map(function(key){
            var x = silos[key];
            x.siloAddress = key;
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
