import React from 'react';

import { defaultContactImage } from '../constants.js';

class Card extends React.Component {
    constructor(props, context) {
        super(props, context);
    }

    render() {
        const successNetwork = this.props.successNetwork;

        let pictureJsx = successNetwork.StaffPictureBase64
                         ? <img src={'data:image/png;base64,' + successNetwork.StaffPictureBase64} />
                         : <img src={defaultContactImage} />;

        return (
            <div>
                <div className='col-md-2 card hidden-sm hidden-xs' onClick={() => this.props.openContactDetails(this.props.id, successNetwork.StaffPictureBase64)}>
                    <div>
                        {pictureJsx}
                    </div>
                    <div className='full-name'><h4>{successNetwork.StaffMemberName}</h4></div>
                    <div>{successNetwork.StaffRoleName}</div>
                </div>
                <div className='col-sm-4 visible-sm visible-xs card small-card' onClick={() => this.props.openContactDetails(this.props.id, successNetwork.StaffPictureBase64)}>
                    <span>
                        {pictureJsx}
                        <span className='title'>
                            <span className='h4'>{successNetwork.StaffMemberName}</span>
                            <br />{successNetwork.StaffRoleName}
                        </span>
                    </span>
                </div>
            </div>);
    }
}

export default Card;