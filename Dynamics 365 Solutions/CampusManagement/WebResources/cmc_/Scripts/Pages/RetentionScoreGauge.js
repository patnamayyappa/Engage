(function (global) {
    'use strict';

    global.CampusManagement = global.CampusManagement || {};

    global.CampusManagement.RetentionScoreGauge = (function () {

        function executeOnLoad() {
            $(document).ready(function () {
                var contactId = GetGlobalContext().getQueryStringParameters().id,
                    // Parent.Xrm.Page is used as there isn't a good way to get an attribute from
                    // the parent form in Dynamics CRM 9.0 and it doesn't make sense to do a query
                    // for it.
                    currentRetentionScore = parent.Xrm.Page.getAttribute("cmc_currentretentionscore") ? parent.Xrm.Page.getAttribute("cmc_currentretentionscore").getValue() : 0,
                    fetchXml = [
                        '<fetch count="1">',
                            '<entity name="cmc_retentionscorehistory">',
                                '<attribute name="cmc_retentionscorehistoryid"/>',
                                '<order descending="true" attribute="createdon"/>',
                                '<filter type="and">',
                                    '<condition attribute="cmc_studentid" operator="eq" value="' + contactId + '" />',
                                '</filter>',
                                '<link-entity name="cmc_scoredefinition" from="cmc_scoredefinitionid" to="cmc_scoredefinitionid" alias="studentGroup">',
                                    '<attribute name="cmc_greenscorethreshold"/>',
                                    '<attribute name="cmc_yellowscorethreshold"/>',
                                    '<attribute name="cmc_redscorethreshold"/>',
                                '</link-entity>',
                            '</entity>',
                        '</fetch>'
                    ].join('');

                if (contactId == null)
                    return;

                Xrm.WebApi.retrieveMultipleRecords('cmc_retentionscorehistory', '?fetchXml=' + fetchXml)
                    .then(
                        function (results) {
                            var latestRetentionHistory = results.entities.length > 0 ? results.entities[0] : {},
                                greenThreshold = latestRetentionHistory['studentGroup.cmc_greenscorethreshold'],
                                yellowThreshold = latestRetentionHistory['studentGroup.cmc_yellowscorethreshold'],
                                redThreshold = latestRetentionHistory['studentGroup.cmc_redscorethreshold'];

                            if ((currentRetentionScore || currentRetentionScore === 0)
                                && (greenThreshold || greenThreshold === 0)
                                && (yellowThreshold || yellowThreshold === 0)
                                && (redThreshold || redThreshold === 0)) {
                                createGauge(currentRetentionScore, greenThreshold, yellowThreshold, redThreshold);
                            }
                            else {
                                createGauge(currentRetentionScore, 80, 150, 180);
                            }
                        },
                        function (error) {
                            multiLingualAlert("ErrorPrefix", "An unexpected error has occurred", error.message);
                        }
                    );
            });
        }

        function createGauge(pointerValue, greenThreshold, yellowThreshold, redThreshold) {
            $("#gauge").kendoRadialGauge({

                pointer: {
                    value: pointerValue
                },

                scale: {
                    minorUnit: 5,
                    startAngle: -30,
                    endAngle: 210,
                    max: greenThreshold,
                    labels: {
                        position: "inside"
                    },
                    ranges: [
                        {
                            from: 0,
                            to: redThreshold,
                            color: "#c20000"
                        },
                        {
                            from: redThreshold,
                            to: yellowThreshold,
                            color: "#ffc700"
                        },
                        {
                            from: yellowThreshold,
                            to: greenThreshold,
                            color: "#008000"
                        }
                    ]
                }
            });
        }

        function multiLingualAlert(translationKey, defaultMessage, error) {
            CampusManagement.retrieveMultiLingualValues.get([translationKey, 'okButton']).then(function (translations) {
                // Parent.Xrm is used here as the alert dialog in this window does not display the
                // error message text.
                parent.Xrm.Navigation.openAlertDialog({
                    text: translations[translationKey]
                          ? translations[translationKey] + ': ' + error
                          : defaultMessage + ': ' + error,
                    confirmButtonLabel: translations['okButton'] || 'OK'
                });
            });
        }

        return {
            executeOnLoad: executeOnLoad
        };
    })();

}(this));