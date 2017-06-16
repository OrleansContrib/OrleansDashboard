var React = require('react');


module.exports = React.createClass({

    renderReminder:function(reminderData){
        return <tr>
            <td>{reminderData.grainReference}</td>
            <td><span className="pull-right">{reminderData.activationCount}</span></td>
            <td><span className="pull-right">{reminderData.name}</span></td>
            <td><span className="pull-right">{reminderData.startAt}</span></td>
            <td><span className="pull-right">{reminderData.period}</span></td>
        </tr>
    },
    render:function(){        
        if (!this.props.data) return null;        

        return <table className="table">
            <tbody>
                <tr>
                    <th style={{textAlign:"left"}}>Grain Reference</th>
                    <th></th>
                    <th style={{textAlign:"right"}}>Name</th>
                    <th style={{textAlign:"right"}}>StartAt</th>
                    <th style={{textAlign:"right"}}>Period</th>
                </tr>
                {this.props.data.map(this.renderReminder)}
            </tbody>
        </table>
    }
});
