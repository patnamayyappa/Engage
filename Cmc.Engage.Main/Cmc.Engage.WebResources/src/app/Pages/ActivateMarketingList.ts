module CampusManagement.MarketingList {
    import IMarketingList = Ribbon.MarketingList.IMarketingList;
    import StatusCode = Ribbon.MarketingList.StatusCode;
    import MarketingListType = Ribbon.MarketingList.MarketingListType;
    import MarketingList = Ribbon.MarketingList;

    declare let SonomaCmc: any;
    export function executeOnLoad() {
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
    } catch (e) {
      console.log("Failed to load LoadKendoCultureFiles with exception : ex -" + e);
    }
  }

    // update the new Expiry date for the marketing lists.
    export let updateNewExpiryDatetoMarketingLists = () => {
        let getExpiryDate: Date = $('#activateListInput').data('kendoDatePicker').value();
        let lstStudentGroups = SonomaCmc.LocalStorage.get('cmc_inactiveMarketingLists');
        let todayDate: Date = new Date();
        let updatedMarketingLists: Array<IMarketingList> = [];
        lstStudentGroups.forEach(marketingList => {
            // prepare the updated list
            let lstmarketingList: IMarketingList = {
                '@odata.type': 'Microsoft.Dynamics.CRM.list',
                listid: marketingList.listid,
                statecode: StatusCode.Active
            }
            // update only for the student groups.
            if (marketingList.cmc_marketinglisttype === MarketingListType.StudentGroup) {
                if (new Date(marketingList.cmc_expirationdate) < todayDate) {
                    lstmarketingList.cmc_expirationdate = getExpiryDate;
                }
            }
            updatedMarketingLists.push(lstmarketingList); // push to the list
        });
        toggleLoading(true);
        MarketingList.updateMarketingList(updatedMarketingLists).then(
            function () {
                MarketingList.refreshFormOpenWindow();
                closeWindow();
            }
            , function error(error) { console.log(error); });

    }

    export function cancel() {
        closeWindow();
    }

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
}
