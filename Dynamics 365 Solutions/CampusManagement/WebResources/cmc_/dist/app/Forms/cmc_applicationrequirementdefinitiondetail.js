/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
var CampusManagement;
(function (CampusManagement) {
    var cmc_applicationrequirementdefinitiondetail;
    (function (cmc_applicationrequirementdefinitiondetail) {
        function onLoad(executionContext) {
            var formContext = executionContext.getFormContext();
            var _attributeRequirementTypeAttr = formContext.getAttribute("cmc_requirementtype");
            if (_attributeRequirementTypeAttr != null) {
                requirementTypeChange(executionContext);
                _attributeRequirementTypeAttr.addOnChange(CampusManagement.cmc_applicationrequirementdefinitiondetail.requirementTypeChange);
                _attributeRequirementTypeAttr.addOnChange(CampusManagement.cmc_applicationrequirementdefinitiondetail.resetName);
            }
        }
        cmc_applicationrequirementdefinitiondetail.onLoad = onLoad;
        function requirementTypeChange(executionContext) {
            var formContext = executionContext.getFormContext();
            var _attributeRequirementTypeAttr = formContext.getAttribute("cmc_requirementtype");
            if (_attributeRequirementTypeAttr != null) {
                showHideTestscore(formContext, _attributeRequirementTypeAttr.getValue() == 175490003 /* Testscore */);
            }
        }
        cmc_applicationrequirementdefinitiondetail.requirementTypeChange = requirementTypeChange;
        function resetName(executionContext) {
            var formContext = executionContext.getFormContext();
            var _attributeRequirementTypeAttr = formContext.getAttribute("cmc_requirementtype");
            if (_attributeRequirementTypeAttr != null) {
                var _nameAttr = formContext.getAttribute("cmc_name");
                if (_nameAttr != null)
                    _nameAttr.setValue('');
            }
        }
        cmc_applicationrequirementdefinitiondetail.resetName = resetName;
        function showHideTestscore(formContext, blnShow) {
            var _attributeTestSourceTypeAttr = formContext.getAttribute("cmc_testsourcetype");
            var _attributeTestSourceTypeControl = formContext.getControl("cmc_testsourcetype");
            var _attributeTestscoreTypeAttr = formContext.getAttribute("cmc_testscoretype");
            var _attributeTestscoreTypeControl = formContext.getControl("cmc_testscoretype");
            if (blnShow) {
                if (_attributeTestSourceTypeAttr != null) {
                    _attributeTestSourceTypeControl.setVisible(true);
                    _attributeTestSourceTypeAttr.setRequiredLevel("required");
                }
                if (_attributeTestscoreTypeAttr != null) {
                    _attributeTestscoreTypeControl.setVisible(true);
                    _attributeTestscoreTypeAttr.setRequiredLevel("required");
                }
            }
            else {
                if (_attributeTestSourceTypeAttr != null) {
                    _attributeTestSourceTypeControl.setVisible(false);
                    _attributeTestSourceTypeAttr.setValue("");
                    _attributeTestSourceTypeAttr.setRequiredLevel("none");
                }
                if (_attributeTestscoreTypeAttr != null) {
                    _attributeTestscoreTypeControl.setVisible(false);
                    _attributeTestscoreTypeAttr.setValue("");
                    _attributeTestscoreTypeAttr.setRequiredLevel("none");
                }
            }
        }
    })(cmc_applicationrequirementdefinitiondetail = CampusManagement.cmc_applicationrequirementdefinitiondetail || (CampusManagement.cmc_applicationrequirementdefinitiondetail = {}));
})(CampusManagement || (CampusManagement = {}));
