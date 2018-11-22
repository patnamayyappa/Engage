/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />

module CampusManagement.contact {
 
  export function onLoad(executionContext) {
    var formContext = executionContext.getFormContext();

    if (formContext.getAttribute('parentcustomerid')) {
      filterParentLookup(formContext);

      formContext.getAttribute('parentcustomerid').controls.forEach(function (control) {
        control.setEntityTypes(['account']);
      });
    }
    let primaryContactType: Xrm.Attributes.OptionSetAttribute = Xrm.Page.getAttribute("mshied_contacttype");
    if (primaryContactType !== undefined) {
      primaryContactType.addOnChange(CampusManagement.contact.initalSourceConditionalMandatory);
    }
    if (Xrm.Page.ui.getFormType() === XrmEnum.FormType.Update || Xrm.Page.ui.getFormType() === XrmEnum.FormType.Create) {
      initalSourceConditionalMandatory();
    }
  }

  function filterParentLookup(formContext) {
    var parentCustomerLookup = formContext.getControl('parentcustomerid');
    if (!parentCustomerLookup)
      return;

    parentCustomerLookup.addPreSearch(function (executionContext) {
      addInlineLookupFilters(executionContext);
    });

    parentCustomerLookup.setDefaultView('{7B89B0CD-54A1-E711-8103-5065F38B0191}'); // 'Active Campuses' View
  }

  function addInlineLookupFilters(executionContext) {
    // Inline Lookup - Remove Accounts not included in 'Active Campuses' View
    var accountFilterXml = [
      '<filter type="and">',
      '<condition attribute="statecode" operator="eq" value="0" />',
      '<condition attribute="mshied_accounttype" operator="eq" value="494280000" />', // Campus Account Type
      '</filter>'
    ].join('');

    executionContext.getEventSource().addCustomFilter(accountFilterXml, 'account');
  }
  //Initial Source Method and Category Conditional Mandatory If Primary Contact Type is Alumni or Student
  export function initalSourceConditionalMandatory() {
    let sourceMethod: Xrm.Attributes.LookupAttribute = Xrm.Page.getAttribute("cmc_sourcemethodid");
    let sourceCategory: Xrm.Attributes.LookupAttribute = Xrm.Page.getAttribute("cmc_sourcecategoryid");
    let primaryContactType: Xrm.Attributes.OptionSetAttribute = Xrm.Page.getAttribute("mshied_contacttype");
    if (primaryContactType !== undefined && primaryContactType.getSelectedOption() !== undefined) {
      let selectedItems: any = primaryContactType.getSelectedOption();
      if (selectedItems !== null && selectedItems !== undefined) {
        let isAlumniOrStudent: any = selectedItems.filter(function (e) { return (e.value == Constants.PrimaryContactType.Alumni || e.value == Constants.PrimaryContactType.Student) });
        if (isAlumniOrStudent.length > 0) {
          if (sourceMethod !== undefined)
            sourceMethod.setRequiredLevel("required");
          if (sourceCategory !== undefined)
            sourceCategory.setRequiredLevel("required");
        }
        else {
          if (sourceMethod !== undefined)
            sourceMethod.setRequiredLevel("none");
          if (sourceCategory !== undefined)
            sourceCategory.setRequiredLevel("none");
        }
      }
      else {
        if (sourceMethod !== undefined)
          sourceMethod.setRequiredLevel("none");
        if (sourceCategory !== undefined)
          sourceCategory.setRequiredLevel("none");
      }
    }
    else {
      if (sourceMethod !== undefined)
        sourceMethod.setRequiredLevel("none");
      if (sourceCategory !== undefined)
        sourceCategory.setRequiredLevel("none");
    }
  }
}
