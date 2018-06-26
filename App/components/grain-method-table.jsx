var React = require('react');

module.exports = React.createClass({
    renderRow:function(value){
        return <tr key={`${value.grain}.${value.method}`}>
            <td style={{wordWrap:"break-word"}}>
                <span className="pull-right"><strong>{this.props.valueFormatter(value)}</strong></span>
                {value.method}
                <br/><small><a href={`#/grain/${value.grain}`}>{value.grain}</a></small>
            </td>
            
        </tr>

    },
    render:function(){
        return <table className="table" style={{tableLayout:"fixed", width:"100%"}}>
            <tbody>
                {(this.props.values || []).map(this.renderRow)}
                {(this.props.values. length ? null : <tr><td><i>No data</i></td></tr>)}
            </tbody>
        </table>
    }
});
