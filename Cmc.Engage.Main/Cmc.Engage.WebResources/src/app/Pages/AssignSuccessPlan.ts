module CampusManagement.AssignSuccessPlan {

    declare var SonomaCmc: any;
    declare module String {
        export var format: any;
    }
    var _studentIds,
        _fromForm,
        _selectedItem,
        _translations = {}, localizedStrings;

    export function executeOnLoad() {
        var translationKeys;
        _studentIds = SonomaCmc.LocalStorage.get('cmc_AssignSuccessPlanStudentIds') || [];
        _fromForm = SonomaCmc.LocalStorage.get('cmc_AssignSuccessPlanFromForm');

        document.getElementById('numberOfStudents').innerHTML = _studentIds.length;
      initializeDisplaySettings();     
        $('#successPlanTemplates').kendoAutoComplete({
            select: function (e) {
                _selectedItem = this.dataItem(e.item.index());
            },
            change: function (e) {
                var value = this.value();
                if (_selectedItem !== undefined && _selectedItem.cmc_successplantemplatename !== value) {
                    this.value("");
                    _selectedItem = undefined;
                }
          },
          noDataTemplate: localizedStrings.noDataMessage,
            dataSource: {
                type: 'odata',
                serverFiltering: true,
                transport: {
                    read: {
                        url: SonomaCmc.getClientUrl() + '/api/data/v' + getApiVersion() + '/cmc_successplantemplates?$select=cmc_successplantemplatename',
                        dataType: 'json'
                    },
                    parameterMap: function (options, operation) {
                        if (options.filter && options.filter.filters) {
                            for (var i = 0; i < options.filter.filters.length; i++) {
                                // CRM odata doesn't support 'tolower'
                                options.filter.filters[i].ignoreCase = false;
                                if (options.filter.filters[i].value) {
                                    options.filter.filters[i].value = options.filter.filters[i].value.replace(/%/gi, '[%]')
                                        .replace(/\*/gi, '%');
                                }
                            }
                        }

                        // CRM odata doesn't support $inlinecount or $format
                        var paramMap = kendo.data.transports.odata.parameterMap(options, null);
                        delete paramMap.$inlinecount;
                        delete paramMap.$format;

                        return paramMap;
                    }
                },
                filter: {
                    logic: "and",
                    filters: [{ field: 'statecode', operator: 'eq', value: 0 }]
                },
                schema: {
                    data: function (data) {
                        return data.value;
                    },
                    total: function (data) {
                        return data.value.length;
                    }
                }
            },
            dataTextField: 'cmc_successplantemplatename',
            animation: {
                open: {
                    duration: 0
                },
                close: {
                    duration: 0
                }
            }
        });
        $('#successPlanTemplates').focus();     
                if (!_studentIds || !_studentIds.length) {
                    openAlertDialog(localizedStrings.assignSuccessPlanDialog_NoStudentsMessage)
                        .then(function () {
                            closeWindow();
                        },
                        function () {
                            closeWindow();
                        });
                }
      toggleLoading(false);
      
    }

    function initializeDisplaySettings() {
        $('.dr-crm-title').text(CampusManagement.localization.getResourceString("AssignSuccessPlanTitle"));
        $('#subtitle').text(CampusManagement.localization.getResourceString("SelectSuccessPlan"));
        $('#lookupLabel').text(CampusManagement.localization.getResourceString("SuccessPlanTemplate"));
        $('#assignButton').text(CampusManagement.localization.getResourceString("AssignButton"));
      $('#cancelButton').text(CampusManagement.localization.getResourceString("CancelButton"));
      $('#lookupLabel').attr("title", CampusManagement.localization.getResourceString("SuccessPlanTemplate"));
      $('#assignButton').attr("title", CampusManagement.localization.getResourceString("AssignButton"));
      $('#cancelButton').attr("title", CampusManagement.localization.getResourceString("CancelButton"));
        $("title").text(CampusManagement.localization.getResourceString("AssignSuccessPlanTitle"));


        localizedStrings = {
            assignSuccessPlanDialog_SelectSuccessPlanTemplateMessage: CampusManagement.localization.getResourceString("AssignSuccessPlanDialog_SelectSuccessPlanTemplateMessage"),
            assignSuccessPlanDialog_SuccessMessageForNoDuplicates: CampusManagement.localization.getResourceString("AssignSuccessPlanDialog_SuccessMessageForNoDuplicates"),
            assignSuccessPlanDialog_SuccessMessageForAllDuplicates : CampusManagement.localization.getResourceString("AssignSuccessPlanDialog_SuccessMessageForAllDuplicates"),
            assignSuccessPlanDialog_SuccessMessageForSingleStudent : CampusManagement.localization.getResourceString("AssignSuccessPlanDialog_SuccessMessageForSingleStudent"),
            assignSuccessPlanDialog_FailureMessageForSingleStudent : CampusManagement.localization.getResourceString("AssignSuccessPlanDialog_FailureMessageForSingleStudent"),
            assignSuccessPlanDialog_NoStudentsMessage :CampusManagement.localization.getResourceString("AssignSuccessPlanDialog_NoStudentsMessage"),
            errorPrefix:CampusManagement.localization.getResourceString("ErrorPrefix"),
            okButton: CampusManagement.localization.getResourceString("OkButton"),
            assignSuccessPlanDialog_SuccessMessageForSomeDuplicates:CampusManagement.localization.getResourceString("AssignSuccessPlanDialog_SuccessMessageForSomeDuplicates"),                       
            assignSuccessPlanDialog_FailureMessageForNoAcademicPeriod: CampusManagement.localization.getResourceString("AssignSuccessPlanDialog_FailureMessageForNoAcademicPeriod"),
            assignSuccessPlanDuplicateMessage: CampusManagement.localization.getResourceString("AssignSuccessPlanDuplicateMessage"),
            AssignSuccessPlanDialog_FailureMessageForNoAcademicPeriodFullText: CampusManagement.localization.getResourceString("AssignSuccessPlanDialog_FailureMessageForNoAcademicPeriodFullText"),
            noDataMessage: CampusManagement.localization.getResourceString("NoDataMessage")
        }
  }
    export function cancel() {
        removeLocalStorageValues();
        closeWindow();
    }

    function closeWindow() {
        if (!window.parent) {
            window.close();
        }
        else {
            window.parent.close();
        }
    }
    function successPlanDialogMessage(data) {
        var strReturnMessage = "";
        if (data.AssignSuccessPlanDialogMessage === "AssignSuccessPlanDialog_SuccessMessageForNoDuplicates") {
            strReturnMessage = localizedStrings.assignSuccessPlanDialog_SuccessMessageForNoDuplicates;
        }
        else if (data.AssignSuccessPlanDialogMessage === "AssignSuccessPlanDialog_SuccessMessageForAllDuplicates") {
            strReturnMessage = localizedStrings.assignSuccessPlanDialog_SuccessMessageForAllDuplicates;
        }
        else if (data.AssignSuccessPlanDialogMessage === "AssignSuccessPlanDialog_SuccessMessageForSingleStudent") {
            strReturnMessage = localizedStrings.assignSuccessPlanDialog_SuccessMessageForSingleStudent;
        }
        else if (data.AssignSuccessPlanDialogMessage === "AssignSuccessPlanDialog_FailureMessageForSingleStudent") {
            strReturnMessage = localizedStrings.assignSuccessPlanDialog_FailureMessageForSingleStudent;
        }
        else if (data.AssignSuccessPlanDialogMessage ===
            "AssignSuccessPlanDialog_SuccessMessageForSomeDuplicates" &&
            data.DuplicateList.length > 0) {
            strReturnMessage = String.format(localizedStrings.assignSuccessPlanDuplicateMessage, data.DuplicateList.length, data.TotalCount) + " " + data.DuplicateList.join('; ');
        } else
            strReturnMessage = " ";

        // added string for the academic period is blank.
        if (data.StudentNamesWithoutAcademicPeriod != null && data.StudentNamesWithoutAcademicPeriod.length > 0) {

            strReturnMessage = (data.AssignSuccessPlanDialogMessage === "AssignSuccessPlanDialog_SuccessMessageForSomeDuplicates" ? strReturnMessage + '\r\n' : '') //append other error message in case of failures.
                + String.format(localizedStrings.AssignSuccessPlanDialog_FailureMessageForNoAcademicPeriodFullText, data.StudentNamesWithoutAcademicPeriod.length, data.TotalCount) +" " + data.StudentNamesWithoutAcademicPeriod.join('; ');
        }

        return strReturnMessage;
    }


    export function assign() {
        var students = [],
            successPlanTemplate,
            i, len;

        successPlanTemplate = _selectedItem;

        if (!successPlanTemplate || !successPlanTemplate.cmc_successplantemplateid) {
            openAlertDialog(
                localizedStrings.assignSuccessPlanDialog_SelectSuccessPlanTemplateMessage).then(function () {
                    document.getElementById('successPlanTemplates').focus();
                });
            return;
        }

        for (i = 0, len = _studentIds.length; i < len; i++) {
            students.push({
                '@odata.type': 'Microsoft.Dynamics.CRM.contact',
                contactid: _studentIds[i]
            });
        }

        toggleLoading(true);
        // Custom Actions cannot be called using Xrm.WebAPI at this time
        SonomaCmc.WebAPI.post('cmc_successplantemplates(' + successPlanTemplate.cmc_successplantemplateid + ')/Microsoft.Dynamics.CRM.cmc_createstudentsuccessplansfromtemplate',
            {
                Students: students
            })
            .then(function (result) {
                var message = successPlanDialogMessage(JSON.parse(result.SuccessPlanResponse));
                openAlertDialog(message).then(function () {
                    removeLocalStorageValues();
                    if (result.StudentSuccessPlanId) {
                        Xrm.Navigation.openForm({
                            entityId: result.StudentSuccessPlanId,
                            entityName: 'cmc_successplan',
                            openInNewWindow: true,
                        },null).then(function () {
                            closeWindow();
                        }, function () {
                            closeWindow();
                        });
                    }
                    else {
                        closeWindow();
                    }
                });
            })
            .catch(function (error) {
                openAlertDialog(localizedStrings.errorPrefix + ': ' + error).then(function () {
                        toggleLoading(false);
                    },
                    function () {
                        toggleLoading(false);
                    });
            });
    }

    function getApiVersion() {
        var currentVersion = '8.0',
            globalContext;

        if (!window.GetGlobalContext) {
            globalContext = window.GetGlobalContext();
            currentVersion = globalContext.getVersion();
            currentVersion = (currentVersion)
                ? currentVersion.split('.').slice(0, 2).join('.')
                : '8.0';
        }

        return currentVersion;
    }

    function toggleLoading(isLoading) {
        document.getElementById('loadingOverlay').style.display = isLoading
            ? ''
            : 'none';
    }

    function removeLocalStorageValues() {
        SonomaCmc.LocalStorage.remove('cmc_AssignSuccessPlanStudentIds');
        SonomaCmc.LocalStorage.remove('cmc_AssignSuccessPlanFromForm');
    }
  
    function openAlertDialog(message) {
        return Xrm.Navigation.openAlertDialog({
            text: message,
            confirmButtonLabel: localizedStrings.okButton
        },null);
    }   
}
