import update from 'react-addons-update';

import * as current from './current.js';

import { sortOrders } from '../constants.js';

const initialState = {
    successNetworks: {},
    selectedSuccessNetwork: null
};

export default (state = initialState, action) => {
    switch(action.type) {
        case SET_SUCCESS_NETWORKS:
            return update(state, {
                successNetworks: {
                    $set: action.successNetworks
                }
            });

        case SET_SELECTED_SUCCESS_NETWORK:
            return update(state, {
                selectedSuccessNetwork: {
                    $set: action.selectedSuccessNetwork
                }
            });

        default:
            return state;
    }
};

const SET_SUCCESS_NETWORKS = 'SET_SUCCESS_NETWORKS';
const setSuccessNetworks = successNetworks => {
    return {
        type: SET_SUCCESS_NETWORKS,
        successNetworks
    };
};

const SET_SELECTED_SUCCESS_NETWORK = 'SET_SELECTED_SUCCESS_NETWORK';
const setSelectedSuccessNetwork = selectedSuccessNetwork => {
    return {
        type: SET_SELECTED_SUCCESS_NETWORK,
        selectedSuccessNetwork
    };
};

export const clearSelectedSuccessNetwork = () => {
    return {
        type: SET_SELECTED_SUCCESS_NETWORK,
        selectedSuccessNetwork: null
    };
};

export const retrieveSuccessNetwork = () => {
    return dispatch => {
        $.ajax(`retrieveStudentSuccessNetwork?stopCache=${(+new Date())}`)
            .then(successNetworks => {
                dispatch(setSuccessNetworks(successNetworks));
                dispatch(current.sortSuccessNetworks(sortOrders.lastName.value));
            })
            .fail(() => {
                dispatch(current.addNewMessage({
                    translationKey: 'PortalSuccessNetwork_RetrieveSuccessNetworkError',
                    default: 'An error occurred retrieving Success Networks. Please refresh the page and try again.'
                }));
            })
            .always(() => {
                dispatch(current.setLoading('retrieveSuccessNetwork', false));
                dispatch(current.setInitializedSuccessNetworks());
            });

        dispatch(current.setLoading('retrieveSuccessNetwork', true));
    };
};

// Pass in the picture to avoid making the server call larger
export const retrieveSelectedSuccessNetwork = (successNetworkId, staffPicture) => {
    return dispatch => {
        $.ajax(`retrieveStaffDetails?successNetworkId=${successNetworkId}&stopCache=${(+new Date())}`)
            .then(successNetwork => {
                if (!successNetwork) {
                    dispatch(current.addNewMessage({
                        translationKey: 'PortalSuccessNetwork_RetrieveSelectSuccessNetworkError',
                        default: 'An error occurred retrieving details for the selected Staff Member. Please try again.'
                    }));
                    return;
                }

                successNetwork.StaffPictureBase64 = staffPicture;
                dispatch(setSelectedSuccessNetwork(successNetwork));
            })
            .fail(() => {
                dispatch(current.addNewMessage({
                    translationKey: 'PortalSuccessNetwork_RetrieveSelectSuccessNetworkError',
                    default: 'An error occurred retrieving details for the selected Staff Member. Please try again.'
                }));
            })
            .always(() => {
                dispatch(current.setLoading('retrieveSelectedSuccessNetwork', false));
            });

        dispatch(current.setLoading('retrieveSelectedSuccessNetwork', true));
    };
};