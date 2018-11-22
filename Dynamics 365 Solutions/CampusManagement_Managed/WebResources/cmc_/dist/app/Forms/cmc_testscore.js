var CampusManagement;
(function (CampusManagement) {
    var cmc_testscore;
    (function (cmc_testscore) {
        function navigateToForm() {
            if (Xrm.Page.getAttribute("mshied_testsource")) {
                Xrm.Page.getAttribute("mshied_testsource").addOnChange(CampusManagement.cmc_testscore.SetFlagforScoreCalculation);
            }
            if (Xrm.Page.ui.getFormType() != 1 /* Create */) {
                var testType = Xrm.Page.getAttribute('mshied_testtypeid').getValue()[0].name;
                var formTypes = Xrm.Page.ui.formSelector.items.get();
                if (Xrm.Page.ui.formSelector.getCurrentItem().getLabel() === testType)
                    return;
                for (var type in formTypes) {
                    var i = formTypes[type], id = i.getId(), label = i.getLabel();
                    if (label === testType)
                        Xrm.Page.ui.formSelector.items.get(id).navigate();
                }
            }
        }
        cmc_testscore.navigateToForm = navigateToForm;
        function SetTesttype(executionContext) {
            if (Xrm.Page.ui.getFormType() === 1 /* Create */) {
                var formType = Xrm.Page.ui.formSelector.getCurrentItem().getLabel(), localizedStrings = {
                    errorPrefix: CampusManagement.localization.getResourceString("ErrorPrefix"),
                    okButton: CampusManagement.localization.getResourceString("OkButton"),
                    TypeNotSet: CampusManagement.localization.getResourceString("Generic_ContactAdminError"),
                    TestTypeNotConfigured: CampusManagement.localization.getResourceString("TestType_TypeNotConfigured")
                };
                $.ajax({
                    type: "GET",
                    contentType: "application/json; charset=utf-8",
                    url: window.parent.Xrm.Page.context.getClientUrl() + "/api/data/v9.0/mshied_testtypes?$select=mshied_name&$filter=mshied_name eq '" + formType + "'",
                    beforeSend: function (XMLHttpRequest) {
                        XMLHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
                        XMLHttpRequest.setRequestHeader("OData-Version", "4.0");
                        XMLHttpRequest.setRequestHeader("Accept", "application/json");
                    },
                    async: false,
                    success: function (data, textStatus, xhr) {
                        var testType = data.value[0];
                        if (testType)
                            Xrm.Page.getAttribute("mshied_testtypeid").setValue([
                                { id: testType.mshied_testtypeid, name: testType.mshied_name, entityType: "mshied_testtype" }
                            ]);
                        else {
                            executionContext.getEventArgs().preventDefault();
                            CampusManagement.CommonUiControls.openAlertDialog(localizedStrings.TestTypeNotConfigured, localizedStrings.okButton);
                        }
                    },
                    error: function (xhr, textStatus, errorThrown) {
                        executionContext.getEventArgs().preventDefault();
                        CampusManagement.CommonUiControls.openAlertDialog(localizedStrings.TypeNotSet, localizedStrings.okButton);
                    }
                });
            }
        }
        cmc_testscore.SetTesttype = SetTesttype;
        function SetFlagforScoreCalculation() {
            var testSource = Xrm.Page.getAttribute("mshied_testsource").getValue();
            if (testSource === Constants.TestSource.SelfReported)
                Xrm.Page.getAttribute("cmc_includeinscorecalculations").setValue(false);
            else
                Xrm.Page.getAttribute("cmc_includeinscorecalculations").setValue(true);
        }
        cmc_testscore.SetFlagforScoreCalculation = SetFlagforScoreCalculation;
    })(cmc_testscore = CampusManagement.cmc_testscore || (CampusManagement.cmc_testscore = {}));
})(CampusManagement || (CampusManagement = {}));
