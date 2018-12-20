const React = require('react')
const Chart = require('react-chartjs').Line

const colours = [[120, 57, 136], [236, 151, 31]]

// this control is a bit of a temporary hack, until I have a multi-series chart widget
module.exports = class extends React.Component {
  constructor(props) {
    super(props)
    this.state = { width: 0 }
    this.getWidth = this.getWidth.bind(this)
    this.renderChart = this.renderChart.bind(this)
  }

  getWidth() {
    if (!this.refs.container) return
    this.setState({ width: this.refs.container.offsetWidth - 20 })
  }

  renderChart() {
    if (this.state.width === 0) return setTimeout(this.getWidth, 0)

    var data = {
      labels: this.props.series[0].map(function(x) {
        return ''
      }),
      datasets: this.props.series.map((data, index) => {
        var colourString = colours[index % colours.length].join()
        return {
          label: '',

          backgroundColor: `rgba(${colourString},0.1)`,
          borderColor: `rgba(${colourString},1)`,
          data: data,
          pointRadius: 0
        }
      })
    }

    return (
      <Chart
        data={data}
        options={{
          animation: false,
          legend: { display: false },
          maintainAspectRatio: false,
          responsive: true,
          showTooltips: false,
          scales: { yAxes: [{ ticks: { beginAtZero: true } }] }
        }}
        width={this.state.width}
        height={80}
      />
    )
  }

  render() {
    return <div ref="container">{this.renderChart()}</div>
  }
}
