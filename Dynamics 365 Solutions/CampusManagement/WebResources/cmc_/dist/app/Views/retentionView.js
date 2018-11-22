/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
var CampusManagement;
(function (CampusManagement) {
    var view;
    (function (view) {
        var Retention;
        (function (Retention) {
            function displayRetentionProbabilityIcon(rowData, userLcid) {
                var row, retentionProbability, icon = "", tooltip = "";
                row = JSON.parse(rowData);
                retentionProbability = row.cmc_retentionprobability_Value;
                if (retentionProbability >= 75) {
                    icon = "cmc_/Images/Retention_Green_16px.png";
                    tooltip = retentionProbability.toString();
                }
                else if (retentionProbability >= 40) {
                    icon = "cmc_/Images/Retention_Yellow_16px.png";
                    tooltip = retentionProbability.toString();
                }
                else if (retentionProbability > -1) {
                    icon = "cmc_/Images/Retention_Red_16px.png";
                    tooltip = retentionProbability.toString();
                }
                return [icon, tooltip];
            }
            Retention.displayRetentionProbabilityIcon = displayRetentionProbabilityIcon;
        })(Retention = view.Retention || (view.Retention = {}));
    })(view = CampusManagement.view || (CampusManagement.view = {}));
})(CampusManagement || (CampusManagement = {}));
