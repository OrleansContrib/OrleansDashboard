const React = require('react')

module.exports = class DisplayGrainState extends React.Component {
    constructor(props) {
        super(props);
    }

    render() {
        return (
            <pre style={{margin : '0 0'}}>
                {this.props.code}
            </pre>
        );
    }
}