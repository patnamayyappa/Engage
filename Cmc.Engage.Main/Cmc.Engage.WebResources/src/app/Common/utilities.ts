/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
module CampusManagement.utilities {

  declare let Qs: any;

  export function getDataParameter() {
    if (!location.search || location.search.length == 0) {
      return null;
    }

    var parsedQueryString = Qs.parse(location.search.substr(1));
    if (!parsedQueryString.data) {
      return null;
    }

    return Qs.parse(parsedQueryString.data);
  }

  export function setActivityToFieldForContactOnLoad(executionContext) {
    var formContext = executionContext.getFormContext(),
      regardingobjectid = formContext.getAttribute('regardingobjectid'),
      to = formContext.getAttribute('to'),
      regarding;

    if (formContext.ui.getFormType() !== XrmEnum.FormType.Create || !regardingobjectid || !to ||
      !regardingobjectid.getValue() || regardingobjectid.getValue().length <= 0) {
      return;
    }

    regarding = regardingobjectid.getValue()[0];

    if (regarding.entityType !== 'contact') {
      return;
    }

    to.setValue([regarding]);
  }

  export function disableSection(executionContext, sectionName, disableStatus, disableAllInstances) {
    var formContext: Xrm.FormContext = executionContext.getFormContext(),
      controls: Xrm.Controls.Control[] = formContext.ui.controls.get(),
      sc: Xrm.Controls.StandardControl,
      ctrlSection: string;

    for (let c of controls) {
      if (c.getParent() !== null) {
        ctrlSection = c.getParent().getName();
        if (ctrlSection == sectionName) {
          sc = <Xrm.Controls.StandardControl>c;
          if (sc.getDisabled() !== null) {
            sc.setDisabled(disableStatus);
          }

          if (disableAllInstances) {
            sc.getAttribute().controls.forEach(control => {
              var stdControl = <Xrm.Controls.StandardControl>control;
              stdControl.setDisabled(disableStatus);
            });
          }
        }
      }
    }
  }

  // to disable the controls on the Entity form
  export function disabletheFormControls(executionContext, disableStatus, disableAllInstances) {
    let formContext = executionContext.getFormContext(),
      sc: Xrm.Controls.StandardControl;
    if (Xrm.Page.ui.getFormType() === 2) {
      let controls: Xrm.Controls.Control[] = formContext.ui.controls.get();
      for (let c of controls) {
        if (c.getControlType() != "subgrid") {
          sc = <Xrm.Controls.StandardControl>c;
          if (sc.getDisabled() !== null) {
            sc.setDisabled(disableStatus);
          }
          // disable for all instances.
          if (disableAllInstances) {
            sc.getAttribute().controls.forEach(control => {
              var stdControl = <Xrm.Controls.StandardControl>control;
              stdControl.setDisabled(disableStatus);
            });
          }
        }
      }
    }
  }

  // uses to disable the trip activity based on the trip status "Completed" or "Cancelled"
  export function disableTripActivityonTripStatus(executionContext, tripId) {
    let TripStatus = Constants.TripStatus;
    let fetchXml = [
      '<fetch>',
      '<entity name="cmc_trip" >',
      '<attribute name="cmc_status"/>',
      '<filter type="and" >',
      '<condition attribute="cmc_tripid" value = "' + tripId + '" operator = "eq" />',
      '</filter>',
      '</entity>',
      '</fetch>'
    ].join('');

    Xrm.WebApi.retrieveMultipleRecords("cmc_trip", '?fetchXml=' + fetchXml)
      .then(
        function success(result) {
          if (result.entities.length > 0) {
            let tripStatus = result.entities[0]["cmc_status"];
            if (tripStatus == TripStatus.Completed || tripStatus == TripStatus.Canceled) {
              // disable the control on the form.
              disabletheFormControls(executionContext, true, true);
            }
          }
        },
        error => {
          console.log(error);
        });
  }
  export function executeWorkflow(entityId, workflowId) {
    var query = "";
    try {

      //Define the query to execute the action
      query = "workflows(" + workflowId.replace("}", "").replace("{", "") + ")/Microsoft.Dynamics.CRM.ExecuteWorkflow";

      var data = {
        "EntityId": entityId
      };

      //Create request
      // request url     
      var req = new XMLHttpRequest();
      req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v" + Xrm.Page.context.getVersion().split('.').slice(0, 2).join('.') +"/" + query, true);
      req.setRequestHeader("Accept", "application/json");
      req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
      req.setRequestHeader("OData-MaxVersion", "4.0");
      req.setRequestHeader("OData-Version", "4.0");

      req.onreadystatechange = function () {

        if (this.readyState == 4 /* complete */) {
          req.onreadystatechange = null;

          if (this.status == 200) {
            //success callback this returns null since no return value available.
            var result = JSON.parse(this.response);
            return result;

          } else {
            //error callback
            var error = JSON.parse(this.response).error;
            return error;
          }
        }
      };
      req.send(JSON.stringify(data));

    } catch (err) {
      console.log(err);
    }
  }
}


