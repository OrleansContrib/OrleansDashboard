var React = require('react');

module.exports = React.createClass({
    render:function(){
        return <div>
            <section className="content-header">
                <h1>{this.props.title} <small> {this.props.subTitle}</small></h1>
            </section>
            <section className="content" style={{position:'relative'}}>
                {this.props.children}
            </section>
        </div>
    }
});
