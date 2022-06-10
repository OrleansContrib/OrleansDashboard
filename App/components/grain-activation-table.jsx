const React = require('react')

module.exports = class GrainActivationTable extends React.Component {
  constructor(props) {
    super(props)
    this.renderRow = this.renderRow.bind(this)
  }



  renderRow(value) {
    return (
      <tr key={`${value.grainId}`}>
        <td style={{ wordWrap: 'break-word' }}>
          <span className="pull-left">
            <a href={`#/grainState/${getName(this.props.grainType)}/${transformUrlId(value.grainId)}`}> <strong>{getId(value)}</strong> </a>
          </span>
        </td>
      </tr>
    )
  }
  render() {
    const activations = [];

    
    this.props.simpleGrainStats.forEach(stat => {
      if (stat.grainType !== this.props.grainType) return

      stat.activations.forEach(act => {
        activations.push(act);
      });
    })

    return (
      <table className="table" style={{ tableLayout: 'fixed', width: '100%'  }}>
          <tbody style={{overflowX: 'auto' }}>
            {activations.map(this.renderRow)}
            {activations.length === 0 &&
              <tr>
                <td>
                  <i>No data</i>
                </td>
              </tr>
            }
          </tbody>
        </table>
    )
  }
}


function getName(value) {
  try {
    var parts = value.split(',')[0].split('.')
    return parts[parts.length - 1]
  } catch (error) {
      return value;
  }
}

function getId(value){
  return value.guidId || value.intId || value.grainId;
}

function transformUrlId(value) {
  return value.replace("/","_");
}
