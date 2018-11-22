
interface KendoEditorOptions extends kendo.ui.EditorOptions {
    value: any;
}

module CampusManagement.ribbon.appointment {

    declare var SonomaCmc: any;

    var _inviteeStatuses = {
            accepted: 175490001,
            declined: 175490002
        },

        _acceptDeclineRunning,

        localizedStrings = {
            errorPrefix: CampusManagement.localization.getResourceString("ErrorPrefix"),
            okButton: CampusManagement.localization.getResourceString("OkButton"),
        };

    //Form Ribbon Functions
    export function declineAppointment() {
        setInviteStatus(_inviteeStatuses.declined);
    }

    export function acceptAppointment() {
        setInviteStatus(_inviteeStatuses.accepted);
    }

    export function isCurrentUserOwner() {
        // Xrm.Page is used as there currently isn't a good way to get the execution context
        // or form context into a ribbon script
        var ownerId = Xrm.Page.getAttribute('ownerid'),
            ownerIdValue;

        if (!ownerId) {
            return false;
        }

        ownerIdValue = ownerId.getValue();

        return ownerIdValue && ownerIdValue.length &&
            SonomaCmc.Guid.decapsulate(ownerIdValue[0].id) === SonomaCmc.Guid.decapsulate(Xrm.Page.context.getUserId());
    }

    function setInviteStatus(val) {
        // Xrm.Page is used as there currently isn't a good way to get the execution context
        // or form context into a ribbon script
        var statusField = Xrm.Page.getAttribute("cmc_invitestatus");
        if (statusField) {
            statusField.setValue(val);
            Xrm.Page.data.entity.save();
            Xrm.Page.ui.refreshRibbon();
        }
    }

    //Subgrid Ribbon Functions
    export function acceptAppointmentSubgrid(selectedItems, grid) {
        processSubgridList(selectedItems, grid, "accept");
    }

    export function declineAppointmentSubgrid(selectedItems, grid) {
        processSubgridList(selectedItems, grid, "decline");
    }

    function processSubgridList(selectedItems, grid, acceptOrDecline) {
        var retrieveLoadingMessagePromise,
            promises = [],
            i, len;

        if (!selectedItems && selectedItems.length === 0) {
            return;
        }

        if (_acceptDeclineRunning) {
            return;
        }

        _acceptDeclineRunning = true;
        
        for (i = 0, len = selectedItems.length; i < len; i++) {
            if (selectedItems[i].TypeName == "appointment") {
                promises.push(updateRecord(selectedItems[i].Id, acceptOrDecline, grid));
            }
        }
        if (promises.length === 0) {
            _acceptDeclineRunning = false;
            return;
        }
        
        waitForRequests(promises, acceptOrDecline, selectedItems, grid)
    }

    function waitForRequests(promises, acceptOrDecline, selectedItems, grid) {
        Xrm.Utility.showProgressIndicator(CampusManagement.localization.getResourceString('Ribbon_Loading') )
        SonomaCmc.Promise.Promise.all(promises).then(function () {
            Xrm.Utility.closeProgressIndicator();
            _acceptDeclineRunning = false;
            if (acceptOrDecline === 'accept') {
                multiLingualAlert(CampusManagement.localization.getResourceString('AcceptAppointment_Success'), grid);
            }
            else {
                multiLingualAlert(CampusManagement.localization.getResourceString('DeclineAppointment_Success'), grid);
            }
        })
            .catch(function (error) {
                Xrm.Utility.closeProgressIndicator();
                _acceptDeclineRunning = false;
                if (selectedItems.length > 1) {
                    if (acceptOrDecline === 'accept') {
                        multiLingualAlert(CampusManagement.localization.getResourceString('AcceptAppointment_MultipleErrors'), grid);
                    }
                    else {
                        multiLingualAlert(CampusManagement.localization.getResourceString('DeclineAppointment_MultipleErrors'), grid);
                    }
                }
                else {
                    if (acceptOrDecline === 'accept') {
                        multiLingualAlert(CampusManagement.localization.getResourceString('AcceptAppointment_SingleError'), grid);
                    }
                    else {
                        multiLingualAlert(CampusManagement.localization.getResourceString('AcceptAppointment_SingleError'), grid);
                    }
                }
            });
    }

    function updateRecord(appointmentId, acceptOrDecline, grid) {
        // SonomaCmc.WebAPI is used as Xrm.WebApi.execute does not currently work in the
        // Unified Interface
        return SonomaCmc.WebAPI.post("appointments(" +
            appointmentId.replace("{", "").replace("}", "") +
            ")/Microsoft.Dynamics.CRM.cmc_" +
            acceptOrDecline + "invite", {});
    }
    
    function multiLingualAlert(message, grid) {
        var alertStrings = {
            text: `${message}`,
            confirmButtonLabel: localizedStrings.okButton,
        };

        Xrm.Navigation.openAlertDialog(alertStrings, null).then(function () {if (grid) {grid.refresh();}});
    }
}
