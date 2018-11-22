/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
/// <reference path="../../../node_modules/@types/kendo-ui/index.d.ts" />
var CampusManagement;
(function (CampusManagement) {
    var ToDoProgressBar;
    (function (ToDoProgressBar) {
        var localizedStrings;
        function executeOnLoad() {
            localizedStrings = {
                ErrorPrefix: CampusManagement.localization.getResourceString("ErrorPrefix"),
                okButton: CampusManagement.localization.getResourceString("OkButton")
            };
            var successPlanId = GetGlobalContext().getQueryStringParameters().id;
            var fetchTotalTodos = [
                '<fetch>',
                '<entity name="cmc_todo">',
                '<attribute name="cmc_todoid" />',
                '<filter type="and">',
                '<condition attribute="statuscode" operator="not-in">',
                '<value>175490001</value>',
                '<value>175490002</value>',
                '</condition>',
                '<condition attribute="cmc_successplanid" operator="eq" value="', successPlanId, '" />',
                '</filter>',
                '</entity>',
                '</fetch>'
            ].join('');
            var fetchCompleteToDos = [
                '<fetch>',
                '<entity name="cmc_todo">',
                '<attribute name="cmc_todoid" />',
                '<filter type="and">',
                '<condition attribute="statuscode" operator="eq" value="2" />',
                '<condition attribute="cmc_successplanid" operator="eq" value="', successPlanId, '" />',
                '</filter>',
                '</entity>',
                '</fetch>'
            ].join('');
            SonomaCmc.Promise.all([
                Xrm.WebApi.retrieveMultipleRecords('cmc_todo', '?fetchXml=' + fetchTotalTodos),
                Xrm.WebApi.retrieveMultipleRecords('cmc_todo', '?fetchXml=' + fetchCompleteToDos)
            ]).then(function (results) {
                var total = 0, complete = 0;
                if (results && results.length === 2) {
                    total = (results[0] && results[0].entities && results[0].entities.length) || 0;
                    complete = (results[1] && results[1].entities && results[1].entities.length) || 0;
                }
                createProgressBar(getProgressBarValue(total, complete));
            }, function (error) {
                multiLingualAlert(localizedStrings.ErrorPrefix, error.message);
            });
        }
        ToDoProgressBar.executeOnLoad = executeOnLoad;
        function getProgressBarValue(total, complete) {
            var percentComplete;
            if (complete === 0 && total > 0) {
                percentComplete = 0;
            }
            else if (total === 0) {
                percentComplete = 100;
            }
            else {
                percentComplete = new Number((complete / total) * 100);
            }
            return percentComplete;
        }
        function createProgressBar(percentComplete) {
            $("#progressbar").kendoProgressBar({
                min: 0,
                max: 100,
                value: percentComplete,
                type: "percent"
            });
        }
        function multiLingualAlert(defaultMessage, error) {
            // Parent.Xrm is used here as the alert dialog in this window does not display the
            // error message text.
            parent.Xrm.Navigation.openAlertDialog({
                text: defaultMessage + ': ' + error,
                confirmButtonLabel: localizedStrings.okButton
            }, null);
        }
    })(ToDoProgressBar = CampusManagement.ToDoProgressBar || (CampusManagement.ToDoProgressBar = {}));
})(CampusManagement || (CampusManagement = {}));
