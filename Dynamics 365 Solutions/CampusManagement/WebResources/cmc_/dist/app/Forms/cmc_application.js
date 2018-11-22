/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
var CMC;
(function (CMC) {
    var Engage;
    (function (Engage) {
        var cmc_application;
        (function (cmc_application) {
            var _formLoaded;
            function onLoad(executionContext) {
                var formContext = executionContext.getFormContext(), applicationRegistration = formContext.getAttribute('cmc_applicationregistration');
                if (_formLoaded) {
                    return;
                }
                _formLoaded = true;
                if (applicationRegistration) {
                    applicationRegistration.addOnChange(pullDownFieldsFromApplicationRegistration);
                    // On Create, check if Application Registration is already set and pull down Contact and
                    // Application Status
                    if (formContext.ui.getFormType() === 1 /* Create */) {
                        pullDownFieldsFromApplicationRegistration(executionContext);
                    }
                }
            }
            cmc_application.onLoad = onLoad;
            function pullDownFieldsFromApplicationRegistration(executionContext) {
                var formContext = executionContext.getFormContext(), applicationRegistration = formContext.getAttribute('cmc_applicationregistration'), contact = formContext.getAttribute('cmc_contactid'), applicationRegistrationValue;
                if (!applicationRegistration || !applicationRegistration.getValue() ||
                    applicationRegistration.getValue().length <= 0) {
                    if (contact) {
                        contact.setValue(null);
                    }
                    return;
                }
                applicationRegistrationValue = applicationRegistration.getValue()[0];
                Xrm.WebApi.retrieveRecord(applicationRegistrationValue.entityType, applicationRegistrationValue.id, '$select=cmc_contactid,cmc_applicationstatus&$expand=cmc_contactid($select=contactid,fullname)').then(function (result) {
                    var applicationStatus = formContext.getAttribute('cmc_applicationstatus');
                    if (contact) {
                        if (result.cmc_contactid && result.cmc_contactid.contactid) {
                            contact.setValue([{
                                    id: result.cmc_contactid.contactid,
                                    entityType: 'contact',
                                    name: result.cmc_contactid.fullname
                                }]);
                        }
                        else {
                            contact.setValue(null);
                        }
                    }
                    // Only set Application Status if it's not already set.
                    if (applicationStatus && !applicationStatus.getValue() && result.cmc_applicationstatus) {
                        applicationStatus.setValue(result.cmc_applicationstatus);
                    }
                }, function () {
                    // If an error occurs, just clear out Contact.
                    if (contact) {
                        contact.setValue(null);
                    }
                });
            }
        })(cmc_application = Engage.cmc_application || (Engage.cmc_application = {}));
    })(Engage = CMC.Engage || (CMC.Engage = {}));
})(CMC || (CMC = {}));
