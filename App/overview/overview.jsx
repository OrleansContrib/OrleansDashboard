var React = require('react');
var CounterWidget = require('../components/counter-widget.jsx');
var Panel = require('../components/panel.jsx');
var Chart = require('../components/time-series-chart.jsx');

var ClusterGraph = React.createClass({
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

/*
var stats = {
    activationCount: 0,
    totalSeconds: 0,
    totalAwaitTime : 0,
    totalCalls : 0,
    totalExceptions : 0
};
this.props.dashboardCounters.simpleGrainStats.forEach(stat => {
    if (stat.grainType !== this.props.grainType) return;
    stats.activationCount += stat.activationCount;
    stats.totalSeconds += stat.totalSeconds;
    stats.totalAwaitTime += stat.totalAwaitTime;
    stats.totalCalls += stat.totalCalls;
    stats.totalExceptions += stat.totalExceptions;
});

return <Page title={getName(this.props.grainType)} subTitle={this.props.grainType}>
    <div>

        <div className="row">
            <div className="col-md-3">
                <CounterWidget icon="cubes" counter={stats.activationCount} title="Activations" />
            </div>
            <div className="col-md-3">
                <CounterWidget icon="bug" counter={(stats.totalCalls === 0) ? "0.00" : (100 * stats.totalExceptions / stats.totalCalls).toFixed(2) + "%"} title="Error Rate" />
            </div>
            <div className="col-md-3">
                <CounterWidget icon="tachometer" counter={(stats.totalCalls / stats.totalSeconds).toFixed(2)} title="Req/sec/silo" />
            </div>
            <div className="col-md-3">
                <CounterWidget icon="clock-o" counter={(stats.totalCalls === 0) ? "0" : (stats.totalAwaitTime / stats.totalCalls).toFixed(2) + "ms"} title="Average response time" />
            </div>
        </div>

*/
module.exports = React.createClass({
    render:function(){
        var stats = {
            activationCount: 0,
            totalSeconds: 0,
            totalAwaitTime : 0,
            totalCalls : 0,
            totalExceptions : 0
        };
        this.props.dashboardCounters.simpleGrainStats.forEach(stat => {
            stats.activationCount += stat.activationCount;
            stats.totalSeconds += stat.totalSeconds;
            stats.totalAwaitTime += stat.totalAwaitTime;
            stats.totalCalls += stat.totalCalls;
            stats.totalExceptions += stat.totalExceptions;
        });

        return <div>
            <div className="row">
                <div className="col-md-6">
                    <CounterWidget icon="cubes" counter={this.props.dashboardCounters.totalActivationCount} title="Total Activations" link="#/grains" />
                </div>
                <div className="col-md-6">
                    <CounterWidget icon="database" counter={this.props.dashboardCounters.totalActiveHostCount} title="Active Silos" link="#/silos" />
                </div>
            </div>
            <div className="row">
              <div className="col-md-4">
                  <CounterWidget icon="bug" counter={(stats.totalCalls === 0) ? "0.00" : (100 * stats.totalExceptions / stats.totalCalls).toFixed(2) + "%"} title="Error Rate" />
              </div>
              <div className="col-md-4">
                  <CounterWidget icon="tachometer" counter={(stats.totalCalls / 100).toFixed(2)} title="Req/sec" />
              </div>
              <div className="col-md-4">
                  <CounterWidget icon="clock-o" counter={(stats.totalCalls === 0) ? "0" : (stats.totalAwaitTime / stats.totalCalls).toFixed(2) + "ms"} title="Average response time" />
              </div>
            </div>
            <div className="row">
              <div className="col-md-12">
                <Panel title="Cluster Profiling">
                  <div>
                    <span><strong style={{color:"#783988",fontSize:"25px"}}>/</strong> number of requests per second<br/><strong style={{color:"#EC1F1F",fontSize:"25px"}}>/</strong> failed requests</span>
                    <span className="pull-right"><strong style={{color:"#EC971F",fontSize:"25px"}}>/</strong> average latency in milliseconds</span>
                    <ClusterGraph stats={this.props.clusterStats} />
                  </div>
                </Panel>
              </div>
            </div>
        </div>
    }
});
