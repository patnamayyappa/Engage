/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
module CampusManagement.lead {
  declare let SonomaCmc;
  declare let sourceMethod: Xrm.Attributes.Attribute;
  declare let cmcLinkToTripActivity: Xrm.Controls.LookupControl;
  declare let tripActivityType: Constants.TripActivityType;
  export function onLoad(executionContext) {
    var formContext = executionContext.getFormContext(),
      referringContactId;

    if (formContext.getAttribute('customerid')) {
      formContext.getAttribute('customerid').controls.forEach(function (control) {
        control.setEntityTypes(['contact']);
      });
    }

    if (formContext.ui.getFormType() === XrmEnum.FormType.Create &&
      formContext.getAttribute('cmc_sourcereferringcontactid')) {
      referringContactId = formContext.getAttribute('cmc_sourcereferringcontactid');

      if (referringContactId.getValue() && referringContactId.getValue().length > 0) {
        referringContactId.setValue(null);
      }
    }

    //On Change method Source Method
    sourceMethod = Xrm.Page.getAttribute("cmc_sourcemethodid");
    if (sourceMethod)
      sourceMethod.addOnChange(CampusManagement.lead.sourceMethodOnChange);
    //Hide and Show trip activity based on method on load time based on 
    cmcLinkToTripActivity = Xrm.Page.getControl("cmc_linktotripactivity");
    if (cmcLinkToTripActivity) {
      if (sourceMethod.getValue() != null && sourceMethod.getValue()[0] != null && (sourceMethod.getValue()[0].id == Constants.SourceMethod.EventAttendee || sourceMethod.getValue()[0].id == Constants.SourceMethod.Appointment || sourceMethod.getValue()[0].id == Constants.SourceMethod.Trip))
        cmcLinkToTripActivity.setVisible(true);
      else {
        if (Xrm.Page.getAttribute("cmc_linktotripactivity"))
        Xrm.Page.getAttribute("cmc_linktotripactivity").setValue(null);
        cmcLinkToTripActivity.setVisible(false);
      }
    }
  }

  export function sourceMethodOnChange() {
    try {
      sourceMethod = Xrm.Page.getAttribute("cmc_sourcemethodid");
      cmcLinkToTripActivity = Xrm.Page.getControl("cmc_linktotripactivity");
      if (sourceMethod && sourceMethod.getValue()[0] !== null) {
        if (sourceMethod.getValue()[0].id == Constants.SourceMethod.Appointment) {
          cmcLinkToTripActivity.setVisible(true);
          cmcLinkToTripActivity.removePreSearch(addCustomLookupFilter);
          cmcLinkToTripActivity.addPreSearch(addCustomLookupFilter);
        }
        else if (sourceMethod.getValue()[0].id == Constants.SourceMethod.Trip) {
          cmcLinkToTripActivity.setVisible(true);
          cmcLinkToTripActivity.removePreSearch(addCustomLookupFilter);
        }
        else if (sourceMethod.getValue()[0].id == Constants.SourceMethod.EventAttendee) {
          cmcLinkToTripActivity.setVisible(true);
          cmcLinkToTripActivity.removePreSearch(addCustomLookupFilter);
          cmcLinkToTripActivity.addPreSearch(addCustomLookupFilter);
        }
        else
          cmcLinkToTripActivity.setVisible(false);
      }
    }
    catch (e) { console.log(e); }
  }

  function addCustomLookupFilter() {
    try {
      cmcLinkToTripActivity = Xrm.Page.getControl("cmc_linktotripactivity");
      sourceMethod = Xrm.Page.getAttribute("cmc_sourcemethodid");
      if (sourceMethod && sourceMethod.getValue()[0] !== null) {
        if (sourceMethod.getValue()[0].id == Constants.SourceMethod.Appointment) {
          tripActivityType = Constants.TripActivityType.Appointment;
        }
        else if (sourceMethod.getValue()[0].id == Constants.SourceMethod.EventAttendee) {
          tripActivityType = Constants.TripActivityType.Event;
        }
      }
      let fetchXml = [
        "    <filter type='and'>",
        "      <condition attribute='cmc_activitytype' operator='eq' value='", tripActivityType, "'/>",
        "    </filter>",
      ].join("");
      cmcLinkToTripActivity.addCustomFilter(fetchXml);
    }
    catch (e) { console.log(e); }
  }
}
