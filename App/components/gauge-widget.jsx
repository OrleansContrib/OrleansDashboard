var React = require('react');
var Chart = require("react-chartjs").Doughnut;

module.exports = React.createClass({
	getInitialState:function(){
		return {width:0};
	},

    getWidth:function(){
		this.setState({width: this.refs.container.offsetWidth});
	},

    renderChart: function() {
        if (this.state.width === 0) return setTimeout(this.getWidth,0)

        var data = [
            {
                value: this.props.value,
                color:"#F7464A",
                highlight: "#FF5A5E",
                label: "Utilization"
            },
            {
                value: this.props.max - this.props.value,
                color: "#EEE",
                highlight: "#EEE",
                label: ""
            }
        ]
        console.log(data);
		return <Chart data={data}  width={this.state.width} height={200} />
	},

    render:function(){
        return <div ref="container" style={{textAlign:"center"}}>
            <h4>{this.props.title}</h4>
            {this.renderChart()}
            <span>{this.props.description}</span>
        </div>
    }


});