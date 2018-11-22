/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
/// <reference path="../../../node_modules/@types/kendo-ui/index.d.ts" />

module CMC.Engage.cmc_applicationrequirement {

  enum RequirementType {
    Testscore = 175490003,
    General = 175490000,
    Upload = 175490001,
    UnofficialTranscript = 175490005,
    OfficialTranscript = 175490004,
    Recommendation = 175490002
  }
  declare var CampusManagement: any;
  var _defaultId = '00000000-0000-0000-0000-000000000000',
    _contactId = _defaultId,
    _formLoaded;

  export function onLoad(executionContext) {
    var formContext = executionContext.getFormContext(),
      applicationId = formContext.getAttribute('cmc_applicationid'),
      testScoreId = formContext.getAttribute('cmc_testscoreid'),
      previousEducationId = formContext.getAttribute('cmc_previouseducationid'),
      attributeRequirementType = formContext.getAttribute("cmc_requirementtype");

    if (_formLoaded) {
      return;
    }
    
    _formLoaded = true;

    if (attributeRequirementType != null) {
      onRequirementTypeChange(executionContext);
      attributeRequirementType.addOnChange(CMC.Engage.cmc_applicationrequirement.onRequirementTypeChange);
    }

    if (!applicationId) {
      return;
    }

    applicationId.addOnChange(onApplicationIdChange);
    retrieveFilterRecords(formContext, true);

    if (previousEducationId) {
      addPreSearchFunction(previousEducationId, onPreviousEducationPreSearch);
    }

    if (testScoreId) {
      addPreSearchFunction(testScoreId, onTestScorePreSearch);
    }
  }

  function onApplicationIdChange(executionContext) {
    var formContext = executionContext.getFormContext(),
      recommendationId = formContext.getAttribute('cmc_recommendationid');

    retrieveFilterRecords(formContext);

    if (recommendationId && recommendationId.getValue() && recommendationId.getValue().length >= 0) {
      recommendationId.setValue(null);
      recommendationId.fireOnChange();
    }
  }

  export function onRequirementTypeChange(executionContext) {
    var formContext: Xrm.FormContext = executionContext.getFormContext();
    var _attributeRequirementTypeAttr = formContext.getAttribute("cmc_requirementtype");
    if (_attributeRequirementTypeAttr != null) {
      var reqType = _attributeRequirementTypeAttr.getValue();
      changeControlIsNotRequired(formContext, reqType !== RequirementType.Recommendation
        && reqType !== RequirementType.UnofficialTranscript
        && reqType !== RequirementType.Testscore
        && reqType !== RequirementType.OfficialTranscript); 
    }  
  }

  function changeControlIsNotRequired(formContext: Xrm.FormContext, enableEditing: boolean) {
    var attribute: Xrm.Attributes.Attribute = formContext.getAttribute("cmc_applicationrequirementname");
    var control: any = formContext.getControl("cmc_applicationrequirementname");
    if (enableEditing) {
      attribute.setRequiredLevel("required");
      control.setDisabled(false);
    } else {
      attribute.setRequiredLevel("none");
      control.setDisabled(true);
    }
  }
  function retrieveFilterRecords(formContext, skipClearContactFields = null) {
    var applicationId = formContext.getAttribute('cmc_applicationid');

    if (!applicationId || !applicationId.getValue() || applicationId.getValue().length <= 0) {
      updateContactId(_defaultId, formContext, skipClearContactFields);
      return;
    }

    Xrm.WebApi.retrieveRecord('cmc_application', applicationId.getValue()[0].id, '?$expand=cmc_contactid($select=contactid)').then(
      function (result) {
        var newContactId = (result.cmc_contactid && result.cmc_contactid.contactid) || _defaultId;
        updateContactId(newContactId, formContext, skipClearContactFields);
      },
      function () {
        // If someething goes wrong, set the Contact Id back to the default id
        updateContactId(_defaultId, formContext, skipClearContactFields);
      });
  }

  function updateContactId(newContactId, formContext, skipClearContactFields) {
    var testScoreId = formContext.getAttribute('cmc_testscoreid'),
      previousEducationId = formContext.getAttribute('cmc_previouseducationid');

    if (newContactId !== _contactId && !skipClearContactFields) {
      if (testScoreId && testScoreId.getValue() && testScoreId.getValue().length >= 0) {
        testScoreId.setValue(null);
        testScoreId.fireOnChange();
      }

      if (previousEducationId && previousEducationId.getValue() && previousEducationId.getValue().length >= 0) {
        previousEducationId.setValue(null);
        previousEducationId.fireOnChange();
      }
    }

    _contactId = newContactId;
  }

  function addPreSearchFunction(attribute, preSearchFunction) {
    attribute.controls.forEach(function (control) {
      control.addPreSearch(preSearchFunction);
    });
  }

  function onPreviousEducationPreSearch(executionContext) {
   addCustomFilter('cmc_previouseducationid',
      `<filter type='and'>
         <condition attribute='mshied_studentid' operator='eq' value='${_contactId}' />
       </filter>`, 'mshied_previouseducation', executionContext);
  }

  function onTestScorePreSearch(executionContext) {
    addCustomFilter('cmc_testscoreid',
      `<filter type='and'>
         <condition attribute='mshied_studentid' operator='eq' value='${_contactId}' />
       </filter>`, 'mshied_testscore', executionContext);
  }

  function addCustomFilter(attributeName, filter, entityName, executionContext) {
    executionContext.getFormContext().getAttribute(attributeName).controls.forEach(function (control) {
      control.addCustomFilter(filter, entityName)
    });
  }
}
