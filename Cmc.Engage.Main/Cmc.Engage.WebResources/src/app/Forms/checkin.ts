module CampusManagement.checkin {
  export function onLoad() {
    Xrm.Page.getAttribute("cmc_scanqrcode").addOnChange(CampusManagement.checkin.scanQRCode);
    if (Xrm.Page.ui.getFormType() === XrmEnum.FormType.Create) {
      Xrm.Device.getBarcodeValue().then(
        function success(qrcode) {
          Xrm.Page.getAttribute("cmc_scanqrcode").setValue(qrcode);
          Xrm.Page.getAttribute("cmc_scanqrcode").fireOnChange();
        }
      );
    }
  }
  export function scanQRCode(executionContext) {
    var code = (executionContext.getFormContext().getAttribute("cmc_scanqrcode").getValue() || "").toLocaleLowerCase();
    if (!code.startsWith("sr") && !code.startsWith("er"))
      return;
    getRegistrationInfo(code).then(
      function success(registrationInfo) {
        updateCheckIn(executionContext, registrationInfo);
      });
  }



  function updateCheckIn(executionContext, registrationInfo) {
    var formContext = executionContext.getFormContext();
    if (registrationInfo.entities.length > 0) {
      Xrm.Page.getAttribute("msevtmgt_checkintime").setValue(new Date());
      let registrationEntity = registrationInfo.entities[0];
      if (registrationEntity != null) {
        var code = (registrationEntity.msevtmgt_name || "").toLocaleLowerCase();
        if (code.startsWith("sr")) {
          setEventCheckInType(formContext, Constants.CheckinType.SessionCheckin)
          var registrationId = registrationEntity["_msevtmgt_registrationid_value"];
          var registrationCode = registrationEntity["_msevtmgt_registrationid_value@OData.Community.Display.V1.FormattedValue"];
          var entityName = registrationEntity["_msevtmgt_registrationid_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
          // set registration lookup first
          if (registrationId) {
            setLookupValue(formContext, "msevtmgt_registrationid", registrationId, registrationCode, entityName, true);
          }
          // set session lookup
          setLookupValue(formContext, "msevtmgt_sessionregistration", registrationEntity.msevtmgt_sessionregistrationid, registrationEntity.msevtmgt_name, 'msevtmgt_sessionregistration', true);
        }
        else {
          setEventCheckInType(formContext, Constants.CheckinType.EventCheckIn)
          setLookupValue(formContext, "msevtmgt_registrationid", registrationEntity.msevtmgt_eventregistrationid, registrationEntity.msevtmgt_name, "msevtmgt_eventregistration", true);
        }
      }
    }
  }

  function getRegistrationInfo(code) {
    var qrcode = code;
    var isSessionCheckin = qrcode.startsWith("sr");
    if (isSessionCheckin == true) {
      return getSessionsDetails(qrcode);
    }
    else {
      return getEventRegistrationDetail(qrcode);
    }
  }

  function setEventCheckInType(formContext, value) {
    let checkinType = formContext.getAttribute("msevtmgt_checkintype");
    if (checkinType) {
      checkinType.setValue(value);
      checkinType.fireOnChange();
    }
  }

  function getEventRegistrationDetail(registrationCode) {
    let fetchXml = '<fetch top="1" > ' +
      '<entity name="msevtmgt_eventregistration" >' +
      ' <all-attributes/>' +
      '<attribute name="msevtmgt_name" />' +
      '<attribute name="msevtmgt_eventregistrationid" /> ' +
      '<filter> ' +
      '  <condition attribute="msevtmgt_name" operator="eq" value="' + registrationCode + '" /> ' +
      '</filter> ' +
      '</entity>' +
      '</fetch>';

    return Xrm.WebApi.retrieveMultipleRecords("msevtmgt_eventregistration", "?fetchXml=" + fetchXml);
  }

  function getSessionsDetails(sessionCode) {

    let fetchXml = '<fetch top="1" > ' +
      '<entity name="msevtmgt_sessionregistration" >' +
      ' <all-attributes/>' +
      '<attribute name="msevtmgt_name" />' +
      '<attribute name="msevtmgt_sessionregistrationid" /> ' +
      '<attribute name="msevtmgt_registrationid" /> ' +
      '<filter> ' +
      '  <condition attribute="msevtmgt_name" operator="eq" value="' + sessionCode + '" /> ' +
      '</filter> ' +
      '</entity>' +
      '</fetch>';

    return Xrm.WebApi.retrieveMultipleRecords("msevtmgt_sessionregistration", "?fetchXml=" + fetchXml);
  }

  function setLookupValue(formContext, lookupFieldId, id, name, entityName, fireOnChange) {
    let lookupField = formContext.getAttribute(lookupFieldId);
    if (lookupField) {
      let lookupValue = [{ id: id, name: name, entityType: entityName }];
      lookupField.setValue(lookupValue);
      if (fireOnChange)
        lookupField.fireOnChange();

    }
  }
}
