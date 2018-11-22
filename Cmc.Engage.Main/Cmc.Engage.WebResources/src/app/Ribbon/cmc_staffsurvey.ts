module CampusManagement.ribbon.cmc_staffsurvey {

	declare let Xrm: any;

	export function FeedbackForm_OnSubmit(executionContext) {
		var windowObject = Xrm.Page.getControl("WebResource_FacultyFeedback").getObject().contentWindow.window;
		windowObject.angularComponentRef.zone.run(function () { windowObject.angularComponentRef.submitfn() });
    }
    export function checkForm() {
        if (Xrm.Page.ui.formSelector.getCurrentItem().getLabel() == Constants.StaffSurvey.SurveyFeedbackFormName) {
            return true;
        }
        else
            return false;
    }
}