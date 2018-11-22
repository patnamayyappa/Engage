module CampusManagement.AssignStaffSurvey {

	declare var SonomaCmc: any, document: any;

	var _staffIds,
		_fromForm,
		_selectedItem,
		_surveyIds,
    _translations = {},
    localizedStrings;

	export function executeOnLoad() {
		var translationKeys;
		_staffIds = SonomaCmc.LocalStorage.get('cmc_AssignStaffIds') || [];
		_fromForm = SonomaCmc.LocalStorage.get('cmc_AssignStaffCourseFromForm');

		document.getElementById('numberOfStaff').innerHTML = _staffIds.length;
      initializeDisplaySettings();     
		$('#StaffCourseTemplates').kendoAutoComplete({
			select: function (e) {
				_selectedItem = this.dataItem(e.item.index());
			},
			change: function (e) {
				var value = this.value();
				if (value === "") {
					_selectedItem = undefined;
				}
      },
      noDataTemplate: localizedStrings.noDataMessage,            
			dataSource: {
				type: 'odata',
				serverFiltering: true,
				transport: {
					read: {
						url: SonomaCmc.getClientUrl() + '/api/data/v' + CampusManagement.XrmSystemInformation.getApiVersion() + '/cmc_staffsurveytemplates?$select=cmc_staffsurveytemplatename',
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
			dataTextField: 'cmc_staffsurveytemplatename',
			animation: {
				open: {
					duration: 0
				},
				close: {
					duration: 0
				}
			}
		})
			.focus(); toggleLoading(false);

	}

	function initializeDisplaySettings() {
		$('.dr-crm-title').text(CampusManagement.localization.getResourceString("AssignStaffSurveyTitle"));
		$('#subtitle').text(CampusManagement.localization.getResourceString("SelectStaffSurvey"));
		$('#lookupLabel').text(CampusManagement.localization.getResourceString("StaffSurveyTemplate"));
		$('#assignButton').text(CampusManagement.localization.getResourceString("AssignButton"));
      $('#cancelButton').text(CampusManagement.localization.getResourceString("CancelButton"));
      $('#lookupLabel').attr("title", CampusManagement.localization.getResourceString("StaffSurveyTemplate"));
      $('#assignButton').attr("title", CampusManagement.localization.getResourceString("AssignButton"));
      $('#cancelButton').attr("title", CampusManagement.localization.getResourceString("CancelButton"));

      localizedStrings = {
        okButton: CampusManagement.localization.getResourceString("OkButton"),
        errorPrefix: CampusManagement.localization.getResourceString("ErrorPrefix"),
        selectStaffSurveyTemplate: CampusManagement.localization.getResourceString("SelectStaffSurveyTemplate"),
        staffSurveyNotAssign: CampusManagement.localization.getResourceString("StaffSurveyNotAssign"),
        staffSurveyAssign: CampusManagement.localization.getResourceString("StaffSurveyAssign"),
        noDataMessage: CampusManagement.localization.getResourceString("NoDataMessage")        
      }

  }
    
	export function assign() {
		var staff = [],
			StaffCourseTemplates,
			i, len;
		StaffCourseTemplates = _selectedItem;
		if (!StaffCourseTemplates || !StaffCourseTemplates.cmc_staffsurveytemplateid) {
          CampusManagement.CommonUiControls.openAlertDialog(localizedStrings.selectStaffSurveyTemplate, localizedStrings.okButton).then(function () {
				document.getElementById('StaffCourseTemplates').focus();
			});
			return;
		}

		for (i = 0, len = _staffIds.length; i < len; i++) {
			staff.push({
              '@odata.type': 'Microsoft.Dynamics.CRM.mshied_coursesection',
              mshied_coursesectionid: _staffIds[i]
			});
		}
		toggleLoading(true);
		// Custom Actions cannot be called using Xrm.WebAPI at this time
		SonomaCmc.WebAPI.post('cmc_staffsurveytemplates(' + StaffCourseTemplates.cmc_staffsurveytemplateid + ')/Microsoft.Dynamics.CRM.cmc_CreateStaffSurveyFromStaffSurveyTemplate',
			{
				Staffs: staff
			})
			.then(function (result) {
				var message = JSON.parse(result.staffSurveyResponse);
				var messageTxt;
				if (message.failedStaffCourses.length > 0) {
            messageTxt = localizedStrings.staffSurveyNotAssign;
					$.each(message.failedStaffCourses, function (index, value) {
						messageTxt += "\r\n" + value + ", ";
					})
				}
				else {
             messageTxt = localizedStrings.staffSurveyAssign;
				}
              CampusManagement.CommonUiControls.openAlertDialog(messageTxt, localizedStrings.okButton).then(function () {
					removeLocalStorageValues('assignStaffSurvey');
					closeWindow();
				});
			})
			.catch(function (error) {
              CampusManagement.CommonUiControls.openAlertDialog(localizedStrings.errorPrefix + ': ' + error, localizedStrings.okButton).then(function () {
					toggleLoading(false);
				},
					function () {
						toggleLoading(false);
					});
			});
	}


	export function cancel(page) {
		removeLocalStorageValues(page);
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

	function toggleLoading(isLoading) {
		document.getElementById('loadingOverlay').style.display = isLoading
			? ''
			: 'none';
	}

	function removeLocalStorageValues(page) {
		var keys = [];
		keys = GetStorageKeys(page);
		if (keys.length > 0) {
			for (let i = 0; i > keys.length; i--) {
				SonomaCmc.LocalStorage.remove(keys[i])
			}
		}
	
	}
	function GetStorageKeys(page) {
		if (page != null || page != undefined) {

			if (page == 'assignStaffSurvey')
				return ['cmc_AssignStaffIds', 'cmc_AssignStaffCourseFromForm'];
			else if (page == 'UpdateStaffSurvey') //keys used in StaffSurvey Deactivation or Activation
				return ['cmc_UpdatingSurveyId'];
			else
				return [];
		}
		else
			return [];
	}
}
