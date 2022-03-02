import React, { createRef } from 'react'
import { Line } from 'react-chartjs'

interface IProps {
  timepoints: number[]
  series: any[][]
}

interface IState {
  width: number
}

// this control is a bit of a temporary hack, until I have a multi-series chart widget
export default class TimeSeriesChart extends React.Component<IProps, IState> {
  state = {
    width: 0
  }

  getWidth = () => {
    if (!this.containerRef.current) return

    this.setState({ width: this.containerRef.current.offsetWidth })
  }

  containerRef = createRef<HTMLDivElement>()

  renderChart() {
    if (this.state.width === 0) {
      return setTimeout(this.getWidth, 0)
    }

    var data = {
      labels: this.props.timepoints.map(timepoint => {
        if (timepoint) {
          try {
            if (new Date(timepoint).getSeconds() % 30 === 0) {
              return new Date(timepoint).toLocaleTimeString()
            }
          } catch (e) {
            // not a valid date string
          }
        }

        return ''
      }),
      datasets: [
        {
          label: 'y2',
          backgroundColor: `rgba(236,151,31,0)`,
          borderColor: `rgba(236,151,31,0.8)`,
          data: this.props.series[2],
          pointRadius: 0,
          yAxisID: 'y2'
        },
        {
          label: 'y1',
          backgroundColor: `rgba(246,31,31,0.8)`,
          borderColor: `rgba(246,31,31,0)`,
          data: this.props.series[0],
          pointRadius: 0,
          yAxisID: 'y1'
        },
        {
          label: 'y1',
          backgroundColor: `rgba(120,57,136,0.8)`,
          borderColor: `rgba(120,57,136,0)`,
          data: this.props.series[1],
          pointRadius: 0,
          yAxisID: 'y1'
        }
      ]
    }

    var options = {
      legend: { display: false },
      maintainAspectRatio: false,
      animation: false,
      showTooltips: false,
      responsive: true,
      hoverMode: 'label',
      stacked: false,
      scales: {
        xAxes: [
          {
            display: true,
            gridLines: {
              offsetGridLines: false,
              drawOnChartArea: false
            },
            ticks: {
              autoSkip: false,
              maxRotation: 0,
              minRotation: 0,
              fontSize: 9
            }
          }
        ],
        yAxes: [
          {
            type: 'linear',
            display: true,
            position: 'left',
            id: 'y1',
            gridLines: { drawOnChartArea: false },
            ticks: { beginAtZero: true }
          },
          {
            type: 'linear',
            display: true,
            position: 'right',
            id: 'y2',
            gridLines: { drawOnChartArea: false },
            ticks: { beginAtZero: true }
          }
        ]
      }
    }

    return (
      <Line
        data={data}
        options={options}
        width={this.state.width}
        height={180}
      />
    )
  }

  render() {
    return <div ref={this.containerRef}>{this.renderChart()}</div>
  }
}
