/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
var CampusManagement;
(function (CampusManagement) {
    var cmc_todo;
    (function (cmc_todo) {
        function onLoad(executionContext) {
            var formContext = executionContext.getFormContext();
            if (formContext.getAttribute('cmc_successplanid')) {
                filterSuccessPlanLookup(formContext);
                disableSuccessPlanField(executionContext);
            }
        }
        cmc_todo.onLoad = onLoad;
        function disableSuccessPlanField(executionContext) {
            var formContext = executionContext.getFormContext();
            formContext.getControl("cmc_successplanid").setDisabled(formContext.ui.getFormType() !== 1 /* Create */);
        }
        function filterSuccessPlanLookup(formContext) {
            var successPlanLookup = formContext.getControl('cmc_successplanid');
            if (!successPlanLookup)
                return;
            successPlanLookup.addPreSearch(function (executionContext) {
                addInlineLookupFilters(executionContext);
            });
        }
        function addInlineLookupFilters(executionContext) {
            if (executionContext.getFormContext().getAttribute('cmc_assignedtostudentid')
                && executionContext.getFormContext().getAttribute('cmc_assignedtostudentid').getValue()
                && executionContext.getFormContext().getAttribute('cmc_assignedtostudentid').getValue().length > 0) {
                var studentId = executionContext.getFormContext().getAttribute('cmc_assignedtostudentid').getValue()[0].id;
                var successPlanFilterXml = [
                    '<filter type="and">',
                    '<condition attribute="cmc_assignedtoid" operator="eq" value="' + studentId + '"/>',
                    '</filter>'
                ].join('');
                executionContext.getEventSource().addCustomFilter(successPlanFilterXml, 'cmc_successplan');
            }
        }
    })(cmc_todo = CampusManagement.cmc_todo || (CampusManagement.cmc_todo = {}));
})(CampusManagement || (CampusManagement = {}));
