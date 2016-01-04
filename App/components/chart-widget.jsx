var React = require('react');
var Chart = require("react-chartjs").Line;

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
			labels: this.props.series.map(function(x){ return "" }),
			datasets: [
				{
					label: "",
					fillColor: "rgba(151,187,205,0.5)",
					strokeColor: "rgba(151,187,205,0.8)",
					highlightFill: "rgba(151,187,205,0.75)",
					highlightStroke: "rgba(151,187,205,1)",
					data: this.props.series
				}
			]
		};

		return <Chart data={data} options={{pointDot :false, showTooltips:false}} width={this.state.width} height={120} />
	},

    render:function(){
        return <div ref="container">{this.renderChart()}</div>
    }


});