var React = require('react');
var Chart = require("react-chartjs").Doughnut;

module.exports = React.createClass({
	getInitialState:function(){
		return {width:0};
	},

    getWidth:function(){
		this.setState({width: this.refs.container.offsetWidth});
	},

    getColour:function(){
        var percent = 100 * this.props.value / this.props.max;
        if (percent > 90) return '#c9302c';
        if (percent > 66) return '#ec971f';

        return '#337ab7';
    },

    renderChart: function() {
        if (this.state.width === 0) return setTimeout(this.getWidth,0)

        var data = [
            {
                value: this.props.value,
                color: this.getColour(),
                highlight: this.getColour(),
                label: "Utilization"
            },
            {
                value: this.props.max - this.props.value,
                color: "rgba(0,0,0,0.1)",
                highlight: "rgba(0,0,0,0.1)",
                label: ""
            }
        ]
		return <Chart data={data} options={{showTooltips:false, segmentStrokeColor : "rgba(0,0,0,0.2)", segmentStrokeWidth:1}} width={this.state.width} height={200} />
	},

    render:function(){
        return <div ref="container" style={{textAlign:"center"}}>
            <h4>{this.props.title}</h4>
            {this.renderChart()}
            <span>{this.props.description}</span>
        </div>
    }


});