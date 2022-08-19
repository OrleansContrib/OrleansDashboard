import React from 'react'
import {
  XAxis,
  YAxis,
  Area,
  AreaChart,
  ResponsiveContainer,
  CartesianGrid
} from 'recharts'

interface IProps {
  timepoints: string[]
  series: any[][]
}

// this control is a bit of a temporary hack, until I have a multi-series chart widget
export default class TimeSeriesChart extends React.Component<IProps> {

  render() {

    const data: any[] = []
    for (let i = 0; i < this.props.timepoints.length; i++) {
      data.push({
        name: new Date(this.props.timepoints[i]).toLocaleTimeString(),
        exceptions: this.props.series[0][i],
        count: this.props.series[1][i],
        latency: this.props.series[2][i]
      })
    }

    return (
      <ResponsiveContainer width="100%" height={180}>
        <AreaChart data={data}>
          <CartesianGrid
            strokeDasharray="3 3"
            stroke="rgba(128, 128, 128, 0.2)"
          />
          <XAxis dataKey="name" />
          <YAxis yAxisId="left" />
          <YAxis yAxisId="right" orientation="right" />
          <Area
            yAxisId="left"
            type="monotone"
            dataKey="count"
            stroke="rgba(120,57,136,0)"
            fill="rgba(120,57,136,0.8)"
            isAnimationActive={false}
          />
          <Area
            yAxisId="left"
            type="monotone"
            dataKey="exceptions"
            stroke="rgba(246,31,31,0)"
            fill="rgba(246,31,31,0.8)"
            isAnimationActive={false}
          />
          <Area
            yAxisId="right"
            type="monotone"
            dataKey="latency"
            stroke="rgba(236,151,31,0.8)"
            fill="rgba(236,151,31,0)"
            isAnimationActive={false}
          />
        </AreaChart>
      </ResponsiveContainer>

    )
  }

 
}
