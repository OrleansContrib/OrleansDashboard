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
		this.setState({width: this.refs.container.offsetWidth - 20});
	},

    renderChart: function() {
        if (this.state.width === 0) return setTimeout(this.getWidth,0)

		var data = {
			labels: this.props.series[0].map(function(x){ return "" }),
            datasets : this.props.series.map((data, index) => {
                var colourString = colours[index % colours.length].join();
                return {
                    label: "",

					backgroundColor: `rgba(${colourString},0.1)`,
					borderColor: `rgba(${colourString},1)`,
					data: data,
                    pointRadius:0
                };
            })
		};

		return <Chart data={data} options={{animation:false,legend:{display:false},maintainAspectRatio:false,responsive: true,showTooltips:false,scales:{yAxes:[{ticks:{beginAtZero:true}}]}}} width={this.state.width} height={80} />
	},

    render:function(){
        return <div ref="container">{this.renderChart()}</div>
    }
});
