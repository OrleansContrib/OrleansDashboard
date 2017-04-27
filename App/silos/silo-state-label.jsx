var React = require('react');

module.exports = React.createClass({
    render:function(){

        var labelClassMapper = {
            Created : 'info',
            Joining : 'info',
            Active : 'success',
            ShuttingDown : 'warning',
            Stopping : 'warning',
            Dead : 'danger'
        }
        return <span className={"label label-" + labelClassMapper[this.props.status]}>{this.props.status || "unknown"}</span>
    }

});