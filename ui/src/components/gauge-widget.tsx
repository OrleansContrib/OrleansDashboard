import React, { createRef } from 'react'
import { PieChart, Pie, Cell } from "recharts";

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


    const data:any[] = [
      { name: "", value: this.props.max - this.props.value  },
      { name: "", value: this.props.value}
    ];

    return (
    <PieChart width={this.state.width} height={200}>
      <Pie
        data={data}
        innerRadius={98}
        outerRadius={100}
        paddingAngle={0}
        dataKey="value"
        startAngle={-90}
        endAngle={270}
        strokeOpacity={0}
        fill="rgba(120, 57, 136, 0.8)"
      >
          <Cell key="1" fill="rgba(120, 57, 136, 0.2)" />
          <Cell key="0" fill="rgba(120, 57, 136, 0.8)" />
      </Pie>
    </PieChart>
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
