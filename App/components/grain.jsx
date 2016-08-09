var React = require('react');
var Chart = require('./multi-series-chart-widget.jsx');

module.exports = React.createClass({
    render:function(){
        return <div>
            <a href="#">&larr; Back to Dashboard</a>
            <h2>Grain {this.props.grainType}</h2>
            <div className="well">
                <Chart series={[]}/>
            </div>
        </div>

    }
});
