/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
var CampusManagement;
(function (CampusManagement) {
    var lead;
    (function (lead) {
        function onLoad(executionContext) {
            var formContext = executionContext.getFormContext(), referringContactId;
            if (formContext.getAttribute('customerid')) {
                formContext.getAttribute('customerid').controls.forEach(function (control) {
                    control.setEntityTypes(['contact']);
                });
            }
            if (formContext.ui.getFormType() === 1 /* Create */ &&
                formContext.getAttribute('cmc_sourcereferringcontactid')) {
                referringContactId = formContext.getAttribute('cmc_sourcereferringcontactid');
                if (referringContactId.getValue() && referringContactId.getValue().length > 0) {
                    referringContactId.setValue(null);
                }
            }
        }
        lead.onLoad = onLoad;
    })(lead = CampusManagement.lead || (CampusManagement.lead = {}));
})(CampusManagement || (CampusManagement = {}));
