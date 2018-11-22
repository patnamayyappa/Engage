/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
var CampusManagement;
(function (CampusManagement_1) {
    var cmc_domdefinition;
    (function (cmc_domdefinition) {
        var translations = {
            "UserLookupView": "",
        };
        function onLoad(executionContext) {
            var formContext = executionContext.getFormContext();
            InitializeDisplayStrings();
            domDefinitionFor = formContext.getControl("cmc_domdefinitionforid");
            //apply the custom filter only when clicked the domdefintionfor. 
            domDefinitionFor.addPreSearch(addCustomLookupFilterFordomDefinition);
        }
        cmc_domdefinition.onLoad = onLoad;
        function addCustomLookupFilterFordomDefinition() {
            domDefinitionFor = Xrm.Page.getControl("cmc_domdefinitionforid");
            var domDefinitionFetchXml = [
                '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="true" >',
                '<entity name="systemuser" >',
                '<all-attributes />',
                '<link-entity name="systemuserroles" from="systemuserid" to="systemuserid" alias="r" link-type="inner" >',
                '<link-entity name="role" from="roleid" to="roleid" alias="rp" link-type="inner" >',
                '<link-entity name="roleprivileges" from="roleid" to="roleid" alias="p" link-type="inner" >',
                '<link-entity name="privilege" from="privilegeid" to="privilegeid" alias="sr" link-type="inner" >',
                '<filter type="and" >',
                '<condition attribute="name" operator="eq" value="prvreadcmc_domdefinition" />',
                '</filter>',
                '</link-entity>',
                '</link-entity>',
                '<link-entity name="roleprivileges" from="roleid" to="roleid" alias="p2" link-type="inner" >',
                '<link-entity name="privilege" from="privilegeid" to="privilegeid" alias="sr2" link-type="inner" >',
                '<filter type="and" >',
                '<condition attribute="name" operator="eq" value="prvwritecmc_domdefinition" />',
                '</filter>',
                '</link-entity>',
                '</link-entity>',
                '</link-entity>',
                '</link-entity>',
                '<filter type="and" >',
                '<condition attribute="accessmode" operator="ne" value="1" />',
                '<condition attribute="accessmode" operator="ne" value="2" />',
                '</filter>',
                '</entity>',
                '</fetch>',
            ].join('');
            //crating LayoutXml as per the require attribute and size of the view. 
            var domDefinitionLayoutXml = "<grid name='resultset' object='1' jump='fullname' select='1' icon='1' preview='1'>" +
                "<row name='systemuser' id='systemuserid'>" + "<cell name='fullname' width='300' />" + "<cell name='positionid' width='100' />" +
                "<cell name='address1_telephone1' width='100' />" + "<cell name='businessunitid' width='150' />" + "<cell name='siteid' width='150' />" +
                "<cell name='title' width='95' />" + "<cell name='internalemailaddress' width='200' />" +
                "</row>" + "</grid>";
            //adding Custom view with the UserLookup viewId and viewname. 
            domDefinitionFor.addCustomView(Constants.Systemuser.UserLookupViewId, "systemuser", translations["UserLookupView"], domDefinitionFetchXml, domDefinitionLayoutXml, true);
        }
        function InitializeDisplayStrings() {
            translations.UserLookupView = CampusManagement.localization.getResourceString("UserLookupView");
        }
    })(cmc_domdefinition = CampusManagement_1.cmc_domdefinition || (CampusManagement_1.cmc_domdefinition = {}));
})(CampusManagement || (CampusManagement = {}));
