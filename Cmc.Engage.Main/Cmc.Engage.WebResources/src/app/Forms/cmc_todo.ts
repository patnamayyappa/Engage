/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />

module CampusManagement.cmc_todo {

    export function onLoad(executionContext) {
        var formContext = executionContext.getFormContext();

        if (formContext.getAttribute('cmc_successplanid')) {
            filterSuccessPlanLookup(formContext);
            disableSuccessPlanField(executionContext);
        }        
    }

    function disableSuccessPlanField(executionContext) {
        var formContext: Xrm.FormContext = executionContext.getFormContext();        
        formContext.getControl<Xrm.Controls.LookupControl>("cmc_successplanid").setDisabled(formContext.ui.getFormType() !== XrmEnum.FormType.Create);        
    }

    function filterSuccessPlanLookup(formContext) {
        var successPlanLookup = formContext.getControl('cmc_successplanid');
        if (!successPlanLookup)
            return;

        successPlanLookup.addPreSearch(function (executionContext) {
            addInlineLookupFilters(executionContext);
        });       
    }

    function addInlineLookupFilters(executionContext) {       
        if (executionContext.getFormContext().getAttribute('cmc_assignedtostudentid')
            && executionContext.getFormContext().getAttribute('cmc_assignedtostudentid').getValue() 
            && executionContext.getFormContext().getAttribute('cmc_assignedtostudentid').getValue().length > 0) {
            var studentId = executionContext.getFormContext().getAttribute('cmc_assignedtostudentid').getValue()[0].id;
            var successPlanFilterXml = [
                '<filter type="and">',
                '<condition attribute="cmc_assignedtoid" operator="eq" value="' + studentId + '"/>',
                '</filter>'
            ].join('');

            executionContext.getEventSource().addCustomFilter(successPlanFilterXml, 'cmc_successplan');
        }       
    }
}