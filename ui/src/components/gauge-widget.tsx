import React, { createRef } from 'react'
import { Doughnut } from 'react-chartjs-2'
import { Chart as ChartJS, ArcElement, Tooltip, Legend } from 'chart.js';

ChartJS.register(ArcElement, Tooltip, Legend);

interface IProps {
  value: number
  max: number
  title: string
  description: string
}

interface IState {
  width: number
}

export default class GaugeWidget extends React.Component<IProps, IState> {
  state: IState = { width: 0 }

  getWidth = () => {
    this.setState({ width: this.containerRef.current?.clientWidth || 0 })
  }

  getColour = (alpha: number) => `rgba(120, 57, 136, ${alpha})`

  renderChart = () => {
    if (this.state.width === 0) return setTimeout(this.getWidth, 0)

    var data = {
      labels: ['', ''],
      datasets: [
        {
          data: [this.props.value, this.props.max - this.props.value],
          backgroundColor: [this.getColour(1), this.getColour(0.2)],
          hoverBackgroundColor: [this.getColour(1), this.getColour(0.2)],
          borderWidth: [0, 0],
          hoverBorderWidth: [0, 0]
        }
      ]
    }

    var options = {
      legend: { display: false },
      animation: false,
      showTooltips: false,
      cutoutPercentage: 92
    }

    return (
      <Doughnut
        data={data}
        options={{
          animation: false,
          plugins: {
            legend: { display: false }
          },
          cutout: '92%'
        }}
        width={this.state.width}
        height={200}
      />
    )
  }

  containerRef = createRef<HTMLDivElement>()

  render() {
    var percent = Math.floor((100 * this.props.value) / this.props.max)
    return (
      <div
        ref={this.containerRef}
        style={{
          textAlign: 'center',
          position: 'relative',
          minHeight: '100px'
        }}
      >
        <h4>{this.props.title}</h4>
        <div
          style={{
            position: 'absolute',
            textAlign: 'center',
            fontSize: '60px',
            left: 0,
            right: 0,
            top: '50%',
            marginTop: -45
          }}
        >
          {percent}%
        </div>
        <div
          style={{
            position: 'absolute',
            textAlign: 'center',
            fontSize: '60px',
            width: '100%'
          }}
        />
        {this.renderChart()}
        <span style={{ lineHeight: '40px' }}>{this.props.description}</span>
      </div>
    )
  }
}
