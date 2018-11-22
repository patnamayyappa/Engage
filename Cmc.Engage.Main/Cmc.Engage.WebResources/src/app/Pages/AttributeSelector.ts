/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />

module CMC.Engage.AttributeSelector {
  // parent.Xrm.Page is still used throughout this file as there isn't a good replacement
  // yet for Dynamics CRM 9.0
  declare var CampusManagement: any, SonomaCmc: any, document: any, _$select: any;

  var _attribute: Xrm.Attributes.Attribute,
    _attributeSchema: Xrm.Attributes.Attribute,
    _domMaster: Xrm.Attributes.Attribute,
    _status: Xrm.Attributes.Attribute,
    _removeOptionFromLastSession,
    _optionOnLastSession, localizedStrings;


  export enum StateCode {
    Active = 0,
    Inactive = 1
  };

  export function executeOnLoad() {
    initializeDisplaySettings();
    _attribute = parent.Xrm.Page.getAttribute('cmc_attribute'),
      _attributeSchema = parent.Xrm.Page.getAttribute('cmc_attributeschema'),
      _domMaster = parent.Xrm.Page.getAttribute('cmc_dommasterid');
    _status = parent.Xrm.Page.getAttribute('statecode');

    var $dropdownLabels;
    _$select = $('#dom-definition-logic-attribute-select');

    _optionOnLastSession = _attributeSchema ? _attributeSchema.getValue() : null;
    _$select.change(setAttributeAndAttributeSchema);

    if (_domMaster) {
      _domMaster.addOnChange(clearAttributeAndAttributeSchemaValidation);
      _domMaster.addOnChange(refreshCustomDropdown);
    }

    if (_status) {
      _status.addOnChange(disableField);
    }

    parent.Xrm.Page.data.entity.addOnSave(displayWarningIconOnSave);
    refreshCustomDropdown();
    disableField();

    $dropdownLabels = $('[data-dropdown-label-for]');
    $dropdownLabels.each(function (index, label) {
      $(label).click(onLabelClick);
    });

    _$select.each(function (index, option) {
      $(option).change(onDropdownSet).blur(onDropdownSet);
    });
  }

  function initializeDisplaySettings() {
    $('#attribute-label').text(CampusManagement.localization.getResourceString("Attribute"));
    $('title').text(CampusManagement.localization.getResourceString("AttributeTitle"));
    $("#warning").attr("title", CampusManagement.localization.getResourceString("AttributeSelector_NoAttributeWarning"));

    localizedStrings = {
      loadingText: CampusManagement.localization.getResourceString("Ribbon_Loading"),
      okButton: CampusManagement.localization.getResourceString("OkButton"),
      attributeSelector_NoAttributeWarning: CampusManagement.localization.getResourceString("AttributeSelector_NoAttributeWarning"),
      errorPrefix: CampusManagement.localization.getResourceString("ErrorPrefix")
    };

  }
  function onLabelClick(event) {
    var $label = $(event.currentTarget),
      $dropdown = $('#' + $label.data('dropdown-label-for'));

    if ($label.is('.disabled')) {
      return;
    }

    if ($dropdown.length) {
      $dropdown.show();
      $label.hide();
      $('select', $dropdown).focus()
    }
  }

  function onDropdownSet(event) {
    var $option = $(event.currentTarget),
      $dropdownContainer = $(event.currentTarget).parent('.dropdown'),
      $label = $('[data-dropdown-label-for=' + $dropdownContainer.attr('id') + ']');

    if ($label.length) {
      if (!$option.val() && !$option["0"].options[$option["0"].selectedIndex].label) {
        $label.addClass('no-value');
        $('label', $label).html('&nbsp;');
        displayWarningIconHelper($dropdownContainer, true);
      }
      else {
        $label.removeClass('no-value');
        $('label', $label).text($option["0"].options[$option["0"].selectedIndex].label);
        displayWarningIconHelper($dropdownContainer, false);
      }
      $label.show();
      $dropdownContainer.hide();
    }
  }

  function displayWarningIconHelper($dropdownContainer, activate) {
    if (activate) {
      $dropdownContainer.siblings('.warning-icon').show();
    }
    else {
      $dropdownContainer.siblings('.warning-icon').hide();
    }
  }

  function displayWarningIconOnSave(executionContext) {
    var $dropdownContainer = _$select.parent('dropdown'),
      $label = $('[data-dropdown-label-for=' + $dropdownContainer.attr('id') + ']');

    if (_$select.val()) {
      displayWarningIconHelper(_$select.parent('.dropdown'), false);
    }
    else {
      displayWarningIconHelper(_$select.parent('.dropdown'), true);
      // Prevent Save 
      executionContext.getEventArgs().preventDefault();
    }
  }

  function disableField() {
    if (parent.Xrm.Page.ui.getFormType() !== XrmEnum.FormType.ReadOnly &&
      parent.Xrm.Page.ui.getFormType() !== XrmEnum.FormType.Disabled &&
      _status.getValue() !== StateCode.Inactive) {
      _$select.prop('disabled', false);
      $('.input-label').removeClass('disabled');
    }
    else {
      _$select.prop('disabled', true);
      $('.dropdown').hide();
      $('.input-label').addClass('disabled').show();
    }
  }

  function refreshCustomDropdown() {
    toggleLoading(true);

    getAttributeNames()
      .then(function (attributeNameInput) {
        if (attributeNameInput) {
          getAttributeDisplayNamesJson(attributeNameInput).then(
            function success(results) {
              toggleLoading(false);

              if (results.length > 0) {
                addOptionsToHtmlSelect(results);
              }
              else {
                SonomaCmc.Log.log('Unable to fetch unique DOM Definition Logics for the selected DOM Master');
                _$select.empty();
              }
            },
            function (error) {
              multiLingualAlert(localizedStrings.errorPrefix, error.message);
              toggleLoading(false);
            }
          );
        }
        else {
          toggleLoading(false);
        }
      },
        function (error) {
          multiLingualAlert(localizedStrings.errorPrefix, error.message);
          toggleLoading(false);
        }
      );
  }

  function getAttributeNames() {
    let domMasterId: string = _domMaster.getValue() ? _domMaster.getValue()[0].id : null,
      attributeKeysStr: string = '';

    if (!domMasterId) {
      return new SonomaCmc.Promise.Promise((resolve, reject) => resolve(''));
    }
    return fetchDomDefinitionLogicAttributesFromDomMaster(domMasterId).then(
      function success(results) {
        if (results.entities.length > 0) {
          attributeKeysStr = getAttributeKeysAsString(results.entities);
          SonomaCmc.Log.log('Input to Custom Action: ' + attributeKeysStr);
        }
        else {
          SonomaCmc.Log.log('Unable to fetch unique DOM Definition Logics for the selected DOM Master');
          _$select.empty();
        }
        return attributeKeysStr;
      },
      function (error) {
        multiLingualAlert(localizedStrings.errorPrefix, error.message);
        toggleLoading(false);
      }
    );
  }

  function fetchDomDefinitionLogicAttributesFromDomMaster(domMasterId) {
    // Use the Web API to retrieve unique DOM Definition Logics and their Attribute Schemas for the selected DOM Master.
    var fetchXml = [
      '<fetch distinct="true">',
      '<entity name="cmc_domdefinitionlogic">',
      '<attribute name="cmc_attributeschema" />',
      '<attribute name="cmc_domdefinitionlogicid" />',
      '<link-entity name="cmc_domdefinition" from="cmc_domdefinitionid" to="cmc_domdefinitionid" link-type="inner" alias="domdefinition">',
      '<filter type="and">',
      '<condition attribute="cmc_dommasterid" operator="eq" value="', domMasterId, '" />',
      '<condition attribute="statecode" operator="eq" value="' + StateCode.Active + '"/>',
      '</filter>',
      '</link-entity>',
      '<filter>',
      '<condition attribute="statecode" operator="eq" value="' + StateCode.Active + '"/>',
      '</filter>',
      '</entity>',
      '</fetch>'
    ].join('');

    return Xrm.WebApi.retrieveMultipleRecords('cmc_domdefinitionlogic', `?fetchXml=${fetchXml}`);
  }

  function getAttributeDisplayNamesJson(input): Promise<any> {
    // Custom Actions cannot be called using Xrm.WebAPI at this time
    return SonomaCmc.WebAPI.post('cmc_buildattributedisplay',
      {
        AttributeNames: input
      })
      .then((results) => JSON.parse(results.AttributeDisplayNamesJson))
      .catch(function (error) {
        multiLingualAlert(localizedStrings.errorPrefix, error.message);
        toggleLoading(false);
      });
  }

  function addOptionsToHtmlSelect(optionValues) {
    var output = [];
    var currentAttributeOptionValue = _attributeSchema && _attributeSchema.getValue();
    // Clear outdated option set and add new options to the dropdown
    _$select.empty();

    // Key => Option Value; Value => Option Text
    $.each(optionValues, function (index, option) {
      var selected = currentAttributeOptionValue === option.Key ? ' selected="selected" ' : "";
      output.push('<option value="' + option.Key + '"' + selected + '>' + option.Value + '</option>');
    });

    var optionsLength = output.length;

    if (!currentAttributeOptionValue) {
      output.splice(0, 0, '<option selected="selected" ></option>');
      optionsLength += 1;
    }

    $(_$select).html(output.join(''));

    setSelectOptionLength(optionsLength);

    var $dropdownContainer = _$select.parent('.dropdown'),
      $label = $('[data-dropdown-label-for=' + $dropdownContainer.attr('id') + ']');

    if (!currentAttributeOptionValue) {
      $label.addClass('no-value');
      $('label', $label).html('&nbsp;');
    }
    else {
      $label.removeClass('no-value');
      $('label', $label).text(_$select["0"].options[_$select["0"].selectedIndex].label);
    }
  }

  function setSelectOptionLength(optionsLength) {
    if (optionsLength <= 2) {
      _$select.attr('size', 2);
    }
    else if (optionsLength < 8) {
      _$select.attr('size', optionsLength);
    }
    else {
      _$select.attr('size', 8);
    }
  }

  function isCurrentAttributeSchemaAnOption(): boolean {
    var $options = _$select.find('option');

    for (var i = 0, length = $options.length; i < length; i++) {
      if ($options[i].value === _attributeSchema) {
        return true;
      }
    }
    return false;
  }

  function getAttributeKeysAsString(domDefinitionLogic): string {
    var attributes = [],
      length = domDefinitionLogic.length,
      schema;

    // Convert and return attribute names as string delimited by ','
    for (var i = 0; i < length; i++) {
      schema = domDefinitionLogic[i].cmc_attributeschema;
      if (!(attributes.some(s => s === schema))) {
        attributes.push(domDefinitionLogic[i].cmc_attributeschema);
      }
    }
    if (_attributeSchema && _attributeSchema.getValue() && attributes.indexOf(_attributeSchema.getValue()) === -1) {
      SonomaCmc.Log.log('Attribute Schema previously set but DNE in current list to add to options, temporarily adding: ' + _attributeSchema.getValue() + ' as option in dropdown.');
      attributes.push(_attributeSchema.getValue());
      _removeOptionFromLastSession = true;
    }
    return attributes.join(',');
  }

  function multiLingualAlert(message, error) {
    // Parent.Xrm is used here as the alert dialog in this window does not display the
    // error message text.
    parent.Xrm.Navigation.openAlertDialog({
      text: message+': '+ error,
      confirmButtonLabel: localizedStrings.okButton
    }, null);


  }

  function clearAttributeAndAttributeSchemaValidation() {
    try {
      if (!isCurrentAttributeSchemaAnOption()) {
        _attributeSchema.setValue('');
        _attributeSchema.fireOnChange();

        _attribute.setValue('');
        _attribute.fireOnChange();
      }
      else {
        // If the value in Attribute – Schema exists as an option in the custom dropdown, it must be selected.
        _$select.value(_attributeSchema);
      }
    }
    catch (error) {
      multiLingualAlert(localizedStrings.errorPrefix, error.message);
    }
  }

  function setAttributeAndAttributeSchema() {
    try {
      var option = _$select.find(':selected');

      _attributeSchema.setValue(option.val());
      _attributeSchema.setSubmitMode('always');
      _attributeSchema.fireOnChange();

      _attribute.setValue(option.text());
      _attribute.setSubmitMode('always');
      _attribute.fireOnChange();

      if (_removeOptionFromLastSession) {
        $('option[value="' + _optionOnLastSession + '"]', _$select).remove();
        _removeOptionFromLastSession = false;

        var optionsLength = $("option", _$select).length;
        setSelectOptionLength(optionsLength);
      }
    }
    catch (error) {
      multiLingualAlert(localizedStrings.errorPrefix, error.message);
    }
  }

  function toggleLoading(isLoading) {
    if (isLoading) {
      // The progress dialog blocks the UI until it is closed using the closeProgressIndicator method below
      // TODO: Pull the multi-lingual value for loading dialog
      parent.Xrm.Utility.showProgressIndicator(localizedStrings.loadingText);
    }
    else {
      parent.Xrm.Utility.closeProgressIndicator();
    }
  }
}
