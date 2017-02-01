var React = require('react');

module.exports = React.createClass({
    render:function(){

        return <div className="info-box">
            <span className="info-box-icon bg-purple"><i className={`fa fa-${this.props.icon}`}></i></span>
            <div className="info-box-content">
                <span className="info-box-text">{this.props.title}</span>
                <span className="info-box-number">{this.props.counter}</span>
            </div>
        </div>

    }
});
