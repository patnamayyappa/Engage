import React from 'react';

import Card from './Card.jsx';

class CardGroup extends React.Component {
    constructor(props, context) {
        super(props, context);
    }

    render() {
        return (
            <div className='card-group'>
                {this.props.displayGroup
                    ? (
                        <div>
                            <div className='col-md-12 col-sm-12 col-xs-12 card-group-title'>
                                <h3>{this.props.name}</h3><hr />
                            </div>
                        </div>)
                    : ''}
                {this.props.successNetworkIds.map(successNetworkId =>
                    (<Card key={successNetworkId}
                        id={successNetworkId}
                        successNetwork={this.props.successNetworks[successNetworkId]}
                        openContactDetails={this.props.openContactDetails} /> ))}
            </div>
        );
    }
}

export default CardGroup;