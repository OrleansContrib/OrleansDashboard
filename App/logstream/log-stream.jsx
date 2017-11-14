var React = require('react');
var Panel = require('../components/panel.jsx');
var Page = require('../components/page.jsx');

module.exports = React.createClass({
  getInitialState:function(){
    return {
      log:"Connecting...",
      scrollEnabled:true
    }
  },

  scroll:function(){
    this.refs.log.scrollIntoView(false);
    //body.scrollIntoView(false);
    //body.scrollTop = body.scrollHeight;
  },

  onProgress : function(){
    if (!this.state.scrollEnabled) return;
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

  toggle:function(){
    this.setState({
      scrollEnabled:!this.state.scrollEnabled
    });
  },

  render:function(){
    return <div>
      <pre ref="log" className="log">{this.state.log}</pre>
      <a href="javascript:void"  onClick={this.toggle} className="btn btn-default" style={{marginLeft: "-100px",position: "fixed",top: "75px",left: "100%"}}>
        {this.state.scrollEnabled ? "Pause" : "Resume"}
      </a>
    </div>
  }
});
