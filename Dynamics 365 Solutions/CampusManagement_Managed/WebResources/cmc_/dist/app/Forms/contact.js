/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
var CampusManagement;
(function (CampusManagement) {
    var contact;
    (function (contact) {
        function onLoad(executionContext) {
            var formContext = executionContext.getFormContext();
            if (formContext.getAttribute('parentcustomerid')) {
                filterParentLookup(formContext);
                formContext.getAttribute('parentcustomerid').controls.forEach(function (control) {
                    control.setEntityTypes(['account']);
                });
            }
        }
        contact.onLoad = onLoad;
        function filterParentLookup(formContext) {
            var parentCustomerLookup = formContext.getControl('parentcustomerid');
            if (!parentCustomerLookup)
                return;
            parentCustomerLookup.addPreSearch(function (executionContext) {
                addInlineLookupFilters(executionContext);
            });
            parentCustomerLookup.setDefaultView('{7B89B0CD-54A1-E711-8103-5065F38B0191}'); // 'Active Campuses' View
        }
        function addInlineLookupFilters(executionContext) {
            // Inline Lookup - Remove Accounts not included in 'Active Campuses' View
            var accountFilterXml = [
                '<filter type="and">',
                '<condition attribute="statecode" operator="eq" value="0" />',
                '<condition attribute="mshied_accounttype" operator="eq" value="175490000" />',
                '</filter>'
            ].join('');
            executionContext.getEventSource().addCustomFilter(accountFilterXml, 'account');
        }
    })(contact = CampusManagement.contact || (CampusManagement.contact = {}));
})(CampusManagement || (CampusManagement = {}));
