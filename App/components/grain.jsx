var React = require('react');
var Chart = require('./time-series-chart.jsx');

var GrainGraph = React.createClass({
    render:function(){
        var values = Object.keys(this.props.stats).map(key => this.props.stats[key]);

        if (!values.length) return null;


        while (values.length < 25){
            values.unshift({count:0, elapsedTime :0, period:0})
        }

        return <div>
            <h2>{this.props.grainMethod}</h2>
            <Chart series={[values.map(z => z.count), values.map(z => z.count === 0 ? 0 : z.elapsedTime / z.count)]} />
        </div>
    }
});

// add multiple axis to the chart
// https://jsfiddle.net/devonuto/pa7k6xn9/

module.exports = React.createClass({
    render:function(){
        console.log(this.props);

        return <div>
            <a href="#">&larr; Back to Dashboard</a>
            <h2>{getName(this.props.grainType)} <small>{this.props.grainType}</small></h2>
            <div className="well">
                {(Object.keys(this.props.grainStats).length === 0) ? <span>No messages recorded</span> : null}
                {Object.keys(this.props.grainStats).map(key => <GrainGraph stats={this.props.grainStats[key]} grainMethod={getName(key)} />)}
            </div>
        </div>

    }
});

function getName(value){
    var parts = value.split('.');
    return parts[parts.length - 1];
}
