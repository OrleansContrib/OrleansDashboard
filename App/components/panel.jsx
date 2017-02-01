var React = require('react');

module.exports = React.createClass({
    render:function(){
        if (this.props.children.length){
            var body = this.props.children[0];
            var footer = <div className="box-footer clearfix">{this.props.children[1]}</div>
        } else {
            var body = this.props.children;
            var footer = null;
        }

        return <div className="box">
            <div className="box-header with-border">
              <h3 className="box-title">{this.props.title}<small style={{marginLeft:"10px"}}>{this.props.subTitle}</small></h3>
            </div>
            <div className="box-body">
                {body}
            </div>
            {footer}
          </div>
    }

});
