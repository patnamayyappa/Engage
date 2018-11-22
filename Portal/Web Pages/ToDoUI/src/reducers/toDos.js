import update from 'react-addons-update';
import RSVP from 'rsvp';

import * as current from './current.js';

import {statuscodes, uiStatuses} from '../constants.js';

const initialState = {
    incompleteToDos: {},
    completedToDos: {},
    waivedToDos: {}
};

export default (state = initialState, action) => {
    switch(action.type) {

        case SET_ALL_TO_DOS:
            return update(state, {
                incompleteToDos: {
                    $set: action.incompleteToDos
                },
                completedToDos: {
                    $set: action.completedToDos
                },
                waivedToDos: {
                    $set: action.waivedToDos
                }
            });

        case UPDATE_TO_DOS:
            return update(state, buildUpdateToDos(action.toDosByUiStatus));

        default:
            return state;
    }
};

const buildUpdateToDos = (toDosByUiStatus) => {
    let updates = {};

    Object.keys(toDosByUiStatus).forEach(uiStatus => {
        switch (uiStatus) {
            case uiStatuses.incomplete.value:
                updates.incompleteToDos = {
                    $set: toDosByUiStatus[uiStatus]
                };
                break;
            case uiStatuses.complete.value:
                updates.completedToDos = {
                    $set: toDosByUiStatus[uiStatus]
                };
                break;
            case uiStatuses.canceled.value:
                updates.waivedToDos = {
                    $set: toDosByUiStatus[uiStatus]
                };
                break;
        }
    });

    return updates;
};

const SET_ALL_TO_DOS = 'SET_ALL_TO_DOS';
const setAllToDos = (incompleteToDos, completedToDos, waivedToDos) => {
    return {
        type: SET_ALL_TO_DOS,
        incompleteToDos,
        completedToDos,
        waivedToDos
    };
};

const UPDATE_TO_DOS = 'UPDATE_TO_DOS';
const updateToDos = (toDosByUiStatus) => {
    return {
        type: UPDATE_TO_DOS,
        toDosByUiStatus
    };
};

export const retrieveAllToDos = () => {
    return dispatch => {
        RSVP.hash({
            incomplete: $.ajax(buildRetrieveStudentToDosUrl(statuscodes.incomplete)),
            complete: $.ajax(buildRetrieveStudentToDosUrl(statuscodes.complete)),
            canceled: $.ajax(buildRetrieveStudentToDosUrl(statuscodes.canceled))
        }).then(result => {
            dispatch(setAllToDos(result.incomplete || {}, result.complete || {}, result.canceled || {}));
            dispatch(current.initializeTabs(result.incomplete || {}, result.complete || {}, result.canceled || {}));
        })
        .catch(() => {
            dispatch(current.addNewMessage({
                translationKey: 'PortalToDos_LoadToDosError',
                default: 'An error occurred loading To Dos. Please try again.'
            }));
            dispatch(current.setInitializedTabs());
        });
    };
};

export const markToDosAsRead = (toDoIds) => {
    return () => {
        if (!toDoIds || !toDoIds.length) {
            return;
        }

        // Functionality will continue if this call fails. Do not alert the user.
        $.ajax(`mark-todos-as-read?toDoIds=${toDoIds.join('~|~')}&stopCache=${(+new Date())}`);
    };
};

export const setToDoStatus = (toDoId, status, comment) => {
    return (dispatch, getState) => {
        dispatch(current.setLoading('setToDoStatus', true));
        // Sanitize comments to be safe URI and Fetch Requests
        comment = comment && encodeURIComponent(
            comment.replace(/&/g, '&amp;')
               .replace(/</g, '&lt;')
               .replace(/>/g, '&gt;')
               .replace(/"/g, '&quot;')
               .replace(/'/g, '\\\'')
               );
        $.ajax(`update-todo-status?toDoId=${toDoId}&status=${status}&closeComment=${comment}&stopCache=${(+new Date())}`)
            .then((result) => {
                let oldStatusCode,
                    refreshStatusCode;

                if (!result) {
                    dispatch(current.addNewMessage({
                        translationKey: 'PortalToDos_UpdateToDoStatusError',
                        default: 'An error occurred updating the Status of the To Do. Please try again or refresh the page.'
                    }));
                    return;
                }

                const state = getState().toDos;

                if (state.incompleteToDos[toDoId]) {
                    oldStatusCode = statuscodes.incomplete;
                }
                else if (state.completedToDos[toDoId]) {
                    oldStatusCode = statuscodes.complete;
                }
                else {
                    oldStatusCode = statuscodes.canceled;
                }

                switch(status) {
                    case uiStatuses.incomplete.pluginValue:
                        refreshStatusCode = statuscodes.incomplete;
                        break;
                    case uiStatuses.complete.pluginValue:
                        refreshStatusCode = statuscodes.complete;
                        break;
                    case uiStatuses.canceled.pluginValue:
                        refreshStatusCode = statuscodes.canceled;
                        break;
                    default:
                        refreshStatusCode = null;
                }

                dispatch(refreshToDos([oldStatusCode, refreshStatusCode]));

            })
            .fail(() => {
                dispatch(current.addNewMessage({
                    translationKey: 'PortalToDos_UpdateToDoStatusError',
                    default: 'An error occurred updating the Status of the To Do. Please try again.'
                }));
            })
            .always(() => {
                dispatch(current.setLoading('setToDoStatus', false));
            });
    };
};

export const refreshToDos = refreshStatusCodes => {
    return dispatch => {
        let promises = {};
        refreshStatusCodes.forEach(statuscode => {
            let uiStatus;

            switch (statuscode) {
                case statuscodes.incomplete:
                    uiStatus = uiStatuses.incomplete.value;
                    break;
                case statuscodes.complete:
                    uiStatus = uiStatuses.complete.value;
                    break;
                default:
                    uiStatus = uiStatuses.canceled.value;
                    break;
            }

            promises[uiStatus] = $.ajax(buildRetrieveStudentToDosUrl(statuscode));
        });

        dispatch(current.setLoading('refreshToDos', true));

        RSVP.hash(promises).then(result => {
            dispatch(updateToDos(result));
            dispatch(current.updateTabToDos(result));
        })
        .catch(() => {
            dispatch(current.addNewMessage({
                translationKey: 'PortalToDos_RefreshToDosError',
                default: 'An error occurred refreshing To Dos. Please refresh the page to see the latest data.'
            }));
        })
        .finally(() => {
            dispatch(current.setLoading('refreshToDos', false));
        });
    };
};

const buildRetrieveStudentToDosUrl = statuscode => {
    return `retrievestudentstodos?statusCode=${statuscode}&stopCache=${(+new Date())}`;
};