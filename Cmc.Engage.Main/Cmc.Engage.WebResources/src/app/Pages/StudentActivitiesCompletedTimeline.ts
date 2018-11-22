module CampusManagement.StudentActivitiesCompletedTimeline {
  declare var vis;
  declare module String {
    export var format: any;
  }
  declare var SonomaCmc;
  var listOfLifecyleName = [], listOfLyfecycleId = [], extendedEntites=[] ;
  var timeline,
    localizedStrings,
    items = new vis.DataSet(),
    itemDetails = {},
    appointmentStatus = {
      "Cancelled": 4
    },
    phoneStatus = {
      "Cancelled": 3
    },
    emailStatus = {
      "Cancelled": 5
    },
    todoStatus = {
      "Incomplete": 1,
      "Complete": 2
    },
    directioncode = {

      "incoming": false,
      "outgoing": true
    },
    activityParty = {
      "From": 1
    };


  export function executeOnLoad() {
    initializeDisplaySettings();
    var studentId = GetGlobalContext().getQueryStringParameters().id;
    var container = document.getElementById('timeline-visualization');
    var userLcid = Xrm.Page.context.getUserLcid();
    var options = {
      tooltip: {
        followMouse: true,
        overflowMethod: 'cap'
      },
      margin: {
        item: {
          horizontal: 5,
          vertical: 5
        }
      },
      horizontalScroll: true,
      height: 250,
      moveable: true, // must be 'true' for zoomKey functionality
      selectable: true,
      zoomKey: 'altKey',
      zoomMax: 2678400000, // 31-days in milliseconds
      zoomMin: 3600000, // hour in milliseconds 
      locale: CampusManagement.LanguageMappings.getLocale(userLcid)
    };

    $.ajax({
      type: "GET",
      contentType: "application/json; charset=utf-8",

      url: window.Xrm.Page.context.getClientUrl() +
        "/api/data/v" +
        CampusManagement.XrmSystemInformation.getApiVersion() +
        "/EntityDefinitions?$select=LogicalName&$filter=LogicalName eq 'msevtmgt_event' or LogicalName eq 'cxlvhlp_chatactivity' or LogicalName eq 'msdyncrm_marketingemail'",

      beforeSend: function(XMLHttpRequest) {
        XMLHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
        XMLHttpRequest.setRequestHeader("OData-Version", "4.0");
        XMLHttpRequest.setRequestHeader("Accept", "application/json");
      },
      async: false,
      success: function(result) {
        var entities = result.value;
        if (entities) {
          entities.forEach(function(entity) {
            extendedEntites.push(entity.LogicalName);
          });
          EnableExtendedEntityButton();
        }
      },
      error: function(error) {
        multiLingualAlert(localizedStrings.errorPrefix, error);
      }
    });

    retrieveTimelineActivities(studentId).then(
      function(results) {
        buildVisDataSet(results);
        lifeCycleDropDownList();
        timeline = new vis.Timeline(container, items, options);
        timeline.moveTo(Date()); // focus timeline to current date time.
        // will only trigger if at least one item exists on the timeline
        // this will only work if opened in the same window, otherwise vis.js will make two calls on 'select'
        timeline.on('select',
          function(properties) {
            var itemToOpen = itemDetails[properties.items[0]];
            itemToOpen.openInCrm();
          });
      },
      function(error) {
        multiLingualAlert(localizedStrings.errorPrefix, error);
      });
  }

  function initializeDisplaySettings() {
    $('#phonecall').val(CampusManagement.localization.getResourceString("PhonecallButton"));
    $('#email').val(CampusManagement.localization.getResourceString("EmailButton"));
    $('#to-do').val(CampusManagement.localization.getResourceString("ToDoButton"));
    $('#event').val(CampusManagement.localization.getResourceString("EventButton"));
    $('#appointment').val(CampusManagement.localization.getResourceString("AppointmentButton"));
    $('#commands').text(CampusManagement.localization.getResourceString("ZoomTimeLine"));
    $('#chat').val(CampusManagement.localization.getResourceString("ChatButton"));
    localizedStrings = {
      okButton: CampusManagement.localization.getResourceString("OkButton"),
      errorPrefix: CampusManagement.localization.getResourceString("ErrorPrefix"),
      timelineCheckAll: CampusManagement.localization.getResourceString("TimelineCheckAll"),
      selectLifecycles: CampusManagement.localization.getResourceString("SelectLifecycles"),
      lifecycleSelected: CampusManagement.localization.getResourceString("LifecycleSelected"),
      description: CampusManagement.localization.getResourceString("Description"),
      fromstring: CampusManagement.localization.getResourceString("FromString"),
      duration: CampusManagement.localization.getResourceString("Duration"),
      minutes: CampusManagement.localization.getResourceString("Minutes"),
      assignedStaff: CampusManagement.localization.getResourceString("AssignedStaff"),
      dueDate: CampusManagement.localization.getResourceString("DueDate"),
      completedDate: CampusManagement.localization.getResourceString("CompletedDate"),
      attendees: CampusManagement.localization.getResourceString("Attendees"),
      timeline_Chatwith: CampusManagement.localization.getResourceString("Timeline_Chatwith")
    }

  }
  
  function EnableExtendedEntityButton() {

    extendedEntites.indexOf("msevtmgt_event") > -1 ? $('#event').show() : "";
    extendedEntites.indexOf("cxlvhlp_chatactivity") > -1 ? $('#chat').show() : "";   
  }

  //Creating DropDwown Using Kendo
  var dropdown;
  function lifeCycleDropDownList() {
    var data = [{ defaultText: localizedStrings.lifecycleSelected, text:localizedStrings.timelineCheckAll, value: "", selected: false },];
    var value;  
    //Pushing the LifeCycleName and Correspoing colour into the data 
    for (var i = 0; i < listOfLifecyleName.length; i++) {      
      value = { text: listOfLifecyleName[i] + " " + "<i class='fa fa-refresh vis-item-" + i + "-icon' aria-hidden='true'></i>", value: listOfLyfecycleId[i], selected: false },
        data.push(value);
    }
    //Preparing the dropdown 
    dropdown = $("#dropDownList").kendoDropDownList({
      dataTextField: "defaultText",
      dataValueField: "value",
      template: $("#CheckboxTemplate").html(),
      dataSource: data,
      select: function (e) {
        e.preventDefault();
      }
    }).data("kendoDropDownList");
    dropdown.list.find(".check-input,.check-item").bind("click", function (e) {
      var $item = $(this);
      var $input;

      if ($item.hasClass("check-item")) {
        // Text was clicked
        $input = $item.children(".check-input");
        $input.prop("checked", !$input.is(':checked'));
      }
      else
        // Checkbox was clicked
        $input = $item;

      // Check all clicked?
      if ($input.val() == "")
        dropdown.list.find(".check-input").prop("checked", $input.is(':checked'));
      updateDropDown()
      e.stopImmediatePropagation();
    });
  }
  function updateDropDown() {
    var selectedLyfecycleId = [];
    dropdown.list.find(".check-input").each(function () {
      var $input = $(this);
      if ($input.val() != "" && $input.is(':checked')) {              
        selectedLyfecycleId.push($input.val());
      }
    });
    // Check the Check All if all the items are checked   
    $(dropdown.list.find(".check-input")[0]).prop("checked", selectedLyfecycleId.length == dropdown.list.find(".check-input").length - 1);
    dropdown.text(localizedStrings.lifecycleSelected);
    if (selectedLyfecycleId.length == 0)
      dropdown.text(localizedStrings.selectLifecycles);

    //Collect the UnSelectedLifyCycleName from the CheckBox    
    let unSelectedLyfecycleId = listOfLyfecycleId.filter(item => selectedLyfecycleId.indexOf(item) < 0);

    toggleLifeCycle(selectedLyfecycleId, unSelectedLyfecycleId);
  }

  function removeActivitiesWithLifecycleDuplicates(studentActivities, studentActivitiesForLifecycles) {
    var activitiesWithoutLifecycleDuplicates = studentActivities;

    if (studentActivities.length > 0 && studentActivitiesForLifecycles.length > 0) {
      var ids = getIdsFromActivityArray(studentActivitiesForLifecycles);

      if (ids.length > 0) {
        activitiesWithoutLifecycleDuplicates = studentActivities.filter(function (element) {
          return ids.indexOf(element.activityid) === -1;
        });
      }
    }

    return activitiesWithoutLifecycleDuplicates;
  }

  function getIdsFromActivityArray(activities) {
    var activityIds = [];
    for (var i = 0, len = activities.length; i < len; i++) {
      activityIds.push(activities[i].activityid);
    }

    return activityIds;
  }

  function removeDuplicateActivities(activities,uniqueField) {
    var uniqueActivitys = {};
    if (activities.length > 0) {
      $.each(activities, (index:number, element) => {
        if (!uniqueActivitys[element[uniqueField]]) {
          uniqueActivitys[element[uniqueField]] = element;
        }
      });
    }
    return uniqueActivitys;
  }

  function buildVisDataSet(studentActivityResults) {
    var studentPhonecalls = studentActivityResults[0].entities,
      studentAppointments = studentActivityResults[1].entities,
      studentToDos = studentActivityResults[2].entities,
      studentEmails = studentActivityResults[3].entities,
      studentEmailsForLifecycles = studentActivityResults[4].entities,
      studentAppointmentsForLifecycles = studentActivityResults[5].entities,
      studentPhonecallsForLifecycles = studentActivityResults[6].entities,
      studentEvents = (studentActivityResults[7].entities ? studentActivityResults[7].entities : [] ),
      marketingEmailsSent = studentActivityResults[8],
      studentChats = (studentActivityResults[9].entities ? studentActivityResults[9].entities : []),
      detail,
      i;
    if (studentEvents.length > 0) {
      var uniqueStudentEvents = removeDuplicateActivities(studentEvents, "msevtmgt_eventid"); //Gets the unique events from the array collection by removing duplicate
      var eventIndex: number = 0;
      for (var event in uniqueStudentEvents) {
        var itemId = 'event' + eventIndex++;
        var status, iconClass = "";
        if (uniqueStudentEvents[event]["eventReg.msevtmgt_timescheckedin"]) {
          if (parseInt(uniqueStudentEvents[event]["eventReg.msevtmgt_timescheckedin"]) > 0) {
            status = "completed";
            iconClass = "fa-check";
          }
        }
        try {
          items.add({
            id: itemId,
            content: prepareTimelineContentWithIcon(uniqueStudentEvents[event]["msevtmgt_name"], "Event", "Event", "", "", status, iconClass),
            start: uniqueStudentEvents[event]["msevtmgt_eventstartdate"],
            className: 'event',
            title: "",
            lifeCycleId: "",
            isVisibleInTimeline: true,
            isVisibleInLifeCycle: true,
            isActivityEnabled: true,
            isLifeCycleEnabled: true
          });

          detail = new ItemDetail(itemId, uniqueStudentEvents[event]["msevtmgt_eventid"].toUpperCase(), 'msevtmgt_event');
          itemDetails[detail.timelineId] = detail;

        } catch (error) {
          parent.Xrm.Navigation.openAlertDialog({
            text: error,
            confirmButtonLabel: localizedStrings.okButton
          }, null);
          break;
        }
      };
    }
    if (studentChats.length > 0) {
      for (i = 0; i < studentChats.length; i++) {
        var itemId = 'chat' + i;
        var status, iconClass = "";
        
        try {
          items.add({
            id: itemId,
            content: prepareTimelineContentWithIcon(studentChats[i]["user.fullname"], "chat", "chat", "", "", status, iconClass),
            start: studentChats[i].scheduledstart ? studentChats[i].scheduledstart : studentChats[i].createdon,
            className: 'chat',
            title: setChatActivityDetails(studentChats[i]),
            lifeCycleId: "",
            isVisibleInTimeline: true,
            isVisibleInLifeCycle: true,
            isActivityEnabled: true,
            isLifeCycleEnabled: true
          });

          detail = new ItemDetail(itemId, studentChats[i]["activityid"].toUpperCase(), 'cxlvhlp_chatactivity');
          itemDetails[detail.timelineId] = detail;

        } catch (error) {
          parent.Xrm.Navigation.openAlertDialog({
            text: error,
            confirmButtonLabel: localizedStrings.okButton
          }, null);
          break;
        }
      }
    }
    if (studentPhonecalls.length > 0) {
      studentPhonecalls = removeActivitiesWithLifecycleDuplicates(studentPhonecalls, studentPhonecallsForLifecycles);

      for (i = 0; i < studentPhonecalls.length; i++) {
        try {
          prepareStudentPhonecalls(studentPhonecalls[i], "", "");
        }
        catch (error) {
          // parent.Xrm is used as the Navigation dialog would not appear with any text if it wasn't used.
          parent.Xrm.Navigation.openAlertDialog({
            text: error,
            confirmButtonLabel: localizedStrings.okButton
          }, null);
          break;
        }
      }
    }
    if (studentPhonecallsForLifecycles.length > 0) {
      for (i = 0; i < studentPhonecallsForLifecycles.length; i++) {
        try {
          prepareStudentPhonecalls(studentPhonecallsForLifecycles[i], studentPhonecallsForLifecycles[i]['ae.name'].trim(), studentPhonecallsForLifecycles[i]['ae.opportunityid'].trim());
        }
        catch (error) {
          parent.Xrm.Navigation.openAlertDialog({
            text: error,
            confirmButtonLabel: localizedStrings.okButton
          }, null);
          break;
        }
      }
    }

    var studentAppointmentsLength = studentAppointments.length;
    if (studentAppointmentsLength > 0) {
      studentAppointments = consolidateAppointmentsOnRequiredAttendees(studentAppointments);
      var filteredStudentAppointments = removeActivitiesWithLifecycleDuplicates(studentAppointments, studentAppointmentsForLifecycles);

      for (i = 0; i < filteredStudentAppointments.length; i++) {
        try {
          prepareStudentAppointments(filteredStudentAppointments[i], "", "");

        } catch (error) {
          parent.Xrm.Navigation.openAlertDialog({
            text: error,
            confirmButtonLabel: localizedStrings.okButton
          }, null);
          break;
        }
      }
    }
    if (studentAppointmentsForLifecycles.length > 0) {
      studentAppointmentsForLifecycles = consolidateAppointmentsOnRequiredAttendees(studentAppointmentsForLifecycles);

      for (i = 0; i < studentAppointmentsForLifecycles.length; i++) {
        try {
          prepareStudentAppointments(studentAppointmentsForLifecycles[i], studentAppointmentsForLifecycles[i]['ab.name'].trim(), studentAppointmentsForLifecycles[i]['ab.opportunityid'].trim());

        } catch (error) {
          parent.Xrm.Navigation.openAlertDialog({
            text: error,
            confirmButtonLabel: localizedStrings.okButton
          }, null);
          break;
        }
      }
    }

    if (studentToDos.length > 0) {
      for (i = 0; i < studentToDos.length; i++) {
        var itemId = 'to-do' + i;
        try {
          items.add({
            id: itemId,
            content: prepareTimelineContentWithIcon(studentToDos[i].cmc_todoname, ((studentToDos[i].statuscode === todoStatus.Complete) ? Constants.StudentTimelineActivity.todoCompleteIcon : Constants.StudentTimelineActivity.todoInCompleteIcon), ResourseStrings.StudentTimelineActivity.ToDo, "", ""),
            start: convertUtcDateToLocalDate((studentToDos[i].statuscode === todoStatus.Complete) ? (studentToDos[i].cmc_completedcanceleddate) : (studentToDos[i]['cmc_duedate@OData.Community.Display.V1.FormattedValue']), "YYYY-MM-DDTHH:mm"), // this format added to show up extact timing on the timeline
            className: 'to-do',
            title: setToDoDetails(studentToDos[i]),
            lifeCycleId: "",
            isVisibleInTimeline: true,
            isVisibleInLifeCycle: true,
            isActivityEnabled: true,
            isLifeCycleEnabled: true
          });

          detail = new ItemDetail(itemId, studentToDos[i].cmc_todoid.toUpperCase(), 'cmc_todo');
          itemDetails[detail.timelineId] = detail;

        } catch (error) {
          parent.Xrm.Navigation.openAlertDialog({
            text: error,
            confirmButtonLabel: localizedStrings.okButton
          }, null);
          break;
        }
      }
    }

    if (studentEmails.length > 0) {
      studentEmails = removeActivitiesWithLifecycleDuplicates(studentEmails, studentEmailsForLifecycles);

      for (i = 0; i < studentEmails.length; i++) {
        try {
          prepareEmails(studentEmails[i], "", "");
        }
        catch (error) {
          parent.Xrm.Navigation.openAlertDialog({
            text: error,
            confirmButtonLabel: localizedStrings.okButton
          }, null);
          break;
        }
      }
    }
    if (studentEmailsForLifecycles.length > 0) {
      for (i = 0; i < studentEmailsForLifecycles.length; i++) {
        try {
          prepareEmails(studentEmailsForLifecycles[i], studentEmailsForLifecycles[i]['ac.name'].trim(), studentEmailsForLifecycles[i]['ac.opportunityid'].trim());

        } catch (error) {
          parent.Xrm.Navigation.openAlertDialog({
            text: error,
            confirmButtonLabel: localizedStrings.okButton
          }, null);
          break;
        }
      }
    }
    if (marketingEmailsSent && marketingEmailsSent.length > 0) {
      for (i = 0; i < marketingEmailsSent.length; i++) {
        prepareMarketingEmails(marketingEmailsSent[i]);    
      }
    }
  }

  function consolidateAppointmentsOnRequiredAttendees(appointments) {
    var consolidatedAppointments = [],
      activityids = [];
    appointments.forEach(function (appointment) {
      var i = activityids.indexOf(appointment.activityid);
      //This conditon will satisfy while more then one requiredattendees(with one activityid) into the appointment
      //Merging all string with one requiredattendee.  
      if (i >= 0) {
        var currentAttendees = (consolidatedAppointments[i]['requiredattendee.fullname'] == undefined) ? ((consolidatedAppointments[i]['accountRequiredattendee.name'] == undefined) ? consolidatedAppointments[i]['systemuserRequiredattendee.fullname'] : consolidatedAppointments[i]['accountRequiredattendee.name']) : consolidatedAppointments[i]['requiredattendee.fullname'];
        if (currentAttendees != undefined) {
          var nextAttendee = (appointment['requiredattendee.fullname'] == undefined) ? ((appointment['accountRequiredattendee.name'] == undefined) ? appointment['systemuserRequiredattendee.fullname'] : appointment['accountRequiredattendee.name']) : appointment['requiredattendee.fullname'];
          consolidatedAppointments[i]['requiredattendee.fullname'] = currentAttendees.concat(', ' + nextAttendee);
        }
      }
      //This conditon will satisfy first time requiredattendees into the appointment(default) and if it is only one requiredattendee then after exicute it will be out of the loop.
      else {
        //If required field's  only account or User name then it will merge to requiredattendee. 
        var firstCurrentAttendees =  ((appointment['accountRequiredattendee.name'] == undefined) ? appointment['systemuserRequiredattendee.fullname'] : appointment['accountRequiredattendee.name']);
        if (firstCurrentAttendees){
          appointment['requiredattendee.fullname'] = firstCurrentAttendees;
        }
        consolidatedAppointments.push(appointment);
        activityids.push(appointment.activityid);
      }
    });
    //removing Requiredattendee if more then one with same activity id.   
    consolidatedAppointments.forEach(function (requiredattendee) {
      if (requiredattendee['requiredattendee.fullname'] != undefined)
        var requiredattendees = removeDuplicate(requiredattendee['requiredattendee.fullname']);
      requiredattendee['requiredattendee.fullname'] = requiredattendees;
    });

    return consolidatedAppointments;
  }

  function removeDuplicate(requiredattendees) {
    var requiredattendeeArray = requiredattendees.split(',').map(string => string.trim());
    var removeDuplicateRequiredattendee = [];
    for (var requiredattendee in requiredattendeeArray)
      if (removeDuplicateRequiredattendee.indexOf(requiredattendeeArray[requiredattendee]) == -1) removeDuplicateRequiredattendee.push(requiredattendeeArray[requiredattendee])
    return removeDuplicateRequiredattendee.join(',');
  }

  var countIdforPhonecall = 0;
  function prepareStudentPhonecalls(studentPhoneCalls, lifeCycleName, lifeCycleId) {
    var itemId = 'phonecall' + countIdforPhonecall;
    items.add({
      id: itemId,
      content: prepareTimelineContentWithIcon(studentPhoneCalls.subject, (studentPhoneCalls.directioncode ? Constants.StudentTimelineActivity.outgoingTelephoneIcon : Constants.StudentTimelineActivity.incomingTelephoneIcon), ResourseStrings.StudentTimelineActivity.Phonecall, lifeCycleName, lifeCycleId),
      start: convertUtcDateToLocalDate(studentPhoneCalls.modifiedon, "YYYY-MM-DDTHH:mm"),
      className: 'phonecall',
      title: setPhonecallDetails(studentPhoneCalls),
      lifeCycleId: lifeCycleId,
      classNameBackUp: 'phonecall',
      isVisibleInTimeline: true,
      isVisibleInLifeCycle: true,
      isActivityEnabled: true,
      isLifeCycleEnabled: true
    });

    var detail = new ItemDetail(itemId, studentPhoneCalls.activityid.toUpperCase(), 'phonecall');
    itemDetails[detail.timelineId] = detail;
    countIdforPhonecall++;
  }

  var countIdforAppointment = 0;
  function prepareStudentAppointments(studentAppointments, lifeCycleName, lifeCycleId) {
    var itemId = 'appointment' + countIdforAppointment;
    items.add({
      id: itemId,
      content: prepareTimelineContentWithIcon(studentAppointments.subject, Constants.StudentTimelineActivity.appointmentIcon, ResourseStrings.StudentTimelineActivity.Appointment, lifeCycleName, lifeCycleId),
      start: convertUtcDateToLocalDate(studentAppointments['scheduledstart'], "YYYY-MM-DDTHH:mm"),
      className: 'appointment',
      title: setAppointmentDetails(studentAppointments),
      lifeCycleId: lifeCycleId,
      classNameBackUp: 'appointment',
      isVisibleInTimeline: true,
      isVisibleInLifeCycle: true,
      isActivityEnabled: true,
      isLifeCycleEnabled: true
    });

    var detail = new ItemDetail(itemId, studentAppointments.activityid.toUpperCase(), 'appointment');
    itemDetails[detail.timelineId] = detail;
    countIdforAppointment++;
  }

  var countIdforEmail = 0;
  function prepareEmails(studentEmails, lifeCycleName, lifeCycleId) {
    var itemId = 'email' + countIdforEmail;
    items.add({
      id: itemId,
      content: prepareTimelineContentWithIcon(studentEmails.subject, (studentEmails.directioncode == directioncode.outgoing ? Constants.StudentTimelineActivity.mailReplyIcon : Constants.StudentTimelineActivity.mailSendIcon), ResourseStrings.StudentTimelineActivity.Email, lifeCycleName, lifeCycleId),
      start: convertUtcDateToLocalDate(studentEmails.modifiedon, "YYYY-MM-DDTHH:mm"),
      className: 'email',
      title: setEmailDetails(studentEmails),
      lifeCycleId: lifeCycleId,
      classNameBackUp: 'email',
      isVisibleInTimeline: true,
      isVisibleInLifeCycle: true,
      isActivityEnabled: true,
      isLifeCycleEnabled: true
    });

    var detail = new ItemDetail(itemId, studentEmails.activityid.toUpperCase(), 'email');
    itemDetails[detail.timelineId] = detail;
    countIdforEmail++;
  }

  function prepareMarketingEmails(marketingEmail) {
    // Id is the same as email right now, but Marketing Email will likely need to have its own id.
    var itemId = 'email' + countIdforEmail;
    items.add({
      id: itemId,
      // Marketing Email will likely have it's own icon rather than sharing an Email's icon.
      content: prepareTimelineContentWithIcon(marketingEmail.subject, Constants.StudentTimelineActivity.marketingEmail, ResourseStrings.StudentTimelineActivity.MarketingEmail, '', ''),
      start: convertUtcDateToLocalDate(marketingEmail.Timestamp, "YYYY-MM-DDTHH:mm"),
      // Class defines the background color. Marketing Email would likely have it's own class.
      className: 'email',
      title: setMarketingEmailDetails(marketingEmail),
      lifeCycleId: "",
      classNameBackUp: 'email',
      isVisibleInTimeline: true,
      isVisibleInLifeCycle: true,
      isActivityEnabled: true,
      isLifeCycleEnabled: true
    });

    // Redirect to the Marketing Email on click
    // Note that this will not work outside of the Unified Interface.
    // Ideally we'd only pass in the id and entity name if we were in the Unified Interface.
    // There currently isn't a great way to determine if you're in the Unified Interface though.
    // Two potential ways are:
    // 1. Xrm.Internal.isUci()
    //    - Uses an undocumented method on the Internal namespace, which makes it unsupported.
    // 2. Xrm.Utility.getGlobalContext().getCurrentAppUrl().indexOf("appid=") != -1;
    //   - This just checks if appId is part of the URL, which all Unified Interface apps currently have.
    //   - It would return a false positive if an additional Web Interface app is created.
    var detail = new ItemDetail(itemId, marketingEmail.MessageId.toUpperCase(), 'msdyncrm_marketingemail');
    itemDetails[detail.timelineId] = detail;
    countIdforEmail++;
  }


  function getDateFormattedValue(appointmentFieldOData) {
    // TODO: Handle Foreign Date Formats/Abbreviations
    var formattedOData = appointmentFieldOData.replace(/\s+/, '\x01').split('\x01');
    formattedOData[1] = (formattedOData[1].replace(/\s/g, '')).toLowerCase();

    return formattedOData;
  }

  // OData returns the UTC date, so converting date to local format date to match with local time
  function convertUtcDateToLocalDate(date, format) {
    var localFormatDate = new vis.moment(date);
    return localFormatDate.format(format); // standard format conversion YYYY-MM-DD.
  }

  // prefix the icon before the content.
  function prepareTimelineContentWithIcon(content, iconPath, tooltip, lifecycleName, lifecycleId, status = null, iconClass = null) {
    // maintain the life cycle names to array, and fetch the index
    if (listOfLyfecycleId.indexOf(lifecycleId) < 0 && lifecycleId != "") {
      listOfLyfecycleId.push(lifecycleId);
      listOfLifecyleName.push(lifecycleName);
    }
    if (iconPath === "") // cases when no icon path given : ex: incomleted to do activities.
      return content;
    else if (iconPath == Constants.StudentTimelineActivity.marketingEmail)
      return "<span style='position: relative;'><img class='vis-item-" + iconPath.replace(" ", "") + "-icon' src='../Images/ContactActivitiesTimeline/" + iconPath + "-32x32.svg' title = '" + tooltip + "'><i class='fa fa-medium vis-item-MarketingEmail-fontawesome-icon'> </i> </img></span>" + " " + content;
    else if (lifecycleId === "")
      return "<img class='vis-item-" + tooltip.replace(" ", "") + "-icon' src='../Images/ContactActivitiesTimeline/" + iconPath + "-32x32.svg' title='" + tooltip + "'></img><span title = '" + content + "'> " + content + "</span>" + (status ? "<i class='fa " + iconClass + " vis-item-status " + status + "-icon' aria-hidden='true'></i>" : "");
    else
      return "<img class='vis-item-" + tooltip.replace(" ", "") + "-icon' src='../Images/ContactActivitiesTimeline/" + iconPath + "-32x32.svg' title='" + tooltip + "'></img>" + " " + content + " " + "<i class='fa fa-refresh vis-item-" + listOfLyfecycleId.indexOf(lifecycleId) + "-icon' aria-hidden='true'></i>";

  }
  
  function setAppointmentDetails(appointment) {
    // Assumes start and end occur on the same date
    var scheduledstart = getDateFormattedValue(appointment['scheduledstart@OData.Community.Display.V1.FormattedValue']),
      scheduledend = getDateFormattedValue(appointment['scheduledend@OData.Community.Display.V1.FormattedValue']);

    var title = [
      '<div class="item-title">',
      appointment.subject,
      '<div class="item-details">',
      '<br>', scheduledstart[0], ' ', scheduledstart[1], ' - ', scheduledend[1],
      '<br>' + localizedStrings.attendees+': ', appointment['requiredattendee.fullname'] || 'N/A',
      '</div>',
      '</div>'
    ].join('');

    return title;
  }

  function setToDoDetails(todo) {
    var peekDescription = cleanupActivityDescription(todo.cmc_description),
      ownerid = todo['_ownerid_value@OData.Community.Display.V1.FormattedValue'];

    var title = [
      '<div class="item-title">',
      todo.cmc_todoname,
      '<div class="item-details">',
      localizedStrings.description + ': ', peekDescription, '',
      '<br>' + localizedStrings.assignedStaff+': ', ownerid,
      '<br>' + localizedStrings.dueDate+': ', convertUtcDateToLocalDate(todo.cmc_duedate, "MM-DD-YYYY"),
      ((todo.statuscode === todoStatus.Complete) ? ['<br>' + localizedStrings.completedDate+':' + convertUtcDateToLocalDate(todo.cmc_completedcanceleddate, "MM-DD-YYYY HH:mm")] : []),
      '</div>',
      '</div>'
    ].join('');

    return title;
  }

  function setPhonecallDetails(phonecall) {
    var peekDescription = cleanupActivityDescription(phonecall.description),
      from = getFromNameInActivityParties(phonecall['phonecall_activity_parties']);

    var title = [
      '<div class="item-title">',
      phonecall.subject,
      '<div class="item-details">',
      localizedStrings.description +': ', peekDescription, '',
      '<br>' + localizedStrings.fromstring+': ', from,
      '<br>' + localizedStrings.duration + ': ', phonecall.actualdurationminutes, ' ' + localizedStrings.minutes,
      '</div>',
      '</div>'
    ].join('');

    return title;
  }

  function setEmailDetails(email) {
    var peekDescription = cleanupActivityDescription(email.description),
      from = getFromNameInActivityParties(email['email_activity_parties']);

    var title = [
      '<div class="item-title">',
      email.subject,
      '<div class="item-details">',
      localizedStrings.description + ': ', peekDescription, '',
      '<br>' + localizedStrings.fromstring + ': ', from,
      '</div>',
      '</div>'
    ].join('');

    return title;
  }

  function setMarketingEmailDetails(email) {
    // Details are not confirmed. Details are just similar to Email as an example
    return [
      '<div class="item-title">',
      email.subject,
      '<div class="item-details">',
      localizedStrings.description + ': ', email.description, '',
      '<br>' + localizedStrings.fromstring + ': ', email.from,
      '</div>',
      '</div>'
    ].join('');
  }

  function setChatActivityDetails(chat) {
    // Details are not confirmed. Details are just similar to Email as an example
    return [
      '<div class="item-title">',
      String.format(localizedStrings.timeline_Chatwith,chat["user.fullname"]), '',
      '<div class="item-details">',
      localizedStrings.duration + ': ', (chat.cxlvhlp_duration ? chat.cxlvhlp_duration : 0), '',
      '</div>',
      '</div>'
    ].join('');
  }

  function getFromNameInActivityParties(activityParties) {
    var from = '',
      j;

    for (j = 0; j < activityParties.length; j++) {
      if (activityParties[j]['participationtypemask'] === activityParty.From) {
        from = activityParties[j]['_partyid_value@OData.Community.Display.V1.FormattedValue'] || '';
        break;
      }
    }
    return from;
  }

  function cleanupActivityDescription(description) {
    // Regex was added to remove anything between the head tags. Otherwise extra text would remain.
    var regexHTMLTags = /(<head>.+?(?=<\/head>)<\/head>)|(<([^>]+)>)/ig,
      peekLength = 50;

    if (!description) {
      return 'N/A';
    }
    else {
      var peekDescription = description.replace(regexHTMLTags, ''),
        overflow = peekDescription.length - peekLength;

      if (overflow > 0) {
        peekDescription = peekDescription.slice(0, overflow * -1) + '...';
      }
      return peekDescription;
    }
  }

  function retrieveTimelineActivities(studentId) {

    var fetchEmails = [
      '<fetch distinct="true">',
      '<entity name="email">',
      '<attribute name="subject" />',
      '<attribute name="from" />',
      '<attribute name="modifiedon" />',
      '<attribute name="activityid" />',
      '<attribute name="description" />',   // Body
      '<attribute name="directioncode" />', // to show up the icon
      '<link-entity name="activityparty" from="activityid" to="activityid" alias="activityparty">',
      '<filter type="and">',
      '<condition attribute="partyid" operator="eq" value="', studentId, '" />',
      '</filter>',
      '</link-entity>',
      '<filter type="and">',
      '<condition attribute="statuscode" operator="ne" value="', emailStatus.Cancelled, '" />',
      '</filter>',
      '</entity>',
      '</fetch>'
    ].join('');

    var fetchEmailsForLifecycles = [
      '<fetch distinct="true">',
      '<entity name="email">',
      '<attribute name="subject" />',
      '<attribute name="from" />',
      '<attribute name="modifiedon" />',
      '<attribute name="activityid" />',
      '<attribute name="description" />',   // Body
      '<attribute name="directioncode" />', // to show up the icon
      '<link-entity name="activityparty" from="activityid" to="activityid" alias="activityparty">',
      '<link-entity name="opportunity" alias="ac" link-type="inner" to="partyid" from="opportunityid">',
      '<attribute name="name" />',
      '<attribute name="opportunityid" />',
      '<filter type="and">',
      '<condition attribute="cmc_contactid" operator="eq" value="', studentId, '" />',
      '</filter>',
      '</link-entity>',
      '</link-entity>',
      '<filter>',
      '<condition attribute="statuscode" operator="ne" value="', emailStatus.Cancelled, '" />',
      '</filter>',
      '</entity>',
      '</fetch>'
    ].join('');

    var fetchPhonecalls = [
      '<fetch distinct="true">',
      '<entity name="phonecall">',
      '<attribute name="activityid" />',
      '<attribute name="actualdurationminutes" />',
      '<attribute name="description" />',
      '<attribute name="from" />',
      '<attribute name="modifiedon" />',
      '<attribute name="subject" />',
      '<attribute name="directioncode" />', // to show up the icon
      '<link-entity name="activityparty" from="activityid" to="activityid" alias="activityparty">',
      '<filter type="and">',
      '<condition attribute="partyid" operator="eq" value="', studentId, '" />',
      '</filter>',
      '</link-entity>',
      '<filter type="and">',
      '<condition attribute="statuscode" operator="ne" value="', phoneStatus.Cancelled, '" />',
      '</filter>',
      '</entity>',
      '</fetch>'
    ].join('');

    var fetchPhonecallsForLifecycles = [
      '<fetch distinct="true">',
      '<entity name="phonecall">',
      '<attribute name="activityid" />',
      '<attribute name="actualdurationminutes" />',
      '<attribute name="description" />',
      '<attribute name="from" />',
      '<attribute name="modifiedon" />',
      '<attribute name="subject" />',
      '<attribute name="directioncode" />',  // to show up the icon
      '<link-entity name="activityparty" from="activityid" to="activityid" alias="activityparty">',
      '<link-entity name="opportunity" alias="ae" link-type="inner" to="partyid" from="opportunityid">',
      '<attribute name="name" />',
      '<attribute name="opportunityid" />',
      '<filter type="and">',
      '<condition attribute="cmc_contactid" operator="eq" value="', studentId, '" />',
      '</filter>',
      '</link-entity>',
      '</link-entity>',
      '<filter>',
      '<condition attribute="statuscode" operator="ne" value="', phoneStatus.Cancelled, '" />',
      '</filter>',
      '</entity>',
      '</fetch>'
    ].join('');

    var fetchAppointments = [
      '<fetch>',
      '<entity name="appointment">',
      '<attribute name="activityid" />',
      '<attribute name="instancetypecode" />',
      '<attribute name="location" />',
      '<attribute name="scheduledstart" />',
      '<attribute name="scheduledend" />',
      '<attribute name="subject" />',
      '<link-entity name="activityparty" from="activityid" to="activityid" alias="activityparty1">',
      '<filter type="and">',
      '<condition attribute="partyid" operator="eq" value="', studentId, '" />',
      '</filter>',
      '</link-entity>',
      '<link-entity name="activityparty" from="activityid" to="activityid" alias="activityparty" link-type="outer">',
      '<attribute name="partyid"/>',
      '<filter>',
      '<condition attribute="participationtypemask" operator="eq" value="5" />', // Required Attendee
      '</filter>',
      '<link-entity name="contact" from="contactid" to="partyid" alias="requiredattendee" link-type="outer">',
      '<attribute name="fullname"/>',
      '</link-entity>',
      '<link-entity name="account" from="accountid" to="partyid" alias="accountRequiredattendee" link-type="outer">',
      '<attribute name="name"/>',
      '</link-entity>',
      '<link-entity name="systemuser" from="systemuserid" to="partyid" alias="systemuserRequiredattendee" link-type="outer">',
      '<attribute name="fullname"/>',
      '</link-entity>',
      '</link-entity>',
      '<filter>',
      '<condition attribute="statuscode" operator="ne" value="', appointmentStatus.Cancelled, '" />',
      '</filter>',
      '</entity>',
      '</fetch>'
    ].join('');

    var fetchAppointmentsForLifecycles = [
      '<fetch>',
      '<entity name="appointment">',
      '<attribute name="activityid" />',
      '<attribute name="instancetypecode" />',
      '<attribute name="location" />',
      '<attribute name="scheduledstart" />',
      '<attribute name="scheduledend" />',
      '<attribute name="subject" />',
      '<link-entity name="activityparty" from="activityid" to="activityid" alias="activityparty1">',
      '<link-entity name="opportunity" alias="ab" link-type="inner" to="partyid" from="opportunityid">',
      '<attribute name="name" />',
      '<attribute name="opportunityid" />',
      '<filter type="and">',
      '<condition attribute="cmc_contactid" operator="eq" value="', studentId, '" />',
      '</filter>',
      '</link-entity>',
      '</link-entity>',
      '<link-entity name="activityparty" from="activityid" to="activityid" alias="activityparty" link-type="outer">',
      '<filter>',
      '<condition attribute="participationtypemask" operator="eq" value="5" />', // Required Attendee
      '</filter>',
      '<link-entity name="contact" from="contactid" to="partyid" alias="requiredattendee" link-type="outer">',
      '<attribute name="fullname"/>',
      '</link-entity>',
      '<link-entity name="account" from="accountid" to="partyid" alias="accountRequiredattendee" link-type="outer">',
      '<attribute name="name"/>',
      '</link-entity>',
      '<link-entity name="systemuser" from="systemuserid" to="partyid" alias="systemuserRequiredattendee" link-type="outer">',
      '<attribute name="fullname"/>',
      '</link-entity>',
      '</link-entity>',
      '<filter>',
      '<condition attribute="statuscode" operator="ne" value="', appointmentStatus.Cancelled, '" />',
      '</filter>',
      '</entity>',
      '</fetch>'
    ].join('');

    var fetchToDos = [
      '<fetch distinct="true">',
      '<entity name="cmc_todo">',
      '<attribute name="cmc_description" />',
      '<attribute name="cmc_duedate" />',
      '<attribute name="cmc_todoname" />',
      '<attribute name="cmc_todoid" />',
      '<attribute name="ownerid" />', // Assigned To Staff
      '<attribute name="statuscode" />', // Status Code
      '<attribute name="cmc_completedcanceleddate" />', // Completed Date
      '<filter type="and">',
      '<condition attribute="cmc_assignedtostudentid" operator="eq" value="', studentId, '" />',
      '<condition attribute="statuscode" operator="in">', // enhancment to show up complete and incomplete to do activities assigned to student.
      '<value>', todoStatus.Incomplete, '</value>',
      '<value>', todoStatus.Complete, '</value>',
      '</condition>',
      '</filter>',
      '</entity>',
      '</fetch>'
    ].join('');


    var fetchEvents = [
      '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="true">',
      '<entity name="msevtmgt_event">',
      '<attribute name="msevtmgt_eventid" />',
      '<attribute name="msevtmgt_name" />',
      '<attribute name="createdon" />',
      '<attribute name="msevtmgt_eventstartdate" />',
      '<link-entity name="msevtmgt_eventregistration" from="msevtmgt_eventid" to="msevtmgt_eventid" link-type="inner" alias="eventReg">',
      '<attribute name="msevtmgt_name" />',
      '<attribute name="createdon" />',
      '<attribute name="msevtmgt_eventregistrationid" />',
      '<attribute name="msevtmgt_eventid" />',
      '<attribute name="msevtmgt_timescheckedin" />',
      '<filter type="and">',
      '<condition attribute="msevtmgt_contactid" operator="eq"  value="', studentId, '" />',
      '</filter>',
      '</link-entity>',
      '</entity>',
      '</fetch>'
    ].join('');

    var fetchChats = [
      '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">',
      '<entity name="cxlvhlp_chatactivity">',
      '<attribute name="activityid" />',
      '<attribute name="subject" />',
      '<attribute name="createdon" />',
      '<attribute name="scheduledstart" />',
      '<attribute name="scheduledend" />',
      '<attribute name="cxlvhlp_chatstarttime" />',
      '<attribute name="cxlvhlp_chatendtime" />',
      '<attribute name="cxlvhlp_duration" />',
      '<filter type="and">',
      '<condition attribute="cxlvhlp_customer" operator="eq"  value="', studentId, '" />',
      '</filter>',
      '<link-entity name="contact" from="contactid" to="cxlvhlp_customer" visible="false" link-type="inner" alias="contact">',
      '<attribute name="fullname" />',
      '</link-entity>',
      '<link-entity name = "systemuser" from = "systemuserid" to = "createdby" visible = "false" link-type="inner" alias = "user" >',
      '<attribute name="fullname" />',
      '</link-entity>',
      '</entity>',
      '</fetch>'
    ].join('');

    return SonomaCmc.Promise.all([
      Xrm.WebApi.retrieveMultipleRecords('phonecall', '?fetchXml=' + fetchPhonecalls),
      Xrm.WebApi.retrieveMultipleRecords('appointment', '?fetchXml=' + fetchAppointments),
      Xrm.WebApi.retrieveMultipleRecords('cmc_todo', '?fetchXml=' + fetchToDos),
      Xrm.WebApi.retrieveMultipleRecords('email', '?fetchXml=' + fetchEmails),
      Xrm.WebApi.retrieveMultipleRecords('email', '?fetchXml=' + fetchEmailsForLifecycles),
      Xrm.WebApi.retrieveMultipleRecords('appointment', '?fetchXml=' + fetchAppointmentsForLifecycles),
      Xrm.WebApi.retrieveMultipleRecords('phonecall', '?fetchXml=' + fetchPhonecallsForLifecycles),
      extendedEntites.indexOf("msevtmgt_event") > -1 ? Xrm.WebApi.retrieveMultipleRecords('msevtmgt_event', '?fetchXml=' + fetchEvents): [],
      extendedEntites.indexOf("msdyncrm_marketingemail") > -1 ? retrieveMarketingEmails(studentId): [],
      extendedEntites.indexOf("cxlvhlp_chatactivity") > -1 ? Xrm.WebApi.retrieveMultipleRecords('cxlvhlp_chatactivity', '?fetchXml=' + fetchChats) : [],
    ]).then(function (results) {
      return results;
    },
      function (error) {
        multiLingualAlert(localizedStrings.errorPrefix, error.message);
      });
  }

  function retrieveMarketingEmails(studentId) {
    //should not show allInteractions email to timeline before creating contact.
    if (studentId == "") {
      return [];
    }
    return verifyMarketingIsInstalled().then(function (isInstalled) {
      if (!isInstalled) {
        // If nothing is installed return act like no Marketing Emails exist.
        return [];
      }
    // the Xrm.WebAPI method for web actions behaves oddly in the Unified Interface
    return SonomaCmc.WebAPI.post('msdyncrm_LoadInteractionsPublic', {
      InteractionType: 'EmailSent',
      // Including curly braces on this request will cause an error
      ContactId: SonomaCmc.Guid.decapsulate(studentId),
    });
    }).then(function (response) {
      if (response.length == 0)
        return [];
      // The Web Interface returns responseText and the Unified Interface returns body. 
      // The definition of the Xrm.ExecuteResponse must be updated to include responseText
      // for this to work. Both come back as JSON, however. The double parse is due to the
      // Data property also being JSON
      var allInteractions = JSON.parse(response.Data),
        sendsById = {},
        templateId,
        fetchXml,
        i, totalLength;

      for (i = 0, totalLength = allInteractions.length; i < totalLength; i++) {
        templateId = allInteractions[i].MessageId;
        if (!templateId) {
          continue;
        }

        // Decapsulate and lowercase the Guid to avoid comparison issues
        templateId = SonomaCmc.Guid.decapsulate(allInteractions[i].MessageId).toLowerCase();
        if (!sendsById[templateId]) {
          sendsById[templateId] = [];
        }

        sendsById[templateId].push(allInteractions[i]);
      }

      fetchXml =
        `<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">\
                    <entity name="msdyncrm_marketingemail">\
                    <attribute name="msdyncrm_marketingemailid" />\
                    <attribute name="msdyncrm_subject" />\
                    <attribute name="msdyncrm_emailbody" />\
                    <attribute name="msdyncrm_description" />\                   
                    <attribute name="msdyncrm_fromuser" />\
                    <filter>\
                        <condition attribute="msdyncrm_marketingemailid" operator="in"> \
                        <value>${Object.keys(sendsById).join('</value><value>')}</value> \
                        </condition> \
                    </filter>\
                    </entity>\
                </fetch>`;

      return Xrm.WebApi.retrieveMultipleRecords('msdyncrm_marketingemail', '?fetchXml=' + fetchXml).
        then(function (response) {
          var marketingEmail,
            templateSends,
            finalSends = [],
            peekDescription,
            j, marketingEmailsLength;

          for (i = 0, totalLength = response.entities.length; i < totalLength; i++) {
            marketingEmail = response.entities[i];
            templateId = SonomaCmc.Guid.decapsulate(marketingEmail.msdyncrm_marketingemailid).toLowerCase();

            templateSends = sendsById[templateId];
            // A bit of preparation work is done here to avoid processing the email
            // body several times. This will remove HTML from the body and limit it to 50 characters.
            peekDescription = cleanupActivityDescription(marketingEmail.msdyncrm_description);
            for (j = 0, marketingEmailsLength = templateSends.length; j < marketingEmailsLength; j++) {
              templateSends[j].description = peekDescription;
              templateSends[j].subject = marketingEmail.msdyncrm_subject;
              templateSends[j].from = marketingEmail['_msdyncrm_fromuser_value@OData.Community.Display.V1.FormattedValue'];
            }

            /// A new list of Sends is created so that any Send for a deleted Marketing List is removed.
            finalSends = finalSends.concat(templateSends);
          }

          return finalSends;
        })
    });
  }

  function verifyMarketingIsInstalled() {
    // Before working with the Marketing Solution, it is recommended to check that everything
    // needed for this code exists. If it doesn't, we won't go any further.
    // There are a few alternatives to this code:
    // 1. Verifying a single component exists from the Marketing Solution. 
    //    It's possible this may not be a good indicator though and a component used in this
    //    code still won't exist.
    // 2. Verifying one Marketing solution is installed. 
    //    Just because one solution installed sucesfully doesn't mean all solutions installed
    //    sucesfully though and a component used in this code still won't exist.
    // 3. Write a Custom Action to determine if all components exist in place of API calls.
    //    This just introduces more code to manage.
    // 4. Remove the verification steps and just ignore any errors caused by retiriving Marketing components.
    //    This introduces the risk that legitimate errors could be missed or the error message would need to be parsed.

    var retrieveMarketingWorkflow =
      '<fetch top="1">\
              <entity name="workflow">\
                <attribute name="workflowid" />\
                <order attribute="name" descending="false" />\
                <filter type="and">\
                  <condition attribute="uniquename" operator="eq" value="LoadInteractionsPublic" />\
                </filter>\
              </entity>\
            </fetch>';

    return SonomaCmc.Promise.all([
      Xrm.WebApi.retrieveMultipleRecords('workflow', '?fetchXml=' + retrieveMarketingWorkflow)
    ]).then(function (results) {
      return results[0].entities.length > 0;
    },
      function (error) {
        return false;
      });
  }

  function multiLingualAlert(defaultMessage, error) {
    parent.Xrm.Navigation.openAlertDialog({
      text: defaultMessage + ': ' + error,
      confirmButtonLabel: localizedStrings.okButton
    }, null);
  }
  var itemClass = null; var itemIdPrefix = null;
  export function toggleHideItems(event) {
    itemIdPrefix = $(event.target).attr('id');
    itemClass = $(event.target).attr('class') || 'hidden';
    var togle = $(this).toggleClass(); // Update Related Activity Button

    if (items !== null && itemIdPrefix !== null) {
      if (itemClass === 'hidden' && items.length > 0) {
        items.forEach(function (item) {
          if (item.id.startsWith(itemIdPrefix)) {
            if ((item.name != "" && (item.isVisibleInLifeCycle || item.isLifeCycleEnabled)) || item.name == "")
              items.update({ id: item.id, className: itemIdPrefix, isActivityEnabled: true, isVisibleInTimeline: true, isVisibleInLifeCycle: true });
            else
              items.update({ id: item.id, className: 'hidden', isActivityEnabled: true, isVisibleInTimeline: false, isVisibleInLifeCycle: false });
          }
        });
      }

      if (itemClass !== 'hidden' && items.length > 0) {
        items.forEach(function (item) {
          if (item.id.startsWith(itemIdPrefix)) {
            items.update({ id: item.id, className: 'hidden', isActivityEnabled: false, isVisibleInTimeline: false, isVisibleInLifeCycle: item.isVisibleInLifeCycle });
          }
        });
      }
      timeline.setItems(items);
    }
  }


  function toggleLifeCycle(selectedLyfecycleId, unSelectedLyfeCycleId) {
    //get the items which are not hidden
    var visibleItems = $.map(items._data, function (item, index) {
      if (item.isVisibleInTimeline == true || item.isActivityEnabled == true) { return item; }
    });
    //condtion for UnSelected LifeCycleName into the CheckBox
    //Activities corresponding UnSelected LifeCycle should not be appear 
    for (var i = 0; i < unSelectedLyfeCycleId.length; i++) {
      var trimLfCycleId = unSelectedLyfeCycleId[i].trim();
      var itemsToHide = $.map(items._data, (item: any, index: number) => { if (item.lifeCycleId == trimLfCycleId) return item });
      itemsToHide.forEach(function (item) {
        items.update({ id: item.id, lifeCycleId: item.lifeCycleId, className: 'hidden', isLifeCycleEnabled: false, isVisibleInLifeCycle: false });
      });
    }
    //condtion for Selected LifyCycleName into the CheckBox
    //Activities  Selected LifeCycle should  be appear if the coresponding activities button is enable.
    for (var i = 0; i < selectedLyfecycleId.length; i++) {
      var trimLfCycleId = selectedLyfecycleId[i].trim();
      var lifeCycleItems = $.map(items._data, function (item, index) {
        if (item.lifeCycleId == trimLfCycleId) { return item; }
      })
      $(lifeCycleItems).each(function (index: any, item: any) {
        items.update({ id: item.id, lifeCycleId: trimLfCycleId, className: 'hidden', isLifeCycleEnabled: true });
      });
      let itemsToShow: any = $.map(visibleItems, (item: any, index: number) => { if (item.lifeCycleId == trimLfCycleId) return item });
      $(itemsToShow).each(function (index: any, item: any) {
        items.update({ id: item.id, lifeCycleId: trimLfCycleId, className: item.classNameBackUp, isLifeCycleEnabled: true, isVisibleInLifeCycle: true });
      });
    }
    timeline.setItems(items);
  }

  function ItemDetail(timelineId, crmId, crmEntityType) {
    this.timelineId = timelineId;
    this.crmId = crmId;
    this.crmEntityType = crmEntityType;
  }

  ItemDetail.prototype = {
    openInCrm: function () {
      // Just in case we pass in null Ids or Entity Types this was added
      if (!this.crmId || !this.crmEntityType) {
        return;
      }
      Xrm.Navigation.openForm({
        entityId: this.crmId,
        entityName: this.crmEntityType
      }, null);
    }
  }
}

