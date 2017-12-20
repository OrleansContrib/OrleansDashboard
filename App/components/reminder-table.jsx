var React = require('react');


module.exports = React.createClass({
    getInitialState: function() {
        return {grain_reference: null, primary_key: null, name: null, startAt: null, period: null};
    },
    handleChange: function(e) {
        this.setState({
          [e.target.name]: e.target.value
        })
    },
    renderReminder:function(reminderData){
        return <tr>
            <td>{reminderData.grainReference}</td>
            <td>{reminderData.primaryKey}</td>
            <td><span className="pull-right">{reminderData.activationCount}</span></td>
            <td><span className="pull-right">{reminderData.name}</span></td>
            <td><span className="pull-right">{new Date(reminderData.startAt).toLocaleString()}</span></td>
            <td><span className="pull-right">{reminderData.period}</span></td>
        </tr>
    },
    filterData:function(data){
        return data.filter(x => this.state['grain_reference'] ? x.grainReference.indexOf(this.state['grain_reference']) > -1 : x)
        .filter(x => this.state['primary_key'] ? x.primaryKey.indexOf(this.state['primary_key']) > -1 : x)
        .filter(x => this.state['name'] ? x.name.indexOf(this.state['name']) > -1 : x)
        .filter(x => this.state['startAt'] ? x.startAt.indexOf(this.state['startAt']) > -1 : x)
        .filter(x => this.state['period'] ? x.period.indexOf(this.state['period']) > -1 : x);
    },
    render:function(){
        if (!this.props.data) return null;
        var filteredData = this.filterData(this.props.data);
        return <table className="table">
            <tbody>
                <tr>
                    <th style={{textAlign:"left"}}>Grain Reference</th>
                    <th>Primary Key</th>
                    <th></th>
                    <th style={{textAlign:"left"}}>Name</th>
                    <th style={{textAlign:"left"}}>StartAt</th>
                    <th style={{textAlign:"right"}}>Period</th>
                </tr>
                <tr>
                    <th style={{textAlign:"left"}}><input onChange={this.handleChange} value={this.state['grain_reference']} type="text" name="grain_reference"/></th>
                    <th style={{textAlign:"left"}}><input onChange={this.handleChange} value={this.state['primary_key']} type="text" name="primary_key"/></th>
                    <th></th>
                    <th style={{textAlign:"left"}}><input onChange={this.handleChange} value={this.state['name']} type="text" name="name"/></th>
                    <th style={{textAlign:"left"}}><input onChange={this.handleChange} value={this.state['startAt']} type="text" name="startAt"/></th>
                    <th style={{textAlign:"right"}}><input onChange={this.handleChange} value={this.state['period']} type="text" name="period"/></th>
                </tr>
                {filteredData.map(this.renderReminder)}
            </tbody>
        </table>
    }
});
