var React = require('react');
var Gauge = require('../components/gauge-widget.jsx');
var PropertiesWidget = require('../components/properties-widget.jsx');
var GrainBreakdown = require('../components/grain-table.jsx');
var ChartWidget = require('../components/multi-series-chart-widget.jsx');
var SiloState = require('./silo-state-label.jsx');
var Panel = require('../components/panel.jsx');
var Chart = require('../components/time-series-chart.jsx');

var SiloGraph = React.createClass({
    render:function(){
        var values = Object.keys(this.props.stats).map(key => this.props.stats[key]);

        if (!values.length) return null;

        while (values.length < 100){
            values.unshift({count:0, elapsedTime :0, period:0, exceptionCount:0})
        }

        return <div>
            <Chart series={[values.map(z => z.exceptionCount), values.map(z => z.count), values.map(z => z.count === 0 ? 0 : z.elapsedTime / z.count)]} />
        </div>
    }
});

module.exports = React.createClass({
    hasData:function(value){
        for (var i = 0; i < value.length; i++){
            if (value[i] !== null) return true;
        }
        return false;
    },


    querySeries:function(lambda){
        return this.props.data.map(function(x){
            if (!x) return 0;
            return lambda(x);
        });
    },

    render:function(){
        if (!this.hasData(this.props.data)){
            return <Panel title="Error">
                <div>
                    <p className="lead">No data available for this silo</p>
                    <p><a href="#/silos">Show all silos</a></p>
                </div>
            </Panel>
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
        var silo = this.props.dashboardCounters.hosts.filter(x => x.siloAddress === this.props.silo)[0] || {};

        var configuration = {
            "Host name" : silo.hostName,
            "Role name" : silo.roleName,
            "Silo name" : silo.siloName,
            "Proxy port" : silo.proxyPort,
            "Update zone" : silo.updateZone,
            "Fault zone" : silo.faultZone
        };

        if (this.props.siloProperties.orleansVersion){
            configuration["Orleans version"] = this.props.siloProperties.orleansVersion;
        }

        if (this.props.siloProperties.hostVersion){
            configuration["Host version"] = this.props.siloProperties.hostVersion;
        }

        return <div>
                <Panel title="Overview" >
                    <div className="row">
                        <div className="col-md-4">
                            <Gauge value={last.cpuUsage} max={100} title="CPU Usage" description={Math.floor(last.cpuUsage) + "% utilisation"}/>
                            <ChartWidget series={[this.querySeries(x => x.cpuUsage)]} />
                        </div>
                        <div className="col-md-4">
                            <Gauge value={(last.totalPhysicalMemory || 0) - (last.availableMemory || 0)} max={(last.totalPhysicalMemory || 1)} title="Memory Usage"  description={Math.floor((last.availableMemory || 0) / (1024 * 1024)) + " MB free"}/>
                            <ChartWidget series={[this.querySeries(x => (x.totalPhysicalMemory - x.availableMemory) / (1024 * 1024))]} />
                        </div>
                        <div className="col-md-4">
                            <Gauge value={last.recentlyUsedActivationCount} max={last.activationCount} title="Grain Usage"  description={last.activationCount + " activations, " + Math.floor(last.recentlyUsedActivationCount * 100 / last.activationCount) + "% recently used"}/>
                            <ChartWidget series={[this.querySeries(x => x.activationCount), this.querySeries(x => x.recentlyUsedActivationCount)]} />
                        </div>
                    </div>
                </Panel>

                <Panel title="Silo Profiling">
                  <div>
                    <span><strong style={{color:"#783988",fontSize:"25px"}}>/</strong> number of requests per second<br/><strong style={{color:"#EC1F1F",fontSize:"25px"}}>/</strong> failed requests</span>
                    <span className="pull-right"><strong style={{color:"#EC971F",fontSize:"25px"}}>/</strong> average latency in milliseconds</span>
                    <SiloGraph stats={this.props.siloStats} />
                  </div>
                </Panel>

                <div className="row">
                    <div className="col-md-6">
                        <Panel title="Silo Counters"><PropertiesWidget data={properties}/></Panel>
                    </div>
                    <div className="col-md-6">
                        <Panel title="Silo Properties"><PropertiesWidget data={configuration}/></Panel>
                    </div>
                </div>

                <Panel title="Activations by Type">
                    <GrainBreakdown data={grainStats} silo={this.props.silo}/>
                </Panel>
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
