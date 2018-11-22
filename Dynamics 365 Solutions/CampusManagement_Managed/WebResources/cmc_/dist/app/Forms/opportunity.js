/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
var CampusManagement;
(function (CampusManagement) {
    var opportunity;
    (function (opportunity) {
        // Default mapping
        var _config = {
            // Prospect: Expressed Interest, Not Eligible, Not Interested
            "6b9ce798-221a-4260-90b2-2a95ed51a5bc": ["175490004", "175490000", "175490001"],
            // App in Progress: Incomplete Application, Cancelled
            "650e06b4-789b-46c1-822b-0da76bedb1ed": ["175490003", "175490002"],
            // App Complete: Pending Review, App Requirements Pending, Ready for Decision, Cancelled
            "d3ca8878-8d7b-47b9-852d-fcd838790cfd": ["175490005", "175490006", "175490007", "175490002"],
            // Admit: Admitted with Conditions, Cancelled
            "96784e27-5904-48d6-aa9b-70fb7eb5f2f1": ["175490008", "175490002"],
            // Deposit: Admitted with Conditions, Cancelled
            "298d28f2-8fc7-4ea4-bd25-688eaf06c5d1": ["175490009", "175490002"],
            // Matriculate: Admitted with Conditions
            "bb7e830a-61bd-441b-b1fd-6bb104ffa027": ["175490010"]
        };
        var _lifeCycleStatusAttr;
        function onLoad(executionContext) {
            var formContext = executionContext.getFormContext();
            var clientUrl = formContext.context.getClientUrl();
            var jsonMapping = clientUrl + "/WebResources/cmc_/json/lifecyclestatusmapping.json";
            var xhr = new XMLHttpRequest();
            xhr.open("GET", jsonMapping, true);
            xhr.onreadystatechange = function () {
                if (this.readyState == 4 /* complete */) {
                    this.onreadystatechange = null;
                    if (this.status == 200) {
                        _config = JSON.parse(this.response);
                        filterBpfOnLoad(formContext);
                    }
                }
            };
            xhr.send();
        }
        opportunity.onLoad = onLoad;
        function filterBpfOnLoad(formContext) {
            _lifeCycleStatusAttr = formContext.getAttribute("cmc_lifecyclestatus");
            if (!_lifeCycleStatusAttr)
                return;
            var stageId = formContext.data.process.getActiveStage().getId();
            var selectedLifecycleStatus = _lifeCycleStatusAttr.getSelectedOption();
            formContext.data.process.addOnStageChange(stageOnChange);
            filterLifecycleStatus(stageId, selectedLifecycleStatus.value);
        }
        function filterLifecycleStatus(stageId, selectedLifecycleStatus) {
            var lifeCycleControls = _lifeCycleStatusAttr.controls;
            lifeCycleControls.forEach(function (control) {
                control.clearOptions();
                var stageLifecycleStatuses = _config[stageId];
                for (var i = 0; i < stageLifecycleStatuses.length; i++) {
                    var stageLifecycleStatus = stageLifecycleStatuses[i];
                    var option = _lifeCycleStatusAttr.getOption(stageLifecycleStatus);
                    control.addOption(option);
                }
            });
            _lifeCycleStatusAttr.setValue(selectedLifecycleStatus);
        }
        function stageOnChange(executionContext) {
            var eventArgs = executionContext.getEventArgs();
            var stageId = eventArgs.getStage().getId();
            // Clear the selected lifecycle status when the stage changes
            filterLifecycleStatus(stageId, null);
        }
    })(opportunity = CampusManagement.opportunity || (CampusManagement.opportunity = {}));
})(CampusManagement || (CampusManagement = {}));
