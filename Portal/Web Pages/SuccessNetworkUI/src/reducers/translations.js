import update from 'react-addons-update';

import {allTranslations} from '../constants.js';
import * as current from './current.js';

const initialState = {
    labelTranslations: null
};

export default (state = initialState, action) => {
    switch(action.type) {
        case SET_TRANSLATIONS:
            return update(state, {
                labelTranslations: {
                    $set: action.translations
                }
            });

        default:
            return state;
    }
};

const SET_TRANSLATIONS = 'SET_TRANSLATIONS';
const setTranslations = translations => {
    return {
        type: SET_TRANSLATIONS,
        translations
    };
};

export const retrieveTranslations = () => {
    return dispatch => {
        $.ajax(`/RetrieveMultiLingualValues?keys=${allTranslations.join('~|~')}&stopCache=${(+new Date())}`).then(translations => {
            dispatch(setTranslations(translations));
        })
        .fail(() => {
            // Initialize Translations so something appears
            dispatch(current.addNewMessage({
                default: 'Translations could not be retrieved. Default English Text will be used.'
            }));
            dispatch(setTranslations({ErrorOccurred: ''}));
        });
    };
};


export const getTranslation = (key, defaultText) => {
    return (dispatch, getState) => {
        const labelTranslations = getState().translations.labelTranslations || {};

        return labelTranslations[key] || defaultText;
    };
};