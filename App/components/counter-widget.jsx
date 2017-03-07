var React = require('react');

module.exports = React.createClass({
    renderMore:function(){
        if (!this.props.link) return null;
        return <a href={this.props.link} className="small-box-footer">More info <i className="fa fa-arrow-circle-right"></i></a>
    },

    render:function(){

        return <div className="info-box">
            <span className="info-box-icon bg-purple"><i className={`fa fa-${this.props.icon}`}></i></span>
            <div className="info-box-content">
                <span className="info-box-text">{this.props.title}</span>
                <span className="info-box-number">{this.props.counter}</span>
                {this.renderMore()}
            </div>
        </div>

    }
});
