/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
var CampusManagement;
(function (CampusManagement) {
    var ribbon;
    (function (ribbon) {
        var cmc_staffcourse;
        (function (cmc_staffcourse) {
            function AssignStaffCourse(staffIds, fromForm) {
                SonomaCmc.LocalStorage.set('cmc_AssignStaffIds', staffIds, 1);
                SonomaCmc.LocalStorage.set('cmc_AssignStaffCourseFromForm', fromForm, 1);
                Xrm.Navigation.openWebResource('cmc_/Pages/AssignStaffCourse.html', {
                    height: 250,
                    width: 456,
                    openInNewWindow: true
                });
            }
            cmc_staffcourse.AssignStaffCourse = AssignStaffCourse;
            function AssignStaffCourseForm() {
                AssignStaffCourse([Xrm.Page.data.entity.getId()], true);
            }
            cmc_staffcourse.AssignStaffCourseForm = AssignStaffCourseForm;
        })(cmc_staffcourse = ribbon.cmc_staffcourse || (ribbon.cmc_staffcourse = {}));
    })(ribbon = CampusManagement.ribbon || (CampusManagement.ribbon = {}));
})(CampusManagement || (CampusManagement = {}));
