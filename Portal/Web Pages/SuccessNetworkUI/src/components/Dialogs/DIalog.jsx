import React from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import PropTypes from 'prop-types';
import classnames from 'classnames';

import * as current from '../../reducers/current.js';

class Dialog extends React.Component {
    constructor(props, context) {
        super(props, context);
    }

    componentWillReceiveProps(nextProps) {
        // Purposefully do not use strict equality as it only matters when the prop changes from a
        // truthy value to a falsy value
        if (this.props.isOpen != nextProps.isOpen) {
            this.props.toggleModalDialog(nextProps.isOpen);
        }
    }

    render() {
        return (
            this.props.isOpen ?
            <div>
                <div className='modal fade in' style={{display: 'block'}} tabIndex='-1' role='dialog'>
                    <div className='modal-dialog' role='document'>
                        <div className='modal-content'>
                            <div className='modal-header'>
                                <button type="button" className="close" onClick={this.props.toggleClose}><span>x</span></button>
                                <h4 className='modal-title'>{this.props.title}</h4>
                            </div>
                            <div className={classnames('modal-body', this.props.additionalBodyClass)}>
                                {this.props.body}
                            </div>
                            <div className='modal-footer'>
                                {this.props.footer}
                            </div>
                        </div>
                    </div>
                </div>
                <div className="modal-backdrop fade in"></div>
            </div>
            : null
        );
    }
}

Dialog.propTypes = {
    footer: PropTypes.element,
    title: PropTypes.string,
    content: PropTypes.element,
    isOpen: PropTypes.bool
};

const mapDispatchToProps = dispatch => {
    return bindActionCreators({...current}, dispatch);
};

export default connect(null, mapDispatchToProps)(Dialog);