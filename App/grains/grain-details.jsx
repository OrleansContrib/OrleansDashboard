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

    return (
      <Page
        title="Grain Details"
      >
        <div>

          <Panel title='Grain' subTitle="Only non generic grains are supported">
            <div className="row">
              <div className="col-md-6 col-lg-6 col-xl-6">
                <div class="input-group">
                  <select value={this.state.grainType} className="form-control" onChange={this.handleGrainTypeChange}>
                    <option disabled selected value=""> -- Select an grain type -- </option>
                    {
                      this.props.grainTypes.map((_item) => <option key={_item} value={_item}>{_item}</option>)
                    }
                  </select>
                </div>
              </div>
              <div className="col-md-4 col-lg-4 col-xl-5">
                <div class="input-group">
                  <input type="text" placeholder='Grain Id' className="form-control"
                    value={this.state.grainId} onChange={this.handleGrainIdChange} />
                </div>
              </div>
              <div className="col-md-2 col-lg-2 col-xl-1">
                <div class="input-group">
                  <input type="button" value="Show Details" className='btn btn-default btn-block' onClick={this.handleSubmit} />
                </div>
              </div>
            </div>
          </Panel>

          <Panel title='State' bodyPadding='0px'>
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