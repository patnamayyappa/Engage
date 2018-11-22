/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
module CampusManagement.ribbon.Contact {
   
    declare var SonomaCmc: any;
    export function assignSuccessPlan(studentIds, fromForm) {
        SonomaCmc.LocalStorage.set('cmc_AssignSuccessPlanStudentIds', studentIds, 1);
        SonomaCmc.LocalStorage.set('cmc_AssignSuccessPlanFromForm', fromForm, 1);
        Xrm.Navigation.openWebResource('cmc_/Pages/AssignSuccessPlan.html', {
            height: 250,
            width: 456,
            openInNewWindow: true
        });
    }

    export function assignSuccessPlanForm() {
        assignSuccessPlan([Xrm.Page.data.entity.getId()], true);
    }

    export function predictRetention() {
        
            var input = { "ContactId": Xrm.Page.data.entity.getId() };

            SonomaCmc.WebAPI.post('cmc_PredictRetentionAction', input)
                .then(
                    function (result) {
                        var retentionProbabilityAttr = Xrm.Page.getAttribute('cmc_retentionprobability');
                        if (retentionProbabilityAttr)
                            retentionProbabilityAttr.setValue(result.RetentionProbability);
                        CampusManagement.CommonUiControls.openAlertDialog(CampusManagement.localization.getResourceString("PredictRetention_Success"), CampusManagement.localization.getResourceString("OkButton"));
                        Xrm.Page.data.entity.save();
                    },
                    function (error) {
                        console.log(error);
                        CampusManagement.CommonUiControls.openAlertDialog(CampusManagement.localization.getResourceString("PredictRetention_Failure"), CampusManagement.localization.getResourceString("OkButton"));
                    }
        );        
    }

    export function navigateToDegreePlanningUrl() {
        // Xrm.Page is still used as there currently isn't a good way to get the form context
        // in a ribbon in Dynamics CRM 9.0
        if (Xrm.Page.data.entity.getIsDirty()) {
            CampusManagement.CommonUiControls.openAlertDialog(CampusManagement.localization.getResourceString("DegreePlanningURL_Error"), CampusManagement.localization.getResourceString("OkButton"));
            return;
        }

      var fetchData = {
        statecode: "0"
      };

        var fetch =
          ['<fetch top="1">',
            '<entity name="cmc_configuration">',
            '<attribute name="cmc_degreeplanningurl" />',
                '<filter>',
            '<condition attribute="statecode" operator="eq" value="', fetchData.statecode/*0*/, '" />',
                '</filter>',
                '</entity>',
                '</fetch>'
            ].join('');
        Xrm.Utility.showProgressIndicator(CampusManagement.localization.getResourceString("Ribbon_Loading"));
        Xrm.WebApi.retrieveMultipleRecords('cmc_configuration', '?fetchXml=' + fetch)
            .then(function (result) {
                Xrm.Utility.closeProgressIndicator();
                var configuration = result && result.entities && result.entities.length > 0 ? result.entities[0] : null;
                if (!configuration) {
                    CampusManagement.CommonUiControls.openAlertDialog(CampusManagement.localization.getResourceString("Error_matching_configuration"), CampusManagement.localization.getResourceString("OkButton"));
                    return;
                }

              var url = configuration["cmc_degreeplanningurl"];
                replaceTokensWithAttributeValues(url);
            },
            function (error) {
                Xrm.Utility.closeProgressIndicator();
               CampusManagement.CommonUiControls.openAlertDialog(error.message, CampusManagement.localization.getResourceString("OkButton"));
            });
    }

    function replaceTokensWithAttributeValues(url) {
        var regex = /<\w+?>/g;
        var dirtyAttributeTokens = url.match(regex);
        var formattedUrl = "";

        if (dirtyAttributeTokens !== null) {
            formattedUrl = url;
            var invalidFields = [];
            var len = dirtyAttributeTokens.length;
            var i = 0;

            for (; i < len; i++) {
                var cleanMatch = dirtyAttributeTokens[i];
                cleanMatch = cleanMatch.replace(/[<>]/g, '');
                // Xrm.Page is still used as there currently isn't a good way to get the form
                // context in a ribbon in Dynamics CRM 9.0
                var attribute = Xrm.Page.getAttribute(cleanMatch);

                if (attribute !== null && Xrm.Page.getAttribute(cleanMatch).getValue() !== null) {
                    var attributeValue = Xrm.Page.getAttribute(cleanMatch).getValue();
                    formattedUrl = formattedUrl.replace(dirtyAttributeTokens[i], attributeValue);
                }
                else {
                    invalidFields.push(cleanMatch);
                    formattedUrl = formattedUrl.replace(dirtyAttributeTokens[i], '');
                }
            }
            if (invalidFields.length > 0) {
                CampusManagement.CommonUiControls.openAlertDialog(CampusManagement.localization.getResourceString("DegreePlanningURL_EmptyFields") + invalidFields, CampusManagement.localization.getResourceString("OkButton"));
                return
            }
        }

        if (typeof formattedUrl === 'string' && formattedUrl !== "") {
            window.open(formattedUrl, '_blank');
        }
        else {
            CampusManagement.CommonUiControls.openAlertDialog(CampusManagement.localization.getResourceString("DegreePlanningURL_Undefined"), CampusManagement.localization.getResourceString("OkButton"));
        }
    }
    
}
