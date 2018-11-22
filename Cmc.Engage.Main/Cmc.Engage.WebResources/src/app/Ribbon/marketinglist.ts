

/// Customizations for the marketing list entity///
/// Activate click command is modified ///
module CampusManagement.Ribbon.MarketingList {
    declare let SonomaCmc: any;
    declare let XrmCore: any;
    // from the grid instance
    export function activateMarketingLists(SelectedControl, SelectedControlSelectedItemReferences): void {
        updateActivateMarketingList(SelectedControl, SelectedControlSelectedItemReferences);
    }
    // from the form instance
    export function activateMarketingListForm(FirstPrimaryItemId) {
        activateMarketingLists(null, [{ "Id": FirstPrimaryItemId }]);
    }

    export enum StatusCode {
        Active = 0,
        InActive = 1
    }

    export enum MarketingListType {
        MarketingList = 175490000,
        StudentGroup = 175490001
    }
    export interface IMarketingList {
        '@odata.type': string,
        listid: string,
        statecode: StatusCode,
        cmc_expirationdate?: Date
    }

    export function updateActivateMarketingList(SelectedControl, SelectedControlSelectedItemReferences) {
        let updatedMarketingLists: Array<IMarketingList> = [];
        let currentDate = new Date();
        let invalidExpiryDate: boolean = false;
        let isMarketingList: boolean = false;
        retrieveList(SelectedControlSelectedItemReferences)
            .then(fetchedMarketingLists => {
                fetchedMarketingLists.entities.forEach(marketingList => {
                    let lstmarketingList: IMarketingList = {
                        '@odata.type': 'Microsoft.Dynamics.CRM.list',
                        listid: marketingList.listid,
                        statecode: StatusCode.Active
                    }
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
                    Xrm.Navigation.openWebResource('cmc_/Pages/ActivateMarketingList.html',
                        {
                            height: 360,
                            width: 586,
                            openInNewWindow: true
                        });
                }
                else if (updatedMarketingLists.length > 0) {
                    updateMarketingList(updatedMarketingLists).then(
                        function () {
                            refreshForm();
                        }
                        , function error(error) { console.log(error); });
                }
                
            },
            error => {
                console.log("An unexpected error has occurred" + " " + error.message);
            });
        //
        return updatedMarketingLists;
    }

    let retrieveList = (marketinglistsIds): any => {
        let values = '';
        $.each(marketinglistsIds, (index, value: any) => { values = values + '<value>' + value.Id + '</value>'; });
        let fetchXml = [
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
    }
    // update the status of the marketing list 
    export let updateMarketingList = (lstMarketingList: Array<IMarketingList>) => {
        return SonomaCmc.WebAPI.post('cmc_activatestudentgroups',
            {
                StudentGroups: lstMarketingList
            })
    }
    function refreshForm() {

        let crmGrid: any = $("#crmGrid");
        if (Xrm.Page !== null && Xrm.Page.data != null)
            Xrm.Page.data.refresh(false);
        else if (crmGrid != null && crmGrid.length > 0) {
            crmGrid[0].control.refresh(); // refreshs the grid.
        }
        removeLocalStorageValues();
    }

    export function refreshFormOpenWindow() {
        let crmGrid: any = $("#crmGrid");
        if (window.parent.opener.Xrm.Page.data)
            window.parent.opener.Xrm.Page.data.refresh(false);
        else if (crmGrid.length < 1)
            window.parent.opener.$("#crmGrid")[0].control.refresh();
        removeLocalStorageValues();
    }

    function removeLocalStorageValues() {
        SonomaCmc.LocalStorage.remove('cmc_SelectedControlItemReference');
    }
}