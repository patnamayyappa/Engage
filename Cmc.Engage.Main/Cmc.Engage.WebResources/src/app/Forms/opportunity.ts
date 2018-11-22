/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
module CampusManagement.opportunity {

  declare let sourceMethod: Xrm.Attributes.Attribute;
  declare let cmcLinkToTripActivity: Xrm.Controls.LookupControl;  
  declare let tripActivityType: Constants.TripActivityType;
    // Default mapping
    var _config = {
        // Prospect: Expressed Interest, Not Eligible, Not Interested
        "7d4c900e-9b67-4928-8f1c-2f4947b1ee94": ["175490004", "175490000", "175490001"],
        // App in Progress: Incomplete Application, Cancelled
        "bd6d5c4c-9c81-40f0-b091-159e62e2ecb6": ["175490003", "175490002"],
        // App Complete: Pending Review, App Requirements Pending, Ready for Decision, Cancelled
        "4e823740-ae46-468d-915c-a22890c20079": ["175490005", "175490006", "175490007", "175490002"],
        // Admit: Admitted with Conditions, Cancelled
        "92502357-cc6b-4eff-8876-91de78caf319": ["175490008", "175490002"],
        // Deposit: Admitted with Conditions, Cancelled
        "fa85a6af-941f-4189-858d-a32e7be41865": ["175490009", "175490002"],
        // Matriculate: Admitted with Conditions
        "7ecb3829-c12a-4251-8de8-980cd276882f": ["175490010"]
    };
    var _lifeCycleStatusAttr;

    export function onLoad(executionContext) {
        var formContext = executionContext.getFormContext();
        var clientUrl = formContext.context.getClientUrl();

        var jsonMapping = clientUrl + "/WebResources/cmc_/json/lifecyclestatusmapping.json";
        var xhr = new XMLHttpRequest();
        xhr.open("GET", jsonMapping, true);
        xhr.onreadystatechange = function () {
            if (this.readyState == 4 /* complete */) {
                this.onreadystatechange = null;
                if (this.status == 200) {
                    _config = JSON.parse(this.response);
                    filterBpfOnLoad(formContext);
                }
            }
        };
      xhr.send();

      //On Change method Source Method
      sourceMethod = Xrm.Page.getAttribute("cmc_sourcemethodid");
      if (sourceMethod)
        sourceMethod.addOnChange(CampusManagement.opportunity.sourceMethodOnChange);
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

    function filterBpfOnLoad(formContext) {
        _lifeCycleStatusAttr = formContext.getAttribute("cmc_lifecyclestatus");
        if (!_lifeCycleStatusAttr)
            return;

        var stageId = formContext.data.process.getActiveStage().getId();
        var selectedLifecycleStatus = _lifeCycleStatusAttr.getSelectedOption();

        formContext.data.process.addOnStageChange(stageOnChange);

        filterLifecycleStatus(stageId, selectedLifecycleStatus.value);
    }

    function filterLifecycleStatus(stageId, selectedLifecycleStatus) {
        var lifeCycleControls = _lifeCycleStatusAttr.controls;
        lifeCycleControls.forEach(function (control) {
            control.clearOptions();

            var stageLifecycleStatuses = _config[stageId];
            for (var i = 0; i < stageLifecycleStatuses.length; i++) {
                var stageLifecycleStatus = stageLifecycleStatuses[i];
                var option = _lifeCycleStatusAttr.getOption(stageLifecycleStatus);
                control.addOption(option);
            }
        });

        _lifeCycleStatusAttr.setValue(selectedLifecycleStatus);
    }

    function stageOnChange(executionContext) {
        var eventArgs = executionContext.getEventArgs();
        var stageId = eventArgs.getStage().getId();

        // Clear the selected lifecycle status when the stage changes
        filterLifecycleStatus(stageId, null);
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
