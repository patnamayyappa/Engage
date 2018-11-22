/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />

module CampusManagement.cmc_trip {

  declare let Xrm;
  declare let SonomaCmc;
  let isConfirm = false;
  export function onLoad(executionContext) {
    handleTripApprovalProcess(executionContext);
    if (Xrm.Page.ui.getFormType() === XrmEnum.FormType.Create) {
      setDepartmentOnLoad(executionContext);
      if (Xrm.Page.getAttribute("cmc_statusdate")) {
        Xrm.Page.getAttribute("cmc_statusdate").setValue(Date.now());
      }
    }
    if (Xrm.Page.getAttribute("ownerid")) {
      Xrm.Page.getAttribute("ownerid").addOnChange(CampusManagement.cmc_trip.setDepartmentOnChange);
    }

    // on update scenario disable the form 
    if (Xrm.Page.ui.getFormType() === XrmEnum.FormType.Update) {
      utilities.disableTripActivityonTripStatus(executionContext, SonomaCmc.Guid.decapsulate(Xrm.Page.data.entity.getId()));
    }
    handleBusinessProcessFlows(executionContext);
  }

  export function onSave(executionContext) {

    let owner = Xrm.Page.getAttribute("ownerid").getValue();
    let ownerId = owner[0].id.replace('{', '').replace('}', '');

    let cmcStartDate: Date = new Date(Xrm.Page.getAttribute("cmc_startdate").getValue());
    let cmcEndDate: Date = new Date(Xrm.Page.getAttribute("cmc_enddate").getValue());

    let startDate: any = new Date(cmcStartDate);
    let endDate: any = new Date(cmcEndDate);
    startDate = startDate.format('yyyy-MM-dd');
    endDate = endDate.format('yyyy-MM-dd');

    // substring 1 day from start date to filter trip activity less than start date

    let newStartDate: any = new Date(cmcStartDate.setDate(cmcStartDate.getDate() - 1));
    newStartDate = newStartDate.format('yyyy-MM-dd');
    // adding 1 day to end date to filter trip activity grater than end date
    let newEndDate: any = new Date(cmcEndDate.setDate(cmcEndDate.getDate() + 1));
    newEndDate = newEndDate.format('yyyy-MM-dd');

    let url;
    let urlTripActivity
    if (Xrm.Page.ui.getFormType() == XrmEnum.FormType.Create) {
      url = Xrm.Page.context.getClientUrl() + "/api/data/v" + getVersion() + "/cmc_trips?$select=cmc_startdate&$filter=(_ownerid_value eq (" + ownerId + ")) and (((cmc_startdate le " + startDate + ") and (cmc_enddate ge " + startDate + ")) or ((cmc_startdate le " + endDate + ") and (cmc_enddate ge " + endDate + ")) or ((cmc_startdate ge " + startDate + ") and (cmc_enddate le " + endDate + "))) ";
    }
    else if (Xrm.Page.ui.getFormType() == XrmEnum.FormType.Update) {
      if (Xrm.Page.getAttribute("cmc_enddate").getIsDirty() || Xrm.Page.getAttribute("cmc_startdate").getIsDirty() || Xrm.Page.getAttribute("ownerid").getIsDirty()) {
        let cmcTripId = SonomaCmc.Guid.decapsulate(Xrm.Page.data.entity.getId());
        url = Xrm.Page.context.getClientUrl() + "/api/data/v" + getVersion() + "/cmc_trips?$select=cmc_startdate&$filter=cmc_tripid ne " + Xrm.Page.data.entity.getId().slice(1, -1) + " and(_ownerid_value eq (" + ownerId + ")) and (((cmc_startdate le " + startDate + ") and (cmc_enddate ge " + startDate + ")) or ((cmc_startdate le " + endDate + ") and (cmc_enddate ge " + endDate + ")) or ((cmc_startdate ge " + startDate + ") and (cmc_enddate le " + endDate + "))) ";
        urlTripActivity = Xrm.Page.context.getClientUrl() + "/api/data/v" + getVersion() + "/cmc_tripactivities?$filter=_cmc_trip_value eq (" + cmcTripId + ") and ((cmc_startdatetime le " + newStartDate + ") or (cmc_enddatetime ge " + newEndDate + "))";
      }
      else
        return;
    }
    $.ajax({
      type: "GET",
      contentType: "application/json; charset=utf-8",
      url: url,
      beforeSend: function (XMLHttpRequest) {
        XMLHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
        XMLHttpRequest.setRequestHeader("OData-Version", "4.0");
        XMLHttpRequest.setRequestHeader("Accept", "application/json");
      },
      async: false,
      success: function (data, textStatus, xhr) {

        if (data != null && data.value != null && data.value.length > 0) {
          CampusManagement.CommonUiControls.openAlertDialog(CampusManagement.localization.getResourceString("TravelMgmt_Tripwithsamestart_enddate"), CampusManagement.localization.getResourceString("OkButton"));
          if (Xrm.Page.ui.getFormType() == XrmEnum.FormType.Update) {
            isTripActivityOutOfTripRange(urlTripActivity);
          }
        }
        else {
          if (Xrm.Page.ui.getFormType() == XrmEnum.FormType.Update) {
            isTripActivityOutOfTripRange(urlTripActivity);
          }
        }
      },
      error: function (xhr, textStatus, errorThrown) {

      }
    });

  }

  function isTripActivityOutOfTripRange(url) {
    $.ajax({
      type: "GET",
      contentType: "application/json; charset=utf-8",
      url: url,
      beforeSend: function (XMLHttpRequest) {
        XMLHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
        XMLHttpRequest.setRequestHeader("OData-Version", "4.0");
        XMLHttpRequest.setRequestHeader("Accept", "application/json");
      },
      async: false,
      success: function (data, textStatus, xhr) {
        if (data != null && data.value != null && data.value.length > 0) {
          CampusManagement.CommonUiControls.openAlertDialog(CampusManagement.localization.getResourceString("TravelMgmt_TripActivityExceedsDateRangeOfAssociatedTrip"), CampusManagement.localization.getResourceString("OkButton"));
        }
      },
      error: function (xhr, textStatus, errorThrown) {

      }
    });
  }

  function setDepartmentOnLoad(executionContext) {
    if (Xrm.Page.getAttribute("cmc_department").getValue() === null || Xrm.Page.getAttribute("cmc_department").getValue().length < 1) {
      setDepartment(executionContext);
    }
  }
  export function setDepartmentOnChange(executionContext) { setDepartment(executionContext); }

  function setDepartment(executionContext) {
    let owner = Xrm.Page.getAttribute("ownerid").getValue();
    let ownerId = owner[0].id.replace('{', '').replace('}', '');
    let fetchXml = [
      '<fetch>',
      '<entity name="cmc_department" >',
      '<attribute name = "cmc_departmentid" />',
      '<attribute name="cmc_departmentname" />',
      '<link-entity name = "systemuser" alias = "ab" link = ""  type = "inner" to = "cmc_departmentid" from = "cmc_departmentid" >',
      '<filter type="and" >',
      '<condition attribute="systemuserid" value = "' + ownerId + '"  uitype = "systemuser"  operator = "eq" />',
      '</filter>',
      '</link-entity>',
      '</entity>',
      '</fetch>'
    ].join('');

    Xrm.WebApi.retrieveMultipleRecords("cmc_department", '?fetchXml=' + fetchXml)
      .then(
      function success(result) {
        if (result != null && result.entities != null && result.entities.length > 0) {
          Xrm.Page.getAttribute("cmc_department").setValue([{ id: result.entities[0].cmc_departmentid, name: result.entities[0].cmc_departmentname, entityType: "cmc_department" }]);
        }
        else {
          Xrm.Page.getAttribute("cmc_department").setValue();
        }
      },
      function (error) {
        console.log(error);
      });
  }

  function getVersion() {
    return Xrm.Page.context.getVersion().split('.').slice(0, 2).join('.');
  }
  function enableFields() {

    // Form fields
    if (Xrm.Page.getControl("cmc_approvalcomment")) {
      Xrm.Page.getControl("cmc_approvalcomment").setDisabled(false);
    }

    // Bpf fields
    if (Xrm.Page.getControl("header_process_cmc_approvalcomment")) {
      Xrm.Page.getControl("header_process_cmc_approvalcomment").setDisabled(false);
    }
  }
  function disableFields() {
    // Form fields
    if (Xrm.Page.getControl("cmc_approvedby")) {
      Xrm.Page.getControl("cmc_approvedby").setDisabled(true);
    }
    if (Xrm.Page.getControl("cmc_approvalcomment")) {
      Xrm.Page.getControl("cmc_approvalcomment").setDisabled(true);
    }
    if (Xrm.Page.getControl("cmc_approvaldate")) {
      Xrm.Page.getControl("cmc_approvaldate").setDisabled(true);
    }
    if (Xrm.Page.getControl("cmc_status")) {
      Xrm.Page.getControl("cmc_status").setDisabled(true);
    }
    if (Xrm.Page.getControl("cmc_statusdate")) {
      Xrm.Page.getControl("cmc_statusdate").setDisabled(true);
    }
    if (Xrm.Page.getControl("cmc_reviewerteam")) {
      Xrm.Page.getControl("cmc_reviewerteam").setDisabled(true);
    }
    //Bpf fields
    if (Xrm.Page.getControl("header_process_cmc_status")) {
      Xrm.Page.getControl("header_process_cmc_status").setDisabled(true);
    }
    if (Xrm.Page.getControl("header_process_cmc_approvedby")) {
      Xrm.Page.getControl("header_process_cmc_approvedby").setDisabled(true);
    }
    if (Xrm.Page.getControl("header_process_cmc_approvalcomment")) {
      Xrm.Page.getControl("header_process_cmc_approvalcomment").setDisabled(true);
    }
  }

  function handleTripApprovalProcess(executionContext) {

    // get logged in user details
    var liuId = Xrm.Page.context.getUserId();
    var liuName = Xrm.Page.context.getUserName();

    // get ApprovedBy user details
    var currApprovedBy = Xrm.Page.getAttribute("cmc_approvedby").getValue();

    disableFields();

    if (currApprovedBy) {
      var currApprovedById = currApprovedBy[0].id;
      if (liuId === currApprovedById) {

        // if the logged in user is equal to approved by user then enable Review Stage fields for edition.
        enableFields();

      }
    }
  }
  function handleBusinessProcessFlows(executionContext) {
    Xrm.Page.data.process.addOnStageChange(onBpfStageChange);
    Xrm.Page.data.process.addOnStageSelected(onBpfStageChange);
    disableSubmittedForReviewStageNextAndBackButton();
    disableReviewedStageFinishAndBackButton();
    disableBpfFields();
    enableBpfFields();
    disableCancelBpf();
  }
  function onBpfStageChange() {    
    disableSubmittedForReviewStageNextAndBackButton();
    disableReviewedStageFinishAndBackButton();
    disableBpfFields();
    enableBpfFields();
  }
  // Disable Bpf if trip is cancelled.
  function disableCancelBpf() {
    let tripStatusEnum = Constants.TripStatus;
    let tripStatus = Xrm.Page.getAttribute("cmc_status");
    if (tripStatusEnum.Canceled === tripStatus.getValue()) {
      var processStatus = Xrm.Page.data.process.getStatus();
      if (processStatus != null && (processStatus !== XrmEnum.ProcessStatus.Finished && processStatus !== XrmEnum.ProcessStatus.Aborted)) {
        Xrm.Page.data.process.setStatus(XrmEnum.ProcessStatus.Aborted);
      }
    }   
  }

  // Disable Next Stage button on SubmittedForReview stage.
  function disableSubmittedForReviewStageNextAndBackButton() {
    let submittedForReviewEnum = Constants.TripBusinessProcessFlowsStage.SubmittedForReview;
    if (Xrm.Page.data.process) {
      var activeStage = Xrm.Page.data.process.getActiveStage();
      var stageDivIdBack = parent.document.getElementById("stageBackActionContainer"); 
      var stageDivIdNext = parent.document.getElementById("stageAdvanceActionContainer");
      if (activeStage) {
        if (activeStage.getId() === submittedForReviewEnum) {
          if (stageDivIdNext) {
            stageDivIdNext.classList.add("disabled");
            stageDivIdNext.style.pointerEvents = 'none';
          }
          if (stageDivIdBack) {
            stageDivIdBack.classList.add("disabled");
            stageDivIdBack.style.pointerEvents = 'none';
          }
        }
        else {
          if (stageDivIdNext) {
            stageDivIdNext.classList.remove("disabled");
            stageDivIdNext.style.pointerEvents = 'auto';
          }
          if (stageDivIdBack) {
            stageDivIdBack.classList.remove("disabled");
            stageDivIdBack.style.pointerEvents = 'auto';
          }
        }
      }
    }
  }
  // Disable Finish button when Approver Rejected the trip.
  function disableReviewedStageFinishAndBackButton() {
    let reviewedEnum = Constants.TripBusinessProcessFlowsStage.Reviewed;
    let submittedForReviewEnum = Constants.TripBusinessProcessFlowsStage.SubmittedForReview;
    let tripStatusEnum = Constants.TripStatus;
    let tripStatus = Xrm.Page.getAttribute("cmc_status");
    if (Xrm.Page.data.process) {
      var activeStage = Xrm.Page.data.process.getActiveStage();
      var stageDivIdBack = parent.document.getElementById("stageBackActionContainer"); 
      var stageDivIdFinish = parent.document.getElementById("stageFinishActionContainer");
      if (activeStage) {
        if (activeStage.getId() === reviewedEnum && (tripStatus.getValue() === tripStatusEnum.Rejected || tripStatus.getValue() === tripStatusEnum.Canceled || tripStatus.getValue() === tripStatusEnum.Completed)) {
          if (stageDivIdFinish) {
            stageDivIdFinish.classList.add("disabled");
            stageDivIdFinish.style.pointerEvents = 'none';
          }
          if (stageDivIdBack) {
            stageDivIdBack.classList.add("disabled");
            stageDivIdBack.style.pointerEvents = 'none';
          }
        }
        else {
          if (stageDivIdFinish) {
            stageDivIdFinish.classList.remove("disabled");
            stageDivIdFinish.style.pointerEvents = 'auto';
          }
          if (stageDivIdBack && activeStage.getId() !== submittedForReviewEnum) {
            stageDivIdBack.classList.remove("disabled");
            stageDivIdBack.style.pointerEvents = 'auto';
          }
        }
      }
    }
  }

  function disableBpfFields() {   
    let tripStatusEnum = Constants.TripStatus;
    if (Xrm.Page.getAttribute("cmc_status")) {
      let tripStatus = Xrm.Page.getAttribute("cmc_status");

      //Disable Reviewed section
      if (Xrm.Page.ui.controls.get("header_process_cmc_status_1"))
        Xrm.Page.ui.controls.get("header_process_cmc_status_1").setDisabled(true);
      if (Xrm.Page.ui.controls.get("header_process_cmc_approvedby_1"))
        Xrm.Page.ui.controls.get("header_process_cmc_approvedby_1").setDisabled(true);
      if (Xrm.Page.ui.controls.get("header_process_cmc_approvalcomment_1"))
        Xrm.Page.ui.controls.get("header_process_cmc_approvalcomment_1").setDisabled(true);

      //Disable Create Trip Section      
      if (Xrm.Page.ui.controls.get("header_process_cmc_tripname_1"))
        Xrm.Page.ui.controls.get("header_process_cmc_tripname_1").setDisabled(true);
      if (Xrm.Page.ui.controls.get("header_process_cmc_tripname_2"))
        Xrm.Page.ui.controls.get("header_process_cmc_tripname_2").setDisabled(true);
      if (Xrm.Page.ui.controls.get("header_process_cmc_tripname_3"))
        Xrm.Page.ui.controls.get("header_process_cmc_tripname_3").setDisabled(true);
      if (Xrm.Page.ui.controls.get("header_process_cmc_startdate_1"))
        Xrm.Page.ui.controls.get("header_process_cmc_startdate_1").setDisabled(true);
      if (Xrm.Page.ui.controls.get("header_process_cmc_startdate_2"))
        Xrm.Page.ui.controls.get("header_process_cmc_startdate_2").setDisabled(true);
      if (Xrm.Page.ui.controls.get("header_process_cmc_startdate_3"))
        Xrm.Page.ui.controls.get("header_process_cmc_startdate_3").setDisabled(true);
      if (Xrm.Page.ui.controls.get("header_process_cmc_enddate_1"))
        Xrm.Page.ui.controls.get("header_process_cmc_enddate_1").setDisabled(true);
      if (Xrm.Page.ui.controls.get("header_process_cmc_enddate_2"))
        Xrm.Page.ui.controls.get("header_process_cmc_enddate_2").setDisabled(true);
      if (Xrm.Page.ui.controls.get("header_process_cmc_enddate_3"))
        Xrm.Page.ui.controls.get("header_process_cmc_enddate_3").setDisabled(true);

      // Disable Review Before Submit and Submit for review Based on trip status
      if (tripStatus.getValue() === tripStatusEnum.Approved || tripStatus.getValue() === tripStatusEnum.Canceled || tripStatus.getValue() === tripStatusEnum.Completed || tripStatus.getValue() === tripStatusEnum.Rejected || tripStatus.getValue() === tripStatusEnum.SubmitForApproval) {
        if (Xrm.Page.ui.controls.get("header_process_cmc_tripname"))
          Xrm.Page.ui.controls.get("header_process_cmc_tripname").setDisabled(true);
        if (Xrm.Page.ui.controls.get("header_process_cmc_startdate"))
          Xrm.Page.ui.controls.get("header_process_cmc_startdate").setDisabled(true);
        if (Xrm.Page.ui.controls.get("header_process_cmc_enddate"))
          Xrm.Page.ui.controls.get("header_process_cmc_enddate").setDisabled(true);
        if (Xrm.Page.ui.controls.get("header_process_cmc_travelersdetails"))
          Xrm.Page.ui.controls.get("header_process_cmc_travelersdetails").setDisabled(true);
        if (Xrm.Page.ui.controls.get("header_process_cmc_travelersdetails_1"))
          Xrm.Page.ui.controls.get("header_process_cmc_travelersdetails_1").setDisabled(true);
        if (Xrm.Page.ui.controls.get("header_process_cmc_travelersdetails_2"))
          Xrm.Page.ui.controls.get("header_process_cmc_travelersdetails_2").setDisabled(true);
        if (Xrm.Page.ui.controls.get("header_process_cmc_tripactivitydetails"))
          Xrm.Page.ui.controls.get("header_process_cmc_tripactivitydetails").setDisabled(true);
        if (Xrm.Page.ui.controls.get("header_process_cmc_tripactivitydetails_1"))
          Xrm.Page.ui.controls.get("header_process_cmc_tripactivitydetails_1").setDisabled(true);
        if (Xrm.Page.ui.controls.get("header_process_cmc_tripactivitydetails_2"))
          Xrm.Page.ui.controls.get("header_process_cmc_tripactivitydetails_2").setDisabled(true);
        if (Xrm.Page.ui.controls.get("header_process_cmc_estimatedexpensedetails"))
          Xrm.Page.ui.controls.get("header_process_cmc_estimatedexpensedetails").setDisabled(true);
        if (Xrm.Page.ui.controls.get("header_process_cmc_estimatedexpensedetails_1"))
          Xrm.Page.ui.controls.get("header_process_cmc_estimatedexpensedetails_1").setDisabled(true);
        if (Xrm.Page.ui.controls.get("header_process_cmc_estimatedexpensedetails_2"))
          Xrm.Page.ui.controls.get("header_process_cmc_estimatedexpensedetails_2").setDisabled(true);
        if (Xrm.Page.ui.controls.get("header_process_cmc_status"))
          Xrm.Page.ui.controls.get("header_process_cmc_status").setDisabled(true);
        if (Xrm.Page.ui.controls.get("header_process_cmc_approvedby"))
          Xrm.Page.ui.controls.get("header_process_cmc_approvedby").setDisabled(true);
        if (Xrm.Page.ui.controls.get("header_process_cmc_approvalcomment"))
          Xrm.Page.ui.controls.get("header_process_cmc_approvalcomment").setDisabled(true);
      }
    }
  }
  function enableBpfFields() {
    let tripStatusEnum = Constants.TripStatus;
    if (Xrm.Page.getAttribute("cmc_status")) {
      let tripStatus = Xrm.Page.getAttribute("cmc_status");
      if (tripStatus.getValue() === tripStatusEnum.Planning) {
        if (Xrm.Page.ui.controls.get("header_process_cmc_tripname"))
          Xrm.Page.ui.controls.get("header_process_cmc_tripname").setDisabled(false);
        if (Xrm.Page.ui.controls.get("header_process_cmc_startdate"))
          Xrm.Page.ui.controls.get("header_process_cmc_startdate").setDisabled(false);
        if (Xrm.Page.ui.controls.get("header_process_cmc_enddate"))
          Xrm.Page.ui.controls.get("header_process_cmc_enddate").setDisabled(false);
        if (Xrm.Page.ui.controls.get("header_process_cmc_travelersdetails"))
          Xrm.Page.ui.controls.get("header_process_cmc_travelersdetails").setDisabled(false);
        if (Xrm.Page.ui.controls.get("header_process_cmc_tripactivitydetails"))
          Xrm.Page.ui.controls.get("header_process_cmc_tripactivitydetails").setDisabled(false);
        if (Xrm.Page.ui.controls.get("header_process_cmc_estimatedexpensedetails"))
          Xrm.Page.ui.controls.get("header_process_cmc_estimatedexpensedetails").setDisabled(false);
      }
    }
  }
}
