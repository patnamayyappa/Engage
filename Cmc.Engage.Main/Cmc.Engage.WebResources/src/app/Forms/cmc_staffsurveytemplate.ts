/// <reference path="../../../node_modules/@types/jquery/index.d.ts" />
module CampusManagement.cmc_staffsurveytemplate {

export 	function Form_OnSave(executionContext) {
		//disbaled auto save for the designer form
		executionContext.getEventArgs().preventDefault();
		if (executionContext.getEventArgs().getSaveMode() != XrmEnum.SaveMode.AutoSave) {
			let control: any = Xrm.Page.getControl(Constants.StaffSurvey.SurveyTemplateWebResource);
			let windowObject = control.getObject().contentWindow.window;
			windowObject.angularComponentRef.zone.run(function () { windowObject.angularComponentRef.componentFn() });
		}
	}

export 	function onDesignerLoad() {

		var _loadFunction = Xrm.Page.ui.getFormType() == XrmEnum.FormType.Create ?
			navigateToForm(Constants.StaffSurvey.SurveyTemplateFormName) :
			onLoadContinue();
	}
	 function onLoadContinue() {
		if (Xrm.Page.ui.getFormType() != XrmEnum.FormType.Create) {
			let template: any = Xrm.Page.ui.controls.get(Constants.StaffSurvey.SurveyTemplateWebResource);
			template.setVisible(!0)
		}
	}

	function navigateToForm(formName) {
		let items = Xrm.Page.ui.formSelector.items.get();
		for (let item in items) {
			var form = items[item],
				formId = form.getId(),
				formLabel = form.getLabel();
			formLabel == formName && Xrm.Page.ui.formSelector.items.get(formId).navigate();
		}
	}

}