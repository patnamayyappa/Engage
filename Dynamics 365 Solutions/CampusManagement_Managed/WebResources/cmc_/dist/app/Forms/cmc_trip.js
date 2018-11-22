/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
var CampusManagement;
(function (CampusManagement) {
    var cmc_trip;
    (function (cmc_trip) {
        var disableTripActivityonTripStatus = CampusManagement.utilities.disableTripActivityonTripStatus;
        var isConfirm = false;
        function onLoad(executionContext) {
            handleTripApprovalProcess(executionContext);
            if (Xrm.Page.ui.getFormType() === 1 /* Create */) {
                setDepartmentOnLoad(executionContext);
                if (Xrm.Page.getAttribute("cmc_statusdate")) {
                    Xrm.Page.getAttribute("cmc_statusdate").setValue(Date.now());
                }
            }
            if (Xrm.Page.getAttribute("ownerid")) {
                Xrm.Page.getAttribute("ownerid").addOnChange(CampusManagement.cmc_trip.setDepartmentOnChange);
            }
            // on update scenario disable the form 
            if (Xrm.Page.ui.getFormType() === 2 /* Update */) {
                disableTripActivityonTripStatus(executionContext, SonomaCmc.Guid.decapsulate(Xrm.Page.data.entity.getId()));
            }
        }
        cmc_trip.onLoad = onLoad;
        function onSave(executionContext) {
            var owner = Xrm.Page.getAttribute("ownerid").getValue();
            var ownerId = owner[0].id.replace('{', '').replace('}', '');
            var cmcStartDate = new Date(Xrm.Page.getAttribute("cmc_startdate").getValue());
            var cmcEndDate = new Date(Xrm.Page.getAttribute("cmc_enddate").getValue());
            var startDate = new Date(cmcStartDate);
            var endDate = new Date(cmcEndDate);
            startDate = startDate.format('yyyy-MM-dd');
            endDate = endDate.format('yyyy-MM-dd');
            // substring 1 day from start date to filter trip activity less than start date
            var newStartDate = new Date(cmcStartDate.setDate(cmcStartDate.getDate() - 1));
            newStartDate = newStartDate.format('yyyy-MM-dd');
            // adding 1 day to end date to filter trip activity grater than end date
            var newEndDate = new Date(cmcEndDate.setDate(cmcEndDate.getDate() + 1));
            newEndDate = newEndDate.format('yyyy-MM-dd');
            var url;
            var urlTripActivity;
            if (Xrm.Page.ui.getFormType() == 1 /* Create */) {
                url = Xrm.Page.context.getClientUrl() + "/api/data/v" + getVersion() + "/cmc_trips?$select=cmc_startdate&$filter=(_ownerid_value eq (" + ownerId + ")) and (((cmc_startdate le " + startDate + ") and (cmc_enddate ge " + startDate + ")) or ((cmc_startdate le " + endDate + ") and (cmc_enddate ge " + endDate + ")) or ((cmc_startdate ge " + startDate + ") and (cmc_enddate le " + endDate + "))) ";
            }
            else if (Xrm.Page.ui.getFormType() == 2 /* Update */) {
                if (Xrm.Page.getAttribute("cmc_enddate").getIsDirty() || Xrm.Page.getAttribute("cmc_startdate").getIsDirty() || Xrm.Page.getAttribute("ownerid").getIsDirty()) {
                    var cmcTripId = SonomaCmc.Guid.decapsulate(Xrm.Page.data.entity.getId());
                    url = Xrm.Page.context.getClientUrl() + "/api/data/v" + getVersion() + "/cmc_trips?$select=cmc_startdate&$filter=cmc_tripid ne " + Xrm.Page.data.entity.getId().slice(1, -1) + " and(_ownerid_value eq (" + ownerId + ")) and (((cmc_startdate le " + startDate + ") and (cmc_enddate ge " + startDate + ")) or ((cmc_startdate le " + endDate + ") and (cmc_enddate ge " + endDate + ")) or ((cmc_startdate ge " + startDate + ") and (cmc_enddate le " + endDate + "))) ";
                    urlTripActivity = Xrm.Page.context.getClientUrl() + "/api/data/v" + getVersion() + "/cmc_tripactivities?$filter=_cmc_trip_value eq (" + cmcTripId + ") and ((cmc_startdatetime le " + newStartDate + ") or (cmc_enddatetime ge " + newEndDate + "))";
                }
                else
                    return;
            }
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
                    if (data.value.length > 0) {
                        CampusManagement.CommonUiControls.openAlertDialog(CampusManagement.localization.getResourceString("TravelMgmt_Tripwithsamestart_enddate"), CampusManagement.localization.getResourceString("OkButton"));
                        if (Xrm.Page.ui.getFormType() == 2 /* Update */) {
                            isTripActivityOutOfTripRange(urlTripActivity);
                        }
                    }
                    else {
                        if (Xrm.Page.ui.getFormType() == 2 /* Update */) {
                            isTripActivityOutOfTripRange(urlTripActivity);
                        }
                    }
                },
                error: function (xhr, textStatus, errorThrown) {
                }
            });
        }
        cmc_trip.onSave = onSave;
        function isTripActivityOutOfTripRange(url) {
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
                    if (data.value.length > 0) {
                        CampusManagement.CommonUiControls.openAlertDialog(CampusManagement.localization.getResourceString("TravelMgmt_TripActivityExceedsDateRangeOfAssociatedTrip"), CampusManagement.localization.getResourceString("OkButton"));
                    }
                },
                error: function (xhr, textStatus, errorThrown) {
                }
            });
        }
        function setDepartmentOnLoad(executionContext) {
            if (Xrm.Page.getAttribute("cmc_department").getValue() === null || Xrm.Page.getAttribute("cmc_department").getValue().length < 1) {
                setDepartment(executionContext);
            }
        }
        function setDepartmentOnChange(executionContext) { setDepartment(executionContext); }
        cmc_trip.setDepartmentOnChange = setDepartmentOnChange;
        function setDepartment(executionContext) {
            var owner = Xrm.Page.getAttribute("ownerid").getValue();
            var ownerId = owner[0].id.replace('{', '').replace('}', '');
            var fetchXml = [
                '<fetch>',
                '<entity name="cmc_department" >',
                '<attribute name = "cmc_departmentid" />',
                '<attribute name="cmc_departmentname" />',
                '<link-entity name = "systemuser" alias = "ab" link = ""  type = "inner" to = "cmc_departmentid" from = "cmc_departmentid" >',
                '<filter type="and" >',
                '<condition attribute="systemuserid" value = "' + ownerId + '"  uitype = "systemuser"  operator = "eq" />',
                '</filter>',
                '</link-entity>',
                '</entity>',
                '</fetch>'
            ].join('');
            Xrm.WebApi.retrieveMultipleRecords("cmc_department", '?fetchXml=' + fetchXml)
                .then(function success(result) {
                if (result.entities.length > 0) {
                    Xrm.Page.getAttribute("cmc_department").setValue([{ id: result.entities[0].cmc_departmentid, name: result.entities[0].cmc_departmentname, entityType: "cmc_department" }]);
                }
                else {
                    Xrm.Page.getAttribute("cmc_department").setValue();
                }
            }, function (error) {
                console.log(error);
            });
        }
        function getVersion() {
            return Xrm.Page.context.getVersion().split('.').slice(0, 2).join('.');
        }
        function enableFields() {
            // Form fields
            if (Xrm.Page.getControl("cmc_approvalcomment")) {
                Xrm.Page.getControl("cmc_approvalcomment").setDisabled(false);
            }
            // BPF fields
            if (Xrm.Page.getControl("header_process_cmc_approvalcomment")) {
                Xrm.Page.getControl("header_process_cmc_approvalcomment").setDisabled(false);
            }
        }
        function disableFields() {
            // Form fields
            if (Xrm.Page.getControl("cmc_approvedby")) {
                Xrm.Page.getControl("cmc_approvedby").setDisabled(true);
            }
            if (Xrm.Page.getControl("cmc_approvalcomment")) {
                Xrm.Page.getControl("cmc_approvalcomment").setDisabled(true);
            }
            if (Xrm.Page.getControl("cmc_approvaldate")) {
                Xrm.Page.getControl("cmc_approvaldate").setDisabled(true);
            }
            if (Xrm.Page.getControl("cmc_status")) {
                Xrm.Page.getControl("cmc_status").setDisabled(true);
            }
            if (Xrm.Page.getControl("cmc_statusdate")) {
                Xrm.Page.getControl("cmc_statusdate").setDisabled(true);
            }
            //BPF fields
            if (Xrm.Page.getControl("header_process_cmc_status")) {
                Xrm.Page.getControl("header_process_cmc_status").setDisabled(true);
            }
            if (Xrm.Page.getControl("header_process_cmc_approvedby")) {
                Xrm.Page.getControl("header_process_cmc_approvedby").setDisabled(true);
            }
            if (Xrm.Page.getControl("header_process_cmc_approvalcomment")) {
                Xrm.Page.getControl("header_process_cmc_approvalcomment").setDisabled(true);
            }
        }
        function handleTripApprovalProcess(executionContext) {
            // get logged in user details
            var liuId = Xrm.Page.context.getUserId();
            var liuName = Xrm.Page.context.getUserName();
            // get ApprovedBy user details
            var currApprovedBy = Xrm.Page.getAttribute("cmc_approvedby").getValue();
            disableFields();
            if (currApprovedBy) {
                var currApprovedById = currApprovedBy[0].id;
                if (liuId === currApprovedById) {
                    // if the logged in user is equal to approved by user then enable Review Stage fields for edition.
                    enableFields();
                }
            }
        }
    })(cmc_trip = CampusManagement.cmc_trip || (CampusManagement.cmc_trip = {}));
})(CampusManagement || (CampusManagement = {}));
