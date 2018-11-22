/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
module CampusManagement.staffsurvey {
	declare let Xrm: any;
	declare let SonomaCmc: any;
	export function FeedbackForm_OnSave(executionContext) {

		var windowObject = Xrm.Page.getControl("WebResource_FacultyFeedback").getObject().contentWindow.window;
		windowObject.angularComponentRef.zone.run(function () { windowObject.angularComponentRef.componentfn() });



	}
	export function onDesignerLoad(form, control) {//"Information""WebResource_FacultyFeedback"
		var _loadFunction = Xrm.Page.ui.getFormType() == XrmEnum.FormType.Create ?
			navigateToForm(form) :
			onLoadContinue(control);

	}
	function onLoadContinue(control) {
		if (Xrm.Page.ui.getFormType() != XrmEnum.FormType.Create) {
			var n = Xrm.Page.ui.controls.get(control);
			n.setVisible(!0)
		}

	}

	export function onStaffSurveyLoad(executionContext) {
      var formContext = executionContext.getFormContext();
      formContext.getAttribute("cmc_coursesectionid").addOnChange(CampusManagement.staffsurvey.setInstructor);
		if (Xrm.Page.ui.getFormType() == 2) {
			var controls = Xrm.Page.ui.controls.get();
			for (var i in controls) {
				if (controls[i].getName() != "cmc_cancellationcomment")
					formContext.ui.controls.get(controls[i].getName()).setDisabled(true);
			}
		}
	}


	export function setInstructor(executionContext) {
		var formContext: Xrm.FormContext = executionContext.getFormContext();

      var staffCourse = Xrm.Page.getAttribute("cmc_coursesectionid").getValue();
		var staffCourseId = staffCourse[0].id.replace('{', '').replace('}', '');
		$.ajax({
			type: "GET",
			contentType: "application/json; charset=utf-8",

          url: window.parent.Xrm.Page.context.getClientUrl() + "/api/data/v9.0/mshied_coursesections(" + staffCourseId + ")?$expand=cmc_staffid($select=ownerid,fullname)",

			beforeSend: function (XMLHttpRequest) {
				XMLHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
				XMLHttpRequest.setRequestHeader("OData-Version", "4.0");
				XMLHttpRequest.setRequestHeader("Accept", "application/json");
			},
			async: false,
			success: function (data, textStatus, xhr) {

              Xrm.Page.getAttribute("cmc_userid").setValue([{ id: data.cmc_staffid.ownerid, name: data.cmc_staffid.fullname, entityType: "systemuser" }]);
			},
			error: function (xhr, textStatus, errorThrown) {
				
			}
		});
	}

	function navigateToForm(n) {
		var t = Xrm.Page.ui.formSelector.items.get();
		for (var r in t) {
			var i = t[r],
				u = i.getId(),
				f = i.getLabel();
			f == n && Xrm.Page.ui.formSelector.items.get(u).navigate()
		}
	}
}
