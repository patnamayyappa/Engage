/// constants that are used accross the modules.
module Constants {
    export const StudentTimelineActivity = { 
        outgoingTelephoneIcon: "OutgoingTelephone",
        incomingTelephoneIcon: "IncomingTelephone",
        mailSendIcon: "MailSend",
        mailReplyIcon: "MailReply",
        todoCompleteIcon: "TodoComplete",              
        appointmentIcon: "Appointment",
        todoInCompleteIcon: "TodoInComplete",
        marketingEmail:"MarketingEmail"
    };
  

	export const StaffSurvey = {
		SurveyTemplateWebResource: "WebResource_SurveyTemplate",
        SurveyTemplateFormName: "Campus Survey Template",
        SurveyFeedbackFormName: "Feedback Form"
	}

	export const Common = {
		FormTypeNew: 1
		
  }
  // cmc_trip status enum
  export enum TripStatus {
    Planning = 175490000,
    SubmitForApproval = 175490001,
    Approved = 175490002,
    Rejected = 175490003,
    Completed = 175490004,
    Canceled = 175490005,
  }

  export enum TestSource {
    SelfReported = 494280003
  }

  export const CommonInboundInterest = {
     Referral : "{BE7F1922-152B-E811-A965-000D3A11E605}"
  }

  export const Systemuser = {
    UserLookupViewId: "{E88CA999-0B16-4AE9-B6A9-9EDC840D42D8}"   
  }

  export const TripBusinessProcessFlowsStage = {
    SubmittedForReview: "b09f2827-2f6e-42e0-88fb-36f29e3f3730",
    Reviewed: "ac029f02-2082-462f-875e-53003915917c"
  }

  export const SourceMethod = {
    EventAttendee: "{CABA1D29-142B-E811-A965-000D3A11E605}",
    Appointment:"{8157A11E-25B8-E811-A962-000D3A3740B7}",
    Trip:"{243E5425-25B8-E811-A962-000D3A3740B7}"
  }

  export enum TripActivityType {
    Appointment = 175490000,
    Event = 175490001,
    Others = 175490002
  }

  export enum CheckinType {
    SessionCheckin = 100000000,
    EventCheckIn = 100000001

  }

  export enum PrimaryContactType {
    Alumni = 494280001,
    Student = 494280011
  }
}

