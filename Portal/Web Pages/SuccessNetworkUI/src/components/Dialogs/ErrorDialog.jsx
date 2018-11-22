import React from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';

import Dialog from './Dialog.jsx';

import * as current from '../../reducers/current.js';
import * as translations from '../../reducers/translations.js';

class ErrorDialog extends React.Component {
    constructor(props, context) {
        super(props, context);
    }

    buildBody() {
        if (this.props.messages) {
            return (
                <div>
                    {this.props.messages.map((message, index) => {
                        return (
                            <p key={index}>{this.props.getTranslation(message.translationKey, message.default)}</p>
                        );
                    })}
                </div>
            );
        }
    }

    buildFooter = () => {
        return (
             <button type='button' className='btn btn-default' onClick={this.clearMessages}>{this.props.getTranslation('okButton', 'OK')}</button>
        );
    }

    render() {
        return (
            <Dialog
                title={this.props.getTranslation('PortalSuccessNetwork_Error', 'Error')}
                body={this.buildBody()}
                footer={this.buildFooter()}
                isOpen={this.props.canOpen && this.props.messages ? this.props.messages.length > 0 : false } />
        );
    }

    clearMessages = () => {
        this.props.clearAllMessages();
    }
}

const mapStateToProps = state => {
    return {
        messages: state.current.messages
    };
};

const mapDispatchToProps = dispatch => {
    return bindActionCreators({ ...current, ...translations  }, dispatch);
};

export default connect(mapStateToProps, mapDispatchToProps)(ErrorDialog);