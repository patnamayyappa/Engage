import React from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { observable, computed } from 'mobx';
import { observer } from 'mobx-react';
import classnames from 'classnames';
import moment from 'moment';

import Dialog from './Dialog.jsx';

import * as translations from '../../reducers/translations.js';

import { defaultContactImage } from '../../constants.js';

@observer
class ContactDetails extends React.Component {
    @observable selectedTab;
    @observable selectedWeek;

    constructor(props, context) {
        super(props, context);

        this.selectedTab = 'bio';
        this.selectedWeek = '';
    }

    componentWillReceiveProps(nextProps) {
        if (nextProps.successNetwork && !this.props.successNetwork) {
            this.selectedWeek = '0';
        }
        else {
            this.selectedWeek = '';
        }
    }

    buildBody = () => {
        const successNetwork = this.props.successNetwork;

        if (!successNetwork) {
            return (<div></div>);
        }

        let staffImage = successNetwork.StaffPictureBase64
                         ? <img src={'data:image/png;base64,' + successNetwork.StaffPictureBase64} />
                         : <img src={defaultContactImage} />;

        return (
            <div className='contact-details'>
                <div className='contact-information'>
                    <div className=' hidden-xs'>
                        {staffImage}
                    </div>
                    <div className='visible-xs small-image'>
                        {staffImage}
                    </div>
                    <div>
                        <h3>{successNetwork.StaffMemberName}</h3>
                        <h5>{successNetwork.StaffRoleName}</h5>
                        <div className='icon-row first-row'><span><span className='glyphicon glyphicon-earphone'></span>{successNetwork.StaffPhoneNumber}</span></div>
                        <div className='icon-row text-primary'><span><span className='glyphicon glyphicon-envelope'></span>{successNetwork.StaffEmail}</span></div>
                    </div>
                </div>
                <div className='tab-container'>
                    <ul className='nav nav-tabs'>
                        <li className={classnames({'active': this.selectedTab === 'bio'})} onClick={() => this.selectTab('bio')}>
                            <a href="javascript:void(0)">{this.props.getTranslation('PortalSuccessNetwork_Bio', 'Bio')}</a>
                        </li>
                        <li className={classnames({'active': this.selectedTab === 'officeHours'})} onClick={() => this.selectTab('officeHours')}>
                            <a href="javascript:void(0)">{this.props.getTranslation('PortalSuccessNetwork_OfficeHours', 'Office Hours')}</a>
                        </li>
                    </ul>
                    <div className='tab-content'>
                        <div className={classnames(['tab-pane', 'fade', 'staff-bio', {'active in': this.selectedTab === 'bio'}])}>
                            <div className='col-md-12'>
                                <p>{this.buildBio(successNetwork.StaffBio || '')}</p>
                            </div>
                        </div>
                        <div className={classnames(['tab-pane', 'fade', 'staff-hours', {'active in': this.selectedTab === 'officeHours'}])}>
                            <div className='col-md-6 select-container'>
                                <label htmlFor='weeks'>{this.props.getTranslation('PortalSuccessNetwork_Week', 'Week')}</label>
                                <div className='select-control'>
                                    <select id='weeks' className='form-control'
                                        onChange={e => this.selectedWeek = e.target.value}
                                        value={this.selectedWeek}>
                                        {this.weekOptions.map(weekOption =>
                                            (<option key={weekOption.value} value={weekOption.value}>
                                                {weekOption.label}
                                            </option>))
                                        }
                                    </select>
                                </div>
                            </div>
                            <div className='col-md-12 hours-table-container'>
                                {this.buildOfficeHourTable(successNetwork.OfficeHours[this.selectedWeek])}
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        );
    }

    buildBio = (bio) => {
        let bioParts = bio.split('\n') || [],
            length = bioParts.length;

        return bio.split('\n').map((text, key) => <span key={key}>{text}{length !== (key + 1) ? <br /> : ''}</span>);
    }

    buildOfficeHourTable = officeHour => {
        let campusRow = [<div key='day'>&nbsp;</div>],
            locationRow = [<div key='locationSpace'>&nbsp;</div>],
            dayRows = [],
            startDate = moment.utc((officeHour && officeHour.StartDate) || 'invalid');

        if (!startDate.isValid()) {
            startDate = moment.utc(new Date()).subtract(new Date().getDay(), 'days');
        }

        for (let i = 0; i < 7; i++) {
            dayRows.push([<div key={i}>{startDate.add(i > 0 ? 1 : 0, 'days').format('dddd')}</div>]);
        }

        if (officeHour) {
            officeHour.Locations.forEach(location => {
                const key = `${location.Campus}${location.Location}`;
                campusRow.push(<div key={key} className='header-cell'>{location.Campus}</div>);
                locationRow.push(<div key={key}>{location.Location}</div>);

                for (let i = 0; i < 7; i++) {
                    let hours;
                    switch(i) {
                        case 0:
                            hours = location.Sunday;
                            break;
                        case 1:
                            hours = location.Monday;
                            break;
                        case 2:
                            hours = location.Tuesday;
                            break;
                        case 3:
                            hours = location.Wednesday;
                            break;
                        case 4:
                            hours = location.Thursday;
                            break;
                        case 5:
                            hours = location.Friday;
                            break;
                        case 6:
                            hours = location.Saturday;
                            break;
                    }
                    this.buildHoursColumn(hours, dayRows[i], i, key);
                }
            });
        }

        campusRow.push(<div key='spacer'>&nbsp;</div>);
        locationRow.push(<div key='spacer'>&nbsp;</div>);

        return (
            <div className='hours-table-inner-container'>
                <ul className='table'>
                    <li>
                        {campusRow}
                    </li>
                    <li>
                        {locationRow}
                    </li>
                    {dayRows.map((row, i) => {
                        row.push(<div key='spacer'>&nbsp;</div>);
                        return <li className={classnames({'sunday-row': i === 0})} key={row[0].key}>{row}</li>;
                    })}
                </ul>
            </div>
        );
    }

    buildHoursColumn = (hours, dayRow, day, campusLocationKey) => {
        const key = `${campusLocationKey}${day}`;

        if (!hours || !hours.length) {
            dayRow.push(<div key={key}>--</div>);
            return;
        }

        let formattedHours = {};
        hours.forEach(hour => {
            let today = moment().format('Y-MM-DD'),
                startTime = this.formatTime(today, hour.StartTime),
                endTime = this.formatTime(today, hour.EndTime);

            formattedHours[hour.StartTime + hours.EndTime] = `${startTime} - ${endTime}`;
        });

        dayRow.push(<div key={key}>{Object.keys(formattedHours).sort().map(key => formattedHours[key]).join(', ')}</div>);
    }

    formatTime = (today, time) => {
        return moment.utc(`${today}T${time}`).format('LT').toLowerCase().replace(/ |(:00)/gi, '');
    }

    buildFooter = () => {
        const successNetwork = this.props.successNetwork || {};

        let locationOptions = this.selectedWeek && successNetwork && successNetwork.OfficeHours
                              ? this.buildLocationOptions(successNetwork)
                              : null,
            hasLocationOptions = locationOptions && locationOptions.length;

        return (
            <div className='contact-details'>
                <div className='hidden-sm hidden-xs'>
                    <a href='javascript:void(0)' className='btn btn-default' onClick={this.toggleClose}>
                        {this.props.getTranslation('PortalSuccessNetwork_Cancel', 'Cancel')}
                    </a>
                    {successNetwork.StaffEmail
                    ? (<a href={`mailto:${successNetwork.StaffEmail}`} className='btn btn-primary'>
                        {this.props.getTranslation('PortalSuccessNetwork_SendMessage', 'Send Message')}
                        </a>)
                    : null}
                    <div className='btn-group schedule-appointment'>
                        <a href='javascript:void(0)' className={classnames(['btn btn-primary dropdown-toggle', {'hidden': !hasLocationOptions}])} data-toggle="dropdown">
                            {this.props.getTranslation('PortalSuccessNetwork_ScheduleAppointment', 'Schedule Appointment')}&nbsp;
                            <span className="caret"></span>
                        </a>
                        <ul className='dropdown-menu'>
                            {locationOptions}
                        </ul>
                    </div>
                </div>
                <div className='small-buttons visible-sm visible-xs'>
                    <div className='btn-group col-xs-12 full-width schedule-appointment'>
                        <a href='javascript:void(0)' className={classnames(['btn btn-primary dropdown-toggle', {'hidden': !hasLocationOptions}])} data-toggle="dropdown">
                            {this.props.getTranslation('PortalSuccessNetwork_ScheduleAppointment', 'Schedule Appointment')}&nbsp;
                            <span className="caret"></span>
                        </a>
                        <ul className='dropdown-menu'>
                            {locationOptions}
                        </ul>
                    </div>
                    {successNetwork.StaffEmail
                    ? (<a href={`mailto:${successNetwork.StaffEmail}`} className='btn btn-info col-xs-12'>
                        {this.props.getTranslation('PortalSuccessNetwork_SendMessage', 'Send Message')}
                        </a>)
                    : null}

                    <a href='javascript:void(0)' className='btn btn-default col-xs-12' onClick={this.toggleClose}>
                        {this.props.getTranslation('PortalSuccessNetwork_Cancel', 'Cancel')}
                    </a>
                </div>
            </div>
        );
    }

    buildLocationOptions = (successNetwork) => {
        const departmentId = successNetwork.StaffDepartmentId || '',
            userId = successNetwork.StaffMemberId || '';

        return successNetwork.StaffLocations.map(location => {
            const locationName = location.Name,
                locationId = location.Id || '';
            return (
                <li key={locationId}>
                    <a title={locationName} href={`/staff-calendar?locationId=${locationId}&departmentId=${departmentId}&userId=${userId}`}>{locationName}</a>
                </li>
            );
        });
    }

    selectTab = tab => {
        this.selectedTab = tab;
    }

    toggleClose = () => {
        this.selectedTab = 'bio';
        this.props.toggleClose();
    }

    @computed get weekOptions() {
        if (!this.props.successNetwork || !this.props.successNetwork.OfficeHours) {
            return [];
        }

        return this.props.successNetwork.OfficeHours.map((officeHour, index) => {
            let startDate = moment.utc(officeHour.StartDate || 'invalid'),
                endDate = moment.utc(officeHour.EndDate || 'invalid');

            return {
                value: index,
                label: `${this.getWeekOptionDateText(startDate)} - ${this.getWeekOptionDateText(endDate)}`
            };
        });
    }

    getWeekOptionDateText = date => {
        if (!date.isValid()) {
            // This should never happen except for edge cases and errors. It is not translated for that reason.
            return 'N/A';
        }

        let monthString = date.format('MMM');
        if (monthString.length > 3) {
            monthString = monthString.substring(0, 3);
        }

        return `${monthString} ${date.format('D')}`;
    }

    render() {
        return (
            <Dialog
                title={this.props.getTranslation('PortalSuccessNetwork_ContactDetails', 'Contact Details')}
                body={this.buildBody()}
                footer={this.buildFooter()}
                isOpen={this.props.isOpen}
                toggleClose={this.toggleClose}
                additionalBodyClass='contact-details-window' />
        );
    }
}

const mapDispatchToProps = dispatch => {
    return bindActionCreators({...translations}, dispatch);
};

export default connect(null, mapDispatchToProps)(ContactDetails);