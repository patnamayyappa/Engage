/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
var CMC;
(function (CMC) {
    var Engage;
    (function (Engage) {
        var appointment;
        (function (appointment) {
            var _opportunity = "opportunity", _requiredAttendees = "requiredattendees", _regardingObjectId = "regardingobjectid";
            function onLoad(executionContext) {
                var formContext = executionContext.getFormContext();
                var regardingObjectAttribute = null;
                if (formContext.getAttribute(_regardingObjectId).getValue())
                    regardingObjectAttribute = formContext.getAttribute(_regardingObjectId).getValue()[0];
                if (regardingObjectAttribute !== null) {
                    var regardingObjectType = regardingObjectAttribute.entityType;
                    var regardingObjectId = regardingObjectAttribute.id;
                    if (regardingObjectType === _opportunity) {
                        CampusManagement.CommonActivityLogic.FilterLifecycleActivityPartyList(formContext.getAttribute(_requiredAttendees), regardingObjectId);
                    }
                }
            }
            appointment.onLoad = onLoad;
        })(appointment = Engage.appointment || (Engage.appointment = {}));
    })(Engage = CMC.Engage || (CMC.Engage = {}));
})(CMC || (CMC = {}));
