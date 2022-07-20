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
  ChartData,
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

    var data: ChartData<"line", any[], string> = {
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
          backgroundColor: `rgba(236,151,31,0)`,
          borderColor: `rgba(236,151,31,0.8)`,
          data: this.props.series[0],
          pointRadius: 0,
          showLine: true,
          fill: true,
          yAxisID: 'y'
        },
        {
          backgroundColor: `rgba(246,31,31,0.8)`,
          borderColor: `rgba(246,31,31,0)`,
          data: this.props.series[1],
          pointRadius:10,
          showLine: true,
          fill: true,
          borderWidth: 1,
          yAxisID: 'y1'
        },
        {
          backgroundColor: `rgba(120,57,136,0.8)`,
          borderColor: `rgba(120,57,136,0)`,
          data: this.props.series[2],
          pointRadius: 10,
          showLine: true,
          fill: true,
          borderWidth: 1,
          yAxisID: 'y1'
        },

      ]
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

            y: {
              axis: 'y',
              type: "linear",
              display: true,
              position: 'left',
              grid: { drawOnChartArea: false },
              ticks: { display: true },
              beginAtZero: true,
              
            },
            y1: {
              axis: 'y',
              type: 'linear',
              display: true,
              position: 'right',
              grid: { drawOnChartArea: false },
              ticks: { display: true },
              beginAtZero: true
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
