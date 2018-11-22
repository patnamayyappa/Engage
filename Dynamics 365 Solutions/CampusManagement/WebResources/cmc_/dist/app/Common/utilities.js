/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
var CampusManagement;
(function (CampusManagement) {
    var utilities;
    (function (utilities) {
        function getDataParameter() {
            if (!location.search || location.search.length == 0) {
                return null;
            }
            var parsedQueryString = Qs.parse(location.search.substr(1));
            if (!parsedQueryString.data) {
                return null;
            }
            return Qs.parse(parsedQueryString.data);
        }
        utilities.getDataParameter = getDataParameter;
        function setActivityToFieldForContactOnLoad(executionContext) {
            var formContext = executionContext.getFormContext(), regardingobjectid = formContext.getAttribute('regardingobjectid'), to = formContext.getAttribute('to'), regarding;
            if (formContext.ui.getFormType() !== 1 /* Create */ || !regardingobjectid || !to ||
                !regardingobjectid.getValue() || regardingobjectid.getValue().length <= 0) {
                return;
            }
            regarding = regardingobjectid.getValue()[0];
            if (regarding.entityType !== 'contact') {
                return;
            }
            to.setValue([regarding]);
        }
        utilities.setActivityToFieldForContactOnLoad = setActivityToFieldForContactOnLoad;
        function disableSection(executionContext, sectionName, disableStatus, disableAllInstances) {
            var formContext = executionContext.getFormContext(), controls = formContext.ui.controls.get(), sc, ctrlSection;
            for (var _i = 0, controls_1 = controls; _i < controls_1.length; _i++) {
                var c = controls_1[_i];
                if (c.getParent() !== null) {
                    ctrlSection = c.getParent().getName();
                    if (ctrlSection == sectionName) {
                        sc = c;
                        if (sc.getDisabled() !== null) {
                            sc.setDisabled(disableStatus);
                        }
                        if (disableAllInstances) {
                            sc.getAttribute().controls.forEach(function (control) {
                                var stdControl = control;
                                stdControl.setDisabled(disableStatus);
                            });
                        }
                    }
                }
            }
        }
        utilities.disableSection = disableSection;
        // to disable the controls on the Entity form
        function disabletheFormControls(executionContext, disableStatus, disableAllInstances) {
            var formContext = executionContext.getFormContext(), sc;
            if (Xrm.Page.ui.getFormType() === 2) {
                var controls = formContext.ui.controls.get();
                for (var _i = 0, controls_2 = controls; _i < controls_2.length; _i++) {
                    var c = controls_2[_i];
                    if (c.getControlType() != "subgrid") {
                        sc = c;
                        if (sc.getDisabled() !== null) {
                            sc.setDisabled(disableStatus);
                        }
                        // disable for all instances.
                        if (disableAllInstances) {
                            sc.getAttribute().controls.forEach(function (control) {
                                var stdControl = control;
                                stdControl.setDisabled(disableStatus);
                            });
                        }
                    }
                }
            }
        }
        utilities.disabletheFormControls = disabletheFormControls;
        // uses to disable the trip activity based on the trip status "Completed" or "Cancelled"
        function disableTripActivityonTripStatus(executionContext, tripId) {
            var TripStatus = Constants.TripStatus;
            var fetchXml = [
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
                .then(function success(result) {
                if (result.entities.length > 0) {
                    var tripStatus = result.entities[0]["cmc_status"];
                    if (tripStatus == TripStatus.Completed || tripStatus == TripStatus.Canceled) {
                        // disable the control on the form.
                        disabletheFormControls(executionContext, true, true);
                    }
                }
            }, function (error) {
                console.log(error);
            });
        }
        utilities.disableTripActivityonTripStatus = disableTripActivityonTripStatus;
        function executeWorkflow(entityId, workflowId) {
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
                req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v" + Xrm.Page.context.getVersion().split('.').slice(0, 2).join('.') + "/" + query, true);
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
                        }
                        else {
                            //error callback
                            var error = JSON.parse(this.response).error;
                            return error;
                        }
                    }
                };
                req.send(JSON.stringify(data));
            }
            catch (err) {
                console.log(err);
            }
        }
        utilities.executeWorkflow = executeWorkflow;
    })(utilities = CampusManagement.utilities || (CampusManagement.utilities = {}));
})(CampusManagement || (CampusManagement = {}));
