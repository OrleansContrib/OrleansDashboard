var React = require('react');


module.exports = React.createClass({

    renderStat:function(stat){
        var parts = stat.grainType.split('.');
        var grainClassName = parts[parts.length - 1];
        var systemGrain = stat.grainType.startsWith("Orleans.Runtime.");
        return <tr key={stat.grainType}>
            <td style={{textOverflow: "ellipsis"}} title={stat.grainType}>{grainClassName}</td>
            <td>{systemGrain ? <span className="label label-primary">System Grain</span> : null}</td>
            <td><span className="pull-right"><strong>{stat.activationCount}</strong> <small>activation{stat.activationCount == 1 ? null : "s"}</small></span></td>
        </tr>
    },
    render:function(){
        var grainTypes = {};
        if (!this.props.data) return null;

        this.props.data.forEach(function(stat){
            grainTypes[stat.grainType] = (grainTypes[stat.grainType] || 0) + stat.activationCount;
        });

        var values = Object.keys(grainTypes).map(function(key){
            return {
                grainType:key,
                activationCount:grainTypes[key]
            }
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
