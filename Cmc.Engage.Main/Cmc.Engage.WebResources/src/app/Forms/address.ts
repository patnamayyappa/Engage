/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />

module CampusManagement.address {

    export function onLoad(executionContext) {
        var formContext: Xrm.FormContext = executionContext.getFormContext();
        
        if (formContext.ui.getFormType() !== XrmEnum.FormType.Create) {
            formContext.ui.controls.get<Xrm.Controls.StandardControl>("cmc_customerid").setDisabled(true);
        }

        if (!disablePrimaryFlagIfSet(executionContext)) {
            formContext.data.entity.addOnSave(disablePrimaryFlagIfSet);
        }
    }

    function disablePrimaryFlagIfSet(executionContext) {
        var formContext: Xrm.FormContext = executionContext.getFormContext();

        if (formContext.getAttribute("cmc_isprimary").getValue() === true) {
            formContext.ui.controls.get<Xrm.Controls.StandardControl>("cmc_isprimary").setDisabled(true);
            return true;
        }
        return false;
    }

}