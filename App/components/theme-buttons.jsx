var React = require('react');
var storage = require('../lib/storage');

module.exports = React.createClass({
    getInitialState:function(){

        var style = document.getElementById("dark-theme-style");
        let theme = storage.get("theme") || "light"
        
        if (theme === "light"){
            style.setAttribute("media", "none");
        } else {
            style.setAttribute("media", "");
        }

        return {light:(theme === "light")};
    },
    saveTheme:function(){
        storage.put("theme", this.state.light ? "light" : "dark")
    },
    pickLight:function(){
        var style = document.getElementById("dark-theme-style");
        style.setAttribute("media", "none");
        this.setState({light:true}, this.saveTheme);
    },
    pickDark:function(){
        var style = document.getElementById("dark-theme-style");
        style.setAttribute("media", "");
        this.setState({light:false}, this.saveTheme);
    },
    render:function(){
        return <div className="btn-group btn-group-sm" role="group" style={{marginTop:"10px",marginRight:"10px"}}>
            <a href="javascript:void(0);" className={this.state.light ? "btn btn-primary" : "btn btn-default"} onClick={this.pickLight}>Light</a>
            <a href="javascript:void(0);" className={this.state.light ? "btn btn-default" : "btn btn-primary"} onClick={this.pickDark}>Dark</a>
        </div>
    }
});
