import React from 'react'
import {
  Area,
  AreaChart,
  ResponsiveContainer,
  CartesianGrid
} from 'recharts'

const colours = [
  [120, 57, 136],
  [236, 151, 31]
]

interface IProps {
  series: string[][]
}

// this control is a bit of a temporary hack, until I have a multi-series chart widget
export default class MultiSeriesChartWidget extends React.Component<IProps> {
  render() {
    const data: any[] = []

    for (let i = 0; i < this.props.series[0].length; i++) {
      const item: any = {
        name: ''
      }
      for (let j = 0; j < this.props.series.length; j++) {
        item[`${j}`] = this.props.series[j][i]
      }
      data.push(item)
    }

    return (
      <ResponsiveContainer width="100%" height={80}>
        <AreaChart data={data}>
          <CartesianGrid
            strokeDasharray="3 3"
            stroke="rgba(128, 128, 128, 0.2)"
          />
      

          {this.props.series.map((_, index) => {
            const colourString = colours[index % colours.length].join()
            return (
              <Area
                type="monotone"
                dataKey={`${index}`}
                stroke={`rgba(${colourString},0.8)`}
                fill={`rgba(${colourString},0.1)`}
                isAnimationActive={false}
              />
            )
          })}
        </AreaChart>
      </ResponsiveContainer>
    )
  }
}
