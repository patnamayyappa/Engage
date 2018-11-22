import React from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import classnames from 'classnames';

import ErrorDialog from './Dialogs/ErrorDialog.jsx';
import ContactDetails from './Dialogs/ContactDetails.jsx';

import * as current from '../reducers/current.js';
import * as translations from '../reducers/translations.js';
import * as successNetwork from '../reducers/successNetwork.js';

import { sortOrders } from '../constants.js';

import CardGroup from './CardGroup.jsx';

class MainLayout extends React.Component {
    constructor(props, context) {
        super(props, context);
    }

    componentWillMount() {
        if (this.props.isAuthenticated) {
            this.props.retrieveSuccessNetwork();
        }
        this.props.retrieveTranslations();
    }

    isInitialized = () => {
        return this.props.initializedSuccessNetworks && this.props.initializedTranslations;
    }

    openContactDetails = (successNetworkId, staffPicture) => {
        this.props.retrieveSelectedSuccessNetwork(successNetworkId, staffPicture);
    }

    closeContactDetails = () => {
        this.props.clearSelectedSuccessNetwork();
    }

    render() {
        return (
            <div className={classnames({'modal-open': !!this.props.selectedSuccessNetwork})}>
                {this.isInitialized()
                ? (<div>
                    <div className='container'>
                        <div className='row equal-height'>
                            <div className='col-md-7'>
                                <h1>{this.props.getTranslation('PortalSuccessNetwork_SuccessNetwork', 'Success Network')}</h1>
                            </div>
                            <div className='col-md-5 select-container'>
                                <label htmlFor='sortBy'>{this.props.getTranslation('PortalSuccessNetwork_SortBy', 'Sort By')}</label>
                                <div className='select-control'>
                                    <select id='sortBy' className='form-control'
                                        onChange={e => this.props.sortSuccessNetworks(e.target.value)}
                                        value={this.props.sortValue}>
                                        {Object.keys(sortOrders).map(sortOrderId =>{
                                            const sortOrder = sortOrders[sortOrderId];
                                            return (
                                                <option key={sortOrder.value} value={sortOrder.value}>
                                                    {this.props.getTranslation(sortOrder.translationKey, sortOrder.label)}
                                                </option>);
                                        })}
                                    </select>
                                </div>
                            </div>
                        </div>
                        <div className='row'>
                            {this.props.isAuthenticated
                                ? Object.keys(this.props.groupedSuccessNetworks).map(group =>
                                (<CardGroup key={group} name={group}
                                    displayGroup={this.props.sortValue !== sortOrders.lastName.value}
                                    successNetworkIds={this.props.groupedSuccessNetworks[group]}
                                    successNetworks={this.props.successNetworks}
                                    openContactDetails={this.openContactDetails} /> ))
                                : (<div className='card-group'>
                                    <div className="alert alert-block alert-danger">
                                        {this.props.getTranslation('PortalSuccessNetwork_AccessDeniedError', 'You don\'t have permissions to view these records.')}
                                    </div>
                                </div>)}
                        </div>
                    </div>
                    <ErrorDialog canOpen={this.isInitialized() && !this.props.isLoading} />
                    <ContactDetails isOpen={!!this.props.selectedSuccessNetwork}
                       toggleClose={this.closeContactDetails}
                       successNetwork={this.props.selectedSuccessNetwork} />
                </div>)
                : null }

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
        isAuthenticated: state.current.isAuthenticated,
        initializedSuccessNetworks: !state.current.isAuthenticated || state.current.initializedSuccessNetworks,
        successNetworks: state.successNetwork.successNetworks,
        groupedSuccessNetworks: state.current.groupedSuccessNetworks,
        initializedTranslations: !!state.translations.labelTranslations,
        sortValue: state.current.sortValue,
        isLoading: Object.keys(state.current.loadingActions).some(key => state.current.loadingActions[key]),
        selectedSuccessNetwork: state.successNetwork.selectedSuccessNetwork
    };
};

const mapDispatchToProps = dispatch => {
    return bindActionCreators({ ...current, ...translations, ...successNetwork }, dispatch);
};

export default connect(mapStateToProps, mapDispatchToProps)(MainLayout);
