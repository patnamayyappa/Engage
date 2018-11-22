import update from 'react-addons-update';

import { sortOrders } from '../constants.js';

const initialState = {
    isAuthenticated: false,
    initializedSuccessNetworks: false,
    groupedSuccessNetworks: {},
    sortValue: null,
    loadingActions: {},
    messages: [],
    openModalDialogs: 0,
};

const current = (state = initialState, action) => {
    switch(action.type) {
        case SET_IS_AUTHENTICATED:
            return update(state, {
                isAuthenticated: {
                    $set: action.isAuthenticated
                }
            });

        case SET_INITIALIZED_SUCCESS_NETWORKS:
            return update(state, {
                initializedSuccessNetworks: {
                    $set: action.initializedSuccessNetworks
                }
            });

        case SET_LOADING:
            return update(state, buildLoadingUpdateObject(action.isLoading, action.action, state.loadingActions));

        case SET_GROUPED_SUCCESS_NETWORKS:
            return update(state, {
                groupedSuccessNetworks: {
                    $set: action.groupedSuccessNetworks
                },
                sortValue: {
                    $set: action.sortValue
                }
            });

        case ADD_NEW_MESSAGE:
            return update(state, { messages: { $push: [action.message] } });

        case CLEAR_ALL_MESSAGES:
            return update(state, { messages: { $set: [] }});

        case SET_OPEN_MODAL_DIALOGS:
            return update(state, {
                openModalDialogs: {
                    $set: action.openModalDialogs
                }
            });

        default:
            return state;
    }
};

const buildLoadingUpdateObject = (isLoading, action, loadingActions) => {
    if (isLoading) {
        return {
            loadingActions: {
                [action]: {
                    $set: loadingActions[action] || 0 + 1
                }
            }
        };
    }
    else if (loadingActions[action]) {
        return {
            loadingActions: {
                [action]: {
                    $set: loadingActions[action] - 1
                }
            }
        };
    }
};

const SET_IS_AUTHENTICATED = 'SET_IS_AUTHENTICATED';
export const setIsAuthenticated = isAuthenticated => {
    return {
        type: SET_IS_AUTHENTICATED,
        isAuthenticated
    };
};

const SET_INITIALIZED_SUCCESS_NETWORKS = 'SET_INITIALIZED_SUCCESS_NETWORKS';
export const setInitializedSuccessNetworks = () => {
    return {
        type: SET_INITIALIZED_SUCCESS_NETWORKS,
        initializedSuccessNetworks: true
    };
};

const SET_LOADING = 'SET_LOADING';
export const setLoading = (action, isLoading) => {
    return {
        type: SET_LOADING,
        action,
        isLoading
    };
};

const ADD_NEW_MESSAGE = 'ADD_NEW_MESSAGE';
export const addNewMessage = (message) => {
    return {
        type: ADD_NEW_MESSAGE,
        message
    };
};

const CLEAR_ALL_MESSAGES = 'CLEAR_ALL_MESSAGES';
export const clearAllMessages = () => {
    return {
        type: CLEAR_ALL_MESSAGES
    };
};

const SET_OPEN_MODAL_DIALOGS = 'SET_OPEN_MODAL_DIALOGS';
const setOpenModalDialogs = (openModalDialogs) => {
    return {
        type: SET_OPEN_MODAL_DIALOGS,
        openModalDialogs
    };
};

const SET_GROUPED_SUCCESS_NETWORKS = 'SET_GROUPED_SUCCESS_NETWORKS';
const setGroupedSuccessNetworks = (groupedSuccessNetworks, sortValue) => {
    return {
        type: SET_GROUPED_SUCCESS_NETWORKS,
        groupedSuccessNetworks,
        sortValue
    };
};

export const toggleModalDialog = isOpen => {
    return (dispatch, getState) => {
        const openModalDialogs = getState().current.openModalDialogs;
        let newOpenModalDialogs = 0;

        if (isOpen) {
            newOpenModalDialogs = openModalDialogs + 1;
        }
        else if (openModalDialogs > 0) {
            newOpenModalDialogs = openModalDialogs - 1;
        }

        document.body.classList.toggle('modal-open', newOpenModalDialogs > 0);

        dispatch(setOpenModalDialogs(newOpenModalDialogs));
    };
};

export const sortSuccessNetworks = sortValue => {
    return (dispatch, getState) => {
        const successNetworks = getState().successNetwork.successNetworks;
        let groupedIds = {};

        switch(sortValue) {
            case sortOrders.title.value:
                Object.keys(successNetworks).forEach(successNetworkId => {
                    addToGroup(groupedIds, successNetworks[successNetworkId].StaffRoleName,
                        successNetworkId);
                });
                break;
            default:
                groupedIds['All'] = Object.keys(successNetworks);
                break;
        }

        Object.keys(groupedIds).forEach(group => {
            groupedIds[group].sort((id1, id2) => {
                const lastName1 = successNetworks[id1].StaffMemberLastName,
                    lastName2 = successNetworks[id2].StaffMemberLastName;

                if (!lastName1 && !lastName2) {
                    return 0;
                }
                else if (!lastName1) {
                    return 1;
                }
                else if (!lastName2) {
                    return -1;
                }
                else {
                    return lastName1.toLowerCase().localeCompare(
                        lastName2.toLowerCase());
                }
            });
        });

        dispatch(setGroupedSuccessNetworks(groupedIds, sortValue));
    };
};

const addToGroup = (groupedIds, group, id) => {
    // Set empty group name to an empty string instead of null or undefined
    group = group || '';
    if (!groupedIds[group]) {
        groupedIds[group] = [id];
    }
    else {
        groupedIds[group].push(id);
    }
};

export default current;