/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
//If the DO namespace object is not defined, create it.
var DO;
if (typeof (DO) === "undefined") { DO = {}; }
// Create Namespace container for functions in this library;
DO.DependentOptionSet = {};
DO.DependentOptionSet.config = null;


//function DO.DependentOptionSet.init
 //param {string} webResourceName the name of the JavaScript web resource containing the JSON definition 
// of option dependencies
 
DO.DependentOptionSet.init = function (webResourceName) {
  if (DO.DependentOptionSet.config === null) {
    //Retrieve the JavaScript Web Resource specified by the parameter passed

    var serverUrl = Xrm.Utility.getGlobalContext().getClientUrl();

    var pathToWR = serverUrl + "/WebResources/cmc_/dist/app/Common/" + webResourceName;
    var xhr = new XMLHttpRequest();
    xhr.open("GET", pathToWR, true);
    xhr.onreadystatechange = function () {
      if (this.readyState === 4 /* complete */) {
        this.onreadystatechange = null;
        if (this.status === 200) {
          var wrContent = '[]';
          if (this.response !== undefined) { wrContent = this.response; }
          DO.DependentOptionSet.config = JSON.parse(wrContent);
          DO.DependentOptionSet.completeInitialization();

        }
        else {
          throw new Error("Failed to load configuration data for dependent option sets.");
        }
      }
    };
    xhr.send();
  }
  else {
    DO.DependentOptionSet.completeInitialization();
  }



};

//function DO.DependentOptionSet.completeInitializatio  Initializes the dependent option set options when the form loads


DO.DependentOptionSet.completeInitialization = function () {
  //If the parent field is null, make sure the child field is null and disabled
  // Otherwise, call fireOnChange to filter the child options
  for (var i = 0; i < DO.DependentOptionSet.config.length; i++) {
    var parentAttribute = Xrm.Page.getAttribute(DO.DependentOptionSet.config[i].parent);
    var parentFieldValue = parentAttribute.getValue();
    if (parentFieldValue === null || parentFieldValue === -1) {
      var childAttribute:any = Xrm.Page.getAttribute(DO.DependentOptionSet.config[i].child);
      childAttribute.setValue(null);
      childAttribute.controls.forEach(function (c) { c.setDisabled(true); });
    }
    else {
      parentAttribute.fireOnChange();
    }
  }
}


//function DO.DependentOptionSet.filterDependentField
// Locates the correct set of configurations
 //param {string} parentFieldParam The name of the parent field
 //param {string} childFieldParam The name of the dependent field


DO.DependentOptionSet.filterDependentField = function (parentFieldParam, childFieldParam) {
  //Looping through the array of all the possible dependency configurations
  for (var i = 0; i < DO.DependentOptionSet.config.length; i++) {
    var dependentOptionSet = DO.DependentOptionSet.config[i];

    // Match the parameters to the correct dependent optionset mapping
    if ((dependentOptionSet.parent === parentFieldParam) &&
      (dependentOptionSet.child === childFieldParam)) {

   // Using setTimeout to allow a little time between calling this potentially recursive function.
   // Without including some time between calls, the value at the end of the chain of dependencies 
     // was being set to null on form load.
  
      var fieldConfiguration = JSON.parse(JSON.stringify(dependentOptionSet)); // just make sure we pass by value and not by ref
      setTimeout(DO.DependentOptionSet.filterOptions, 100, parentFieldParam, childFieldParam, fieldConfiguration);
    }
  }
};


//function DO.DependentOptionSet.filterOptions
// Filters options available in dependent fields when the parent field changes
// @param {string} parentFieldParam The name of the parent field
// @param {string} childFieldParam The name of the dependent field
// @param {object} dependentOptionSet The configuration data for the dependent options


DO.DependentOptionSet.filterOptions = function (parentFieldParam, childFieldParam, dependentOptionSet) {
  // Get references to the related fields
  var parentField = Xrm.Page.getAttribute(parentFieldParam);
  var parentFieldValue = parentField.getValue();
  var childField:Xrm.Page.Attribute = Xrm.Page.getAttribute(childFieldParam);
  // Capture the current value of the child field
  var currentChildFieldValue = childField.getValue();
  // If the parent field is null, set the Child field to null 
  //Interactive Service Hub, CRM for Tablets & CRM for phones can return -1 when no option selected
  if (parentFieldValue === null || parentFieldValue === -1) {
    childField.setValue(null);
    childField.fireOnChange(); //filter any dependent optionsets
    // Any attribute may have any number of controls
    // So disable each instance
    childField.controls.forEach(function (c:any) {
      c.setDisabled(true);
    });
    //Nothing more to do when parent attribute is null
    return;
  }

  //The valid child options defined by the configuration


  var validOptionValues = [];
  // deal in case of multi-optionset
  if (Array.isArray(parentFieldValue)) {
    for (var arrCount = 0; arrCount < parentFieldValue.length; arrCount++) {
      var multiValidOptionValues = dependentOptionSet.options[parentFieldValue[arrCount].toString()];
      for (var optCount = 0; optCount < multiValidOptionValues.length; optCount++) {

        // check if the value is already there

        var canAdd = true;
        for (var checkValue = 0; checkValue < validOptionValues.length; checkValue++) {
          if (validOptionValues[checkValue] === multiValidOptionValues[optCount]) {
            canAdd = false; break;
          }
        }

        if (canAdd === true) {
          validOptionValues.push(multiValidOptionValues[optCount]);
        }
      }
    }
  } else {
    validOptionValues = dependentOptionSet.options[parentFieldValue.toString()];
  }

  //When the parent field has a value
  //Any attribute may have more than one control in the form,
  // So iterate over each one
  childField.controls.forEach(function (c:any) {
    c.setDisabled(false);
    c.clearOptions();
    //The attribute contains the valid options
    var childFieldAttribute = c.getAttribute();

    //The attribute options for the Interactive Service Hub, CRM for Tablets & 
    // CRM for phones clients do not include a definition for an unselected option.
    // This will add it

    if (Xrm.Utility.getGlobalContext().client.getClient() === "Mobile") { c.addOption({ text: "", value: -1 }); }

    //For each option value, get the definition from the attribute and add it to the control.
    validOptionValues.sort();
    for (var count = 0; count < validOptionValues.length; count++) {
      var option = childFieldAttribute.getOption(parseInt(validOptionValues[count]));
      c.addOption(option);
    }

  });
  //Set the value back to the current value if it is a valid value.                
  if (currentChildFieldValue !== null) {
    // deal in case of multi-optionset
    if (Array.isArray(currentChildFieldValue)) {
      var validMultiValues = [];
      for (var arrCnt = 0; arrCnt < currentChildFieldValue.length; arrCnt++) {
        for (var optCnt = 0; optCnt < validOptionValues.length; optCnt++) {
          if (currentChildFieldValue[arrCount].toString() === validOptionValues[optCnt]) {
            validMultiValues.push(currentChildFieldValue[arrCnt]);
            break;
          }
        }
      }
      if (validMultiValues.length > 0) {
        validMultiValues.sort();
        childField.setValue(validMultiValues);
      } else {
        childField.setValue(null);
      }
      childField.fireOnChange(); //filter any other dependent optionsets
    } else {
      // for instead indexOf for older IE versions
      for (var count = 0; count < validOptionValues.length; count++) {
        if (currentChildFieldValue.toString() === validOptionValues[count]) {
          childField.setValue(currentChildFieldValue);
          break;
        }
      }
    }
  }
  else {
    //Otherwise set it to null
    childField.setValue(null);
    childField.fireOnChange(); //filter any other dependent optionsets
  }
}
