/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
module CampusManagement.ribbon.cmc_staffcourse {

	declare var SonomaCmc: any;
	export function AssignStaffCourse(staffIds, fromForm) {
		SonomaCmc.LocalStorage.set('cmc_AssignStaffIds', staffIds, 1);
		SonomaCmc.LocalStorage.set('cmc_AssignStaffCourseFromForm', fromForm, 1);
		Xrm.Navigation.openWebResource('cmc_/Pages/AssignStaffCourse.html', {
			height: 250,
			width: 456,
			openInNewWindow: true
		});
	}

	export function AssignStaffCourseForm() {
		AssignStaffCourse([Xrm.Page.data.entity.getId()], true);
	}
}