/// <reference path="../../../node_modules/@types/jquery/index.d.ts" />
var CampusManagement;
(function (CampusManagement) {
    var cmc_staffsurveytemplate;
    (function (cmc_staffsurveytemplate) {
        function Form_OnSave(executionContext) {
            //disbaled auto save for the designer form
          
 executionContext.getEventArgs().preventDefault();
            if ( executionContext.getEventArgs().getSaveMode() != 70 /* AutoSave */) {
                var control = Xrm.Page.getControl(Constants.StaffSurvey.SurveyTemplateWebResource);
                var windowObject_1 = control.getObject().contentWindow.window;
                windowObject_1.angularComponentRef.zone.run(function () { windowObject_1.angularComponentRef.componentFn(); });
            }
        }
        cmc_staffsurveytemplate.Form_OnSave = Form_OnSave;
        function onDesignerLoad() {
            var _loadFunction = Xrm.Page.ui.getFormType() == 1 /* Create */ ?
                navigateToForm(Constants.StaffSurvey.SurveyTemplateFormName) :
                onLoadContinue();
        }
        cmc_staffsurveytemplate.onDesignerLoad = onDesignerLoad;
        function onLoadContinue() {
            if (Xrm.Page.ui.getFormType() != 1 /* Create */) {
                var template = Xrm.Page.ui.controls.get(Constants.StaffSurvey.SurveyTemplateWebResource);
                template.setVisible(!0);
            }
        }
        function navigateToForm(formName) {
            var items = Xrm.Page.ui.formSelector.items.get();
            for (var item in items) {
                var form = items[item], formId = form.getId(), formLabel = form.getLabel();
                formLabel == formName && Xrm.Page.ui.formSelector.items.get(formId).navigate();
            }
        }
    })(cmc_staffsurveytemplate = CampusManagement.cmc_staffsurveytemplate || (CampusManagement.cmc_staffsurveytemplate = {}));
})(CampusManagement || (CampusManagement = {}));
