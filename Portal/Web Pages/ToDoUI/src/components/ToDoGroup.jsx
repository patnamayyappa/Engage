import React from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';

import ToDo from './ToDo.jsx';

import * as translations from '../reducers/translations.js';

class ToDoGroup extends React.Component {
    constructor(props, context) {
        super(props, context);
    }

    getGroupTranslation = name => {
        if (!name) {
            return this.props.getTranslation('PortalToDos_NA', 'N/A');
        }

        return this.props.getTranslation(`PortalToDos_${name.replace(/[ -]/gi, '')}`, name);
    }

    render() {
        return(
            <div>
                {this.props.displayCategory
                    ? (
                        <div className='row'>
                            <div className='col-md-12 col-sm-12 col-xs-12 to-do-group-title'>
                                <h3>{this.getGroupTranslation(this.props.name)}</h3><hr />
                            </div>
                        </div>)
                    : ''}
                {this.props.toDoIds.map(id => <ToDo key={id} toDo={this.props.toDos[id]} openCloseToDoDialog={this.props.openCloseToDoDialog} />)}
            </div>
        );
    }
}

const mapDispatchToProps = dispatch => {
    return bindActionCreators({...translations}, dispatch);
};

export default connect(null, mapDispatchToProps)(ToDoGroup);