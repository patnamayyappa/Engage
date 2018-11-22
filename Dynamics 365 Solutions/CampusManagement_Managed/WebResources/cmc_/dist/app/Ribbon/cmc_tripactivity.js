var CampusManagement;
(function (CampusManagement) {
    var Ribbon;
    (function (Ribbon) {
        var cmc_tripactivity;
        (function (cmc_tripactivity) {
            // send email 
            function SendEmail() {
                console.log("Trip activity id is " + Xrm.Page.data.entity.getId());
                var data = {
                    '@odata.type': 'Microsoft.Dynamics.CRM.cmc_tripactivity',
                    cmc_tripactivityid: Xrm.Page.data.entity.getId(),
                };
                SonomaCmc.WebAPI.post('cmc_tripactivitysendemailaction', {
                    TripActivityId: data
                }).then(function (result) {
                    CampusManagement.CommonUiControls.openAlertDialog(CampusManagement.localization.getResourceString("Email_Success"), CampusManagement.localization.getResourceString("OkButton"));
                });
            }
            cmc_tripactivity.SendEmail = SendEmail;
            // Send Email button display rule
            function showSendEmailButton() {
                var returnValue = false;
                var trip = Xrm.Page.getAttribute("cmc_trip").getValue();
                if (trip != null) {
                    var tripId = SonomaCmc.Guid.decapsulate(trip[0].id);
                    var url = Xrm.Page.context.getClientUrl() + "/api/data/v" + getVersion() + "/cmc_trips?$select=cmc_status&$filter=(cmc_tripid eq " + tripId + ")";
                    $.ajax({
                        type: "GET",
                        contentType: "application/json; charset=utf-8",
                        url: url,
                        beforeSend: function (xmlHttpRequest) {
                            xmlHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
                            xmlHttpRequest.setRequestHeader("OData-Version", "4.0");
                            xmlHttpRequest.setRequestHeader("Accept", "application/json");
                        },
                        async: false,
                        success: function (result) {
                            if (result.value.length > 0) {
                                var trip_1 = result.value[0];
                                if (trip_1 != null) { // to be safe
                                    console.log("Result of the Trip status   is " + trip_1.cmc_status);
                                    if (trip_1.cmc_status === Constants.TripStatus.Canceled || trip_1.cmc_status === Constants.TripStatus.Completed)
                                        returnValue = false; //button will hide
                                    else
                                        returnValue = true; //button will not hide
                                }
                            }
                        },
                        error: function () {
                            returnValue = false;
                        }
                    });
                }
                return returnValue;
            }
            cmc_tripactivity.showSendEmailButton = showSendEmailButton;
            function getVersion() {
                return Xrm.Page.context.getVersion().split('.').slice(0, 2).join('.');
            }
        })(cmc_tripactivity = Ribbon.cmc_tripactivity || (Ribbon.cmc_tripactivity = {}));
    })(Ribbon = CampusManagement.Ribbon || (CampusManagement.Ribbon = {}));
})(CampusManagement || (CampusManagement = {}));
