var CampusManagement;
(function (CampusManagement) {
    var MarketingList;
    (function (MarketingList_1) {
        var StatusCode = CampusManagement.Ribbon.MarketingList.StatusCode;
        var MarketingListType = CampusManagement.Ribbon.MarketingList.MarketingListType;
        var MarketingList = CampusManagement.Ribbon.MarketingList;
        function executeOnLoad() {
            initializeDisplaySettings();
            loadKendoLocalizationFiles();
            var todayDate = new Date(new Date().valueOf());
            var tomorrowDate = todayDate.setDate(todayDate.getDate() + 1);
            $('#activateListInput').kendoDatePicker({ min: new Date(tomorrowDate) }).focus();
            $('#activateListInput').attr('disabled', 'disabled');
            $('#activateButton').attr('disabled', 'disabled');
            $('#activateListInput').change(function () {
                $("#activateButton").removeAttr("disabled");
            });
            toggleLoading(false);
        }
        MarketingList_1.executeOnLoad = executeOnLoad;
        function initializeDisplaySettings() {
            $('.dr-crm-title').text(CampusManagement.localization.getResourceString("ActivateStudentGroupTitle"));
            $('#subtitle').text(CampusManagement.localization.getResourceString("ActiveGroups"));
            $('#lookupLabel').text(CampusManagement.localization.getResourceString("ExpireDate"));
            $('#activateButton').text(CampusManagement.localization.getResourceString("ActiveButton"));
            $('#cancelButton').text(CampusManagement.localization.getResourceString("CancelButton"));
            $('#lookupLabel').attr("title", CampusManagement.localization.getResourceString("ExpireDate"));
            $('#activateButton').attr("title", CampusManagement.localization.getResourceString("ActiveButton"));
            $('#cancelButton').attr("title", CampusManagement.localization.getResourceString("CancelButton"));
            $("title").text(CampusManagement.localization.getResourceString("MarketingListTitle"));
        }
        function loadKendoLocalizationFiles() {
            try {
                var userLcid = Xrm.Page.context.getUserLcid();
                var $head = $('head'), $script = $('<script>'), $script1 = $('<script>');
                $head.append($script1.attr('type', 'text/javascript').attr('src', "../Scripts/Libraries/kendo.messages." + CampusManagement.LanguageMappings.getCultureName(userLcid) + ".min.js"));
                $head.append($script.attr('type', 'text/javascript').attr('src', "../Scripts/Libraries/kendo.culture." + CampusManagement.LanguageMappings.getCultureName(userLcid) + ".min.js"));
                kendo.culture(CampusManagement.LanguageMappings.getCultureName(userLcid));
            }
            catch (e) {
                console.log("Failed to load LoadKendoCultureFiles with exception : ex -" + e);
            }
        }
        // update the new Expiry date for the marketing lists.
        MarketingList_1.updateNewExpiryDatetoMarketingLists = function () {
            var getExpiryDate = $('#activateListInput').data('kendoDatePicker').value();
            var lstStudentGroups = SonomaCmc.LocalStorage.get('cmc_inactiveMarketingLists');
            var todayDate = new Date();
            var updatedMarketingLists = [];
            lstStudentGroups.forEach(function (marketingList) {
                // prepare the updated list
                var lstmarketingList = {
                    '@odata.type': 'Microsoft.Dynamics.CRM.list',
                    listid: marketingList.listid,
                    statecode: StatusCode.Active
                };
                // update only for the student groups.
                if (marketingList.cmc_marketinglisttype === MarketingListType.StudentGroup) {
                    if (new Date(marketingList.cmc_expirationdate) < todayDate) {
                        lstmarketingList.cmc_expirationdate = getExpiryDate;
                    }
                }
                updatedMarketingLists.push(lstmarketingList); // push to the list
            });
            toggleLoading(true);
            MarketingList.updateMarketingList(updatedMarketingLists).then(function () {
                MarketingList.refreshFormOpenWindow();
                closeWindow();
            }, function error(error) { console.log(error); });
        };
        function cancel() {
            closeWindow();
        }
        MarketingList_1.cancel = cancel;
        function closeWindow() {
            if (!window.parent) {
                window.close();
            }
            else {
                window.parent.close();
            }
        }
        function toggleLoading(isLoading) {
            document.getElementById('loadingOverlay').style.display = isLoading
                ? ''
                : 'none';
        }
    })(MarketingList = CampusManagement.MarketingList || (CampusManagement.MarketingList = {}));
})(CampusManagement || (CampusManagement = {}));
