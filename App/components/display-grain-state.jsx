const React = require('react')

module.exports = class DisplayGrainState extends React.Component {
    constructor(props) {
        super(props);
    }

    componentDidMount() {
        var ace = window.ace;
        ace.config.set('basePath', 'https://pagecdn.io/lib/ace/1.4.14/');
        const editor = ace.edit('editor_state');
        editor.setTheme("ace/theme/dracula");
        editor.getSession().setMode("ace/mode/json");
        editor.setShowPrintMargin(false);
        editor.setOptions({ minLines: 25 });
        editor.setReadOnly(true);
        editor.setValue(this.props.code);
        editor.resize();
        this.editor_state = editor;
    }

    componentDidUpdate(){
        this.editor_state.setValue(this.props.code);
        this.editor_state.resize();
    }

    render() {
        const style = { fontSize: '14px !important', border: '1px solid lightgray', minHeight: '250px' };
        return (
            <div id="editor_state" style={style}>
            </div>
        );
    }
}