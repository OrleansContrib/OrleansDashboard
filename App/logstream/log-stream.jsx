var React = require('react');
var Panel = require('../components/panel.jsx');
var Page = require('../components/page.jsx');

module.exports = React.createClass({
  getInitialState:function(){
    return {
      log:"Connecting..."
    }
  },

  scroll:function(){
    body.scrollIntoView(false);
    //body.scrollTop = body.scrollHeight;
  },

  onProgress : function(){
    this.setState({
      log:this.props.xhr.responseText
    }, this.scroll);
  },

  componentWillMount:function(){
    this.props.xhr.onprogress = this.onProgress;
  },

  componentWillUnmount:function(){
    this.props.xhr.abort();
  },

  render:function(){
    return <pre ref="log" className="log">{this.state.log}</pre>
  }
});
