var React = require('react');

module.exports = React.createClass({
    renderRow:function(value){
        return <tr key={`${value.grain}.${value.method}`}>
            <td style={{textOverflow: "ellipsis"}}>
                {value.method}
                <br/><small><a href={`#/grain/${value.grain}`}>{value.grain}</a></small>
            </td>
            <td style={{"textAlign":"right"}}><strong>{this.props.valueFormatter(value)}</strong></td>
        </tr>

    },
    render:function(){
        return <table className="table">
            <tbody>
                {(this.props.values || []).map(this.renderRow)}
                {(this.props.values. length ? null : <tr><td><i>No data</i></td></tr>)}
            </tbody>
        </table>
    }
});
