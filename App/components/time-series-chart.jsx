var React = require('react');
var Chart = require("react-chartjs").Line;

var colours = [
    [120, 57, 136],
    [236, 151, 31],
    [236, 31, 31]
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
            datasets : [
                {
                    label: "y2",
                    backgroundColor: `rgba(236,151,31,0)`,
                    borderColor: `rgba(236,151,31,0.8)`,
                    data: this.props.series[2],
                    pointRadius:0,
                    yAxisID:"y2"
                },
                {
                label: "y1",
                backgroundColor: `rgba(246,31,31,0.8)`,
				borderColor: `rgba(246,31,31,0)`,
				data: this.props.series[0],
                pointRadius:0,
                yAxisID:"y1"
                },
                {
                    label: "y1",
                    backgroundColor: `rgba(120,57,136,0.8)`,
                    borderColor: `rgba(120,57,136,0)`,
                    data: this.props.series[1],
                    pointRadius:0,
                    yAxisID:"y1"
                }

            ]
    	};

        var options = {
            legend:{display:false},
            maintainAspectRatio:false,
            animation:false,
            showTooltips:false,
            responsive: true,
            hoverMode: 'label',
            stacked: false,
            scales:
            {
                xAxes: [
                    {
                        display:true,
                        gridLines: {
                            offsetGridLines: false,
                            drawOnChartArea: false
                        }
                    }
                ],
                yAxes:[
                    {
                        type: "linear",
                        display: true,
                        position: "left",
                        id: "y1",
                        gridLines: { drawOnChartArea: false},
                        ticks: { beginAtZero:true }
                    },
                    {
                        type: "linear",
                        display: true,
                        position: "right",
                        id: "y2",
                        gridLines: { drawOnChartArea: false},
                        ticks: { beginAtZero:true }
                    }
                ]
            }
        }

        return <Chart data={data} options={options} width={this.state.width} height={180} />
	},

    render:function(){
        return <div ref="container">{this.renderChart()}</div>
    }
});
