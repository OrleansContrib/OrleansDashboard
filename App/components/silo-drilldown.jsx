var React = require('react');
var Gauge = require('./gauge-widget.jsx');
var PropertiesWidget = require('./properties-widget.jsx');
var GrainBreakdown = require('./grain-breakdown.jsx');
var SiloState = require('./silo-state.jsx');

module.exports = React.createClass({

    renderOverloaded:function(){
        if (!this.props.data.isOverloaded) return null;
        return <small><span className="label label-danger">OVERLOADED</span> <SiloState status={status}/></small>
    },

    render:function(){
        var properties = {
            "Clients" : this.props.data.clientCount || '0',
            "Messages recieved" : this.props.data.receivedMessages || '0',
            "Messages sent" : this.props.data.sentMessages || '0',
            "Receive queue" : this.props.data.receiveQueueLength || '0',
            "Request queue" : this.props.data.requestQueueLength || '0',
            "Send queue" : this.props.data.sendQueueLength || '0'
        };

        var grainStats = (this.props.dashboardCounters.simpleGrainStats || []).filter(function(x){
            return x.siloAddress === this.props.silo;
        }, this);

        var status = (this.props.dashboardCounters.hosts || {})[this.props.silo];

        return <div>
            <a href="#">&larr;Back to Dashboard</a>
            <h2>Silo {this.props.silo} <small><SiloState status={status}/></small> {this.renderOverloaded()}</h2>
            <div className="well">
                <div className="row">
                    <div className="col-md-4">
                        <Gauge value={this.props.data.cpuUsage} max={100} title="CPU Usage" description={Math.floor(this.props.data.cpuUsage) + "%"}/>
                    </div>
                    <div className="col-md-4">
                        <Gauge value={this.props.data.totalPhysicalMemory - this.props.data.availableMemory} max={this.props.data.totalPhysicalMemory} title="Memory Usage"  description={Math.floor(this.props.data.availableMemory / (1024 * 1024)) + " MB Free"}/>
                    </div>
                    <div className="col-md-4">
                        <Gauge value={this.props.data.recentlyUsedActivationCount} max={this.props.data.activationCount} title="Grain Usage"  description={Math.floor(this.props.data.recentlyUsedActivationCount * 100 / this.props.data.activationCount) + "% active grains"}/>
                    </div>
                </div>
                <div className="row" style={{marginTop: "25px"}}>
                    <div className="col-md-6">
                        <h4>Silo counters</h4>
                        <PropertiesWidget data={properties}/>
                    </div>
                    <div className="col-md-6">
                        <h4>Grains</h4>
                        <GrainBreakdown data={grainStats}/>
                    </div>
                </div>
            </div>
        </div>
    }
});
/*

dateTime: "2015-12-30T17:02:32.6695724Z"

cpuUsage: 11.8330326
activationCount: 4
availableMemory: 4301320000
totalPhysicalMemory: 8589934592
memoryUsage: 8618116
recentlyUsedActivationCount: 2


clientCount: 0
isOverloaded: false

receiveQueueLength: 0
requestQueueLength: 0
sendQueueLength: 0

receivedMessages: 0
sentMessages: 0

*/