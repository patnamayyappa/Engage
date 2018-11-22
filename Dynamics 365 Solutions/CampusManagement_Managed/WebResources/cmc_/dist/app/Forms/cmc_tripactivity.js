/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
var CampusManagement;
(function (CampusManagement) {
    var cmc_tripactivity;
    (function (cmc_tripactivity) {
        var disableTripActivityonTripStatus = CampusManagement.utilities.disableTripActivityonTripStatus;
        function onLoad(executionContext) {
            getTripDetailsSetPrimaryStaff(executionContext);
            setTripActivityNameReadOnly(executionContext);
            if (Xrm.Page.getAttribute("cmc_activitytype")) {
                Xrm.Page.getAttribute("cmc_activitytype").addOnChange(CampusManagement.cmc_tripactivity.tripActivityTypeChange);
            }
            if (Xrm.Page.ui.getFormType() === 1 /* Create */) {
                if (Xrm.Page.getControl('cmc_linkedtoevent')) {
                    var eventControl = Xrm.Page.getControl('cmc_linkedtoevent');
                    eventControl.setVisible(false);
                }
                var appointmentControl = Xrm.Page.getControl('cmc_linkedtoappointment');
                appointmentControl.setVisible(true);
            }
            if (Xrm.Page.ui.getFormType() === 2 /* Update */) {
                var tripActivityType = Xrm.Page.getAttribute("cmc_activitytype");
                var appointmentControl = Xrm.Page.getControl('cmc_linkedtoappointment');
                if (tripActivityType.getSelectedOption().value === 175490000 /* Appointment */) {
                    if (Xrm.Page.getControl('cmc_linkedtoevent')) {
                        var eventControl = Xrm.Page.getControl('cmc_linkedtoevent');
                        eventControl.setVisible(false);
                    }
                    appointmentControl.setVisible(true);
                }
                else if (tripActivityType.getSelectedOption().value === 175490001 /* Event */) {
                    if (Xrm.Page.getControl('cmc_linkedtoevent')) {
                        var eventControl = Xrm.Page.getControl('cmc_linkedtoevent');
                        eventControl.setVisible(true);
                    }
                    appointmentControl.setVisible(false);
                }
                else if (tripActivityType.getSelectedOption().value === 175490002 /* Other */) {
                    if (Xrm.Page.getControl('cmc_linkedtoevent')) {
                        var eventControl = Xrm.Page.getControl('cmc_linkedtoevent');
                        eventControl.setVisible(false);
                    }
                    appointmentControl.setVisible(false);
                    setFieldsToEnable();
                }
            }
            if (Xrm.Page.getAttribute("cmc_linkedtoevent")) {
                Xrm.Page.getAttribute("cmc_linkedtoevent").addOnChange(CampusManagement.cmc_tripactivity.eventSelectionChange);
            }
            // on update scenario
            if (Xrm.Page.ui.getFormType() === 2 /* Update */) {
                var trip = Xrm.Page.getAttribute("cmc_trip").getValue();
                if (trip != null) {
                    var tripId = SonomaCmc.Guid.decapsulate(trip[0].id);
                    // disable the trip activity based on the trip status
                    disableTripActivityonTripStatus(executionContext, tripId);
                }
            }
        }
        cmc_tripactivity.onLoad = onLoad;
        function onSave(executionContext) {
            if (Xrm.Page.getAttribute("cmc_startdatetime").getIsDirty() || Xrm.Page.getAttribute("cmc_enddatetime").getIsDirty()) {
                var trip = Xrm.Page.getAttribute("cmc_trip").getValue();
                var tripId = trip[0].id.replace('{', '').replace('}', '');
                var tripActivityStartDate = Date.parse(Xrm.Page.getAttribute("cmc_startdatetime").getValue());
                var tripActivityEndDate = Date.parse(Xrm.Page.getAttribute("cmc_enddatetime").getValue());
                if (!(((tripStartDate <= tripActivityStartDate) && (tripEndDate >= tripActivityStartDate)) && ((tripStartDate <= tripActivityEndDate) && (tripEndDate >= tripActivityEndDate)))) {
                    CampusManagement.CommonUiControls.openAlertDialog(CampusManagement.localization.getResourceString("TravelMgmt_TripActivityStart_EndDateOutOfTripStart_EndDate"), CampusManagement.localization.getResourceString("OkButton"));
                }
            }
        }
        cmc_tripactivity.onSave = onSave;
        function getVersion() {
            return Xrm.Page.context.getVersion().split('.').slice(0, 2).join('.');
        }
        function getTripDetailsSetPrimaryStaff(executionContext) {
            var trip = Xrm.Page.getAttribute("cmc_trip").getValue();
            if (trip != null) {
                var tripId = trip[0].id.replace('{', '').replace('}', '');
                var fetchXml = [
                    '<fetch>',
                    '<entity name="systemuser" >',
                    '<attribute name="fullname"/>',
                    '<attribute name="systemuserid"/>',
                    '<link-entity name="cmc_trip" alias="ab" link-type="inner" to="systemuserid" from="owninguser">',
                    '<attribute name="cmc_enddate"/>',
                    '<attribute name="cmc_startdate"/>',
                    '<attribute name="owninguser" />',
                    '<filter type="and" >',
                    '<condition attribute="cmc_tripid" value = "' + tripId + '" uitype = "cmc_trip"  operator = "eq" />',
                    '</filter>',
                    '</link-entity>',
                    '</entity>',
                    '</fetch>'
                ].join('');
                Xrm.WebApi.retrieveMultipleRecords("systemuser", '?fetchXml=' + fetchXml)
                    .then(function success(result) {
                    if (result.entities.length > 0) {
                        tripStartDate = Date.parse(result.entities[0]["ab.cmc_startdate"]);
                        tripEndDate = Date.parse(result.entities[0]["ab.cmc_enddate"]);
                    }
                }, function (error) {
                    console.log(error);
                });
            }
        }
        function setTripActivityNameReadOnly(executionContext) {
            var tripActivityName;
            var tripActivityType;
            tripActivityName = Xrm.Page.ui.controls.get("cmc_name");
            tripActivityType = Xrm.Page.getAttribute("cmc_activitytype");
            if (Xrm.Page.ui.getFormType() === 1 /* Create */) {
                tripActivityName.setDisabled(true);
            }
            else if (Xrm.Page.ui.getFormType() !== 1 /* Create */) {
                if (tripActivityType.getSelectedOption().value === 175490002 /* Other */) {
                    tripActivityName.setDisabled(false);
                }
                else {
                    tripActivityName.setDisabled(true);
                }
            }
        }
        //used onchange of the appoint in trip activity
        function appointmentSelectionChange(executionContext, evtargs) {
            var selectedAppointment = Xrm.Page.getAttribute("cmc_linkedtoappointment").getValue();
            if (selectedAppointment != null) {
                {
                    var selectedAppointmentId = selectedAppointment[0].id.replace('{', '').replace('}', '');
                    if (selectedAppointmentId != null) {
                        var fetchXml = [
                            "<fetch top='50'>",
                            "  <entity name='appointment'>",
                            "    <attribute name='scheduledstart' />",
                            "    <attribute name='location' />",
                            "    <attribute name='scheduledend' />",
                            "    <filter>",
                            "      <condition attribute='activityid' operator='eq' value='", selectedAppointmentId, "'/>",
                            "    </filter>",
                            "  </entity>",
                            "</fetch>"
                        ].join("");
                        Xrm.WebApi.retrieveMultipleRecords("appointment", '?fetchXml=' + fetchXml)
                            .then(function success(result) {
                            console.log("Result of the appointment on change event is " + result);
                            if (result.entities.length > 0) {
                                var appointment = result.entities[0];
                                if (appointment != null) { // to be safe
                                    if (appointment.scheduledstart != undefined)
                                        Xrm.Page.getAttribute("cmc_startdatetime").setValue(Date.parse(appointment.scheduledstart));
                                    if (appointment.scheduledend != undefined)
                                        Xrm.Page.getAttribute("cmc_enddatetime").setValue(Date.parse(appointment.scheduledend));
                                    if (appointment.location != undefined)
                                        Xrm.Page.getAttribute("cmc_location").setValue(appointment.location);
                                }
                            }
                        }, function (error) {
                            console.log(error);
                        });
                    }
                }
            }
        }
        cmc_tripactivity.appointmentSelectionChange = appointmentSelectionChange;
        function tripActivityTypeChange(executionContext) {
            var tripActivityName;
            var tripActivityType;
            tripActivityName = Xrm.Page.ui.controls.get("cmc_name");
            tripActivityType = Xrm.Page.getAttribute("cmc_activitytype");
            if (tripActivityType.getSelectedOption().value === 175490002 /* Other */) {
                tripActivityName.setDisabled(false);
                if (Xrm.Page.getAttribute("cmc_linkedtoappointment")) {
                    Xrm.Page.getAttribute("cmc_linkedtoappointment").setValue(null);
                }
                if (Xrm.Page.getAttribute("cmc_linkedtoevent")) {
                    Xrm.Page.getAttribute("cmc_linkedtoevent").setValue(null);
                }
                if (Xrm.Page.getControl('cmc_linkedtoevent')) {
                    var linkedtoeventControl = Xrm.Page.getControl('cmc_linkedtoevent');
                    linkedtoeventControl.setVisible(false);
                    ;
                }
                if (Xrm.Page.getControl('cmc_linkedtoappointment')) {
                    var linkedtoappointmentControl = Xrm.Page.getControl('cmc_linkedtoappointment');
                    linkedtoappointmentControl.setVisible(false);
                }
                if (Xrm.Page.getAttribute("cmc_startdatetime")) {
                    Xrm.Page.getAttribute("cmc_startdatetime").setValue("");
                    var cmcStartDateTime = Xrm.Page.ui.controls.get("cmc_startdatetime");
                    cmcStartDateTime.setDisabled(false);
                }
                if (Xrm.Page.getAttribute("cmc_enddatetime")) {
                    Xrm.Page.getAttribute("cmc_enddatetime").setValue("");
                    var cmcEndDateTime = Xrm.Page.ui.controls.get("cmc_enddatetime");
                    cmcEndDateTime.setDisabled(false);
                }
                if (Xrm.Page.getAttribute("cmc_location")) {
                    Xrm.Page.getAttribute("cmc_location").setValue("");
                    var cmcLocation = Xrm.Page.ui.controls.get("cmc_location");
                    cmcLocation.setDisabled(false);
                }
            }
            else if (tripActivityType.getSelectedOption().value === 175490000 /* Appointment */) {
                tripActivityName.setDisabled(true);
                if (Xrm.Page.getAttribute("cmc_linkedtoappointment")) {
                    Xrm.Page.getAttribute("cmc_linkedtoappointment").setValue(null);
                }
                if (Xrm.Page.getControl('cmc_linkedtoevent')) {
                    var linkedtoeventControl = Xrm.Page.getControl('cmc_linkedtoevent');
                    linkedtoeventControl.setVisible(false);
                    ;
                }
                if (Xrm.Page.getControl('cmc_linkedtoappointment')) {
                    var linkedtoappointmentControl = Xrm.Page.getControl('cmc_linkedtoappointment');
                    linkedtoappointmentControl.setVisible(true);
                }
                if (Xrm.Page.getAttribute("cmc_startdatetime")) {
                    var cmcStartDateTime = Xrm.Page.ui.controls.get("cmc_startdatetime");
                    cmcStartDateTime.setDisabled(true);
                }
                if (Xrm.Page.getAttribute("cmc_enddatetime")) {
                    var cmcEndDateTime = Xrm.Page.ui.controls.get("cmc_enddatetime");
                    cmcEndDateTime.setDisabled(true);
                }
                if (Xrm.Page.getAttribute("cmc_location")) {
                    var cmcLocation = Xrm.Page.ui.controls.get("cmc_location");
                    cmcLocation.setDisabled(true);
                }
            }
            else if (tripActivityType.getSelectedOption().value === 175490001 /* Event */) {
                tripActivityName.setDisabled(true);
                if (Xrm.Page.getAttribute("cmc_linkedtoevent")) {
                    Xrm.Page.getAttribute("cmc_linkedtoevent").setValue(null);
                }
                if (Xrm.Page.getControl('cmc_linkedtoevent')) {
                    var linkedtoeventControl = Xrm.Page.getControl('cmc_linkedtoevent');
                    linkedtoeventControl.setVisible(true);
                    ;
                }
                if (Xrm.Page.getControl('cmc_linkedtoappointment')) {
                    var linkedtoappointmentControl = Xrm.Page.getControl('cmc_linkedtoappointment');
                    linkedtoappointmentControl.setVisible(false);
                }
                if (Xrm.Page.getAttribute("cmc_startdatetime")) {
                    var cmcStartDateTime = Xrm.Page.ui.controls.get("cmc_startdatetime");
                    cmcStartDateTime.setDisabled(true);
                }
                if (Xrm.Page.getAttribute("cmc_enddatetime")) {
                    var cmcEndDateTime = Xrm.Page.ui.controls.get("cmc_enddatetime");
                    cmcEndDateTime.setDisabled(true);
                }
                if (Xrm.Page.getAttribute("cmc_location")) {
                    var cmcLocation = Xrm.Page.ui.controls.get("cmc_location");
                    cmcLocation.setDisabled(true);
                }
            }
        }
        cmc_tripactivity.tripActivityTypeChange = tripActivityTypeChange;
        function eventSelectionChange(executionContext) {
            var selectedEvent;
            if (Xrm.Page.getAttribute("cmc_linkedtoevent")) {
                selectedEvent = Xrm.Page.getAttribute("cmc_linkedtoevent").getValue();
            }
            console.log(selectedEvent);
            if (selectedEvent != null) {
                {
                    var selectedEventId = selectedEvent[0].id.replace('{', '').replace('}', '');
                    if (selectedEventId != null) {
                        var fetchXml = [
                            "<fetch top='50'>",
                            "  <entity name='msevtmgt_event'>",
                            "    <attribute name='msevtmgt_eventstartdate' />",
                            "    <attribute name='msevtmgt_eventenddate' />",
                            "    <filter type='and'>",
                            "      <condition attribute='msevtmgt_eventid' operator='eq' value='", selectedEventId, "'/>",
                            "    </filter>",
                            "    <link-entity name='msevtmgt_venue' from='msevtmgt_venueid' to='msevtmgt_primaryvenue' alias='venue'>",
                            "      <attribute name='msevtmgt_country' />",
                            "      <attribute name='msevtmgt_city' />",
                            "      <attribute name='msevtmgt_addressline1' />",
                            "      <attribute name='msevtmgt_addressline3' />",
                            "      <attribute name='msevtmgt_addressline2' />",
                            "      <attribute name='msevtmgt_venueid' />",
                            "      <attribute name='msevtmgt_stateprovince' />",
                            "      <attribute name='msevtmgt_postalcode' />",
                            "    </link-entity>",
                            "  </entity>",
                            "</fetch>",
                        ].join("");
                        Xrm.WebApi.retrieveMultipleRecords("msevtmgt_event", '?fetchXml=' + fetchXml)
                            .then(function success(result) {
                            console.log("Result of the event on change event is " + result);
                            if (result.entities.length > 0) {
                                var eventDetails = result.entities[0];
                                if (eventDetails != null) { // to be safe
                                    if (eventDetails.msevtmgt_eventstartdate != undefined)
                                        Xrm.Page.getAttribute("cmc_startdatetime").setValue(Date.parse(eventDetails.msevtmgt_eventstartdate));
                                    if (eventDetails.msevtmgt_eventenddate != undefined)
                                        Xrm.Page.getAttribute("cmc_enddatetime").setValue(Date.parse(eventDetails.msevtmgt_eventenddate));
                                    if (eventDetails["venue.msevtmgt_venueid"] != undefined) {
                                        var addressDetails = null;
                                        if (eventDetails["venue.msevtmgt_addressline1"] != undefined)
                                            addressDetails += eventDetails["venue.msevtmgt_addressline1"] + " ";
                                        if (eventDetails["venue.msevtmgt_addressline2"] != undefined)
                                            addressDetails += eventDetails["venue.msevtmgt_addressline2"] + " ";
                                        if (eventDetails["venue.msevtmgt_addressline3"] != undefined)
                                            addressDetails += eventDetails["venue.msevtmgt_addressline3"] + " ";
                                        if (eventDetails["venue.msevtmgt_city"] != undefined)
                                            addressDetails += eventDetails["venue.msevtmgt_city"] + " ";
                                        if (eventDetails["venue.msevtmgt_stateprovince"] != undefined)
                                            addressDetails += eventDetails["venue.msevtmgt_stateprovince"] + " ";
                                        if (eventDetails["venue.msevtmgt_country"] != undefined)
                                            addressDetails += eventDetails["venue.msevtmgt_country"] + " ";
                                        if (eventDetails["venue.msevtmgt_postalcode"] != undefined)
                                            addressDetails += eventDetails["venue.msevtmgt_postalcode"] + " ";
                                        Xrm.Page.getAttribute("cmc_location").setValue(addressDetails);
                                    }
                                }
                            }
                        }, function (error) {
                            console.log(error);
                        });
                    }
                }
            }
        }
        cmc_tripactivity.eventSelectionChange = eventSelectionChange;
    })(cmc_tripactivity = CampusManagement.cmc_tripactivity || (CampusManagement.cmc_tripactivity = {}));
})(CampusManagement || (CampusManagement = {}));
