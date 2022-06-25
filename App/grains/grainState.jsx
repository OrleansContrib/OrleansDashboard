const React = require('react')
const Chart = require('../components/time-series-chart.jsx')
const CounterWidget = require('../components/counter-widget.jsx')
const SiloBreakdown = require('./silo-table.jsx')
const Panel = require('../components/panel.jsx')
const Page = require('../components/page.jsx')
const http = require('../lib/http')

module.exports = class GrainState extends React.Component {



  constructor(props) {
    super(props);
    this.state = {grainId: '', grainType: '', grainState: ''};
    this.handleGrainIdChange = this.handleGrainIdChange.bind(this);
    this.handleGrainTypeChange = this.handleGrainTypeChange.bind(this);
    this.handleSubmit = this.handleSubmit.bind(this);
  }

  updateTextArea() {
    const textArea = document.getElementById("txtGrainState");
    const scrollHeight = textArea.scrollHeight;
    textArea.style.height = (scrollHeight + 5) + "px";
  }

  handleGrainIdChange(event) {
    this.setState({grainId: event.target.value});
  }
  handleGrainTypeChange(event) {
    this.setState({grainType: event.target.value});
  }


  handleSubmit(event)  {
    let component = this;

    http.get('GrainState?grainId=' + this.state.grainId + '&grainType='+ this.state.grainType, function(err, data) {
      component.setState({grainState: data.value});
    }).then( () => {

    });

    event.preventDefault();
  }

  renderEmpty() {
    return <span>No state retrieved</span>
  }

  renderState() {
    return (
      <Page
        title="Grain Details"
      >
        <div>
          <div className="row">
            <div className="col-md-4">
              <select value={this.state.grainType} onChange={this.handleGrainTypeChange}>
                {
                  this.props.grainTypes.value.map((_item) => <option key={_item} value={_item}>{_item}</option>)
                }
              </select>
            </div>
            <div className="col-md-4">
              <label>
                Nome:
                <input type="text" value={this.state.grainId} onChange={this.handleGrainIdChange} />
              </label>
            </div>
            <div className="col-md-4">
            <input type="button" value="Show Details" onClick={this.handleSubmit} />
            </div>
          </div>
          <div className="row">
            <div className="col-md-12">
              <textarea id="txtGrainState" value={this.state.grainState} disabled style={{ width: "100%" }}>
                
              </textarea>
            </div>
          </div>
        </div>
      </Page>
    )
  }

  render() {

    setTimeout(this.updateTextArea, 100);

    // if (Object.keys(this.props.grainTypes).length === 0)
    //   return this.renderEmpty()
    return this.renderState()
  }
}