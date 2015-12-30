var React = require('react');
var Gauge = require('./gauge-widget.jsx');

module.exports = React.createClass({
    render:function(){
        console.log(this.props);
        return <div>
            <h2>Silo {this.props.silo}</h2>
            <a href="#">Back to Dashboard</a>
            <div className="row">
                <div className="col-md-3"><Gauge value={this.props.data.cpuUsage} max={100} title="CPU Usage" description={Math.floor(this.props.data.cpuUsage) + " %"}/></div>
                <div className="col-md-3"><Gauge value={this.props.data.totalPhysicalMemory - this.props.data.availableMemory} max={this.props.data.totalPhysicalMemory} title="Memory Usage"  description={Math.floor(this.props.data.availableMemory / (1024 * 1024)) + " MB Free"}/></div>
            </div>
        </div>
    }
});
/*

activationCount: 4
availableMemory: 4301320000
clientCount: 0
cpuUsage: 11.8330326
dateTime: "2015-12-30T17:02:32.6695724Z"
isOverloaded: false
memoryUsage: 8618116
receiveQueueLength: 0
receivedMessages: 0
recentlyUsedActivationCount: 2
requestQueueLength: 0
sendQueueLength: 0
sentMessages: 0
totalPhysicalMemory: 8589934592

*/