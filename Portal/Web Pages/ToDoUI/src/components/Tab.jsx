import React from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';

import classnames from 'classnames';

import ToDoGroup from './ToDoGroup.jsx';
import HeaderControls from './HeaderControls.jsx';

import * as current from '../reducers/current.js';
import * as translations from '../reducers/translations.js';

import {uiStatuses, sortOrders, newestCategories} from '../constants.js';

class Tab extends React.Component {

    constructor(props, context) {
        super(props, context);
    }

    buildToDos = (groupedAndSortedIds, toDos, sortOrder, openCloseToDoDialog) => {
        return this.props.sortedGroups.map(key => {
            if (!groupedAndSortedIds[key] || !groupedAndSortedIds[key]) {
                return;
            }

            return <ToDoGroup key={key}
                displayCategory={sortOrder.value !== sortOrders.dueDate.value}
                name={key}
                toDoIds={groupedAndSortedIds[key]}
                toDos={toDos}
                openCloseToDoDialog={openCloseToDoDialog}
                displayAccessError={this.props.displayAccessError} />;
        });
    }

    render() {
        const tab = this.props.tab;
        return (
            <div className='container'>
                <div className='row equal-height to-do-header'>
                    <div className='col-md-7'>
                        <h1>{this.props.getTranslation('PortalToDos_MyToDos', 'My To Dos')}</h1>
                    </div>
                    <div className='col-md-5 flex-no-wrap hidden-sm hidden-xs'>
                        {!this.props.displayAccessError &&
                        <HeaderControls
                            tabKey={tab.key}
                            hideOptionalToDos={tab.hideOptionalToDos}
                            setHideOptionalToDos={this.props.setHideOptionalToDos}
                            getTranslation={this.props.getTranslation}
                            sortTabToDos={this.props.sortTabToDos}
                            sortOrder={tab.sortOrder}
                            sortOrders={tab.sortOrders} />}
                    </div>
                </div>
                {!this.props.displayAccessError
                    ?(<div className='row to-do-body'>
                        <div className='col-md-3'>
                            <ul className='list-group'>
                                {Object.keys(uiStatuses).map(status => {
                                    let uiStatus = uiStatuses[status];
                                    return (
                                        <li key={status}
                                            className={classnames('list-group-item', 'tab-item', {active: uiStatus.value === this.props.selectedTab})}
                                            onClick={() => this.props.setSelectedTab(uiStatus.value)}>
                                                {this.props.getTranslation(uiStatus.translationKey, uiStatus.value)}
                                            <span className='badge'>{this.props.tabRecordCount[uiStatus.value]}</span>
                                        </li>);
                                }
                                )}
                            </ul>
                            <div className='visible-sm-block visible-xs-block hidden-md header-content'>
                                <HeaderControls
                                    tabKey={tab.key}
                                    hideOptionalToDos={tab.hideOptionalToDos}
                                    setHideOptionalToDos={this.props.setHideOptionalToDos}
                                    getTranslation={this.props.getTranslation}
                                    sortTabToDos={this.props.sortTabToDos}
                                    sortOrder={tab.sortOrder}
                                    sortOrders={tab.sortOrders} />
                            </div>
                        </div>
                        <div className='col-md-9'>
                            {this.buildToDos(this.props.visibleToDoIds, tab.toDos, tab.sortOrder, this.props.openCloseToDoDialog)}
                        </div>
                    </div>)
                    : ((<div className="alert alert-block alert-danger">{this.props.getTranslation('PortalToDos_AccessDeniedError', 'You don\'t have permissions to view these records.')}</div>))
                }
            </div>
        );
    }
}

const getToDoIds = (state, props) =>{
    return props.tab.groupedAndSortedIds;
};

const getToDos = (state, props) => {
    return props.tab.toDos;
};

const getHideOptionalToDos = (state, props) => {
    return props.tab.hideOptionalToDos;
};

const getSortOrder = (state, props) => {
    return props.tab.sortOrder;
};

const getVisibleToDoIds = createSelector(
    [getToDoIds, getToDos, getHideOptionalToDos],
    (toDoIds, toDos, hideOptionalToDos) => {
        let visibleToDos = {};

        if(!hideOptionalToDos) {
            return toDoIds;
        }

        Object.keys(toDoIds).forEach(key => {
            visibleToDos[key] = [];
            toDoIds[key].forEach(id => {
                if (!toDos[id].isRequired) {
                    return;
                }

                visibleToDos[key].push(id);
            });

            if (Object.keys(visibleToDos[key]).length == 0) {
                delete visibleToDos[key];
            }
        });

        return visibleToDos;
    }
);

const getSortedGroups = createSelector(
    [getToDoIds, getSortOrder],
    (toDoIds, sortOrder) => {
        if (!sortOrder || sortOrder.value === sortOrders.dueDate.value) {
            return Object.keys(toDoIds);
        }

        switch(sortOrder.value)  {
            case sortOrders.newest.value:
                return Object.keys(toDoIds).sort(key1 => {
                    if (key1 === newestCategories.isNew) {
                        return -1;
                    }
                    else {
                        return 1;
                    }
                });
            case sortOrders.category.value:
            case sortOrders.successPlan.value:
            default:
                return Object.keys(toDoIds).sort((key1, key2) => {
                    if (!key1 && !key2) {
                        return 0;
                    }
                    else if (!key1) {
                        return 1;
                    }
                    else if (!key2) {
                        return -1;
                    }
                    else {
                        return key1.toLowerCase().localeCompare(key2.toLowerCase());
                    }
                });
        }

    }
);

const mapStateToProps = (state, ownProps) => {
    return {
        visibleToDoIds: getVisibleToDoIds(state, ownProps),
        sortedGroups: getSortedGroups(state, ownProps)
    };
};

const mapDispatchToProps = dispatch => {
    return bindActionCreators({...current, ...translations}, dispatch);
};

export default connect(mapStateToProps, mapDispatchToProps)(Tab);