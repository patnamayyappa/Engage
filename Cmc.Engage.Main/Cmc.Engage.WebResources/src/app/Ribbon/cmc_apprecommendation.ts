module CampusManagement.Ribbon.cmc_apprecommendation {
  declare let SonomaCmc: any;
  declare let Xrm: any;
  export function FindandExecuteMatchingWorkflow() {
    let application = Xrm.Page.getAttribute("cmc_applicationid").getValue();
    let applicationId: any;
    if (application != null) {
      applicationId = SonomaCmc.Guid.decapsulate(application[0].id);     
    }

    var fetchXml = [
        '<fetch top="1" >',
        '<entity name="cmc_application" >',
        '<filter>',
        '<condition attribute="cmc_applicationid" operator="eq" value="' + applicationId + '" />',
        '</filter>',
        '<link-entity name="cmc_applicationregistration" from="cmc_applicationregistrationid" to="cmc_applicationregistration" alias="AppReg" link-type="inner" >',
        '<link-entity name="cmc_applicationdefinitionversion" from="cmc_applicationdefinitionversionid" to="cmc_applicationdefinitionversionid" alias="Appdefver" link-type="inner" >',
        '<link-entity name="cmc_applicationdefinition" from="cmc_applicationdefinitionid" to="cmc_applicationdefinitionid" alias="Appdef" link-type="inner" >',
        '<link-entity name="cmc_applicationrecommendationdefinition" from="cmc_applicationrecommendationdefinitionid" to="cmc_recommendationdefinitionid" alias="AppRecdef" link-type="inner" >',
        '<attribute name="cmc_recommendationinvitationworkflow" />',
        '</link-entity>',
        '</link-entity>',
        '</link-entity>',
        '</link-entity>',
        '</entity>',
        '</fetch>'
      ].join('');
      return Xrm.WebApi.retrieveMultipleRecords("cmc_application", `?fetchXml=${fetchXml}`)
        .then(
        function success(result) {
          if (result.entities.length > 0) {
            let inviteWorkFlowID = result.entities[0]["AppRecdef.cmc_recommendationinvitationworkflow"];
            if (inviteWorkFlowID != null) {
              var _attributeAppRecID = Xrm.Page.data.entity.getId();
              if (_attributeAppRecID != null) {
                var output: any;
                output = CampusManagement.utilities.executeWorkflow(_attributeAppRecID, inviteWorkFlowID);
                CommonUiControls.openAlertDialog(localization.getResourceString("Email_Success"), localization.getResourceString("OkButton"));
              }
            }
            else {
              CommonUiControls.openAlertDialog(localization.getResourceString("RecommenderInviteWorkflow_NotConfigured"), localization.getResourceString("OkButton"));
            }
          }
          else
          {
            CommonUiControls.openAlertDialog(localization.getResourceString("RecommenderInviteWorkflow_NotConfigured"), localization.getResourceString("OkButton"));
          }

        },
        function (error) {
          console.log()
          CommonUiControls.openAlertDialog(localization.getResourceString("RecommenderInviteWorkflow_NotConfigured"), localization.getResourceString("OkButton"));
        }
        );
  } 
  
  // Send Email button display rule
  export function showSendButton(buttontype): boolean
  {
    
    let returnValue = false;
    if (Xrm.Page.ui.getFormType() === XrmEnum.FormType.Update) {
      returnValue = true;
    }
    if (returnValue != false)
    {        
      let appRecomendationID = Xrm.Page.data.entity.getId();  
      if (appRecomendationID != null) {        
        let url = Xrm.Page.context.getClientUrl() + "/api/data/v" + getVersion() + "/cmc_applicationrecommendations?$select=cmc_applicationrecommendationid,cmc_isrecommendationsubmitted,cmc_isrequestemailsent&$filter=(cmc_applicationrecommendationid eq " + appRecomendationID + ")";
        $.ajax({
          type: "GET",
          contentType: "application/json; charset=utf-8",
          url: url,
          beforeSend: xmlHttpRequest => {
            xmlHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
            xmlHttpRequest.setRequestHeader("OData-Version", "4.0");
            xmlHttpRequest.setRequestHeader("Accept", "application/json");
          },
          async: false,
          success(result) {
            if (result.value.length > 0) {
              let apprecommendation = result.value[0];
              if (apprecommendation != null) {               
                if (buttontype === "Send") {
                  if (apprecommendation.cmc_isrecommendationsubmitted === true)
                    returnValue = false;//Recommendation submitted, Send request button will hide
                  else if (apprecommendation.cmc_isrequestemailsent === true)
                    returnValue = false;//Recommendation Invite sent,  Send request button will hide
                }
                else if (buttontype === "Resend")
                {
                  if (apprecommendation.cmc_isrecommendationsubmitted === true)
                    returnValue = false;//Recommendation submitted, Re-Send request button will hide
                  else if (apprecommendation.cmc_isrequestemailsent === false || apprecommendation.cmc_isrequestemailsent === null)
                    returnValue = false;//Recommendation Invite not sent, Re-Send request button will hide
                }
              }
            }
          },
          error() {
            returnValue = false;
          }
        });
        }
    }
    return returnValue;
  }
  
  function getVersion() {
    return Xrm.Page.context.getVersion().split('.').slice(0, 2).join('.');
  }
}
