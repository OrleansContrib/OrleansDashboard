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

        var data = [
            {
                value: this.props.value,
                color: this.getColour(1),
                highlight: this.getColour(1),
                label: "Utilization"
            },
            {
                value: this.props.max - this.props.value,
                color: this.getColour(0.2),
                highlight: this.getColour(0.2),
                label: ""
            }
        ]
		return <Chart data={data} options={{showTooltips:false, segmentStrokeColor : "rgba(0,0,0,0)", segmentStrokeWidth:0, percentageInnerCutout:92}} width={this.state.width} height={200} />
	},

    render:function(){
		var percent = Math.floor(100 * this.props.value / this.props.max);
        return <div ref="container" style={{textAlign:"center"}}>
            <h4>{this.props.title}</h4>
			<div style={{position:"absolute", textAlign:"center", fontSize:"60px", fontWeight:"100", width:"90%", paddingTop:"60px"}}>{percent}%</div>
            {this.renderChart()}
            <span>{this.props.description}</span>
        </div>
    }


});
