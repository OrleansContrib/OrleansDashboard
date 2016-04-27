var React = require('react');
var Gauge = require('./gauge-widget.jsx');
var PropertiesWidget = require('./properties-widget.jsx');
var GrainBreakdown = require('./grain-breakdown.jsx');
var SiloState = require('./silo-state.jsx');
var ChartWidget = require('./multi-series-chart-widget.jsx');



module.exports = React.createClass({
    hasData:function(value){
        for (var i = 0; i < value.length; i++){
            if (value[i] !== null) return true;
        }
        return false;
    },
    renderOverloaded:function(){
        if (!this.props.data[this.props.data.length-1].isOverloaded) return null;
        return <small><span className="label label-danger">OVERLOADED</span> <SiloState status={status}/></small>
    },

    querySeries:function(lambda){
        return this.props.data.map(function(x){
            if (!x) return 0;
            return lambda(x);
        });
    },

    render:function(){
        if (!this.hasData(this.props.data)){
            return <div>
                <a href="#">&larr; Back to Dashboard</a>
                <h2>Silo {this.props.silo}</h2>
                <p className="lead">No data available for this silo</p>
            </div>
        }

        var last = this.props.data[this.props.data.length-1];
        var properties = {
            "Clients" : last.clientCount || '0',
            "Messages recieved" : last.receivedMessages || '0',
            "Messages sent" : last.sentMessages || '0',
            "Receive queue" : last.receiveQueueLength || '0',
            "Request queue" : last.requestQueueLength || '0',
            "Send queue" : last.sendQueueLength || '0'
        };

        var grainStats = (this.props.dashboardCounters.simpleGrainStats || []).filter(function(x){
            return x.siloAddress === this.props.silo;
        }, this);

        var status = (this.props.dashboardCounters.hosts || {})[this.props.silo];

        return <div>
            <a href="#">&larr; Back to Dashboard</a>
            <h2>Silo {this.props.silo} <small><SiloState status={status}/></small> {this.renderOverloaded()}</h2>
            <div className="well">
                <div className="row">
                    <div className="col-md-4">
                        <Gauge value={last.cpuUsage} max={100} title="CPU Usage" description={Math.floor(last.cpuUsage) + "% utilisation"}/>
                        <ChartWidget series={[this.querySeries(function (x){ return x.cpuUsage })]} />
                    </div>
                    <div className="col-md-4">
                        <Gauge value={last.totalPhysicalMemory - last.availableMemory} max={last.totalPhysicalMemory} title="Memory Usage"  description={Math.floor(last.availableMemory / (1024 * 1024)) + " MB free"}/>
                        <ChartWidget series={[this.querySeries(function(x){ return (x.totalPhysicalMemory - x.availableMemory) / (1024 * 1024)})]} />
                    </div>
                    <div className="col-md-4">
                        <Gauge value={last.recentlyUsedActivationCount} max={last.activationCount} title="Grain Usage"  description={last.activationCount + " activations, " + Math.floor(last.recentlyUsedActivationCount * 100 / last.activationCount) + "% recently used"}/>
                        <ChartWidget series={[this.querySeries(function(x){ return x.activationCount}), this.querySeries(function(x){ return x.recentlyUsedActivationCount})]} />
                    </div>
                </div>
                <div className="row" style={{marginTop: "25px"}}>
                    <div className="col-md-6">
                        <h4>Silo Counters</h4>
                        <PropertiesWidget data={properties}/>
                    </div>
                    <div className="col-md-6">
                        <h4>Activations by Type</h4>
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
