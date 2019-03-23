const React = require('react')

module.exports = class GrainTable extends React.Component {
  constructor(props) {
    super(props)
    this.state = {
      sortBy: 'activationCount',
      sortByAsc: false
    }
    this.handleChangeSort = this.handleChangeSort.bind(this)
  }

  getSorter() {
    let sorter
    switch (this.state.sortBy) {
      case 'activationCount':
        sorter = this.state.sortByAsc
          ? sortByActivationCountAsc
          : sortByActivationCountDesc
        break
      case 'grain':
        sorter = this.state.sortByAsc ? sortByGrainAsc : sortBygrainDesc
        break
      case 'exceptionRate':
        sorter = this.state.sortByAsc
          ? sortByExceptionRateAsc
          : sortByExceptionRateDesc
        break
      case 'totalCalls':
        sorter = this.state.sortByAsc
          ? sortBytotalCallsAsc
          : sortBytotalCallsDec
        break
      case 'totalAwaitTime':
        sorter = this.state.sortByAsc
          ? sortByTotalAwaitTimeAsc
          : sortByTotalAwaitTimeDesc
        break
      default:
        sorter = null
        break
    }
    return sorter
  }

  handleChangeSort(e) {
    let column = e.currentTarget.dataset['column']
    if (column) {
      this.setState({
        sortBy: column,
        sortByAsc: this.state.sortBy === column ? !this.state.sortByAsc : false
      })
    }
  }

  renderStat(stat) {
    var parts = stat.grainType.split('.')
    var grainClassName = parts[parts.length - 1]
    var systemGrain = stat.grainType.startsWith('Orleans.')
    var dashboardGrain = stat.grainType.startsWith('OrleansDashboard.')
    return (
      <tr key={stat.grainType}>
        <td style={{ textOverflow: 'ellipsis' }} title={stat.grainType}>
          <a href={`#/grain/${stat.grainType}`}>{grainClassName}</a>
        </td>
        <td>
          {systemGrain ? (
            <span className="label label-primary">System Grain</span>
          ) : null}
          {dashboardGrain ? (
            <span className="label label-primary">Dashboard Grain</span>
          ) : null}
        </td>
        <td>
          <span className="pull-right">
            <strong>{stat.activationCount}</strong>
          </span>
        </td>
        <td>
          <span className="pull-right">
            <strong>
              {stat.totalCalls === 0
                ? '0.00'
                : ((100 * stat.totalExceptions) / stat.totalCalls).toFixed(2)}
            </strong>{' '}
            <small>%</small>
          </span>
        </td>
        <td>
          <span className="pull-right">
            <strong>{(stat.totalCalls / 100).toFixed(2)}</strong>{' '}
            <small>req/sec</small>
          </span>
        </td>
        <td>
          <span className="pull-right">
            <strong>
              {stat.totalCalls === 0
                ? '0'
                : (stat.totalAwaitTime / stat.totalCalls).toFixed(2)}
            </strong>{' '}
            <small>ms/req</small>
          </span>
        </td>
      </tr>
    )
  }

  render() {
    var grainTypes = {}
    if (!this.props.data) return null

    this.props.data.forEach(stat => {
      if (this.props.silo && stat.siloAddress !== this.props.silo) return

      if (!grainTypes[stat.grainType]) {
        grainTypes[stat.grainType] = {
          activationCount: 0,
          totalSeconds: 0,
          totalAwaitTime: 0,
          totalCalls: 0,
          totalExceptions: 0
        }
      }

      var x = grainTypes[stat.grainType]
      x.activationCount += stat.activationCount
      x.totalSeconds += stat.totalSeconds
      x.totalAwaitTime += stat.totalAwaitTime
      x.totalCalls += stat.totalCalls
      x.totalExceptions += stat.totalExceptions
    })

    var values = Object.keys(grainTypes).map(key => {
      var x = grainTypes[key]
      x.grainType = key
      return x
    })

    values.sort(this.getSorter())

    return (
      <table className="table">
        <tbody>
          <tr>
            <th data-column="grain" onClick={this.handleChangeSort}>
              Grain{' '}
              {this.state.sortBy === 'grain' ? (
                this.state.sortByAsc ? (
                  <i className="fa fa-arrow-up" />
                ) : (
                  <i className="fa fa-arrow-down" />
                )
              ) : null}
            </th>
            <th />
            <th
              data-column="activationCount"
              onClick={this.handleChangeSort}
              style={{ textAlign: 'right' }}
            >
              Activations{' '}
              {this.state.sortBy === 'activationCount' ? (
                this.state.sortByAsc ? (
                  <i className="fa fa-arrow-up" />
                ) : (
                  <i className="fa fa-arrow-down" />
                )
              ) : null}
            </th>
            <th
              data-column="exceptionRate"
              onClick={this.handleChangeSort}
              style={{ textAlign: 'right' }}
            >
              Exception rate{' '}
              {this.state.sortBy === 'exceptionRate' ? (
                this.state.sortByAsc ? (
                  <i className="fa fa-arrow-up" />
                ) : (
                  <i className="fa fa-arrow-down" />
                )
              ) : null}
            </th>
            <th
              data-column="totalCalls"
              onClick={this.handleChangeSort}
              style={{ textAlign: 'right' }}
            >
              Throughput{' '}
              {this.state.sortBy === 'totalCalls' ? (
                this.state.sortByAsc ? (
                  <i className="fa fa-arrow-up" />
                ) : (
                  <i className="fa fa-arrow-down" />
                )
              ) : null}
            </th>
            <th
              data-column="totalAwaitTime"
              onClick={this.handleChangeSort}
              style={{ textAlign: 'right' }}
            >
              Latency{' '}
              {this.state.sortBy === 'totalAwaitTime' ? (
                this.state.sortByAsc ? (
                  <i className="fa fa-arrow-up" />
                ) : (
                  <i className="fa fa-arrow-down" />
                )
              ) : null}
            </th>
          </tr>
          {values.map(this.renderStat)}
        </tbody>
      </table>
    )
  }
}

function sortByActivationCountAsc(a, b) {
  return a.activationCount - b.activationCount
}

function sortByActivationCountDesc(a, b) {
  return sortByActivationCountAsc(b, a)
}

function sortByGrainAsc(a, b) {
  var parts = x => x.grainType.split('.')
  var grainClassName = x => parts(x)[parts(x).length - 1]
  return grainClassName(a) < grainClassName(b)
    ? -1
    : grainClassName(a) > grainClassName(b)
    ? 1
    : 0
}

function sortBygrainDesc(a, b) {
  return sortByGrainAsc(b, a)
}

function sortByExceptionRateAsc(a, b) {
  return a.totalExceptions - b.totalExceptions
}

function sortByExceptionRateDesc(a, b) {
  return sortByExceptionRateAsc(b, a)
}

function sortBytotalCallsAsc(a, b) {
  return a.totalCalls - b.totalCalls
}

function sortBytotalCallsDec(a, b) {
  return sortBytotalCallsAsc(b, a)
}

function sortByTotalAwaitTimeAsc(a, b) {
  if (a.totalCalls === 0 && b.totalCalls === 0) {
    return 0
  } else if (a.totalCalls === 0 || b.totalCalls === 0) {
    return a.totalAwaitTime - b.totalAwaitTime
  } else {
    return a.totalAwaitTime / a.totalCalls - b.totalAwaitTime / b.totalCalls
  }
}

function sortByTotalAwaitTimeDesc(a, b) {
  return sortByTotalAwaitTimeAsc(b, a)
}
