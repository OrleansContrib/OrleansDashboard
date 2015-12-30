var React = require('react');

module.exports = React.createClass({
    render:function(){
        return <div style={{textAlign:"center"}}>
            <h1>{this.props.counter}</h1>
            <h4>{this.props.title}</h4>
        </div>

    }
});