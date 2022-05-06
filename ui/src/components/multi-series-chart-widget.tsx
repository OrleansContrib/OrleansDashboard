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


const colours = [
  [120, 57, 136],
  [236, 151, 31]
]

interface IProps {
  series: string[][]
}

interface IState {
  width: number
}

// this control is a bit of a temporary hack, until I have a multi-series chart widget
export default class MultiSeriesChartWidget extends React.Component<
  IProps,
  IState
> {
  state: IState = { width: 0 }

  getWidth = () => {
    if (!this.containerRef.current) return
    this.setState({ width: this.containerRef.current.offsetWidth - 20 })
  }

  renderChart = () => {
    if (this.state.width === 0) return setTimeout(this.getWidth, 0)

    var data = {
      labels: this.props.series[0].map(function (x) {
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
      <Line
        data={data}
        options={{
          animation: false,
          maintainAspectRatio: false,
          responsive: true,
          plugins: {
            tooltip: { enabled: false },
            legend: { display: false }
          },
          scales: {
            xAxes: {
              ticks: { display: true }
            },
            yAxes: {
              ticks: { display: true }
            }
          }
        }}
        width={this.state.width}
        height={80}
      />
    )
  }

  containerRef = createRef<HTMLDivElement>()

  render() {
    return <div ref={this.containerRef}>{this.renderChart()}</div>
  }
}
