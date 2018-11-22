/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
var CampusManagement;
(function (CampusManagement) {
    var CommonActivityLogic;
    (function (CommonActivityLogic) {
        var entitytypecode;
        (function (entitytypecode) {
            entitytypecode["Account"] = "1";
        })(entitytypecode = CommonActivityLogic.entitytypecode || (CommonActivityLogic.entitytypecode = {}));
        var _opportunity = "opportunity", _contact = "contact";
        function getLifecycleContact(opportunityId) {
            var fetchXml = "<fetch top=\"1\">\n              <entity name=\"opportunity\">\n                <attribute name=\"cmc_contactid\" />\n                <filter type=\"and\">\n                  <condition attribute=\"opportunityid\" operator=\"eq\" value=\"" + opportunityId + "\" />\n                </filter>\n              </entity>\n            </fetch>";
            return Xrm.WebApi.retrieveMultipleRecords(_opportunity, "?fetchXml=" + fetchXml)
                .then(function success(result) {
                if (result.entities.length > 0) {
                    var opportunity_1 = result.entities[0];
                    return {
                        "name": opportunity_1["_cmc_contactid_value@OData.Community.Display.V1.FormattedValue"],
                        "id": opportunity_1["_cmc_contactid_value"],
                        "entityType": _contact
                    };
                }
                else {
                    return null;
                }
            }, function (error) {
                console.log(error);
                return null;
            });
        }
        function FilterLifecycleActivityPartyList(partyListAttribute, opportunityId) {
            getLifecycleContact(opportunityId).then(function (contact) {
                var toParties = partyListAttribute.getValue();
                var newParties = [];
                newParties.push(contact);
                var contactId = "{" + contact.id.toUpperCase() + "}";
                if (toParties !== null) {
                    newParties = newParties.concat(toParties.filter(function (party) { return party.id !== contactId; })); //know the new contact                              
                }
                partyListAttribute.setValue(newParties);
            });
        }
        CommonActivityLogic.FilterLifecycleActivityPartyList = FilterLifecycleActivityPartyList;
    })(CommonActivityLogic = CampusManagement.CommonActivityLogic || (CampusManagement.CommonActivityLogic = {}));
})(CampusManagement || (CampusManagement = {}));
