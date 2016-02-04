var React = require('react');

module.exports = React.createClass({
    renderRow:function(key){
        return <tr key={key}>
            <td style={{textOverflow: "ellipsis"}}>{key}</td>
            <td style={{"textAlign":"right"}}>{this.props.data[key]}</td>
        </tr>

    },
    render:function(){
        return <table className="table">
            <tbody>
                {Object.keys(this.props.data).map(this.renderRow)}
            </tbody>
        </table>
    }
});