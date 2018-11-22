var CampusManagement;
(function (CampusManagement) {
    var ribbon;
    (function (ribbon) {
        var cmc_staffsurvey;
        (function (cmc_staffsurvey) {
            function FeedbackForm_OnSubmit(executionContext) {
                var windowObject = Xrm.Page.getControl("WebResource_FacultyFeedback").getObject().contentWindow.window;
                windowObject.angularComponentRef.zone.run(function () { windowObject.angularComponentRef.submitfn(); });
            }
            cmc_staffsurvey.FeedbackForm_OnSubmit = FeedbackForm_OnSubmit;
            function checkForm() {
                if (Xrm.Page.ui.formSelector.getCurrentItem().getLabel() == Constants.StaffSurvey.SurveyFeedbackFormName) {
                    return true;
                }
                else
                    return false;
            }
            cmc_staffsurvey.checkForm = checkForm;
        })(cmc_staffsurvey = ribbon.cmc_staffsurvey || (ribbon.cmc_staffsurvey = {}));
    })(ribbon = CampusManagement.ribbon || (CampusManagement.ribbon = {}));
})(CampusManagement || (CampusManagement = {}));
