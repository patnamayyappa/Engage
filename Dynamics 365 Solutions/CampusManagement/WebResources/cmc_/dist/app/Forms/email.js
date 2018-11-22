/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
var CMC;
(function (CMC) {
    var Engage;
    (function (Engage) {
        var email;
        (function (email) {
            var _opportunity = "opportunity", _to = "to", _regardingObjectId = "regardingobjectid";
            function onLoad(executionContext) {
                var formContext = executionContext.getFormContext();
                var regardingObjectAttribute = formContext.getAttribute(_regardingObjectId).getValue()[0];
                if (regardingObjectAttribute !== null) {
                    var regardingObjectType = regardingObjectAttribute.entityType;
                    var regardingObjectId = regardingObjectAttribute.id;
                    if (regardingObjectType === _opportunity) {
                        CampusManagement.CommonActivityLogic.FilterLifecycleActivityPartyList(formContext.getAttribute(_to), regardingObjectId);
                    }
                }
            }
            email.onLoad = onLoad;
        })(email = Engage.email || (Engage.email = {}));
    })(Engage = CMC.Engage || (CMC.Engage = {}));
})(CMC || (CMC = {}));
