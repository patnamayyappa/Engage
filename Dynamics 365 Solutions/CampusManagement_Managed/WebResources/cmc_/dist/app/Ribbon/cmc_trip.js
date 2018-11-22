/// Customizations for the trip entity///
/// complete and cancel click command is modified ///
var CampusManagement;
(function (CampusManagement) {
    var Ribbon;
    (function (Ribbon) {
        var cmc_trip;
        (function (cmc_trip) {
            // to show approve and reject visiblity
            function showApproveRejectOption() {
                if ((Xrm.Page.getAttribute("cmc_status").getValue() != Constants.TripStatus.SubmitForApproval) || (!Xrm.Page.getAttribute("cmc_approvedby").getValue()))
                    return false;
                if (Xrm.Page.getAttribute("cmc_approvedby").getValue()[0].id == Xrm.Page.context.getUserId())
                    return true;
                else
                    return false;
            }
            cmc_trip.showApproveRejectOption = showApproveRejectOption;
            // on approve click
            function OnApprove() {
                if (Xrm.Page.getAttribute("cmc_status")) {
                    Xrm.Page.getAttribute("cmc_status").setValue(Constants.TripStatus.Approved);
                    Xrm.Page.data.save();
                    var activeProcess = Xrm.Page.data.process.getActiveProcess();
                    if (activeProcess.getId() === "7e760628-460a-4452-a2cd-db21dcad3b9b") {
                        Xrm.Page.data.process.moveNext(function (result) {
                            if (result == "success") {
                            }
                            else {
                                // ** Code for failure goes here
                                //alert("did not moved to next stage - Issue");
                            }
                            ;
                        });
                    }
                    ;
                }
            }
            cmc_trip.OnApprove = OnApprove;
            // on reject click
            function OnReject() {
                if (Xrm.Page.getAttribute("cmc_status")) {
                    Xrm.Page.getAttribute("cmc_status").setValue(Constants.TripStatus.Rejected);
                    Xrm.Page.data.save();
                    var activeProcess = Xrm.Page.data.process.getActiveProcess();
                    if (activeProcess.getId() === "7e760628-460a-4452-a2cd-db21dcad3b9b") {
                        Xrm.Page.data.process.moveNext(function (result) {
                            if (result == "success") {
                            }
                            else {
                                // ** Code for failure goes here
                                //alert("did not moved to next stage - Issue");
                            }
                            ;
                        });
                    }
                    ;
                }
            }
            cmc_trip.OnReject = OnReject;
            // from the grid instance
            function onCompleteorCancelTripAction(trip, actionType) {
                var tripId = SonomaCmc.Guid.decapsulate(trip);
                var _loadingText = CampusManagement.localization.getResourceString("Ribbon_Loading");
                var localizedStrings = {
                    okButton: CampusManagement.localization.getResourceString("OkButton"),
                };
                var confirmStrings = {
                    text: CampusManagement.localization.getResourceString((actionType == "Complete") ? "Trip_Complete" : "Trip_Cancel"),
                    title: CampusManagement.localization.getResourceString("Trip_DialogTitle"),
                    confirmButtonLabel: CampusManagement.localization.getResourceString("OkButton"),
                    cancelButtonLabel: CampusManagement.localization.getResourceString("CancelButton"),
                };
                var confirmOptions = { height: 250, width: 500 };
                Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(function (success) {
                    if (success.confirmed) {
                        Xrm.Utility.showProgressIndicator(_loadingText);
                        //CompleteorCancelTrip
                        SonomaCmc.WebAPI.post('cmc_CompleteorCancelTrip', {
                            TripId: tripId,
                            ActionType: actionType
                        }).then(function (result) {
                            // close the indicator
                            Xrm.Utility.closeProgressIndicator();
                            console.log(result);
                            var response = result.Response;
                            if (response !== null && response !== "") {
                                console.log("response " + response);
                                CampusManagement.CommonUiControls.openAlertDialog(CampusManagement.localization.getResourceString(response), localizedStrings.okButton);
                            }
                            else {
                                // on success make it as read only
                                Xrm.Page.data.refresh(true);
                                Xrm.Utility.openEntityForm(Xrm.Page.data.entity.getEntityName(), Xrm.Page.data.entity.getId()); // open the form again to disable or read only.
                            }
                        }).catch(function (error) {
                            Xrm.Utility.closeProgressIndicator();
                            alert(error);
                            console.log(error);
                        });
                    }
                }, function (cancel) {
                    console.log("cancel clicked.");
                    return;
                });
            }
            cmc_trip.onCompleteorCancelTripAction = onCompleteorCancelTripAction;
        })(cmc_trip = Ribbon.cmc_trip || (Ribbon.cmc_trip = {}));
    })(Ribbon = CampusManagement.Ribbon || (CampusManagement.Ribbon = {}));
})(CampusManagement || (CampusManagement = {}));
