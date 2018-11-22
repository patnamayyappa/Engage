/// <reference path="../common/utilities.ts" />
var CampusManagement;
(function (CampusManagement) {
    var msdyn_surveyinvite;
    (function (msdyn_surveyinvite) {
        function onLoad(executionContext) {
            if (CampusManagement.utilities && CampusManagement.utilities.setActivityToFieldForContactOnLoad)
                CampusManagement.utilities.setActivityToFieldForContactOnLoad(executionContext);
        }
        msdyn_surveyinvite.onLoad = onLoad;
    })(msdyn_surveyinvite = CampusManagement.msdyn_surveyinvite || (CampusManagement.msdyn_surveyinvite = {}));
})(CampusManagement || (CampusManagement = {}));
