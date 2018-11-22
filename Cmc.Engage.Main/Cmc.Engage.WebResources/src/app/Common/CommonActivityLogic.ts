/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />

module CampusManagement.CommonActivityLogic {

    export enum entitytypecode {
        Account = "1"
    }

    const _opportunity = "opportunity",
        _contact = "contact";

    function getLifecycleContact(opportunityId) {
        var fetchXml =
            `<fetch top="1">
              <entity name="opportunity">
                <attribute name="cmc_contactid" />
                <filter type="and">
                  <condition attribute="opportunityid" operator="eq" value="${opportunityId}" />
                </filter>
              </entity>
            </fetch>`;

        return Xrm.WebApi.retrieveMultipleRecords(_opportunity, `?fetchXml=${fetchXml}`)
            .then(
            function success(result) {
                if (result.entities.length > 0) {
                    let opportunity = result.entities[0];
                    return {
                        "name": opportunity["_cmc_contactid_value@OData.Community.Display.V1.FormattedValue"],
                        "id": opportunity["_cmc_contactid_value"],
                        "entityType": _contact
                    };
                } else {
                    return null;
                }
            },
            function (error) {
                console.log(error);
                return null;
            });
    }

    export function FilterLifecycleActivityPartyList(partyListAttribute, opportunityId) {
        getLifecycleContact(opportunityId).then(
            contact => {
                let toParties = partyListAttribute.getValue();
                let newParties = [];
                newParties.push(contact);
                let contactId = "{" + contact.id.toUpperCase() + "}";
                if (toParties !== null) {                  
                  newParties = newParties.concat(toParties.filter(party => party.id !== contactId)); //know the new contact                              
                }              
                partyListAttribute.setValue(newParties);
            }
        )
    }
}
