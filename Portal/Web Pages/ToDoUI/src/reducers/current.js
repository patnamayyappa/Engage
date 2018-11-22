import update from 'react-addons-update';
import moment from 'moment';

import {sortOrders, uiStatuses, newestCategories} from '../constants.js';

import * as toDos from './toDos.js';

const defaultSortOrders = [
    sortOrders.dueDate,
    sortOrders.category,
    sortOrders.successPlan
];

const initialState = {
    isAuthenticated: false,
    initializedTabs: false,
    tabs: {
        [uiStatuses.incomplete.value]: {
            toDos: {},
            groupedAndSortedIds: {},
            sortOrder: '',
            sortOrders: [],
            hideOptionalToDos: false,
            key: uiStatuses.incomplete.value
        },
        [uiStatuses.complete.value]: {
            toDos: {},
            groupedAndSortedIds: {},
            sortOrder: '',
            sortOrders: defaultSortOrders,
            hideOptionalToDos: false,
            key: uiStatuses.complete.value
        },
        [uiStatuses.canceled.value]: {
            toDos: {},
            groupedAndSortedIds: {},
            sortOrder: '',
            sortOrders: defaultSortOrders,
            hideOptionalToDos: false,
            key: uiStatuses.canceled.value
        }
    },
    tabRecordCount : {
        [uiStatuses.incomplete.value]: 0,
        [uiStatuses.complete.value]: 0,
        [uiStatuses.canceled.value]: 0
    },
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

        case SET_INITIALIZED_TABS:
            return update(state, {
                initializedTabs: {
                    $set: true
                }
            });

        case SET_TABS_INITIAL:
            return update(state, {
                initializedTabs: {
                    $set: true
                },
                tabs: {
                    [uiStatuses.incomplete.value]: {
                        toDos: {
                            $set: action.incompleteToDoProperties.toDos
                        },
                        groupedAndSortedIds: {
                            $set: action.incompleteToDoProperties.groupedAndSortedIds
                        },
                        sortOrders: {
                            $set: action.incompleteToDoProperties.sortOrders
                        },
                        sortOrder: {
                            $set: action.incompleteToDoProperties.sortOrder
                        }
                    },
                    [uiStatuses.complete.value]: {
                        toDos: {
                            $set: action.completeToDoProperties.toDos
                        },
                        groupedAndSortedIds: {
                            $set: action.completeToDoProperties.groupedAndSortedIds
                        },
                        sortOrder: {
                            $set: action.completeToDoProperties.sortOrder
                        }
                    },
                    [uiStatuses.canceled.value]: {
                        toDos: {
                            $set: action.canceledToDoProperties.toDos
                        },
                        groupedAndSortedIds: {
                            $set: action.canceledToDoProperties.groupedAndSortedIds
                        },
                        sortOrder: {
                            $set: action.canceledToDoProperties.sortOrder
                        }
                    }
                },
                tabRecordCount: {
                    [uiStatuses.incomplete.value]: {
                        $set: Object.keys(action.incompleteToDoProperties.toDos).length
                    },
                    [uiStatuses.complete.value]: {
                        $set: Object.keys(action.completeToDoProperties.toDos).length
                    },
                    [uiStatuses.canceled.value]: {
                        $set: Object.keys(action.canceledToDoProperties.toDos).length
                    }
                }
            });

        case SET_TABS_UPDATE:
            return update(state, buildTabsUpdate(action.tabUpdates));

        case SET_SORTED_TO_DOS:
            return update(state, {
                tabs: {
                    [action.tabKey]: {
                        groupedAndSortedIds: {
                            $set: action.sortedToDos
                        },
                        sortOrder: {
                            $set: action.sortOrder
                        }
                    }
                }
            });

        case SET_HIDE_OPTIONAL_TO_DOS:
            return update(state, {
                tabs: {
                    [action.tabKey]: {
                        hideOptionalToDos:  {
                            $set: action.hideOptionalToDos
                        }
                    }
                }
            });

        case SET_LOADING:
            return update(state, buildLoadingUpdateObject(action.isLoading, action.action, state.loadingActions));

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

const buildTabsUpdate = (updates) => {
    let tabUpdate = {
        tabs: {},
        tabRecordCount: {}
    };

    Object.keys(updates).forEach(uiStatus => {
        tabUpdate.tabs[uiStatus] = {
            $merge: updates[uiStatus]
        };

        tabUpdate.tabRecordCount[uiStatus] = {
            $set:Object.keys(updates[uiStatus].toDos).length
        };
    });

    return tabUpdate;
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
export const setIsAuthenticated = (isAuthenticated) => {
    return {
        type: SET_IS_AUTHENTICATED,
        isAuthenticated
    };
};

const SET_TABS_INITIAL = 'SET_TABS_INITIAL';
const setTabsInitial = (incompleteToDoProperties, completeToDoProperties, canceledToDoProperties) => {
    return {
        type: SET_TABS_INITIAL,
        incompleteToDoProperties,
        completeToDoProperties,
        canceledToDoProperties
    };
};

const SET_INITIALIZED_TABS = 'SET_INITIALIZED_TABS';
export const setInitializedTabs = () => {
    return {
        type: SET_INITIALIZED_TABS
    };
};

const SET_TABS_UPDATE = 'SET_TABS_UPDATE';
const setTabsUpdate = (tabUpdates) => {
    return {
        type: SET_TABS_UPDATE,
        tabUpdates
    };
};

const SET_SORTED_TO_DOS = 'SET_SORTED_TO_DOS';
const setSortedToDos = (tabKey, sortedToDos, sortOrder) => {
    return {
        type: SET_SORTED_TO_DOS,
        tabKey,
        sortedToDos,
        sortOrder
    };
};

const SET_HIDE_OPTIONAL_TO_DOS = 'SET_HIDE_OPTIONAL_TO_DOS';
export const setHideOptionalToDos = (tabKey, hideOptionalToDos) => {
    return {
        type: SET_HIDE_OPTIONAL_TO_DOS,
        tabKey,
        hideOptionalToDos
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

export const initializeTabs = (incompleteToDos, completeToDos, canceledToDos) => {
    return (dispatch, getState) => {
        const tabs = getState().current.tabs;

        let incompleteToDoProperties = setIncompleteToDoProperties(incompleteToDos,
            tabs[uiStatuses.incomplete.value], dispatch);

        let completeToDoProperties = setToDoProperties(completeToDos,
            uiStatuses.complete.value, sortOrders.dueDate, tabs[uiStatuses.complete.value]);

        let canceledToDoProperties = setToDoProperties(canceledToDos, uiStatuses.canceled.value,
            sortOrders.dueDate, tabs[uiStatuses.canceled.value]);

        dispatch(setTabsInitial(incompleteToDoProperties, completeToDoProperties,
            canceledToDoProperties));
    };
};

export const updateTabToDos = (toDosByUIStatus) => {
    return (dispatch, getState) => {
        const state = getState().current;
        let updates = {};

        Object.keys(toDosByUIStatus).forEach(uiStatus => {
            const tab = state.tabs[uiStatus];
            switch (uiStatus) {
                case uiStatuses.incomplete.value:
                    updates[uiStatuses.incomplete.value] = setIncompleteToDoProperties(
                        toDosByUIStatus[uiStatus], tab, dispatch);
                    break;
                default:
                    updates[uiStatus] = setToDoProperties(toDosByUIStatus[uiStatus], uiStatus,
                        sortOrders.dueDate, tab);
            }
        });

        dispatch(setTabsUpdate(updates));
    };
};

export const sortTabToDos = (tabKey, sortOrderValue) => {
    return (dispatch, getState) => {
        const tab = getState().current.tabs[tabKey],
            sortOrder = sortOrders[sortOrderValue];

        let groupedAndSortedIds = sortToDos(tab.toDos, sortOrder.value, tabKey);

        dispatch(setSortedToDos(tabKey, groupedAndSortedIds, sortOrder));
    };
};

const setIncompleteToDoProperties = (incompleteToDos, tab, dispatch) => {
    let incompleteToDoProperties = setToDoProperties(incompleteToDos,
            uiStatuses.incomplete.value, sortOrders.newest, tab);

    if (incompleteToDoProperties.sortOrder.value === sortOrders.newest.value) {
        if (incompleteToDoProperties.groupedAndSortedIds[newestCategories.isNew] && incompleteToDoProperties.groupedAndSortedIds[newestCategories.isNew].length) {
            dispatch(toDos.markToDosAsRead(incompleteToDoProperties.groupedAndSortedIds[newestCategories.isUnread]));
            delete incompleteToDoProperties.groupedAndSortedIds[newestCategories.isUnread];
            incompleteToDoProperties.sortOrders = [
                sortOrders.newest,
                sortOrders.dueDate,
                sortOrders.category,
                sortOrders.successPlan
            ];
        }
        else {
            incompleteToDoProperties.sortOrder = sortOrders.dueDate;
            incompleteToDoProperties.sortOrders = defaultSortOrders;
        }
    }

    return incompleteToDoProperties;
};

const setToDoProperties = (toDos, uiStatus, defaultSortOrder, state) => {
    const sortOrder = state.sortOrder || defaultSortOrder;
    return {
        toDos: toDos,
        sortOrder: sortOrder,
        groupedAndSortedIds: sortToDos(toDos, sortOrder.value, uiStatus, true)
    };
};

const sortToDos = (toDos, sortOrder, status, getUnread) => {
    let groupedIds = {};
    switch (sortOrder) {
        case sortOrders.newest.value:
            Object.keys(toDos).forEach(id => {
                if (toDos[id].isNew) {
                    addToGroup(groupedIds, newestCategories.isNew, id);

                    if (getUnread && toDos[id].isUnread) {
                        addToGroup(groupedIds, newestCategories.isUnread, id);
                    }
                }
                else {
                    addToGroup(groupedIds, newestCategories.isExisting, id);
                }
            });
            break;
        case sortOrders.category.value:
            Object.keys(toDos).forEach(id => {
                addToGroup(groupedIds, toDos[id].category, id);
            });
            break;
        case sortOrders.successPlan.value:
            Object.keys(toDos).forEach(id => {
                addToGroup(groupedIds, toDos[id].successPlan, id);
            });
            break;
        default:
            groupedIds['All'] = Object.keys(toDos);
            break;
    }

    Object.keys(groupedIds).forEach(group => {
        if (group === newestCategories.isUnread) {
            return;
        }

        groupedIds[group].sort((id1, id2) => {
            // If Date is null, act as though it is invalid for sorting purposes
            let date1 = moment(toDos[id1].dueDate || 'invalid'),
                date2 = moment(toDos[id2].dueDate || 'invalid');

            if (!date1.isValid() && !date2.isValid()) {
                return 0;
            }

            if (status === uiStatuses.incomplete.value) {
                return date1.diff(date2);
            }
            else {
                return date2.diff(date1);
            }
        });
    });

    return groupedIds;
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