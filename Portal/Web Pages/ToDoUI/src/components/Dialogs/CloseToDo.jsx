import React from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { observable } from 'mobx';
import { observer } from 'mobx-react';
import classnames from 'classnames';

import Dialog from './Dialog.jsx';

import * as translations from '../../reducers/translations.js';

@observer
class CloseToDo extends React.Component {
    @observable closeComment;

    constructor(props, context) {
        super(props, context);

        this.closeComment = '';
    }

    componentWillReceiveProps(nextProps) {
        if (this.props.isOpen && !nextProps.isOpen) {
            this.closeComment = '';
        }
    }

    buildBody = () => {
        return (
            <div className='form-group'>
                <label htmlFor='close-comment'>{this.props.getTranslation('PortalToDos_CommentsOptional', 'Comments (optional)')}</label>
                <textarea id='close-comment'
                    maxLength='500'
                    className='form-control'
                    value={this.closeComment}
                    autoFocus='true'
                    onChange={e => this.closeComment = e.target.value && e.target.value.replace(/\n/g, '')} />
            </div>
        );
    }

    buildFooter = () => {
        return (
            <div>
                <button type='button' className='btn btn-default' onClick={this.props.toggleClose}>{this.props.getTranslation('cancelButton', 'Cancel')}</button>
                <button type='button'
                    className={classnames('btn','font-to-upper', this.props.actionButtonClass)}
                    onClick={() => {this.props.action(this.closeComment);}}>
                    {this.props.actionText}
                </button>
            </div>
        );
    }

    render() {
        return (
            <Dialog
                title={this.props.actionText}
                body={this.buildBody()}
                footer={this.buildFooter()}
                isOpen={this.props.isOpen}
                toggleClose={this.props.toggleClose} />
        );
    }
}

const mapDispatchToProps = dispatch => {
    return bindActionCreators({...translations}, dispatch);
};

export default connect(null, mapDispatchToProps)(CloseToDo);