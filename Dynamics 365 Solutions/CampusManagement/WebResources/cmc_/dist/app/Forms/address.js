/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
var CampusManagement;
(function (CampusManagement) {
    var address;
    (function (address) {
        function onLoad(executionContext) {
            var formContext = executionContext.getFormContext();
            if (formContext.ui.getFormType() !== 1 /* Create */) {
                formContext.ui.controls.get("cmc_customerid").setDisabled(true);
            }
            if (!disablePrimaryFlagIfSet(executionContext)) {
                formContext.data.entity.addOnSave(disablePrimaryFlagIfSet);
            }
        }
        address.onLoad = onLoad;
        function disablePrimaryFlagIfSet(executionContext) {
            var formContext = executionContext.getFormContext();
            if (formContext.getAttribute("cmc_isprimary").getValue() === true) {
                formContext.ui.controls.get("cmc_isprimary").setDisabled(true);
                return true;
            }
            return false;
        }
    })(address = CampusManagement.address || (CampusManagement.address = {}));
})(CampusManagement || (CampusManagement = {}));
