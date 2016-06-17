var React = require('react');
var Chart = require("react-chartjs").Line;

var colours = [
    [120, 57, 136],
    [236, 151, 31]
];
// this control is a bit of a temporary hack, until I have a multi-series chart widget
module.exports = React.createClass({
	getInitialState:function(){
		return {width:0};
	},

    getWidth:function(){
        if (!this.refs.container) return;
		this.setState({width: this.refs.container.offsetWidth});
	},

    renderChart: function() {
        if (this.state.width === 0) return setTimeout(this.getWidth,0)

		var data = {
			labels: this.props.series[0].map(function(x){ return "" }),
            datasets : this.props.series.map((data, index) => {
                var colourString = colours[index % colours.length].join();
                return {
                    label: "",
					fillColor: `rgba(${colourString},0.1)`,
					strokeColor: `rgba(${colourString},1)`,
					highlightFill: `rgba(${colourString},0.1)`,
					highlightStroke: `rgba(${colourString},1)`,
					data: data
                };
            })
		};

		return <Chart data={data} options={{pointDot :false, showTooltips:false}} width={this.state.width} height={120} />
	},

    render:function(){
        return <div ref="container">{this.renderChart()}</div>
    }
});
