import React from 'react';

import {uiStatuses} from '../constants.js';

class HeaderControls extends React.Component {
    constructor(props, context) {
        super(props, context);
    }

    render() {
        return (
            <div>
                <div className='col-md-5'>
                    {this.props.tabKey === uiStatuses.incomplete.value
                    ? (
                        <div className='checkbox'>
                            <label>
                                <input type='checkbox' checked={this.props.hideOptionalToDos} onChange={e => this.props.setHideOptionalToDos(this.props.tabKey, e.target.checked)} />{this.props.getTranslation('PortalToDos_HideOptionalToDos', 'Hide optional To-Dos')}
                            </label>
                        </div>)
                    : ''}
                </div>
                <div className='col-md-7 sort-by'>
                    <label htmlFor='sortBy'>{this.props.getTranslation('PortalToDos_SortBy', 'Sort By')}</label>
                    <select id='sortBy' className='form-control' onChange={e => this.props.sortTabToDos(this.props.tabKey, e.target.value)} value={this.props.sortOrder.value}>
                        {this.props.sortOrders.map(sortOrder =>
                            (<option key={sortOrder.value} value={sortOrder.value}>{this.props.getTranslation(sortOrder.translationKey, sortOrder.label)}</option>)
                        )}
                    </select>
                </div>
            </div>
        );
    }
}

export default HeaderControls;