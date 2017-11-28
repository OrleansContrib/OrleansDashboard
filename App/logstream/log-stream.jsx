var React = require('react');
var Panel = require('../components/panel.jsx');
var Page = require('../components/page.jsx');

module.exports = React.createClass({
  getInitialState:function(){
    return {
      log:"Connecting...",
      filter: "",      
      scrollEnabled:true
    }
  },

  scroll:function(){
    this.refs.log.scrollTop = this.refs.log.scrollHeight;
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

  filterChanged:function(event){
    this.setState({
      filter: event.target.value,
      filterRegex: new RegExp(`[^\s-]* (Trace|Debug|Information|Warning|Error):.*${event.target.value}.*`, 'gmi')      
    })
  },

  getFilteredLog: function(){
    if (!this.state.filter) return this.state.log;
    
    const matches = this.state.log.match(this.state.filterRegex);
    return matches ? matches.join('\r\n') : "";
  },

  render:function(){
    return <div style={{display: 'flex', flexDirection: 'column', overflow: 'hidden', maxHeight: '100%'}}>
      <a href="javascript:void"  onClick={this.toggle} className="btn btn-default" style={{marginLeft: "-100px", position: "fixed", top: "115px", left: "100%"}}>
        {this.state.scrollEnabled ? "Pause" : "Resume"}
      </a>
      <input type="search" name="filter" className="text log-filter" style={{width: '100%', height: '40px'}} value={this.state.filter} 
        onChange={this.filterChanged} placeholder={'Regex Filter'} />
      <pre ref="log" className="log" style={{overflowY: 'auto', width: '100%', height: 'calc(100vh - 100px)', whiteSpace: 'pre-wrap'}}>
        {this.getFilteredLog()}
      </pre>
    </div>
  }
});
