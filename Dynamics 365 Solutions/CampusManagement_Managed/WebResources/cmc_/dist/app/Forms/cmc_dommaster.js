/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
var CampusManagement;
(function (CampusManagement_1) {
    var cmc_dommaster;
    (function (cmc_dommaster) {
        var _previousMemberType = null, _runAssignmentAttr, _attributeNameToBeAssignedAttr, _otherUserLookupAttr, _otherUserLookupCtrl;
        var _config = {
            // Account: 
            "1": ["175490000"],
            // Contact: Contact, Lifecycle (Opportunity)
            "2": ["175490001", "175490003"],
            // Lead: Lead
            "4": ["175490002"],
            // Default: Account, Contact, Lifecycle, Lead
            "default": ["175490000", "175490001", "175490003", "175490002"]
        };
        var AttributeNameToBeAssignedType;
        (function (AttributeNameToBeAssignedType) {
            AttributeNameToBeAssignedType[AttributeNameToBeAssignedType["RecordOwner"] = 175490000] = "RecordOwner";
            AttributeNameToBeAssignedType[AttributeNameToBeAssignedType["OtherUserLookup"] = 175490001] = "OtherUserLookup";
        })(AttributeNameToBeAssignedType = cmc_dommaster.AttributeNameToBeAssignedType || (cmc_dommaster.AttributeNameToBeAssignedType = {}));
        var RunAssignmentForEntityType;
        (function (RunAssignmentForEntityType) {
            RunAssignmentForEntityType[RunAssignmentForEntityType["Account"] = 175490000] = "Account";
            RunAssignmentForEntityType[RunAssignmentForEntityType["Contact"] = 175490001] = "Contact";
            RunAssignmentForEntityType[RunAssignmentForEntityType["Lead"] = 175490002] = "Lead";
            RunAssignmentForEntityType[RunAssignmentForEntityType["Opportunity"] = 175490003] = "Opportunity";
        })(RunAssignmentForEntityType = cmc_dommaster.RunAssignmentForEntityType || (cmc_dommaster.RunAssignmentForEntityType = {}));
        var translations = {
            "DomExecutionOrderAttributeMismatch_SingleError": "",
            "DomExecutionOrderAttributeMismatch_MultipleErrors": "",
            "DefinitionLogicAttributeMismatch_SingleError": "",
            "DefinitionLogicAttributeMismatch_MultipleErrors": "",
            "UserLookupView": "",
        };
        function onLoad(executionContext) {
            var formContext = executionContext.getFormContext();
            InitializeDisplayStrings();
            runValidations(formContext);
            fallBackUser = formContext.getControl("cmc_fallbackuserid");
            //apply the custom filter only when clicked the fall back user.
            fallBackUser.addPreSearch(addCustomLookupFilterForfallBackUser);
            _runAssignmentAttr = formContext.getAttribute("cmc_runassignmentforentity");
            _attributeNameToBeAssignedAttr = formContext.getAttribute("cmc_attributenametobeassigned");
            _otherUserLookupAttr = formContext.getAttribute("cmc_otheruserlookup");
            _otherUserLookupCtrl = formContext.getControl("WebResource_AttributePicker");
            if (formContext.getAttribute("cmc_marketinglistid")) {
                var previousMarketingList = formContext.getAttribute("cmc_marketinglistid").getValue();
                if (previousMarketingList && previousMarketingList.length > 0) {
                    getMemberType(previousMarketingList[0].id).then(function (memberType) {
                        _previousMemberType = memberType;
                        if (_previousMemberType != "2") {
                            filterRunAssignmentForEntity(_previousMemberType, _config[_previousMemberType][0]);
                        }
                        else {
                            filterRunAssignmentForEntity(_previousMemberType, _runAssignmentAttr.getValue());
                        }
                    });
                }
                else {
                    _previousMemberType = null;
                    marketingListOnNull(formContext);
                }
                formContext.getAttribute("cmc_marketinglistid").addOnChange(onMarketingListChange);
            }
            if (_runAssignmentAttr) {
                setupAttributePicker(formContext);
                _runAssignmentAttr.addOnChange(setupAttributePicker);
            }
            if (_attributeNameToBeAssignedAttr && _otherUserLookupCtrl) {
                toggleOtherUserLookup(formContext);
                _attributeNameToBeAssignedAttr.addOnChange(toggleOtherUserLookup);
            }
        }
        cmc_dommaster.onLoad = onLoad;
        function setupAttributePicker(formContext) {
            var entity;
            if (_runAssignmentAttr) {
                var forEntityInt = _runAssignmentAttr.getValue();
                switch (forEntityInt) {
                    case RunAssignmentForEntityType.Account:
                        entity = "account";
                        break;
                    case RunAssignmentForEntityType.Contact:
                        entity = "contact";
                        break;
                    case RunAssignmentForEntityType.Lead:
                        entity = "lead";
                        break;
                    case RunAssignmentForEntityType.Opportunity:
                        entity = "opportunity";
                        break;
                }
            }
            if (!entity) {
                return;
            }
            var input = '{"EntityLogicalName": "' + entity + '"}';
            if (_otherUserLookupCtrl)
                _otherUserLookupCtrl.setSrc("$webresources/cmc_/dist/attributePicker/index.html?data=" + encodeURIComponent("component=attributePicker&entity=" + entity + "&showLabel=false&schemafield=cmc_otheruserlookup&retrieveMetadataJsonAction=cmc_retrieveuserlookupsforentity&retrieveMetadataJsonInput=" + input));
        }
        function toggleOtherUserLookup(formContext) {
            if (!_attributeNameToBeAssignedAttr)
                return;
            var attributeNameToBeAssignedValue = _attributeNameToBeAssignedAttr.getValue();
            if (!attributeNameToBeAssignedValue)
                return;
            if (attributeNameToBeAssignedValue === AttributeNameToBeAssignedType.OtherUserLookup) {
                _otherUserLookupCtrl.setVisible(true);
                setupAttributePicker(formContext);
            }
            else {
                _otherUserLookupCtrl.setVisible(false);
                _otherUserLookupAttr.setValue();
                _otherUserLookupAttr.fireOnChange();
            }
        }
        function runValidations(formContext) {
            retrieveRelatedRecords(formContext.data.entity.getId())
                .then(function (relatedRecords) {
                var flags = [], executionOrderAttributes = [], definitionLogicAttributes = [], l = relatedRecords.entities.length, i;
                for (i = 0; i < l; i++) {
                    if (!relatedRecords.entities[i]["eo.cmc_attributeschema"] || flags[relatedRecords.entities[i]["eo.cmc_attributeschema"]])
                        continue;
                    flags[relatedRecords.entities[i]["eo.cmc_attributeschema"]] = true;
                    var schema = relatedRecords.entities[i]["eo.cmc_attributeschema"];
                    if (schema)
                        schema = schema.toLowerCase();
                    executionOrderAttributes.push({ schema: schema, attribute: relatedRecords.entities[i]["eo.cmc_attribute"] });
                }
                flags = [];
                for (i = 0; i < l; i++) {
                    if (!relatedRecords.entities[i]["ddl.cmc_attributeschema"] || flags[relatedRecords.entities[i]["ddl.cmc_attributeschema"]])
                        continue;
                    flags[relatedRecords.entities[i]["ddl.cmc_attributeschema"]] = true;
                    var schema = relatedRecords.entities[i]["ddl.cmc_attributeschema"];
                    if (schema)
                        schema = schema.toLowerCase();
                    definitionLogicAttributes.push({ schema: schema, attribute: relatedRecords.entities[i]["ddl.cmc_attribute"] });
                }
                attributeValidation(formContext, executionOrderAttributes, definitionLogicAttributes, translations["DefinitionLogicAttributeMismatch_SingleError"], translations["DefinitionLogicAttributeMismatch_MultipleErrors"]);
                attributeValidation(formContext, definitionLogicAttributes, executionOrderAttributes, translations["DomExecutionOrderAttributeMismatch_SingleError"], translations["DomExecutionOrderAttributeMismatch_MultipleErrors"]);
            }, function (error) {
                console.log(error);
            });
        }
        function attributeValidation(formContext, mainAttributeList, subAttributeList, singleErrorKey, multipleErrorsKey) {
            var inValidAttributes = [];
            for (var i = 0; i < mainAttributeList.length; i++) {
                var attribute = mainAttributeList[i];
                if (!isSchemaInList(subAttributeList, attribute.schema))
                    inValidAttributes.push(attribute.attribute);
            }
            if (inValidAttributes.length === 1) {
                formContext.ui.setFormNotification(singleErrorKey.replace("{0}", inValidAttributes[0]), "WARNING");
            }
            else if (inValidAttributes.length > 1) {
                formContext.ui.setFormNotification(multipleErrorsKey, "WARNING");
            }
        }
        function isSchemaInList(list, value) {
            for (var i = 0; i < list.length; i++) {
                var schema = list[i].schema;
                if (!schema)
                    continue;
                var val = schema.toLowerCase();
                if (val === value)
                    return true;
            }
            return false;
        }
        function getMemberType(marketingListId) {
            var curMemberType = null;
            return Xrm.WebApi.retrieveRecord("list", marketingListId, "?$select=createdfromcode")
                .then(function success(result) {
                curMemberType = result.createdfromcode;
                return curMemberType;
            }, function (error) {
                filterRunAssignmentForEntity("default", _runAssignmentAttr.getValue());
            });
        }
        function filterRunAssignmentForEntity(memberType, optionValue) {
            var runAssignmentForEntity = _runAssignmentAttr.controls, runAssignmentForEntityOptions;
            if (memberType) {
                if (memberType in _config) {
                    runAssignmentForEntityOptions = _config[memberType];
                }
                else {
                    // Reset to known default option set values, member type exists but is not part of current config
                    runAssignmentForEntityOptions = _config["default"];
                }
                runAssignmentForEntity.forEach(function (control) {
                    control.clearOptions();
                    for (var i = 0; i < runAssignmentForEntityOptions.length; i++) {
                        var runAssignmentForEntityOption = runAssignmentForEntityOptions[i];
                        var option = _runAssignmentAttr.getOption(runAssignmentForEntityOption);
                        control.addOption(option);
                    }
                });
            }
            else {
                runAssignmentForEntity.forEach(function (control) {
                    control.clearOptions();
                });
            }
            _runAssignmentAttr.setValue(optionValue);
        }
        function marketingListOnNull(formContext) {
            // Remove all values options and clear Run Assignment Field
            filterRunAssignmentForEntity(null, null);
        }
        function onMarketingListChange(executionContext) {
            var formContext = executionContext.getFormContext();
            var marketingList = formContext.getAttribute("cmc_marketinglistid").getValue();
            if (marketingList && marketingList.length > 0) {
                var option_1 = null;
                var marketingListId = marketingList[0].id;
                getMemberType(marketingListId).then(function (memberType) {
                    if (_previousMemberType !== memberType) {
                        // On update, clear Run Assignment for Entity if Member Type is 'Contact'
                        if (memberType != "2") {
                            option_1 = _config[memberType][0];
                        }
                        filterRunAssignmentForEntity(memberType, option_1);
                    }
                    // Update previous cached member type
                    _previousMemberType = memberType;
                });
            }
            else {
                marketingListOnNull(formContext);
                _previousMemberType = null;
            }
        }
        function retrieveRelatedRecords(domMasterId) {
            var fetchXml = [
                '<fetch>',
                '<entity name="cmc_dommaster">',
                '<attribute name="cmc_dommasterid" />',
                '<link-entity name="cmc_domdefinitionexecutionorder" to="cmc_dommasterid" from="cmc_dommasterid" link-type="outer" alias="eo">',
                '<attribute name="cmc_domdefinitionexecutionorderid" />',
                '<attribute name="cmc_attribute" />',
                '<attribute name="cmc_attributeschema" />',
                '</link-entity>',
                '<link-entity name="cmc_domdefinition" to="cmc_dommasterid" from="cmc_dommasterid" link-type="outer" alias="dd">',
                '<link-entity name="cmc_domdefinitionlogic" to="cmc_domdefinitionid" from="cmc_domdefinitionid" link-type="outer" alias="ddl">',
                '<attribute name="cmc_domdefinitionlogicid" />',
                '<attribute name="cmc_attribute" />',
                '<attribute name="cmc_attributeschema" />',
                '</link-entity>',
                '</link-entity>',
                '<filter>',
                '<condition attribute="cmc_dommasterid" operator="eq" value="' + domMasterId + '" />',
                '</filter>',
                '</entity>',
                '</fetch>'
            ].join('');
            return Xrm.WebApi.retrieveMultipleRecords('cmc_dommaster', '?fetchXml=' + fetchXml);
        }
        function addCustomLookupFilterForfallBackUser() {
            fallBackUser = Xrm.Page.getControl("cmc_fallbackuserid");
            var fallBackUserFetchxml = [
                '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="true" >',
                '<entity name="systemuser" >',
                '<all-attributes />',
                '<link-entity name="systemuserroles" from="systemuserid" to="systemuserid" alias="r" link-type="inner" >',
                '<link-entity name="role" from="roleid" to="roleid" alias="rp" link-type="inner" >',
                '<link-entity name="roleprivileges" from="roleid" to="roleid" alias="p" link-type="inner" >',
                '<link-entity name="privilege" from="privilegeid" to="privilegeid" alias="sr" link-type="inner" >',
                '<filter type="and" >',
                '<condition attribute="name" operator="eq" value="prvreadcmc_dommaster" />',
                '</filter>',
                '</link-entity>',
                '</link-entity>',
                '<link-entity name="roleprivileges" from="roleid" to="roleid" alias="p2" link-type="inner" >',
                '<link-entity name="privilege" from="privilegeid" to="privilegeid" alias="sr2" link-type="inner" >',
                '<filter type="and" >',
                '<condition attribute="name" operator="eq" value="prvwritecmc_dommaster" />',
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
            var fallBackUserLayoutXml = "<grid name='resultset' object='1' jump='fullname' select='1' icon='1' preview='1'>" +
                "<row name='systemuser' id='systemuserid'>" + "<cell name='fullname' width='300' />" + "<cell name='positionid' width='100' />" +
                "<cell name='address1_telephone1' width='100' />" + "<cell name='businessunitid' width='150' />" + "<cell name='siteid' width='150' />" +
                "<cell name='title' width='95' />" + "<cell name='internalemailaddress' width='200' />" +
                "</row>" + "</grid>";
            //adding Custom view with the UserLookup viewId and viewname. 
            fallBackUser.addCustomView(Constants.Systemuser.UserLookupViewId, "systemuser", translations["UserLookupView"], fallBackUserFetchxml, fallBackUserLayoutXml, true);
        }
        function InitializeDisplayStrings() {
            translations.DomExecutionOrderAttributeMismatch_SingleError =
                CampusManagement.localization.getResourceString("DomExecutionOrderAttributeMismatch_SingleError");
            translations.DomExecutionOrderAttributeMismatch_MultipleErrors =
                CampusManagement.localization.getResourceString("DomExecutionOrderAttributeMismatch_MultipleErrors");
            translations.DefinitionLogicAttributeMismatch_SingleError =
                CampusManagement.localization.getResourceString("DefinitionLogicAttributeMismatch_SingleError");
            translations.DefinitionLogicAttributeMismatch_MultipleErrors =
                CampusManagement.localization.getResourceString("DefinitionLogicAttributeMismatch_MultipleErrors");
            translations.UserLookupView =
                CampusManagement.localization.getResourceString("UserLookupView");
        }
    })(cmc_dommaster = CampusManagement_1.cmc_dommaster || (CampusManagement_1.cmc_dommaster = {}));
})(CampusManagement || (CampusManagement = {}));
