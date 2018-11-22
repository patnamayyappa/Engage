/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />

module CampusManagement.CommonInboundInterestLogic {

  var _subCategoryFilterXml = "";

  export function onLoad(executionContext) {
    var formContext: Xrm.FormContext = executionContext.getFormContext();
    var entityLogicalName = formContext.data.entity.getEntityName();

    if (formContext.ui.getFormType() == XrmEnum.FormType.Create) {
      formContext.getAttribute("cmc_sourcedate").setValue(new Date());
    }

    if (formContext.ui.getFormType() !== XrmEnum.FormType.Create) {
      var initialSource = "source_section";
      CampusManagement.utilities.disableSection(executionContext, initialSource, true, true);
      if ((entityLogicalName === "lead" || entityLogicalName === "contact" || entityLogicalName === "opportunity")) {
        initialSource = "initial_source_section";
        CampusManagement.utilities.disableSection(executionContext, initialSource, true, true);
      }
    }

    filterSubCategoryByCategory(executionContext);

    if (formContext.ui.getFormType() !== XrmEnum.FormType.Create) {
      var initialSource = "source_section";
      CampusManagement.utilities.disableSection(executionContext, initialSource, true, true);
    }
  }
  function setControlVisibility(controls, isVisible) {
    controls.forEach(function (item) {
      item.setVisible(isVisible);
    });
  }
  function filterSubCategoryByCategory(executionContext) {
    var formContext = executionContext.getFormContext(),
      subCategory = formContext.getAttribute('cmc_sourcesubcategoryid'),
      subCategoryControl = formContext.getControl('cmc_sourcesubcategoryid'),
      category = formContext.getAttribute('cmc_sourcecategoryid'),
      referringContact = formContext.getControl('cmc_sourcereferringcontactid'),
      referringOrganization = formContext.getControl('cmc_sourcereferringorganizationid'),
      referringStaff = formContext.getControl('cmc_sourcereferringstaffid'),
      referringContactAttribute = formContext.getAttribute('cmc_sourcereferringcontactid'),
      referringOrganizationAttribute = formContext.getAttribute('cmc_sourcereferringorganizationid'),
      referringStaffAttribute = formContext.getAttribute('cmc_sourcereferringstaffid'),
      categoryLookup;
    let controls: Array<any> = [];
    controls.push(referringContact);
    controls.push(referringOrganization);
    controls.push(referringStaff);

    if (!subCategory || !subCategoryControl || !category)
      return;
    categoryLookup = category.getValue();
    if (!categoryLookup || categoryLookup.length <= 0) {
      subCategoryControl.setDisabled(true);
      setControlVisibility(controls, false);
    }
    else {
      if (categoryLookup[0].id == Constants.CommonInboundInterest.Referral) {
        setControlVisibility(controls, true);
      }
      else
        setControlVisibility(controls, false);
    }
    subCategoryControl.addPreSearch(function (executionContext) {
      if (_subCategoryFilterXml)
        executionContext.getEventSource().addCustomFilter(_subCategoryFilterXml, 'cmc_sourcesubcategory');
    });
    category.addOnChange(function (executionContext) {
      subCategory.setValue();

      categoryLookup = category.getValue();

      referringContactAttribute.setValue();
      referringOrganizationAttribute.setValue();
      referringStaffAttribute.setValue();

      if (!categoryLookup || categoryLookup.length <= 0) {
        subCategoryControl.setDisabled(true);
        setControlVisibility(controls, false);
        return;
      }
      subCategoryControl.setDisabled(false);

      if (categoryLookup[0].id == Constants.CommonInboundInterest.Referral) {
        setControlVisibility(controls, true);
      }
      else {
        setControlVisibility(controls, false);
      }
      var fetchXml = [
        '<fetch>',
        '<entity name="cmc_sourcesubcategory">',
        '<link-entity name="cmc_sourcecategory_sourcesubcategory" from="cmc_sourcesubcategoryid" to="cmc_sourcesubcategoryid" link-type="inner" >',
        '<filter>',
        '<condition operator="eq" attribute="cmc_sourcecategoryid" value="' + categoryLookup[0].id + '" />',
        '</filter>',
        '</link-entity>',
        '</entity>',
        '</fetch>'
      ].join('');

      Xrm.WebApi.retrieveMultipleRecords('cmc_sourcesubcategory', '?fetchXml=' + fetchXml)
        .then(
          function (results) {
            if (results.entities.length === 0) {
              _subCategoryFilterXml = [
                '<filter>',
                '<condition attribute="cmc_sourcesubcategoryid" operator="null" />',
                '</filter>'
              ].join('');

              return;
            }

            var valuesXml = [];
            results.entities.forEach(function (record) {
              valuesXml.push('<value>' + record.cmc_sourcesubcategoryid + '</value>');
            });

            _subCategoryFilterXml = [
              '<filter>',
              '<condition attribute="cmc_sourcesubcategoryid" operator="in">',
              valuesXml.join(''),
              '</condition>',
              '</filter>'
            ].join('');
          }
        );
    });
  }
}
