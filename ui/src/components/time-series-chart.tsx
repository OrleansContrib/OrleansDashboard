import React, { createRef } from 'react'

import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
} from 'chart.js';

import { Line } from 'react-chartjs-2'

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend
);

interface IProps {
  timepoints: string[]
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
      labels: this.props.timepoints.map((timepoint: string) => {
        if (timepoint) {
          try {
            console.log(timepoint)
            if ((new Date().getTime() / 1000) % 10 === 0) {
              return new Date(timepoint).toLocaleTimeString()
            }
          } catch (e) {
            console.log({ e })
            // not a valid date string
          }
        }

        return ''
      }),
      datasets: [
        {
          label: 'y1',
          backgroundColor: `rgba(120,57,136,0.8)`,
          borderColor: `rgba(120,57,136,0)`,
          data: this.props.series[1],
          pointRadius: 0,
          yAxisID: 'y1'
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
          label: 'y2',
          backgroundColor: `rgba(236,151,31,0)`,
          borderColor: `rgba(236,151,31,0.8)`,
          data: this.props.series[2],
          pointRadius: 0,
          yAxisID: 'y2'
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
      pointDot: false,
      scales: {
        // xAxes: [
        //   {
        //     display: true,
        //     gridLines: {
        //       offsetGridLines: false,
        //       drawOnChartArea: false
        //     },
        //     ticks: {
        //       autoSkip: false,
        //       maxRotation: 0,
        //       minRotation: 0,
        //       fontSize: 9
        //     }
        //   }
        // ],
        y1: {
          type: 'linear',
          display: true,
          position: 'left',
        },
        y2: {
          type: 'linear',
          display: true,
          position: 'right'
        }
        // yAxes: [
        //   {
        //     type: 'linear',
        //     display: true,
        //     position: 'left',
        //     id: 'y1',
        //     gridLines: { drawOnChartArea: false },
        //     ticks: { beginAtZero: true }
        //   },
        //   {
        //     type: 'linear',
        //     display: true,
        //     position: 'right',
        //     id: 'y2',
        //     gridLines: { drawOnChartArea: false },
        //     ticks: { beginAtZero: true }
        //   }
        // ]
      }
    }

    return (
      <Line
        data={data}
        options={{
          plugins: {
            legend: {
              display: false
            },
            tooltip: { enabled: false }
          },
          maintainAspectRatio: false,
          animation: false,
          responsive: true,
          scales: {
            y1: {
              type: "linear",
              display: true,
              position: 'left',
              grid: { display: false },
              ticks: { display: true }
            },
            y2: {
              type: 'linear',
              display: true,
              position: 'right',
              grid: { display: false },
              ticks: { display: true }
            }
          }
        }}
        width={this.state.width}
        height={180}
      />
    )
  }

  render() {
    return <div ref={this.containerRef}>{this.renderChart()}</div>
  }
}
