var React = require('react');
var Panel = require('../components/panel.jsx');

module.exports = React.createClass({
    renderItem : function(item){
      return <tr key={item.name}>
          <td style={{textOverflow: "ellipsis"}}>{item.name}</td>
          <td><strong>{item.value}</strong></td>
      </tr>
    },
    render:function(){
        return <div>
                <Panel title="Silo Counters">
                    <div>
                        <table className="table">
                            <tbody>
                                {this.props.counters.map(this.renderItem)}
                            </tbody>
                        </table>
                        {this.props.counters.length === 0 ? <span><p className="lead">No counters available.</p> It may take a few minutes for data to be published.</span> : null}
                    </div>
                </Panel>
            </div>
    }
});
