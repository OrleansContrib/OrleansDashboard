var React = require('react');

module.exports = React.createClass({
    getInitialState:function(){
        var style = document.getElementById("dark-theme-style");
        var theme = "light"
        if (localStorage){
            theme = localStorage.getItem("theme") || "light";
        }
        if (theme === "light"){
            style.setAttribute("media", "none");
        } else {
            style.setAttribute("media", "");
        }

        return {light:(theme === "light")};
    },
    saveTheme:function(){
        if (!localStorage) return;
        localStorage.setItem("theme", this.state.light ? "light" : "dark");
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
        return <div className="btn-group btn-group-sm" role="group" style={{opacity:0.5, marginTop:"27px"}}>
            <a href="javascript:void(0);" className={this.state.light ? "btn btn-primary" : "btn btn-default"} onClick={this.pickLight}>Light</a>
            <a href="javascript:void(0);" className={this.state.light ? "btn btn-default" : "btn btn-primary"} onClick={this.pickDark}>Dark</a>
        </div>
    }
});


