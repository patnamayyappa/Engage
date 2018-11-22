/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
module CampusManagement.cmc_tripactivity {

  declare let SonomaCmc: any;
  declare let tripStartDate: any;
  declare let tripEndDate: any;
  export function onLoad(executionContext) {
    setFieldsToDisable();
    getTripDetailsSetPrimaryStaff(executionContext);
    setTripActivityNameReadOnly(executionContext);
    if (Xrm.Page.getAttribute("cmc_activitytype")) {
      Xrm.Page.getAttribute("cmc_activitytype").addOnChange(CampusManagement.cmc_tripactivity.tripActivityTypeChange);
    }
    if (Xrm.Page.ui.getFormType() === XrmEnum.FormType.Create) {
      if (Xrm.Page.getControl('cmc_linkedtoevent')) {
        let eventControl: any = Xrm.Page.getControl('cmc_linkedtoevent');
        eventControl.setVisible(false);
      }
      let appointmentControl: any = Xrm.Page.getControl('cmc_linkedtoappointment')
      appointmentControl.setVisible(true);
    }
    if (Xrm.Page.ui.getFormType() === XrmEnum.FormType.Update) {
      let tripActivityType: any = Xrm.Page.getAttribute("cmc_activitytype");
      let appointmentControl: any = Xrm.Page.getControl('cmc_linkedtoappointment');
      let tripActivityName: any = Xrm.Page.getAttribute("cmc_name");

      if (tripActivityType.getSelectedOption().value === TripActivityType.Appointment) {
        if (Xrm.Page.getControl('cmc_linkedtoevent')) {
          let eventControl: any = Xrm.Page.getControl('cmc_linkedtoevent');
          eventControl.setVisible(false);
        }
        appointmentControl.setVisible(true);
        tripActivityName.setRequiredLevel("none");
      }
      else if (tripActivityType.getSelectedOption().value === TripActivityType.Event) {
        if (Xrm.Page.getControl('cmc_linkedtoevent')) {
          let eventControl: any = Xrm.Page.getControl('cmc_linkedtoevent');
          eventControl.setVisible(true);
        }
        appointmentControl.setVisible(false);
        tripActivityName.setRequiredLevel("none");
      }
      else if (tripActivityType.getSelectedOption().value === TripActivityType.Other) {
        if (Xrm.Page.getControl('cmc_linkedtoevent')) {
          let eventControl: any = Xrm.Page.getControl('cmc_linkedtoevent');
          eventControl.setVisible(false);
        }
        appointmentControl.setVisible(false);
        setFieldsToEnable();
        tripActivityName.setRequiredLevel("required");
      }
    }
    if (Xrm.Page.getAttribute("cmc_linkedtoevent")) {
      Xrm.Page.getAttribute("cmc_linkedtoevent").addOnChange(CampusManagement.cmc_tripactivity.eventSelectionChange);
    }

    // on update scenario
    if (Xrm.Page.ui.getFormType() === XrmEnum.FormType.Update) {
      let trip = Xrm.Page.getAttribute("cmc_trip").getValue();
      if (trip != null) {
        let tripId = SonomaCmc.Guid.decapsulate(trip[0].id);
        // disable the trip activity based on the trip status
        utilities.disableTripActivityonTripStatus(executionContext, tripId);
      }
    }
  }
  export function onSave(executionContext) {
    if (Xrm.Page.getAttribute("cmc_startdatetime").getIsDirty() || Xrm.Page.getAttribute("cmc_enddatetime").getIsDirty()) {
      let trip = Xrm.Page.getAttribute("cmc_trip").getValue();
      let tripId = trip[0].id.replace('{', '').replace('}', '');
      let tripActivityStartDate = Date.parse(Xrm.Page.getAttribute("cmc_startdatetime").getValue());
      let tripActivityEndDate = Date.parse(Xrm.Page.getAttribute("cmc_enddatetime").getValue());

      if (!(((tripStartDate <= tripActivityStartDate) && (tripEndDate >= tripActivityStartDate)) && ((tripStartDate <= tripActivityEndDate) && (tripEndDate >= tripActivityEndDate)))) {
        CampusManagement.CommonUiControls.openAlertDialog(CampusManagement.localization.getResourceString("TravelMgmt_TripActivityStart_EndDateOutOfTripStart_EndDate"), CampusManagement.localization.getResourceString("OkButton"));
      }
    }
  }

  function getVersion() {
    return Xrm.Page.context.getVersion().split('.').slice(0, 2).join('.');
  }

  function getTripDetailsSetPrimaryStaff(executionContext) {

    let trip = Xrm.Page.getAttribute("cmc_trip").getValue();
    if (trip != null) {
      let tripId = trip[0].id.replace('{', '').replace('}', '');
      let fetchXml = [
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
        .then(
        function success(result) {
          if (result.entities.length > 0) {
            tripStartDate = Date.parse(result.entities[0]["ab.cmc_startdate"]);
            tripEndDate = Date.parse(result.entities[0]["ab.cmc_enddate"]);
          }
        },
        error => {
          console.log(error);
        });
    }
  }

  function setTripActivityNameReadOnly(executionContext) {
    let tripActivityName: any;
    let tripActivityType: any;
    tripActivityName = Xrm.Page.ui.controls.get("cmc_name");
    tripActivityType = Xrm.Page.getAttribute("cmc_activitytype");
    if (Xrm.Page.ui.getFormType() === XrmEnum.FormType.Create) {
      tripActivityName.setDisabled(true);
    }
    else if (Xrm.Page.ui.getFormType() !== XrmEnum.FormType.Create) {
      if (tripActivityType.getSelectedOption().value === TripActivityType.Other) {
        tripActivityName.setDisabled(false);
      }
      else {
        tripActivityName.setDisabled(true);
      }
    }
  }

  //used onchange of the appoint in trip activity
  export function appointmentSelectionChange(executionContext, evtargs): void {
    let selectedAppointment = Xrm.Page.getAttribute("cmc_linkedtoappointment").getValue();
    setFieldsToNull();
    if (selectedAppointment != null) {
      {
        let selectedAppointmentId = selectedAppointment[0].id.replace('{', '').replace('}', '');
        if (selectedAppointmentId != null) {

          let fetchXml = [
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
            .then(
            function success(result) {
              console.log("Result of the appointment on change event is " + result);
              if (result.entities.length > 0) {
                let appointment = result.entities[0];
                if (appointment != null) { // to be safe
                  if (appointment.scheduledstart != undefined)
                    Xrm.Page.getAttribute("cmc_startdatetime").setValue(new Date(appointment["scheduledstart@OData.Community.Display.V1.FormattedValue"]));
                  if (appointment.scheduledend != undefined)
                    Xrm.Page.getAttribute("cmc_enddatetime").setValue(new Date(appointment["scheduledend@OData.Community.Display.V1.FormattedValue"]));
                  if (appointment.location != undefined)
                    Xrm.Page.getAttribute("cmc_location").setValue(appointment.location);
                }
              }
            },
            error => {
              console.log(error);
            });
        }
      }
    }
  }

  export function tripActivityTypeChange(executionContext) {
    setFieldsToNull();
    let tripActivityNameAttribute: any = Xrm.Page.getAttribute("cmc_name");
    let tripActivityName: any = Xrm.Page.ui.controls.get("cmc_name");
    let tripActivityType: any = Xrm.Page.getAttribute("cmc_activitytype");
    if (tripActivityType.getSelectedOption().value === TripActivityType.Other) {
      tripActivityName.setDisabled(false);
      setFieldsToEnable();
      if (Xrm.Page.getAttribute("cmc_linkedtoappointment")) {
        Xrm.Page.getAttribute("cmc_linkedtoappointment").setValue(null);
        let linkedtoappointmentControl: any = Xrm.Page.getControl('cmc_linkedtoappointment');
        linkedtoappointmentControl.setVisible(false);
      }
      if (Xrm.Page.getAttribute("cmc_linkedtoevent")) {
        Xrm.Page.getAttribute("cmc_linkedtoevent").setValue(null);
        let linkedtoeventControl: any = Xrm.Page.getControl('cmc_linkedtoevent');
        linkedtoeventControl.setVisible(false);;
      }
      tripActivityNameAttribute.setRequiredLevel("required");
      // Added Code for auto populate name of type others
      if (Xrm.Page.ui.getFormType() === XrmEnum.FormType.Create) {
        if (Xrm.Page.getAttribute("cmc_trip")) {
          tripActivityNameAttribute.setValue(Xrm.Page.getAttribute("cmc_trip").getValue()[0].name);
        }
      }
    }
    else if (tripActivityType.getSelectedOption().value === TripActivityType.Appointment) {
      setFieldsToDisable();
      tripActivityName.setDisabled(true);
      if (Xrm.Page.getAttribute("cmc_linkedtoappointment")) {
        Xrm.Page.getAttribute("cmc_linkedtoappointment").setValue(null);
        let linkedtoappointmentControl: any = Xrm.Page.getControl('cmc_linkedtoappointment');
        linkedtoappointmentControl.setVisible(true);
      }

      if (Xrm.Page.getControl('cmc_linkedtoevent')) {
        let linkedtoeventControl: any = Xrm.Page.getControl('cmc_linkedtoevent');
        linkedtoeventControl.setVisible(false);
        Xrm.Page.getAttribute("cmc_linkedtoevent").setValue(null);
      }
      tripActivityNameAttribute.setRequiredLevel("none");
      // Added Code for remove auto populate name of type others
      if (Xrm.Page.ui.getFormType() === XrmEnum.FormType.Create) {
        tripActivityNameAttribute.setValue(null);
      }
    }
    else if (tripActivityType.getSelectedOption().value === TripActivityType.Event) {
      setFieldsToDisable();
      tripActivityName.setDisabled(true);
      if (Xrm.Page.getAttribute("cmc_linkedtoevent")) {
        Xrm.Page.getAttribute("cmc_linkedtoevent").setValue(null);
        let linkedtoeventControl: any = Xrm.Page.getControl('cmc_linkedtoevent');
        linkedtoeventControl.setVisible(true);
      }

      if (Xrm.Page.getControl('cmc_linkedtoappointment')) {
        let linkedtoappointmentControl: any = Xrm.Page.getControl('cmc_linkedtoappointment');
        linkedtoappointmentControl.setVisible(false);
        Xrm.Page.getAttribute("cmc_linkedtoappointment").setValue(null);
      }
      tripActivityNameAttribute.setRequiredLevel("none");
      // Added Code for remove auto populate name of type others
      if (Xrm.Page.ui.getFormType() === XrmEnum.FormType.Create) {
        tripActivityNameAttribute.setValue(null);
      }
    }
  }

  export function eventSelectionChange(executionContext): void {
    let selectedEvent;
    if (Xrm.Page.getAttribute("cmc_linkedtoevent")) {
      selectedEvent = Xrm.Page.getAttribute("cmc_linkedtoevent").getValue();
    }
    setFieldsToNull();
    if (selectedEvent != null) {
      {
        let selectedEventId = selectedEvent[0].id.replace('{', '').replace('}', '');
        if (selectedEventId != null) {
          var fetchXml = [
            "<fetch top='50'>",
            "  <entity name='msevtmgt_event'>",
            "    <attribute name='cmc_startdatetime' />",
            "    <attribute name='cmc_enddatetime' />",
            "    <filter type='and'>",
            "      <condition attribute='msevtmgt_eventid' operator='eq' value='", selectedEventId, "'/>",
            "    </filter>",
            "    <link-entity name='msevtmgt_venue' from='msevtmgt_venueid' to='msevtmgt_primaryvenue' alias='venue' link-type='outer'>",
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
            .then(
            function success(result) {
              console.log("Result of the event on change event is " + result);
              if (result.entities.length > 0) {
                let eventDetails = result.entities[0];
                if (eventDetails != null) { // to be safe
                  if (eventDetails.cmc_startdatetime != undefined)
                    Xrm.Page.getAttribute("cmc_startdatetime").setValue(new Date(eventDetails["cmc_startdatetime@OData.Community.Display.V1.FormattedValue"]));
                  if (eventDetails.cmc_enddatetime != undefined)
                    Xrm.Page.getAttribute("cmc_enddatetime").setValue(new Date(eventDetails["cmc_enddatetime@OData.Community.Display.V1.FormattedValue"]));
                  if (eventDetails["venue.msevtmgt_venueid"] != undefined) {
                    let addressDetails = "";
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
            },
            error => {
              console.log(error);
            });
        }
      }
    }
  }

  const enum TripActivityType {
    Appointment = 175490000,
    Event = 175490001,
    Other = 175490002
  }

  function setFieldsToNull() {
    Xrm.Page.getAttribute("cmc_startdatetime").setValue(null);
    Xrm.Page.getAttribute("cmc_enddatetime").setValue(null);
    Xrm.Page.getAttribute("cmc_location").setValue(null);
  }
  function setFieldsToDisable() {
    if (Xrm.Page.getAttribute("cmc_startdatetime")) {
      let cmcStartDateTime: any = Xrm.Page.ui.controls.get("cmc_startdatetime");
      cmcStartDateTime.setDisabled(true);
    }
    if (Xrm.Page.getAttribute("cmc_enddatetime")) {
      let cmcEndDateTime: any = Xrm.Page.ui.controls.get("cmc_enddatetime");
      cmcEndDateTime.setDisabled(true);
    }
    if (Xrm.Page.getAttribute("cmc_location")) {
      let cmcLocation: any = Xrm.Page.ui.controls.get("cmc_location");
      cmcLocation.setDisabled(true);
    }
  }
  function setFieldsToEnable() {
    if (Xrm.Page.getAttribute("cmc_startdatetime")) {
      let cmcStartDateTime: any = Xrm.Page.ui.controls.get("cmc_startdatetime");
      cmcStartDateTime.setDisabled(false);
    }
    if (Xrm.Page.getAttribute("cmc_enddatetime")) {
      let cmcEndDateTime: any = Xrm.Page.ui.controls.get("cmc_enddatetime");
      cmcEndDateTime.setDisabled(false);
    }
    if (Xrm.Page.getAttribute("cmc_location")) {
      let cmcLocation: any = Xrm.Page.ui.controls.get("cmc_location");
      cmcLocation.setDisabled(false);
    }
  }
}
