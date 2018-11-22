/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />

module CampusManagement.cmc_applicationrequirementdefinitiondetail{
  const enum RequirementType{
    Testscore = 175490003
  }

  export function onLoad(executionContext) {
    var formContext: Xrm.FormContext = executionContext.getFormContext();
    var _attributeRequirementTypeAttr = formContext.getAttribute("cmc_requirementtype");
    if (_attributeRequirementTypeAttr != null)
    {
      requirementTypeChange(executionContext);
      _attributeRequirementTypeAttr.addOnChange(CampusManagement.cmc_applicationrequirementdefinitiondetail.requirementTypeChange);
      _attributeRequirementTypeAttr.addOnChange(CampusManagement.cmc_applicationrequirementdefinitiondetail.resetName);
    }
      
  }

  export function requirementTypeChange(executionContext) {
    var formContext: Xrm.FormContext = executionContext.getFormContext();
    var _attributeRequirementTypeAttr = formContext.getAttribute("cmc_requirementtype");
    if (_attributeRequirementTypeAttr != null) {
       showHideTestscore(formContext, _attributeRequirementTypeAttr.getValue() == RequirementType.Testscore);
    }  
  }

  export function resetName(executionContext) {
    var formContext: Xrm.FormContext = executionContext.getFormContext();
    var _attributeRequirementTypeAttr = formContext.getAttribute("cmc_requirementtype");
    if (_attributeRequirementTypeAttr != null) {
      var _nameAttr = formContext.getAttribute("cmc_name");
      if (_nameAttr != null)
        _nameAttr.setValue('');
    }
  }

  
  function showHideTestscore(formContext, blnShow) {
      var _attributeTestSourceTypeAttr = formContext.getAttribute("cmc_testsourcetype");
      let _attributeTestSourceTypeControl: any = formContext.getControl("cmc_testsourcetype");
      var _attributeTestscoreTypeAttr = formContext.getAttribute("cmc_testscoretype");
      let _attributeTestscoreTypeControl: any = formContext.getControl("cmc_testscoretype");
      if (blnShow){
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

}
