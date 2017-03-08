var React = require('react');

module.exports = React.createClass({
    handleClick:function(){
        if (this.props.onClose){
            this.props.onClose();
        }
    },
    render:function(){
        return <div className="alert alert-danger alert-dismissible">
            <button type="button" className="close" onClick={this.handleClick} ariaHidden="true">×</button>
            <h4><i className="icon fa fa-ban"></i> {this.props.title || "Error"}</h4>
            {this.props.children}.
        </div>
    }
});
