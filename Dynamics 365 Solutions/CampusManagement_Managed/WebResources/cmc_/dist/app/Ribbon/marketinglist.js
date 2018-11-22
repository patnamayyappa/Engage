/// Customizations for the marketing list entity///
/// Activate click command is modified ///
var CampusManagement;
(function (CampusManagement) {
    var Ribbon;
    (function (Ribbon) {
        var MarketingList;
        (function (MarketingList) {
            // from the grid instance
            function activateMarketingLists(SelectedControl, SelectedControlSelectedItemReferences) {
                updateActivateMarketingList(SelectedControl, SelectedControlSelectedItemReferences);
            }
            MarketingList.activateMarketingLists = activateMarketingLists;
            // from the form instance
            function activateMarketingListForm(FirstPrimaryItemId) {
                activateMarketingLists(null, [{ "Id": FirstPrimaryItemId }]);
            }
            MarketingList.activateMarketingListForm = activateMarketingListForm;
            var StatusCode;
            (function (StatusCode) {
                StatusCode[StatusCode["Active"] = 0] = "Active";
                StatusCode[StatusCode["InActive"] = 1] = "InActive";
            })(StatusCode = MarketingList.StatusCode || (MarketingList.StatusCode = {}));
            var MarketingListType;
            (function (MarketingListType) {
                MarketingListType[MarketingListType["MarketingList"] = 175490000] = "MarketingList";
                MarketingListType[MarketingListType["StudentGroup"] = 175490001] = "StudentGroup";
            })(MarketingListType = MarketingList.MarketingListType || (MarketingList.MarketingListType = {}));
            function updateActivateMarketingList(SelectedControl, SelectedControlSelectedItemReferences) {
                var updatedMarketingLists = [];
                var currentDate = new Date();
                var invalidExpiryDate = false;
                var isMarketingList = false;
                retrieveList(SelectedControlSelectedItemReferences)
                    .then(function (fetchedMarketingLists) {
                    fetchedMarketingLists.entities.forEach(function (marketingList) {
                        var lstmarketingList = {
                            '@odata.type': 'Microsoft.Dynamics.CRM.list',
                            listid: marketingList.listid,
                            statecode: StatusCode.Active
                        };
                        // check if it is student group type, and validate the expiry date.
                        if (marketingList.cmc_marketinglisttype === MarketingListType.StudentGroup) {
                            if ((new Date(marketingList.cmc_expirationdate) < currentDate)) {
                                invalidExpiryDate = true;
                            }
                        }
                        updatedMarketingLists.push(lstmarketingList); // push to the list
                    });
                    // get the new Expiry date from the form and update the list back.
                    if (invalidExpiryDate) {
                        SonomaCmc.LocalStorage.set('cmc_inactiveMarketingLists', fetchedMarketingLists.entities, 1);
                        Xrm.Navigation.openWebResource('cmc_/Pages/ActivateMarketingList.html', {
                            height: 360,
                            width: 586,
                            openInNewWindow: true
                        });
                    }
                    else if (updatedMarketingLists.length > 0) {
                        MarketingList.updateMarketingList(updatedMarketingLists).then(function () {
                            refreshForm();
                        }, function error(error) { console.log(error); });
                    }
                }, function (error) {
                    console.log("An unexpected error has occurred" + " " + error.message);
                });
                //
                return updatedMarketingLists;
            }
            MarketingList.updateActivateMarketingList = updateActivateMarketingList;
            var retrieveList = function (marketinglistsIds) {
                var values = '';
                $.each(marketinglistsIds, function (index, value) { values = values + '<value>' + value.Id + '</value>'; });
                var fetchXml = [
                    '<fetch>',
                    '<entity name="list">',
                    '<attribute name="listid"/>',
                    '<attribute name="cmc_expirationdate"/>',
                    '<attribute name="cmc_marketinglisttype"/>',
                    '<filter type="and">',
                    '<condition attribute="statecode" operator="eq" value="1"/>',
                    '<condition attribute="listid" operator="in">',
                    values,
                    '</condition>',
                    '</filter>',
                    '</entity>',
                    '</fetch>'
                ].join('');
                return Xrm.WebApi.retrieveMultipleRecords('list', '?fetchXml=' + fetchXml);
            };
            // update the status of the marketing list 
            MarketingList.updateMarketingList = function (lstMarketingList) {
                return SonomaCmc.WebAPI.post('cmc_activatestudentgroups', {
                    StudentGroups: lstMarketingList
                });
            };
            function refreshForm() {
                var crmGrid = $("#crmGrid");
                if (Xrm.Page !== null && Xrm.Page.data != null)
                    Xrm.Page.data.refresh(false);
                else if (crmGrid != null && crmGrid.length > 0) {
                    crmGrid[0].control.refresh(); // refreshs the grid.
                }
                removeLocalStorageValues();
            }
            function refreshFormOpenWindow() {
                var crmGrid = $("#crmGrid");
                if (window.parent.opener.Xrm.Page.data)
                    window.parent.opener.Xrm.Page.data.refresh(false);
                else if (crmGrid.length < 1)
                    window.parent.opener.$("#crmGrid")[0].control.refresh();
                removeLocalStorageValues();
            }
            MarketingList.refreshFormOpenWindow = refreshFormOpenWindow;
            function removeLocalStorageValues() {
                SonomaCmc.LocalStorage.remove('cmc_SelectedControlItemReference');
            }
        })(MarketingList = Ribbon.MarketingList || (Ribbon.MarketingList = {}));
    })(Ribbon = CampusManagement.Ribbon || (CampusManagement.Ribbon = {}));
})(CampusManagement || (CampusManagement = {}));
