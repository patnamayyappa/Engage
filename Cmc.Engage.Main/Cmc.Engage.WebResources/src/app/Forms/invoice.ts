/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
module CMC.Engage.invoice {
  var _formLoaded;

// All logic for the Invoice Form regarding an Application occurs in JavaScript for two reasons:
// 1. To keep the code in a single place.
// 2. JavaScript can be reused across selected forms.
// Business Rules can run across a single form or all forms.
// They cannot be selected depending on the form.
  export function onLoad(executionContext) {
    var formContext = executionContext.getFormContext(),
      shipTo = formContext.getAttribute('willcall'),
      applicationId = formContext.getAttribute('cmc_applicationid'),
      customerId = formContext.getAttribute('customerid'),
      priceLevelId = formContext.getAttribute('pricelevelid'),
      disableApplicationControlledFields;

    // Disable Application Id if the form type is not create. Do this if the form loads again too.
    if (Xrm.Page.ui.getFormType() !== XrmEnum.FormType.Create) {
      disableEnableField(applicationId, true);
    }

    if (_formLoaded) {
      return;
    }
    _formLoaded = true;

    if (formContext.ui.getFormType() === XrmEnum.FormType.Create && shipTo) {
      shipTo.setValue(true);
      shipTo.setSubmitMode('always');
    }

    // Only populate Contact and Price List if Application is on the form and Customer or
    // Price Level are on the form.
    if (applicationId && (customerId || priceLevelId)) {
      applicationId.addOnChange(populateContactAndPriceListFromApplication);

      // On Create, attempt to populate Contact and Price List. On other form types, disable
      // Customer and Price List if Application is populated.
      if (formContext.ui.getFormType() === XrmEnum.FormType.Create) {
        populateContactAndPriceListFromApplication(executionContext, true);
      }
      else {
        disableApplicationControlledFields = applicationId.getValue() && applicationId.getValue().length > 0;
        disableEnableField(customerId, disableApplicationControlledFields);
        disableEnableField(priceLevelId, disableApplicationControlledFields);
      }
    }
  }

  function populateContactAndPriceListFromApplication(executionContext, skipClearField) {
    var formContext = executionContext.getFormContext(),
      applicationId = formContext.getAttribute('cmc_applicationid'),
      customerId = formContext.getAttribute('customerid'),
      priceLevelId = formContext.getAttribute('pricelevelid'),
      loadingText,
      fetchXml;

    // If Customer, Price Level, and Application are not on the form no logic needs to be executed.
    if (!customerId && !priceLevelId && !applicationId) {
      return;
    }


    if (!applicationId || !applicationId.getValue() || applicationId.getValue().length <= 0) {
      disableEnableClearSetField(customerId, false, skipClearField);
      disableEnableClearSetField(priceLevelId, false, skipClearField);
      return;
    }

    loadingText = CampusManagement.localization.getResourceString('Ribbon_Loading');

    // Fetch XML must be encoded ahead of time for the Unified Interface. This works in both UIs though.
    fetchXml = encodeURIComponent(
      `<fetch top="1">
        <entity name="cmc_application">
          <attribute name="cmc_contactid" />
          <filter>
            <condition attribute='cmc_applicationid' operator='eq' value='${applicationId.getValue()[0].id}' />
          </filter>
          <link-entity name="cmc_applicationregistration" from="cmc_applicationregistrationid" to="cmc_applicationregistration" link-type="outer" alias="applicationRegistration">
            <attribute name="cmc_contactid" />
            <link-entity name="cmc_applicationdefinitionversion" from="cmc_applicationdefinitionversionid" to="cmc_applicationdefinitionversionid" link-type="outer">
              <link-entity name="cmc_applicationdefinition" from="cmc_applicationdefinitionid" to="cmc_applicationdefinitionid" link-type="outer">
                <link-entity name="cmc_invoicedefinition" from="cmc_invoicedefinitionid" to="cmc_invoicedefinitionid" link-type="outer" alias="invoiceDefinition">
                  <attribute name='cmc_pricelistid' />
                </link-entity>
              </link-entity>
            </link-entity>
          </link-entity>
        </entity>
      </fetch>`);

    Xrm.WebApi.retrieveMultipleRecords('cmc_application', '?fetchXml=' + fetchXml).then(
      function (results) {
        Xrm.Utility.closeProgressIndicator();

        var record = results.entities[0],
          // Grab Contact from the Application or the Application Registration if it's not on Application
          contactId = record['_cmc_contactid_value'] || record['applicationRegistration.cmc_contactid'],
          contactName = record['_cmc_contactid_value@OData.Community.Display.V1.FormattedValue'] || record['applicationRegistration.cmc_contactid@OData.Community.Display.V1.FormattedValue'],
          priceListId = record['invoiceDefinition.cmc_pricelistid'],
          priceListName = record['invoiceDefinition.cmc_pricelistid@OData.Community.Display.V1.FormattedValue'];

        disableEnableClearSetField(customerId, true, skipClearField,
          contactId ? { id: contactId, entityType: 'contact', name: contactName } : null);
        disableEnableClearSetField(priceLevelId, true, skipClearField,
          priceListId ? { id: priceListId, entityType: 'pricelevel', name: priceListName } : null);
      },
      function (error) {
        Xrm.Utility.closeProgressIndicator();

        // If an error occurs, disable and clear Customer and Price List
        disableEnableClearSetField(customerId, true, skipClearField);
        disableEnableClearSetField(priceLevelId, true, skipClearField);
      });

    Xrm.Utility.showProgressIndicator(loadingText);
  }

  function disableEnableClearSetField(attribute, disabled, skipClearField, value = undefined) {
    if (!attribute) {
      // Attribute is not on the form, exit early.
      return;
    }

    disableEnableField(attribute, disabled);

    if (!disabled && !skipClearField) {
      attribute.setValue(null);
      attribute.fireOnChange();
    }

    if (value) {
      attribute.setValue([value]);
      attribute.fireOnChange();
    }
  }

  function disableEnableField(attribute, disabled) {
    if (!attribute) {
      // Attribute is not on the form, exit early.
      return;
    }

    attribute.controls.forEach(function (control) {
      control.setDisabled(disabled);
    });
  }
}
