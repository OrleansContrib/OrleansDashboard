const React = require('react')
const Chart = require('../components/time-series-chart.jsx')
const CounterWidget = require('../components/counter-widget.jsx')
const SiloBreakdown = require('./silo-table.jsx')
const Panel = require('../components/panel.jsx')
const Page = require('../components/page.jsx')

module.exports = class GrainState extends React.Component {


  updateTextArea() {
    const textArea = document.getElementById("txtGrainState");
    const scrollHeight = textArea.scrollHeight;
    textArea.style.height = (scrollHeight + 5) + "px";
  }

  renderEmpty() {
    return <span>No state retrieved</span>
  }

  renderState() {
    
    return (
      <Page
        title={getName(this.props.grainType)}
        subTitle={this.props.grainDisplayId}
      >
        <div>
          <div className="row">
            <div className="col-md-12">
                <textarea id="txtGrainState" disabled style={{width: "100%"}}>
                {JSON.stringify(this.props.state, null, "\t")}
                </textarea>
            </div>
            
          </div>
        </div>
      </Page>
    )
  }

  render() {

    setTimeout(this.updateTextArea,100);

    if (Object.keys(this.props.state).length === 0)
        return this.renderEmpty()
    return this.renderState()
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