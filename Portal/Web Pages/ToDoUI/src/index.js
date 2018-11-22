import './index.html';
import 'babel-polyfill';
import 'whatwg-fetch';
import React from 'react';
import { render } from 'react-dom';
import { Provider } from 'react-redux';
import { applyMiddleware, createStore, combineReducers } from 'redux';
import thunk from 'redux-thunk';

import MainLayout from './components/MainLayout.jsx';

import current from './reducers/current.js';
import {setIsAuthenticated} from './reducers/current.js';
import toDos from './reducers/toDos.js';
import translations from './reducers/translations.js';

const middlewares = [thunk];
if (process.env.NODE_ENV !== 'production') {
    const { logger } = require('redux-logger');
    middlewares.push(logger);
}

const store = createStore(
    combineReducers({
        current,
        toDos,
        translations
    }),
    applyMiddleware(...middlewares)
);

store.dispatch(setIsAuthenticated(StudentPortal && StudentPortal.ToDos && StudentPortal.ToDos.authenticated));

render(
    <Provider store={store}>

        <MainLayout />

    </Provider>
, document.getElementById('app'));