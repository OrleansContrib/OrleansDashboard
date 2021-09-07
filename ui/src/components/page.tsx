import React from 'react'

interface IProps {
  title: string
  subTitle: string
}

export default class Page extends React.Component<IProps> {
  render() {
    return (
      <div>
        <section className="content-header">
          <h1>
            {this.props.title} <small> {this.props.subTitle}</small>
          </h1>
        </section>
        <section className="content" style={{ position: 'relative' }}>
          {this.props.children}
        </section>
      </div>
    )
  }
}
