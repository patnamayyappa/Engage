import React from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { observable } from 'mobx';
import { observer } from 'mobx-react';
import classnames from 'classnames';

import Tab from './Tab.jsx';
import CloseToDo from './Dialogs/CloseToDo.jsx';
import ErrorDialog from './Dialogs/ErrorDialog.jsx';

import * as current from '../reducers/current.js';
import * as toDos from '../reducers/toDos.js';
import * as translations from '../reducers/translations.js';

import {uiStatuses} from '../constants.js';

@observer
class MainLayout extends React.Component {
    @observable selectedTab;
    @observable closeToDoDialog;

    constructor(props, context) {
        super(props, context);

        this.closeToDoDialog = observable.map();
    }

    componentWillMount() {
        this.selectedTab = uiStatuses.incomplete.value;
        if (this.props.isAuthenticated) {
            this.props.retrieveAllToDos();
        }
        this.props.retrieveTranslations();
    }

    openCloseToDoDialog = (actionButtonClass, actionText, action) => {
        this.closeToDoDialog.set('actionButtonClass', actionButtonClass);
        this.closeToDoDialog.set('actionText', actionText);
        this.closeToDoDialog.set('action', comment => {
            if (action) {
                action(comment);
            }
            this.toggleDialog(this.closeToDoDialog);});

        this.toggleDialog(this.closeToDoDialog);
    }

    toggleDialog = (dialog) => {
        dialog.set('isOpen',!dialog.get('isOpen'));
    }

    dialogIsOpen = () => {
        return this.closeToDoDialog.get('open');
    }

    isInitialized = () => {
        return this.props.initializedTabs && this.props.initializedTranslations;
    }

    render() {
        return (
            <div className={classnames({'modal-open': this.dialogIsOpen})}>
                {this.isInitialized()
                    ? (<div>
                        <Tab tab={this.props.tabs[this.selectedTab]}
                            setSelectedTab={selectedTab => this.selectedTab = selectedTab}
                            selectedTab={this.selectedTab}
                            tabRecordCount={this.props.tabRecordCount}
                            openCloseToDoDialog={this.openCloseToDoDialog}
                            displayAccessError={!this.props.isAuthenticated} />
                        <CloseToDo isOpen={this.closeToDoDialog.get('isOpen')}
                            actionButtonClass={this.closeToDoDialog.get('actionButtonClass')}
                            actionText={this.closeToDoDialog.get('actionText')}
                            action={this.closeToDoDialog.get('action')}
                            toggleClose={() => this.toggleDialog(this.closeToDoDialog)} />
                        <ErrorDialog canOpen={this.isInitialized() && !this.props.isLoading} />
                       </div>)
                    : null}

                <div className={classnames('loading', {'hidden': this.isInitialized() && !this.props.isLoading})}>
                    <span className='spinner'>
                        <i></i><i></i><i></i><i></i><i></i><i></i><i></i><i></i><i></i><i></i><i></i><i></i>
                    </span>
                </div>
            </div>
        );
    }
}

const mapStateToProps = state => {
    return {
        tabs: state.current.tabs,
        initializedTabs: !state.current.isAuthenticated || state.current.initializedTabs,
        tabRecordCount: state.current.tabRecordCount,
        isLoading: Object.keys(state.current.loadingActions).some(key => state.current.loadingActions[key]),
        initializedTranslations: Object.keys(state.translations.labelTranslations).length,
        isAuthenticated: state.current.isAuthenticated
    };
};

const mapDispatchToProps = dispatch => {
    return bindActionCreators({...current, ...toDos, ...translations}, dispatch);
};

export default connect(mapStateToProps, mapDispatchToProps)(MainLayout);
