import React from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { observable } from 'mobx';
import { observer } from 'mobx-react';

import moment from 'moment';
import classnames from 'classnames';

import * as toDos from '../reducers/toDos.js';
import * as translations from '../reducers/translations.js';
import {uiStatuses} from '../constants.js';

@observer
class ToDo extends React.Component {
    @observable openDescription;
    @observable descriptionOverflow;

    constructor(props, context) {
        super(props, context);

        this.openDescription = false;
    }

    componentDidMount() {
        this.update();
    }

    componentDidUpdate() {
        this.update();
    }

    update = () => {
        if (!this.description) {
            return;
        }

        if (this.openDescription) {
            return;
        }

        if (this.description.scrollHeight > this.description.offsetHeight) {
            this.descriptionOverflow = true;
        }
    }

    buildDateBlock = (toDo) => {
        let dueDate,
            date,
            monthString,
            yearString;

        dueDate = moment(toDo.dueDate);

        if (dueDate.isValid()) {
            date = dueDate.date();

            monthString = dueDate.format('MMM');
            if (monthString.length > 3) {
                monthString = monthString.substring(0,3);
            }

            yearString = dueDate.format('YYYY');
        }

        const isOverDue = toDo.isIncomplete && dueDate.isBefore(moment());

        return (
            <div className={classnames('due-date-block', {well: !isOverDue}, {'alert-danger': isOverDue})}>
                <div className='font-to-upper'>
                    {!isOverDue
                     ? this.props.getTranslation('PortalToDos_Due', 'Due')
                     : this.props.getTranslation('PortalToDos_Overdue','Overdue')}
                </div>
                <div>
                    <span className={classnames('h1', {'alert-danger': isOverDue})}>{date}</span>
                </div>
                <div className='font-to-upper'>
                    {`${monthString} ${yearString}`}
                </div>
            </div>
        );
    };

    buildCommentsRow = toDo => {
        if (!this.isToDoClosed(toDo) || !toDo.completionCancellationComment) {
            return;
        }

        return (
            <div className='comments-row'>
                <em>{this.props.getTranslation('PortalToDos_Comments', 'Comments')}: {toDo.completionCancellationComment}</em>
            </div>
        );
    }

    buildDetailsRow = toDo => {
        let elements = [],
            ownerString = '';

        if (toDo.isIncomplete) {
            if (toDo.isRequired) {
                elements.push(<span key='required' className='label label-danger'>{this.props.getTranslation('PortalToDos_Required', 'Required')}</span>);
            }

            if (toDo.isNew) {
                elements.push(<span key='new' className='label label-info'>{this.props.getTranslation('PortalToDos_New', 'New')}</span>);
            }
        }

        if (toDo.category) {
            elements.push(<span key={toDo.category} className='label label-default'>{toDo.category}</span>);
        }

        if (toDo.successPlan) {
            elements.push(<span key={toDo.successPlan} className='label label-default success-plan-label'>{toDo.successPlan}</span>);
        }

        if (toDo.owner) {
            if (toDo.isRequired) {
                ownerString = `${this.props.getTranslation('PortalToDos_AssignedBy', 'Assigned by')} `;
            }
            else {
                ownerString = `${this.props.getTranslation('PortalToDos_RequestedBy', 'Requested by')} `;
            }

            ownerString += toDo.owner;

            if (toDo.ownerTitle) {
                ownerString +=  ', ' + toDo.ownerTitle;
            }
        }

        elements.push(<em key='owner' className='small-block'>{ownerString}</em>);

        return elements;
    }

    buildStatusRow = toDo => {
        let completedCanceledDate = moment(toDo.completedCanceledDate),
            statusClasses,
            dateClasses,
            completedCancledDateString = '';

        if (!this.isToDoClosed(toDo)) {
            return;
        }

        statusClasses = classnames('label',
            {'label-primary': toDo.isMarkedAsComplete},
            {'label-success': toDo.isComplete},
            {'label-warning': toDo.isCanceled || toDo.isWaived});

        dateClasses = classnames('small-block',
            {'text-primary': toDo.isMarkedAsComplete},
            {'text-success': toDo.isComplete},
            {'text-warning': toDo.isCanceled || toDo.isWaived});


        if (completedCanceledDate.isValid()) {
            completedCancledDateString = completedCanceledDate.format('LL') + ` ${this.props.getTranslation('PortalToDos_At', 'at')} ` + completedCanceledDate.format('LT');
        }

        return (
            <div className='to-do-status'>
                <span className={statusClasses}>{toDo.statusReason}</span>
                <span className={dateClasses}>{completedCancledDateString}</span>
            </div>
        );
    }

    buildActions = toDo => {
        let actions = [],
            buttonClasses,
            dropDownClasses;

        if (toDo.isWaived) {
            return;
        }
        else if (toDo.isIncomplete) {
            actions.push({
                label: this.props.getTranslation('PortalToDos_MarkComplete', 'Mark Complete'),
                action: this.markComplete
            });

            if (!toDo.isRequired) {
                actions.push({
                    label: this.props.getTranslation('PortalToDos_CancelToDoTask', 'Cancel To-Do Task'),
                    action: this.cancelToDo
                });
            }
        }
        else if (toDo.isCanceled) {
            actions.push({
                label: this.props.getTranslation('PortalToDos_MarkComplete', 'Mark Complete'),
                action: this.markComplete
            }, {
                label: this.props.getTranslation('PortalToDos_MarkIncomplete', 'Mark Incomplete'),
                action: this.markIncomplete
            });
        }
        else if (toDo.isMarkedAsComplete) {
            actions.push({
                label: this.props.getTranslation('PortalToDos_MarkIncomplete', 'Mark Incomplete'),
                action: this.markIncomplete
            });
        }
        else if (toDo.isComplete && toDo.studentCanComplete) {
            actions.push({
                label: this.props.getTranslation('PortalToDos_MarkIncomplete', 'Mark Incomplete'),
                action: this.markIncomplete
            });
        }

        if (actions.length === 0) {
            return;
        }

        buttonClasses = classnames('btn',
            {'btn-success': toDo.isIncomplete},
            {'btn-default': this.isToDoClosed(toDo)});

        if (actions.length === 1) {
            return (
            <div className='btn-group full-width'>
                <a href='javascript:void(0)' className={buttonClasses} onClick={actions[0].action}>{actions[0].label}</a>
            </div>
            );
        }

        dropDownClasses = classnames('btn',
            {'btn-success': toDo.isIncomplete},
            {'btn-default': this.isToDoClosed(toDo)},
            'dropdown-toggle');

        return (
            <div className='btn-group full-width'>
                <a href='javascript:void(0)' className={buttonClasses} onClick={actions[0].action}>{actions[0].label}</a>
                <a href='javascript:void(0)' className={dropDownClasses} data-toggle='dropdown'><span className='caret'></span></a>
                <ul className='dropdown-menu'>
                    {actions.map((action, index) => {
                        if (index === 0) {
                            return;
                        }

                        return (<li key={action.label}><a href='javascript:void(0)' onClick={action.action}>{action.label}</a></li>);
                    })}
                </ul>
            </div>
        );
    }

    markIncomplete = e => {
        e.stopPropagation();
        this.props.setToDoStatus(this.props.toDo.id, uiStatuses.incomplete.pluginValue);
    }

    markComplete = e => {
        e.stopPropagation();
        this.props.openCloseToDoDialog('btn-success', this.props.getTranslation('PortalToDos_MarkComplete', 'Mark Complete'), comment => {
            this.props.setToDoStatus(this.props.toDo.id, uiStatuses.complete.pluginValue, comment);
        });
    }

    cancelToDo = e => {
        e.stopPropagation();
        this.props.openCloseToDoDialog('btn-primary',  this.props.getTranslation('PortalToDos_CancelToDoTask', 'Cancel To-Do Task'), comment => {
            this.props.setToDoStatus(this.props.toDo.id, uiStatuses.canceled.pluginValue, comment);
        });
    }

    isToDoClosed = toDo => {
        return toDo.isComplete || toDo.isMarkedAsComplete ||
            toDo.isCanceled || toDo.isWaived;
    }

    toggleOpenDescription = () => {
        this.openDescription = !this.openDescription;
    }

    render() {
        return(
            <div className={classnames('to-do-row-container', {clickable: this.descriptionOverflow})}
                onClick={this.descriptionOverflow && (() => this.toggleOpenDescription())}>
                <div className='row to-do-row'>
                    <div className='col-md-9 col-sm-12 col-xs-12 to-do-table-content'>
                        <div>
                            {this.buildDateBlock(this.props.toDo)}
                        </div>
                        <div className='to-do-content'>
                            <div>
                                <h3>{this.props.toDo.name}</h3>
                            </div>
                            <div>
                                <div>
                                    <div className={classnames('to-do-description', {'show-all': this.openDescription}, {'show-more': this.descriptionOverflow})}
                                        ref={c => this.description = c} dangerouslySetInnerHTML={{__html: this.props.toDo.description}}></div>
                                    {this.descriptionOverflow && !this.openDescription
                                     ? (<a href='javascript:void(0)'>Show More</a>)
                                     : ('')}
                                </div>
                            </div>
                            {this.buildCommentsRow(this.props.toDo)}
                            <div>
                                <div>
                                    {this.buildDetailsRow(this.props.toDo)}
                                </div>
                                {this.buildStatusRow(this.props.toDo)}
                            </div>
                        </div>
                    </div>
                    <div className='col-md-3 col-sm-12 col-xs-12 to-do-actions'>
                        {this.buildActions(this.props.toDo)}
                    </div>
                    <hr />
                </div>
                <hr />
            </div>
        );
    }
}

const mapDispatchToProps = dispatch => {
    return bindActionCreators({...toDos, ...translations}, dispatch);
};

export default connect(null, mapDispatchToProps)(ToDo);