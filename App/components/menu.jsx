var React = require('react');

var MenuSection = React.createClass({
    render:function(){
        var key = 0;
        return <li className={(this.props.active ? "active" : "") + " treeview"}>
          <a href={this.props.path}>{this.props.name}</a>
        </li>
    }
});

module.exports = React.createClass({
    render:function(){
        return <ul className="sidebar-menu">
            {this.props.menu.map(x => <MenuSection active={x.active} icon={x.icon} name={x.name} path={x.path} />)}
        </ul>
    }
});
