/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
var CampusManagement;
(function (CampusManagement) {
    var XrmSystemInformation;
    (function (XrmSystemInformation) {
        function getApiVersion() {
            var currentVersion = '8.0', globalContext;
            if (!window.GetGlobalContext) {
                globalContext = window.GetGlobalContext();
                currentVersion = globalContext.getVersion();
                currentVersion = (currentVersion)
                    ? currentVersion.split('.').slice(0, 2).join('.')
                    : '8.0';
            }
            return currentVersion;
        }
        XrmSystemInformation.getApiVersion = getApiVersion;
    })(XrmSystemInformation = CampusManagement.XrmSystemInformation || (CampusManagement.XrmSystemInformation = {}));
})(CampusManagement || (CampusManagement = {}));
