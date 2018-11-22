/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
var CampusManagement;
(function (CampusManagement) {
    var staffsurvey;
    (function (staffsurvey) {
        function FeedbackForm_OnSave(executionContext) {
            var windowObject = Xrm.Page.getControl("WebResource_FacultyFeedback").getObject().contentWindow.window;
            windowObject.angularComponentRef.zone.run(function () { windowObject.angularComponentRef.componentfn(); });
        }
        staffsurvey.FeedbackForm_OnSave = FeedbackForm_OnSave;
        function onDesignerLoad(form, control) {
            var _loadFunction = Xrm.Page.ui.getFormType() == 1 /* Create */ ?
                navigateToForm(form) :
                onLoadContinue(control);
        }
        staffsurvey.onDesignerLoad = onDesignerLoad;
        function onLoadContinue(control) {
            if (Xrm.Page.ui.getFormType() != 1 /* Create */) {
                var n = Xrm.Page.ui.controls.get(control);
                n.setVisible(!0);
            }
        }
        function onStaffSurveyLoad(executionContext) {
            var formContext = executionContext.getFormContext();
            if (Xrm.Page.ui.getFormType() == 2) {
                var controls = Xrm.Page.ui.controls.get();
                for (var i in controls) {
                    if (controls[i].getName() != "cmc_cancellationcomment")
                        formContext.ui.controls.get(controls[i].getName()).setDisabled(true);
                }
            }
        }
        staffsurvey.onStaffSurveyLoad = onStaffSurveyLoad;
        function setInstructor(executionContext) {
            var formContext = executionContext.getFormContext();
            var staffCourse = Xrm.Page.getAttribute("cmc_coursesectionid").getValue();
            var staffCourseId = staffCourse[0].id.replace('{', '').replace('}', '');
            $.ajax({
                type: "GET",
                contentType: "application/json; charset=utf-8",
                url: window.parent.Xrm.Page.context.getClientUrl() + "/api/data/v9.0/mshied_coursesection(" + staffCourseId + ")?$expand=mshied_instructorid($select=ownerid,fullname)",
                beforeSend: function (XMLHttpRequest) {
                    XMLHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
                    XMLHttpRequest.setRequestHeader("OData-Version", "4.0");
                    XMLHttpRequest.setRequestHeader("Accept", "application/json");
                },
                async: false,
                success: function (data, textStatus, xhr) {
                    Xrm.Page.getAttribute("cmc_userid").setValue([{ id: data.mshied_instructorid.ownerid, name: data.mshied_instructorid.fullname, entityType: "systemuser" }]);
                },
                error: function (xhr, textStatus, errorThrown) {
                }
            });
        }
        staffsurvey.setInstructor = setInstructor;
        function navigateToForm(n) {
            var t = Xrm.Page.ui.formSelector.items.get();
            for (var r in t) {
                var i = t[r], u = i.getId(), f = i.getLabel();
                f == n && Xrm.Page.ui.formSelector.items.get(u).navigate();
            }
        }
    })(staffsurvey = CampusManagement.staffsurvey || (CampusManagement.staffsurvey = {}));
})(CampusManagement || (CampusManagement = {}));
