var React = require('react');
var SiloState = require('./silo-state-label.jsx');

module.exports = React.createClass({

    renderSilo:function(silo){
        return <div className="well well-sm">
            <a href={"#/host/" + silo.siloAddress}>{silo.siloAddress}</a> <small><SiloState status={silo.status}/></small>
        </div>
    },

    renderZone:function(updateZone, faultZone){
        var matchingSilos = (this.props.dashboardCounters.hosts || []).filter(x => x.updateZone == updateZone && x.faultZone == faultZone);
        return <span>{matchingSilos.map(this.renderSilo)}</span>
    },

    render:function(){

        var hosts = this.props.dashboardCounters.hosts || [];

        if (hosts.length == 0) return <span>no data</span>

        var updateZones = hosts.map(x => x.updateZone).sort().filter((v, i, a) => a.indexOf(v) === i);
        var faultZones = hosts.map(x => x.faultZone).sort().filter((v, i, a) => a.indexOf(v) === i);

        return <div>
            <table className="table table-bordered table-hovered">
                <tbody>
                    <tr>
                        <td></td>
                        {faultZones.map(faultZone => {
                            return <th>Fault Zone {faultZone}</th>
                        })}
                    </tr>
                    {updateZones.map(updateZone => {
                        return <tr key={updateZone}>
                            <th>Update Zone {updateZone}</th>
                            {faultZones.map(faultZone => {
                                return <td key={faultZone}>{this.renderZone(updateZone, faultZone)}</td>
                            })}
                        </tr>
                    })}
                </tbody>
            </table>
        </div>

    }
});
