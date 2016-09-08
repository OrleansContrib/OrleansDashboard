var React = require('react');
var Chart = require('./multi-series-chart-widget.jsx');
var ChartWidget = require('./multi-series-chart-widget.jsx');

var GrainGraph = React.createClass({
    render:function(){
        var values = Object.keys(this.props.stats).map(key => this.props.stats[key]);

        if (!values.length) return null;

        while (values.length < 25){
            values.unshift({count:0, elapsedTime :0})
        }

        return <div>
            <h2>{values[0].method}</h2>
            <ChartWidget series={[values.map(x => x.count), values.map(x => x.count === 0 ? 0 : x.elapsedTime / x.count)]} />
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
            <h2>Grain {this.props.grainType}</h2>
            <div className="well">
                {Object.keys(this.props.grainStats).map(key => <GrainGraph stats={this.props.grainStats[key]} />)}
            </div>
        </div>

    }
});
