var CMC;
(function (CMC) {
    var Engage;
    (function (Engage) {
        var cmc_applicationrequirement;
        (function (cmc_applicationrequirement) {
            var _defaultId = '00000000-0000-0000-0000-000000000000', _contactId = _defaultId, _formLoaded;
            function onLoad(executionContext) {
                var formContext = executionContext.getFormContext(), applicationId = formContext.getAttribute('cmc_applicationid'), testScoreId = formContext.getAttribute('cmc_testscoreid'), previousEducationId = formContext.getAttribute('cmc_previouseducationid');
                if (_formLoaded) {
                    return;
                }
                _formLoaded = true;
                if (!applicationId) {
                    return;
                }
                applicationId.addOnChange(onApplicationIdChange);
                retrieveFilterRecords(formContext, true);
                if (previousEducationId) {
                    addPreSearchFunction(previousEducationId, onPreviousEducationPreSearch);
                }
                if (testScoreId) {
                    addPreSearchFunction(testScoreId, onTestScorePreSearch);
                }
            }
            cmc_applicationrequirement.onLoad = onLoad;
            function onApplicationIdChange(executionContext) {
                var formContext = executionContext.getFormContext(), recommendationId = formContext.getAttribute('cmc_recommendationid');
                retrieveFilterRecords(formContext);
                if (recommendationId && recommendationId.getValue() && recommendationId.getValue().length >= 0) {
                    recommendationId.setValue(null);
                    recommendationId.fireOnChange();
                }
            }
            function retrieveFilterRecords(formContext, skipClearContactFields) {
                if (skipClearContactFields === void 0) { skipClearContactFields = null; }
                var applicationId = formContext.getAttribute('cmc_applicationid');
                if (!applicationId || !applicationId.getValue() || applicationId.getValue().length <= 0) {
                    updateContactId(_defaultId, formContext, skipClearContactFields);
                    return;
                }
                Xrm.WebApi.retrieveRecord('cmc_application', applicationId.getValue()[0].id, '?$expand=cmc_contactid($select=contactid)').then(function (result) {
                    var newContactId = (result.cmc_contactid && result.cmc_contactid.contactid) || _defaultId;
                    updateContactId(newContactId, formContext, skipClearContactFields);
                }, function () {
                    // If someething goes wrong, set the Contact Id back to the default id
                    updateContactId(_defaultId, formContext, skipClearContactFields);
                });
            }
            function updateContactId(newContactId, formContext, skipClearContactFields) {
                var testScoreId = formContext.getAttribute('cmc_testscoreid'), previousEducationId = formContext.getAttribute('cmc_previouseducationid');
                if (newContactId !== _contactId && !skipClearContactFields) {
                    if (testScoreId && testScoreId.getValue() && testScoreId.getValue().length >= 0) {
                        testScoreId.setValue(null);
                        testScoreId.fireOnChange();
                    }
                    if (previousEducationId && previousEducationId.getValue() && previousEducationId.getValue().length >= 0) {
                        previousEducationId.setValue(null);
                        previousEducationId.fireOnChange();
                    }
                }
                _contactId = newContactId;
            }
            function addPreSearchFunction(attribute, preSearchFunction) {
                attribute.controls.forEach(function (control) {
                    control.addPreSearch(preSearchFunction);
                });
            }
            function onPreviousEducationPreSearch(executionContext) {
                addCustomFilter('cmc_previouseducationid', "<filter type='and'>\n         <condition attribute='mshied_studentid' operator='eq' value='" + _contactId + "' />\n       </filter>", 'mshied_previouseducation', executionContext);
            }
            function onTestScorePreSearch(executionContext) {
                addCustomFilter('cmc_testscoreid', "<filter type='and'>\n         <condition attribute='mshied_studentid' operator='eq' value='" + _contactId + "' />\n       </filter>", 'mshied_testscore', executionContext);
            }
            function addCustomFilter(attributeName, filter, entityName, executionContext) {
                executionContext.getFormContext().getAttribute(attributeName).controls.forEach(function (control) {
                    control.addCustomFilter(filter, entityName);
                });
            }
        })(cmc_applicationrequirement = Engage.cmc_applicationrequirement || (Engage.cmc_applicationrequirement = {}));
    })(Engage = CMC.Engage || (CMC.Engage = {}));
})(CMC || (CMC = {}));
