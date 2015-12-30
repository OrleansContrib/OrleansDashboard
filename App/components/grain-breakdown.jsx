var React = require('react');

module.exports = React.createClass({
    renderStat:function(stat){
        return <tr key={stat.grainType}>
            <td>{stat.grainType}</td>
            <td><span className="pull-right"><strong>{stat.activationCount}</strong> <small>activations</small></span></td>
        </tr>
    },
    render:function(){
        var grainTypes = {};
        this.props.dashboardCounters.simpleGrainStats.forEach(function(stat){
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
