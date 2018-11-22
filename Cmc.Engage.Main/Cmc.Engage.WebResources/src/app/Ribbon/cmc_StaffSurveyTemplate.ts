﻿module CampusManagement.ribbon.cmc_StaffSurveyTemplate {
    declare let SonomaCmc;

    export function copy() {
        var recordId = SonomaCmc.Guid.decapsulate(Xrm.Page.data.entity.getId()),
            localizedStrings = {
                errorPrefix: CampusManagement.localization.getResourceString("ErrorPrefix"),
                okButton: CampusManagement.localization.getResourceString("OkButton"),
            };
        if (!Xrm.Page.data.entity.getIsDirty()) {
            var confirmStrings: Xrm.Navigation.ConfirmStrings = { text: CampusManagement.localization.getResourceString("StaffSurveyTemplate_Copy_DialogMessage"), title: CampusManagement.localization.getResourceString("StaffSurveyTemplate_Copy_DialogTitle") };
            var confirmOptions: Xrm.Navigation.DialogSizeOptions = { height: 250, width: 500 };
            Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
                function (success) {
                    if (success.confirmed) {
                        Xrm.Utility.showProgressIndicator(CampusManagement.localization.getResourceString("StaffSurveyTemplate_Copy_Loading"));
                        SonomaCmc.WebAPI.post('cmc_staffsurveytemplates(' + recordId + ')/Microsoft.Dynamics.CRM.cmc_CopyStaffSurveyTemplate', {})
                            .then(function (result) {
                                if (result.copysurveyTemplateResponse) {
                                    Xrm.Navigation.openForm({
                                        entityId: (result.copysurveyTemplateResponse).toString(),
                                        entityName: 'cmc_staffsurveytemplate',
                                        openInNewWindow: true,
                                    }, null).then(
                                        function (success) {
                                            Xrm.Utility.closeProgressIndicator();
                                        },
                                        function (error) {
                                            Xrm.Utility.closeProgressIndicator();
                                            CampusManagement.CommonUiControls.openAlertDialog(localizedStrings.errorPrefix + ": "+ error.message, localizedStrings.okButton);
                                        });
                                }
                            }).catch(function (error) {
                                Xrm.Utility.closeProgressIndicator();
                                CampusManagement.CommonUiControls.openAlertDialog(localizedStrings.errorPrefix + ": "+ error.message, localizedStrings.okButton);
                            });
                    }
                }, function (cancel) {
                    Xrm.Utility.closeProgressIndicator();
                    return;
                });
        }
        else {
            CampusManagement.CommonUiControls.openAlertDialog(CampusManagement.localization.getResourceString("RecordFieldDirtyMessage"), localizedStrings.okButton);
            return;
        }
    }
    export function checkForm() {
        if (Xrm.Page.ui.formSelector.getCurrentItem().getLabel() == Constants.StaffSurvey.SurveyTemplateFormName) {
            return true;
        }
        else
            return false;
    }
}