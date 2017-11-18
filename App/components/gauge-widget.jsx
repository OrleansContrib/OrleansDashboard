var React = require('react');
var Chart = require("react-chartjs").Doughnut;

module.exports = React.createClass({
	getInitialState:function(){
		return {width:0};
	},

    getWidth:function(){
		this.setState({width: this.refs.container.offsetWidth});
	},

    getColour:function(alpha){
		return `rgba(120, 57, 136, ${alpha})`;
		/*
        var percent = 100 * this.props.value / this.props.max;
        if (percent > 90) return 'rgba(201,48,44,' + alpha.toString() + ')';
        if (percent > 66) return 'rgba(236,151,31,' + alpha.toString() + ')';
        return 'rgba(51,122,183,' + alpha.toString() + ')';
		*/
    },

    renderChart: function() {
        if (this.state.width === 0) return setTimeout(this.getWidth,0)

        var data = {
			labels:["",""],
			datasets:[{
				data:[this.props.value,this.props.max - this.props.value],
                backgroundColor: [this.getColour(1),this.getColour(0.2)],
				hoverBackgroundColor: [this.getColour(1),this.getColour(0.2)],
				borderWidth:[0,0],
				hoverBorderWidth:[0,0]

            }]
        };

		var options = {
			legend:{display:false},
			animation:false,
			showTooltips:false,
			cutoutPercentage:92
		};


		return <Chart data={data} options={options} width={this.state.width} height={200} />
	},

    render:function(){
		var percent = Math.floor(100 * this.props.value / this.props.max);
        return <div ref="container" style={{textAlign:"center", position:"relative", minHeight:"100px"}}>
            <h4>{this.props.title}</h4>
			<div style={{position:"absolute", textAlign:"center", fontSize:"60px", fontWeight:"100", left:"0",right:"0", top:"50%", marginTop:"-45px"}}>{percent}%</div>
			<div style={{position:"absolute", textAlign:"center", fontSize:"60px", fontWeight:"100", width:"100%", }}></div>
            	{this.renderChart()}
            <span style={{lineHeight:"40px"}}>{this.props.description}</span>
        </div>
    }


});
