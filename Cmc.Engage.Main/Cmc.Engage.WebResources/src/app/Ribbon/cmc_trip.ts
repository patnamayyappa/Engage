/// Customizations for the trip entity///
/// complete and cancel click command is modified ///
module CampusManagement.Ribbon.cmc_trip {
  declare let SonomaCmc: any;
  declare let XrmCore: any;
  declare let Xrm: any;
  // to show approve and reject visiblity
  export function showApproveRejectOption() {
    if ((Xrm.Page.getAttribute("cmc_status").getValue() != Constants.TripStatus.SubmitForApproval))
      return false;

    if (Xrm.Page.getAttribute("cmc_approvedby").getValue() != null && Xrm.Page.getAttribute("cmc_approvedby").getValue()[0] != null && Xrm.Page.getAttribute("cmc_approvedby").getValue()[0].id == Xrm.Page.context.getUserId())
      return true;
    if (Xrm.Page.getAttribute("cmc_reviewerteam").getValue() != null && Xrm.Page.getAttribute("cmc_reviewerteam").getValue().length > 0)
      return isUserBelongsApprovalTeam();    
    else
      return false;
  }  

  function onApproveSaveSuccessCallback() {
    var activeProcess = Xrm.Page.data.process.getActiveProcess();
    if (activeProcess.getId() === "7e760628-460a-4452-a2cd-db21dcad3b9b") {
      Xrm.Page.data.process.moveNext(function (result) {
        if (result == "success") {
          var processStatus = Xrm.Page.data.process.getStatus();
          if (processStatus != null && processStatus !== "finished") {
            Xrm.Page.data.process.setStatus("finished");
          }
        }
        else {
          // ** Code for failure goes here
        };
      });
    };    
  }

  function onRejectSaveSuccessCallback() {
    var activeProcess = Xrm.Page.data.process.getActiveProcess();
    if (activeProcess.getId() === "7e760628-460a-4452-a2cd-db21dcad3b9b") {
      moveNextStage();
    };
  }

  function moveNextStage() {
    Xrm.Page.data.process.moveNext(function (result) {
      if (result == "success") {
      }
      else {
        // ** Code for failure goes here
      };
    });
  }


  function onApproveRejectSaveErrorCallback() {
    // ** ToDo Handle for save failure
    return;
  }

  // on approve click
  export function OnApprove() {    
    if (Xrm.Page.getAttribute("cmc_status")) {
      if (Xrm.Page.getAttribute("cmc_approvedby") && Xrm.Page.getAttribute("cmc_approvedby").getValue()) {
        Xrm.Page.getAttribute("cmc_status").setValue(Constants.TripStatus.Approved);
        Xrm.Page.data.refresh(true).then(onApproveSaveSuccessCallback, onApproveRejectSaveErrorCallback);
      }
      else {
        if (isTripStatusSubmitForApproval()) {
          Xrm.Page.getAttribute("cmc_approvedby").setValue([{ id: Xrm.Page.context.getUserId(), name: Xrm.Page.context.getUserName(), entityType: "systemuser" }]);
          Xrm.Page.getAttribute("cmc_status").setValue(Constants.TripStatus.Approved);          
          Xrm.Page.data.refresh(true).then(onApproveSaveSuccessCallback, onApproveRejectSaveErrorCallback);
        }
        else
        {
          CampusManagement.CommonUiControls.openAlertDialog(CampusManagement.localization.getResourceString("TravelMgmt_TripAlreadyApprovedOrRejected"), CampusManagement.localization.getResourceString("OkButton"));
        }
      }
    }
  }

  // on reject click
  export function OnReject() {
    if (Xrm.Page.getAttribute("cmc_status")) {
      if (Xrm.Page.getAttribute("cmc_approvedby") && Xrm.Page.getAttribute("cmc_approvedby").getValue()) {
        Xrm.Page.getAttribute("cmc_status").setValue(Constants.TripStatus.Rejected);
        Xrm.Page.data.refresh(true).then(onRejectSaveSuccessCallback, onApproveRejectSaveErrorCallback);
      }
      else {
        if (isTripStatusSubmitForApproval()) {
          Xrm.Page.getAttribute("cmc_approvedby").setValue([{ id: Xrm.Page.context.getUserId(), name: Xrm.Page.context.getUserName(), entityType: "systemuser" }]);
          Xrm.Page.getAttribute("cmc_status").setValue(Constants.TripStatus.Rejected);
          Xrm.Page.data.refresh(true).then(onRejectSaveSuccessCallback, onApproveRejectSaveErrorCallback);
        }
        else {
          CampusManagement.CommonUiControls.openAlertDialog(CampusManagement.localization.getResourceString("TravelMgmt_TripAlreadyApprovedOrRejected"), CampusManagement.localization.getResourceString("OkButton"));
        }
      }
    }
  }



  // from the grid instance
  export function onCompleteorCancelTripAction(trip: any, actionType: string): void {
    let tripId = SonomaCmc.Guid.decapsulate(trip);
    var _loadingText = CampusManagement.localization.getResourceString("Ribbon_Loading");
    let localizedStrings = {
      okButton: CampusManagement.localization.getResourceString("OkButton"),
    };
    let confirmStrings: Xrm.Navigation.ConfirmStrings = {
      text: CampusManagement.localization.getResourceString((actionType == "Complete") ? "Trip_Complete" : "Trip_Cancel"),
      title: CampusManagement.localization.getResourceString("Trip_DialogTitle"),
      confirmButtonLabel: CampusManagement.localization.getResourceString("OkButton"),
      cancelButtonLabel: CampusManagement.localization.getResourceString("CancelButton"),
    };
    let confirmOptions: Xrm.Navigation.DialogSizeOptions = { height: 250, width: 500 };
    Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
      success => {
        if (success.confirmed) {
          Xrm.Utility.showProgressIndicator(_loadingText);
          //CompleteorCancelTrip
          SonomaCmc.WebAPI.post('cmc_CompleteorCancelTrip',
            {
              TripId: tripId,
              ActionType: actionType
            }).then(result => {
              // close the indicator
              Xrm.Utility.closeProgressIndicator();
              console.log(result);
              var response = result.Response;
              if (response !== null && response !== "") {
                console.log("response " + response);
                CampusManagement.CommonUiControls.openAlertDialog(
                  CampusManagement.localization.getResourceString(response),
                  localizedStrings.okButton);
              } else {
                // on success make it as read only
                Xrm.Utility.openEntityForm(Xrm.Page.data.entity.getEntityName(), Xrm.Page.data.entity.getId()); // open the form again to disable or read only.
              }

            }).catch(error => {
              Xrm.Utility.closeProgressIndicator();
              alert(error);
              console.log(error);
            });
        }
      }, cancel => {
        console.log("cancel clicked.");
        return;
      });


  }

  // This method will check the loged in user belongs to trip approval team or not 
  function isUserBelongsApprovalTeam(): boolean {
    let returnValue = false;
    let userId = Xrm.Page.context.getUserId();
    let teamId = Xrm.Page.getAttribute("cmc_reviewerteam").getValue()[0].id;
    let url = Xrm.Page.context.getClientUrl() + "/api/data/v" + getVersion() + "/teammemberships?$select=teammembershipid&$filter=((teamid eq " + teamId + ") and (systemuserid eq " + userId + "))";
    $.ajax({
      type: "GET",
      contentType: "application/json; charset=utf-8",
      url: url,
      beforeSend: function (XMLHttpRequest) {
        XMLHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
        XMLHttpRequest.setRequestHeader("OData-Version", "4.0");
        XMLHttpRequest.setRequestHeader("Accept", "application/json");
      },
      async: false,
      success: function (data, textStatus, xhr) {        
        if (data.value.length > 0)
          returnValue = true;
        else
          returnValue = false;
      },
      error: function (xhr, textStatus, errorThrown) {
        returnValue = false;
      }
    });
    return returnValue;
  }

  // This method will check the status of trip is submitForApproval or not 
  function isTripStatusSubmitForApproval(): boolean {
    let returnValue = true;
    let tripId  = Xrm.Page.data.entity.getId();   
    let url = Xrm.Page.context.getClientUrl() + "/api/data/v" + getVersion() + "/cmc_trips?$select=cmc_tripid&$filter=((cmc_tripid eq " + tripId + ") and (cmc_status eq " + Constants.TripStatus.SubmitForApproval + "))";
      $.ajax({
        type: "GET",
        contentType: "application/json; charset=utf-8",
        url: url,
        beforeSend: function (XMLHttpRequest) {
          XMLHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
          XMLHttpRequest.setRequestHeader("OData-Version", "4.0");
          XMLHttpRequest.setRequestHeader("Accept", "application/json");
        },
        async: false,
        success: function (data, textStatus, xhr) {          
          if (data.value.length > 0)
            returnValue = true;
          else
            returnValue = false;
        },
        error: function (xhr, textStatus, errorThrown) {
          returnValue = false;
        }
      });        
    return returnValue;
  }

  function getVersion() {
    return Xrm.Page.context.getVersion().split('.').slice(0, 2).join('.');
  }
}
