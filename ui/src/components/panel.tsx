import React from 'react'

interface IProps {
  title: string
  subTitle?: string
  footer?: React.ReactFragment
}

export default class Panel extends React.Component<IProps> {
  render() {
    let body = this.props.children
    let footer = null

    if (this.props.footer) {
      footer = <div className="box-footer clearfix">{this.props.footer}</div>
    }

    return (
      <div className="box">
        <div className="box-header with-border">
          <h3 className="box-title">
            {this.props.title}
            <small style={{ marginLeft: '10px' }}>{this.props.subTitle}</small>
          </h3>
        </div>
        <div className="box-body">{body}</div>
        {footer}
      </div>
    )
  }
}
