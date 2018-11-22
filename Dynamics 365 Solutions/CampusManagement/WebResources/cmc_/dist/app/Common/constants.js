/// constants that are used accross the modules.
var Constants;
(function (Constants) {
    Constants.StudentTimelineActivity = {
        outgoingTelephoneIcon: "OutgoingTelephone",
        incomingTelephoneIcon: "IncomingTelephone",
        mailSendIcon: "MailSend",
        mailReplyIcon: "MailReply",
        todoCompleteIcon: "TodoComplete",
        appointmentIcon: "Appointment",
        todoInCompleteIcon: "TodoInComplete",
        marketingEmail: "MarketingEmail"
    };
    Constants.StaffSurvey = {
        SurveyTemplateWebResource: "WebResource_SurveyTemplate",
        SurveyTemplateFormName: "Campus Survey Template",
        SurveyFeedbackFormName: "Feedback Form"
    };
    Constants.Common = {
        FormTypeNew: 1
    };
    // cmc_trip status enum
    var TripStatus;
    (function (TripStatus) {
        TripStatus[TripStatus["Planning"] = 175490000] = "Planning";
        TripStatus[TripStatus["SubmitForApproval"] = 175490001] = "SubmitForApproval";
        TripStatus[TripStatus["Approved"] = 175490002] = "Approved";
        TripStatus[TripStatus["Rejected"] = 175490003] = "Rejected";
        TripStatus[TripStatus["Completed"] = 175490004] = "Completed";
        TripStatus[TripStatus["Canceled"] = 175490005] = "Canceled";
    })(TripStatus = Constants.TripStatus || (Constants.TripStatus = {}));
    var TestSource;
    (function (TestSource) {
        TestSource[TestSource["SelfReported"] = 494280003] = "SelfReported";
    })(TestSource = Constants.TestSource || (Constants.TestSource = {}));
    Constants.CommonInboundInterest = {
        Referral: "{BE7F1922-152B-E811-A965-000D3A11E605}"
    };
    Constants.Systemuser = {
        UserLookupViewId: "{E88CA999-0B16-4AE9-B6A9-9EDC840D42D8}"
    };
    Constants.TripBusinessProcessFlowsStage = {
        SubmittedForReview: "b09f2827-2f6e-42e0-88fb-36f29e3f3730",
        Reviewed: "ac029f02-2082-462f-875e-53003915917c"
    };
    Constants.SourceMethod = {
        EventAttendee: "{CABA1D29-142B-E811-A965-000D3A11E605}",
        Appointment: "{8157A11E-25B8-E811-A962-000D3A3740B7}",
        Trip: "{243E5425-25B8-E811-A962-000D3A3740B7}"
    };
    var TripActivityType;
    (function (TripActivityType) {
        TripActivityType[TripActivityType["Appointment"] = 175490000] = "Appointment";
        TripActivityType[TripActivityType["Event"] = 175490001] = "Event";
        TripActivityType[TripActivityType["Others"] = 175490002] = "Others";
    })(TripActivityType = Constants.TripActivityType || (Constants.TripActivityType = {}));
    var CheckinType;
    (function (CheckinType) {
        CheckinType[CheckinType["SessionCheckin"] = 100000000] = "SessionCheckin";
        CheckinType[CheckinType["EventCheckIn"] = 100000001] = "EventCheckIn";
    })(CheckinType = Constants.CheckinType || (Constants.CheckinType = {}));
    var PrimaryContactType;
    (function (PrimaryContactType) {
        PrimaryContactType[PrimaryContactType["Alumni"] = 494280001] = "Alumni";
        PrimaryContactType[PrimaryContactType["Student"] = 494280011] = "Student";
    })(PrimaryContactType = Constants.PrimaryContactType || (Constants.PrimaryContactType = {}));
})(Constants || (Constants = {}));
