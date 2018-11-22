/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
var CampusManagement;
(function (CampusManagement) {
    var marketinglist;
    (function (marketinglist) {
        function onLoad(executionContext) {
            var formContext = executionContext.getFormContext();
            formContext.getControl("cmc_marketinglisttype").setDisabled(formContext.ui.getFormType() !== 1 /* Create */);
        }
        marketinglist.onLoad = onLoad;
    })(marketinglist = CampusManagement.marketinglist || (CampusManagement.marketinglist = {}));
})(CampusManagement || (CampusManagement = {}));
