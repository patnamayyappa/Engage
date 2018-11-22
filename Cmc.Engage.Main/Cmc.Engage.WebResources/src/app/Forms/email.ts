/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />

module CMC.Engage.email {

    const _opportunity = "opportunity",
        _to = "to",
        _regardingObjectId = "regardingobjectid";

    export function onLoad(executionContext) {
      let formContext = executionContext.getFormContext();
      let regardingObjectAttribute = null;
      if (formContext.getAttribute(_regardingObjectId).getValue())
         regardingObjectAttribute = formContext.getAttribute(_regardingObjectId).getValue()[0];
      if (regardingObjectAttribute !== null) {          
            let regardingObjectType = regardingObjectAttribute.entityType;
            let regardingObjectId = regardingObjectAttribute.id;

            if (regardingObjectType === _opportunity) {
                CampusManagement.CommonActivityLogic.FilterLifecycleActivityPartyList(formContext.getAttribute(_to), regardingObjectId);
            }
        }
    }
}
