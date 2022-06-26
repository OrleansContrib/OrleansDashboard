const React = require('react')
const Page = require('../components/page.jsx')
const http = require('../lib/http')
const DisplayGrainState = require('../components/display-grain-state.jsx');
const Panel = require('../components/panel.jsx')

module.exports = class GrainDetails extends React.Component {

  constructor(props) {
    super(props);
    this.state = { grainId: '', grainType: null, grainState: '' };
    this.handleGrainIdChange = this.handleGrainIdChange.bind(this);
    this.handleGrainTypeChange = this.handleGrainTypeChange.bind(this);
    this.handleSubmit = this.handleSubmit.bind(this);
  }

  handleGrainIdChange(event) {
    this.setState({ grainId: event.target.value });
  }
  handleGrainTypeChange(event) {
    this.setState({ grainType: event.target.value });
  }

  handleSubmit(event) {
    let component = this;

    http.get('GrainState?grainId=' + this.state.grainId + '&grainType=' + this.state.grainType, function (err, data) {
      component.setState({ grainState: data.value });
    }).then(() => {

    });

    event.preventDefault();
  }

  renderEmpty() {
    return <span>No state retrieved</span>
  }

  renderState() {

    let displayComponent;

    if (this.state.grainState != '') {
      displayComponent = <DisplayGrainState code={this.state.grainState} />;
    } else {
      displayComponent = <div></div>;
    }

    const inputStyle = { height: '25px', marginLeft: '5px' };

    return (
      <Page
        title="Grain Details"
      >
        <div>

          <Panel title='Grain' subTitle="Only Non Generic Grains and non Compound Key supported">
            <div className="row">
              <div className="col-md-12 pull-left">
                <select value={this.state.grainType} style={inputStyle} onChange={this.handleGrainTypeChange}>
                  <option disabled selected value=""> -- Select an grain type -- </option>
                  {
                    this.props.grainTypes.value.map((_item) => <option key={_item} value={_item}>{_item}</option>)
                  }
                </select>

                <input type="text" placeholder='Grain Id' style={inputStyle}
                  value={this.state.grainId} onChange={this.handleGrainIdChange} />

                <input type="button" value="Show Details" style={inputStyle} onClick={this.handleSubmit} />
              </div>

            </div>
          </Panel>

          <Panel title='State'>
            <div className="row">
              <div className="col-md-12">
                {displayComponent}
              </div>
            </div>
          </Panel>

        </div>
      </Page>
    )
  }

  render() {
    return this.renderState()
  }
}