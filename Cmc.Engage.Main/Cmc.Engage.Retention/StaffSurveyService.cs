using System;
using Cmc.Core.Xrm.ServerExtension.Core;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;
using Microsoft.Xrm.Sdk;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Microsoft.Xrm.Sdk.Query;
using Cmc.Engage.Models;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Cmc.Engage.Common;
using Microsoft.Xrm.Sdk.Messages;
using Cmc.Engage.Common.Utilities;
using Cmc.Engage.Common.Utilities.Constants;
using Newtonsoft.Json;
using Cmc.Engage.Common.Utilities.Helpers;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Workflow.Activities;
using Workflow = Cmc.Engage.Models.Workflow;

namespace Cmc.Engage.Retention
{
	/// <summary>
	/// StaffSurveyService
	/// </summary>
	public class StaffSurveyService : IStaffSurveyService
	{
		private ILogger _logger;

		private IOrganizationService _orgService;
		private readonly IConfigurationService _retriveConfigurationDetails;
		private readonly ILanguageService _retrieveMultiLingualValues;
		/// <summary>
		/// StaffSurveyService constructor
		/// </summary>
		/// <param name="tracer">logger reference</param>
		/// <param name="orgService">organization service reference</param>
		public StaffSurveyService(ILogger tracer, IOrganizationService orgService, IConfigurationService retriveConfigurationDetails, ILanguageService retrieveMultiLingualValues)
		{
			_logger = tracer ?? throw new ArgumentException(nameof(tracer));
			_orgService = orgService ?? throw new ArgumentException(nameof(orgService));
			_retriveConfigurationDetails = retriveConfigurationDetails;
			_retrieveMultiLingualValues = retrieveMultiLingualValues;
		}

		/// <summary>
		/// creates the staff survey question responses
		/// </summary>
		/// <param name="context">holds the execution context</param>
		public void CreateStaffSurveyQuestionResponses(IExecutionContext context)
		{
			var serviceProvider = context.XrmServiceProvider;
			var pluginContext = serviceProvider.GetPluginExecutionContext();

			var target = pluginContext.GetInputParameter<EntityReference>("Target");
			var surveyResponses = pluginContext.GetInputParameter<EntityCollection>("surveyData");
			int isSubmitted = Convert.ToInt32(pluginContext.GetInputParameter("isSubmitted"));

			var logic = CreateQuestionResponse(target, surveyResponses, isSubmitted);
			pluginContext.OutputParameters["staffSurveyResponse"] = JsonConvert.SerializeObject(logic);
		}


		/// <summary>
		/// creates the Question Response instances
		/// </summary>
		/// <param name="surveyId">staffSurvey entity reference</param>
		/// <param name="surveyResponses">List of survey responses</param>
		/// <param name="isSubmitted">flag to indicate if the survey is submitted</param>
		/// <returns></returns>
		public StaffSurveyQuestionResponse CreateQuestionResponse(EntityReference surveyId, EntityCollection surveyResponses, int isSubmitted)
		{
			_logger.Trace("Inside Logic : {0}", surveyResponses.Entities.Count);
			StaffSurveyQuestionResponse returnResponse = new StaffSurveyQuestionResponse();

			if (surveyResponses == null || surveyResponses.Entities == null || !surveyResponses.Entities.Any())
			{
				return returnResponse;
			}
			else
			{
				var result = CreateorUpdateQuestionResponse(surveyId, surveyResponses, isSubmitted);
				return result;
			}
		}

		/// <summary>
		/// used to create or update the survey questions responses
		/// </summary>
		/// <param name="surveyId">survey entity reference</param>
		/// <param name="surveyResponses">list of survey responses</param>
		/// <param name="isSubmitted">flag to indicate if the survey is submitted</param>
		/// <returns></returns>
		public StaffSurveyQuestionResponse CreateorUpdateQuestionResponse(EntityReference surveyId, EntityCollection surveyResponses, int isSubmitted)
		{
			StaffSurveyQuestionResponse returnResponse = new StaffSurveyQuestionResponse();

			if (RetrieveSurveyResponses(surveyResponses.Entities[0].GetAttributeValue<EntityReference>("cmc_staffsurveyresponseid")).Entities.Count > 0)
			{
				_logger?.Trace("Updating surveyResponses");
				ExecuteBulkEntities.BulkUpdateBatch(_orgService, surveyResponses.Entities.ToList());
				_logger?.Trace("Updated surveyResponses");
			}
			else
			{
				foreach (var surveyResponse in surveyResponses.Entities)
				{
					_logger?.Trace("Inside Loop to create surveyResponses");
					var staffSurveyResponse = new cmc_staffsurveyquestionresponse();
					staffSurveyResponse.cmc_staffsurveyquestionid = surveyResponse.GetAttributeValue<EntityReference>("cmc_staffsurveyquestionid");
					staffSurveyResponse.cmc_response = surveyResponse.GetAttributeValue<string>("cmc_response");
					staffSurveyResponse.cmc_staffsurveyresponseid = surveyResponse.GetAttributeValue<EntityReference>("cmc_staffsurveyresponseid");
					staffSurveyResponse.Id = _orgService.Create(staffSurveyResponse);
				}
			}
			if (isSubmitted > 0)
			{
				Entity staffsurvey = new Entity(cmc_staffsurvey.EntityLogicalName) { Id = surveyId.Id };
				staffsurvey["cmc_issubmit"] = true;
				staffsurvey["statecode"] = new OptionSetValue((int)cmc_staffsurveyState.Inactive);
				staffsurvey["statuscode"] = new OptionSetValue((int)cmc_staffsurvey_statuscode.Completed);
				_orgService.Update(staffsurvey);

				_logger?.Trace("Submitted Survey SucessfRelationully");

			}

			return returnResponse;
		}

		/// <summary>
		/// creates or updates the staff survey template
		/// </summary>
		/// <param name="context">plugin execution context</param>
		public void SaveStaffSurveyTemplate(Core.Xrm.ServerExtension.Core.IExecutionContext context)
		{
			_logger.Trace($"entered the SaveStaffSurveyTemplate method");
			var serviceProvider = context.XrmServiceProvider;
			var pluginContext = serviceProvider.GetPluginExecutionContext();
			var staffSurveyTemplate = pluginContext.GetInputParameter<EntityReference>("Target");
			var staffSurveyQuestions = pluginContext.GetInputParameter<EntityCollection>("Questions");
			//getting the template questions
			var staffSurveyTemplateQuestions = GetTemplateQuestions(staffSurveyTemplate);
			//loop through the records to identify deleted intances
			if (staffSurveyTemplateQuestions.Count() > 0)
			{
				_logger.Trace($"checking if any records are deleted");
				foreach (cmc_staffsurveyquestion question in staffSurveyTemplateQuestions)
				{
					var records = staffSurveyQuestions?.Entities?.Select(x => x.ToEntity<cmc_staffsurveyquestion>()).ToList();
					//checking if there are any deleted records
					if (records != null && !records.Any(r => r.cmc_staffsurveyquestionId != Guid.Empty
										&& r.cmc_staffsurveyquestionId == question.cmc_staffsurveyquestionId))
					{
						_logger.Trace($"deleting the template question with id:{(Guid)question.ToEntity<cmc_staffsurveyquestion>().cmc_staffsurveyquestionId}");
						_orgService.Delete("cmc_staffsurveyquestion", (Guid)question.ToEntity<cmc_staffsurveyquestion>().cmc_staffsurveyquestionId);
					}

				}
			}
			_logger.Trace($"looping staffSurveyQuestions.Entities");
			foreach (var question in staffSurveyQuestions.Entities)
			{
				var questionRecord = question.ToEntity<cmc_staffsurveyquestion>();
				_logger.Trace($"preparing the staffsurveyquestion entity");
				var questionEntity = new cmc_staffsurveyquestion();
				questionEntity.cmc_choice = questionRecord.cmc_choice;
				questionEntity.cmc_questionorder = questionRecord.cmc_questionorder;
				questionEntity.cmc_staffsurveyquestionname = questionRecord.cmc_staffsurveyquestionname;
				questionEntity.cmc_staffsurveytemplateid = staffSurveyTemplate;
				questionEntity.Attributes.Add("cmc_questiontype", new OptionSetValue((int)questionRecord.cmc_QuestionType.Value));
				//identify if the record is new record or update record
				_logger.Trace($"checking if the record has to be updated or created");
				if (questionRecord.cmc_staffsurveyquestionId != Guid.Empty
					&& staffSurveyTemplateQuestions.Any(x => x.cmc_staffsurveyquestionId.Equals(
										  questionRecord.cmc_staffsurveyquestionId)))
				{
					questionEntity.cmc_staffsurveyquestionId = questionRecord.cmc_staffsurveyquestionId;
					_logger.Trace($"updating the record with name:{questionEntity.cmc_staffsurveyquestionname}");
					_orgService.Update(questionEntity);
					_logger.Trace($"updated entity:{questionEntity.cmc_staffsurveyquestionname}");
				}
				else
				{
					_logger.Trace($"creating the question entity record with name:{questionEntity.cmc_staffsurveyquestionname}");
					_orgService.Create(questionEntity);
					_logger.Trace("created eth question entity record");
				}

			}
		}

		/// <summary>
		/// used to copy the staff survey template instance
		/// </summary>
		/// <param name="context">plugin execution context</param>
		public void CopyStaffSurveyTemplate(IExecutionContext context)
		{
			_logger.Trace("Inside Function to Copy Staffsurvey template");
			var serviceProvider = context.XrmServiceProvider;
			var pluginContext = serviceProvider.GetPluginExecutionContext();
			var target = pluginContext.GetInputParameter<EntityReference>("Target");
			var SurveyTemplateId = CreateCopyofStaffSurveyTemplate(target);
			pluginContext.OutputParameters["copysurveyTemplateResponse"] = SurveyTemplateId;
		}

		/// <summary>
		/// used to copy the staff survey template instance
		/// </summary>
		/// <param name="staffsurveyTemplate"> staff survey template instance</param>
		public string CreateCopyofStaffSurveyTemplate(EntityReference staffsurveyTemplate)
		{
			var copystaffsurveyTemplate = GetStaffsurveyTemplate(staffsurveyTemplate.Id);

			if (copystaffsurveyTemplate != null)
			{
				_logger.Trace($"Creating Copy of Staff Survey Template");
				var staffSurveyTemplateEntity = new cmc_staffsurveytemplate();
				staffSurveyTemplateEntity.cmc_staffsurveytemplatename = GetStaffSurveyTemplateName(copystaffsurveyTemplate.cmc_staffsurveytemplatename);
				staffSurveyTemplateEntity.cmc_description = copystaffsurveyTemplate.cmc_description;
				staffSurveyTemplateEntity.cmc_startdatecalculationtype = copystaffsurveyTemplate.cmc_startdatecalculationtype;
				if (copystaffsurveyTemplate.cmc_startdatecalculationtype == cmc_staffsurveytemplate_cmc_startdatecalculationtype.Calculated)
				{
					staffSurveyTemplateEntity.cmc_startdatenumberofdays = copystaffsurveyTemplate.cmc_startdatenumberofdays;
					staffSurveyTemplateEntity.cmc_startdatedaysdirection = copystaffsurveyTemplate.cmc_startdatedaysdirection;
					staffSurveyTemplateEntity.cmc_startdatecalculationfield = copystaffsurveyTemplate.cmc_startdatecalculationfield;
				}
				else
					staffSurveyTemplateEntity.cmc_startdatestatic = copystaffsurveyTemplate.cmc_startdatestatic;

				staffSurveyTemplateEntity.cmc_duedatecalculationtype = copystaffsurveyTemplate.cmc_duedatecalculationtype;
				if (copystaffsurveyTemplate.cmc_duedatecalculationtype == cmc_staffsurveytemplate_cmc_duedatecalculationtype.Calculated)
				{
					staffSurveyTemplateEntity.cmc_duedatenumberofdays = copystaffsurveyTemplate.cmc_duedatenumberofdays;
					staffSurveyTemplateEntity.cmc_duedatedaysdirection = copystaffsurveyTemplate.cmc_duedatedaysdirection;
					staffSurveyTemplateEntity.cmc_duedatecalculationfield = copystaffsurveyTemplate.cmc_duedatecalculationfield;
				}
				else
					staffSurveyTemplateEntity.cmc_duedatestatic = copystaffsurveyTemplate.cmc_duedatestatic;

				staffSurveyTemplateEntity.Id = _orgService.Create(staffSurveyTemplateEntity);

				_logger.Trace($"Created Copy of Staff Survey Template - '{ staffSurveyTemplateEntity.cmc_staffsurveytemplatename}'");

				_logger.Trace("Retriving associated Staffsurvey template Questions.");

				IEnumerable<cmc_staffsurveyquestion> surveyTemplateQuestions = GetTemplateQuestions(staffsurveyTemplate);

				if (surveyTemplateQuestions.Count() > 0)
				{
					EntityReferenceCollection SurveyQuestionsCollection = CopyStaffSurveyTemplateQuestions(surveyTemplateQuestions);

					if (SurveyQuestionsCollection.Count() > 0)
					{
						_logger.Trace("Associating questions to new template.");
						Relationship QuestionRelationship = new Relationship("cmc_staffsurveytemplate_cmc_staffsurveyqu");
						_orgService.Associate(cmc_staffsurveytemplate.EntityLogicalName, staffSurveyTemplateEntity.Id, QuestionRelationship, SurveyQuestionsCollection);
					}
				}
				else
					_logger.Trace("Questions not found for provided template.");
				return (staffSurveyTemplateEntity.Id).ToString();
			}
			else
			{
				_logger.Trace("Unable to create Staffsurveytemplate.");
				return "";
			}
		}
		private string GetStaffSurveyTemplateName(string oldTemplateName)
		{
			string newTemplateName = oldTemplateName + "-Copy";
			string[] staffSurveyTemplateNames;

			_logger.Trace($"Fetching Staff Survey template names similar to - '{oldTemplateName}'");

			var fetch = $@"<fetch >
                      <entity name='cmc_staffsurveytemplate'>
                        <attribute name='cmc_staffsurveytemplatename' />                        
                        <filter type='and'>
                          <condition attribute='cmc_staffsurveytemplatename'  operator='like' value='%{oldTemplateName}%' />
                        </filter>
                      </entity>
                    </fetch>";

			var entityCollection = _orgService.RetrieveMultiple(new FetchExpression(fetch))?.Entities;
			if (entityCollection != null && entityCollection.Count() > 0)
			{
				staffSurveyTemplateNames = entityCollection.Cast<cmc_staffsurveytemplate>().ToList<cmc_staffsurveytemplate>().Select(I => Convert.ToString(I.cmc_staffsurveytemplatename)).ToArray();

				if (Array.IndexOf(staffSurveyTemplateNames, newTemplateName) == -1)
					return newTemplateName;
				string uniqueName;
				for (int i = 1; i <= staffSurveyTemplateNames.Length; i++)
				{
					uniqueName = oldTemplateName + "-Copy(" + i + ")";
					if (Array.IndexOf(staffSurveyTemplateNames, uniqueName) == -1)
					{
						newTemplateName = uniqueName;
						break;
					}
				}
			}
			return newTemplateName;
		}
		private cmc_staffsurveytemplate GetStaffsurveyTemplate(Guid staffSurveyTemplate)
		{
			_logger.Trace("Fetching Staffsurvey Template Details");
			return (_orgService.RetrieveMultipleAll($@"<fetch mapping='logical' version='1.0' distinct='false' output-format='xml-platform'>
                      <entity name='cmc_staffsurveytemplate'>
                        <attribute name='cmc_staffsurveytemplateid' />
                        <attribute name='cmc_staffsurveytemplatename' />
                        <attribute name='cmc_startdatestatic' />
                        <attribute name='cmc_startdatenumberofdays' />
                        <attribute name='cmc_startdatedaysdirection' />
                        <attribute name='cmc_startdatecalculationtype' />
                        <attribute name='cmc_startdatecalculationfield' />
                        <attribute name='cmc_duedatestatic' />
                        <attribute name='cmc_duedatenumberofdays' />
                        <attribute name='cmc_duedatedaysdirection' />
                        <attribute name='cmc_duedatecalculationtype' />
                        <attribute name='cmc_duedatecalculationfield' />
                        <attribute name='cmc_description' />
                        <filter type='and'>
                          <condition attribute='cmc_staffsurveytemplateid'  operator='eq' value='{staffSurveyTemplate}' />
                        </filter>
                      </entity>
                    </fetch>")?.Entities)?.Cast<cmc_staffsurveytemplate>()?.FirstOrDefault();
		}
		/// <summary>
		/// used to validate the staff survey instance
		/// </summary>
		/// <param name="executionContext">plugin execution context</param>
		public void ValidateSurveyInstance(IExecutionContext executionContext)
		{
			_logger.Trace("validating the survey instance");
			var serviceProvider = executionContext.XrmServiceProvider;
			var pluginContext = serviceProvider.GetPluginExecutionContext();
			cmc_staffsurvey staffSurvey = pluginContext.GetTargetEntity<cmc_staffsurvey>();
			var staffCourse = staffSurvey.cmc_coursesectionid;
			//retreive the staffSurveyTemplate details
			var surveyTemplate = _orgService.Retrieve(staffSurvey.cmc_staffsurveytemplateid, new ColumnSet(true));
			//retreive the staff course details
			var staffcourseDetails = _orgService.Retrieve(staffCourse, new ColumnSet(true));
			var staffCourseStatusCode = staffcourseDetails.GetAttributeValue<OptionSetValue>("statecode");
			var academicPeriodId = staffcourseDetails.GetAttributeValue<EntityReference>("mshied_academicperiodid");
			var cmc_startdate = GetStaffSurveyStartDate(surveyTemplate, academicPeriodId.Id);
			var cmc_duedate = GetStaffSurveyDueDate(surveyTemplate, academicPeriodId.Id);
			if (cmc_startdate == null || cmc_duedate == null)
			{
				_logger.Trace("Survey creation failed as the start date or due date is null");
				throw new InvalidPluginExecutionException(_retrieveMultiLingualValues.Get("StaffSurveyValidationError"));
			}

			if (staffCourseStatusCode.Value == (int)mshied_coursesectionState.Inactive)
			{
				_logger.Trace("survey creation failed as the staff course is inactive");
				throw new InvalidPluginExecutionException(_retrieveMultiLingualValues.Get("StaffSurveyValidationError"));
			}

			if (!(IsStaffSurveyValid(staffCourse.Id, cmc_startdate.Value, cmc_duedate.Value)))
			{
				_logger.Trace("survey creation failed as there could be a duplicate survey record for the selected staff course and template");
				throw new InvalidPluginExecutionException(_retrieveMultiLingualValues.Get("StaffSurveyValidationError"));
			}

			_logger.Trace($"updating the staff survey required fields post validation");
			staffSurvey.cmc_startdate = cmc_startdate;
			staffSurvey.cmc_duedate = cmc_duedate;
			staffSurvey.cmc_description = surveyTemplate.GetAttributeValue<string>("cmc_description");
			staffSurvey.cmc_userid = staffcourseDetails.GetAttributeValue<EntityReference>("cmc_staffid");
		}

		/// <summary>
		/// used to perform the post creation operation of Staff Survey
		/// </summary>
		/// <param name="executionContext">plugin execution context</param>
		public void PerformStaffSurveyPostOperationLogic(IExecutionContext executionContext)
		{
			_logger.Trace($"performing the staff survey post creation logic");
			var serviceProvider = executionContext.XrmServiceProvider;
			var pluginContext = serviceProvider.GetPluginExecutionContext();
			var staffSurvey = pluginContext.GetTargetEntity<cmc_staffsurvey>();

			var staffcourseDetails = _orgService.Retrieve(staffSurvey.cmc_coursesectionid, new ColumnSet(true));
			//update the description from the StaffSurveyTemplate
			_logger.Trace($"retrieving the staffSurveyTemplate records details");
			var surveyTemplateRecord = _orgService.Retrieve(staffSurvey.cmc_staffsurveytemplateid, new ColumnSet(true));

			_orgService.Update(staffSurvey);

			//Make a copy of the StaffSurveyTemplateQuestions and associate them to the StaffSurvey
			var questionRelationship = new Relationship("cmc_staffsurvey_cmc_staffsurveyquestion");
			_logger.Trace($"retrieving the staff course template questions");
			var surveyTemplateQuestions = GetTemplateQuestions(staffSurvey.cmc_staffsurveytemplateid);
			_logger.Trace($"creating a copy of the staff course template questions");
			EntityReferenceCollection staffSurveyQuestions = CopyStaffSurveyTemplateQuestions(surveyTemplateQuestions);
			_logger.Trace($"associating the questions to the staff survey");
			_orgService.Associate(cmc_staffsurvey.EntityLogicalName, staffSurvey.Id, questionRelationship, staffSurveyQuestions);

			//Get the StaffCourseStudents associated to the StaffCourse and create a Staff Survey Responses
			var staffCourse = staffSurvey.cmc_coursesectionid;
			_logger.Trace($"retrieving the staff course students from the staffcourse");
			var staffCourseStudents = RetrieveStaffCourseStudents(staffCourse);
			_logger.Trace($"creating the staff survey responses");
			CreateStaffSurveyResponses(staffCourseStudents, staffSurvey);
		}

		/// <summary>
		/// used to cretae the staff survey instance from the staff survey template
		/// </summary>
		/// <param name="context">plugin execution context</param>
		public void CreateStaffSurveyFromTemplate(IExecutionContext context)
		{
			var serviceProvider = context.XrmServiceProvider;
			var pluginContext = serviceProvider.GetPluginExecutionContext();

			var target = pluginContext.GetInputParameter<EntityReference>("Target");
			var staffs = pluginContext.GetInputParameter<EntityCollection>("Staffs");

			var logic = CreateStaffSurvey(target, staffs);
			pluginContext.OutputParameters["staffSurveyResponse"] = JsonConvert.SerializeObject(logic);
		}

		/// <summary>
		/// used to create the StaffSurvey
		/// </summary>
		/// <param name="surveyTemplateId">staff survey template entity reference</param>
		/// <param name="staffCourses">list of staff courses</param>
		/// <returns></returns>
		public AssignStaffSurveyResponse CreateStaffSurvey(EntityReference surveyTemplateId, EntityCollection staffCourses)
		{
			var createStaffSurveyResponse = new AssignStaffSurveyResponse();
			if (staffCourses == null || staffCourses.Entities == null || !staffCourses.Entities.Any())
			{
				return createStaffSurveyResponse;
			}
			var staffSurveyId = new List<EntityReference>();
			var staffSurveyQuestions = new EntityReferenceCollection();
			var staffSurveyTemplate = _orgService.Retrieve(surveyTemplateId, new ColumnSet(true));
			var surveyTemplateName = staffSurveyTemplate.GetAttributeValue<string>("cmc_staffsurveytemplatename");

			//copy the staff survey template questions
			_logger?.Trace("copying the staff survey template questions");

			//loop through the StaffCourses and get the contacts associated to 
			//each staff course to associate with survey response
			foreach (var staffcourse in staffCourses.Entities)
			{
				_logger?.Trace("Started looping for create staffCourseSurvey");
				var staffcourseId = staffcourse.ToEntityReference();
				//get the staffCourseDetails
				var staffcourseDetails = _orgService.Retrieve(staffcourse.ToEntityReference(), new ColumnSet(true));
				//get the staffCourse status code
				OptionSetValue staffCourseStateCode = staffcourseDetails.GetAttributeValue<OptionSetValue>("statecode");
				//get the acedemic period record for the given staff course
				var academicPeriod = staffcourseDetails.GetAttributeValue<EntityReference>("mshied_academicperiodid");
				//get the staffSurveyStartDate
				var cmc_startdate = GetStaffSurveyStartDate(staffSurveyTemplate, academicPeriod.Id);
				//get the staffSurveyEndDate
				var cmc_duedate = GetStaffSurveyDueDate(staffSurveyTemplate, academicPeriod.Id);
				//prepare the staffSurvey object for creation
				#region staffSurvey creation
				var staffSurvey = new cmc_staffsurvey();
				staffSurvey.cmc_userid = staffcourseDetails.GetAttributeValue<EntityReference>("cmc_staffid");
				staffSurvey.cmc_coursesectionid = staffcourseId;
				staffSurvey.cmc_staffsurveytemplateid = surveyTemplateId;
				staffSurvey["statuscode"] = new OptionSetValue((int)cmc_staffsurvey_statuscode.New);
				if (cmc_startdate == null || cmc_duedate == null)
				{
					createStaffSurveyResponse.failedStaffCourses.Add(staffcourseDetails.GetAttributeValue<string>("mshied_name"));
					_logger?.Trace($"the staffCourse:{staffcourseDetails.GetAttributeValue<string>("mshied_name")} does not have valid start or end dates");
					continue;
				}
				staffSurvey.cmc_startdate = cmc_startdate;
				staffSurvey.cmc_duedate = cmc_duedate;
				if (!(IsStaffSurveyValid(staffcourseId.Id, staffSurvey.cmc_startdate.Value, staffSurvey.cmc_duedate.Value)))
				{
					createStaffSurveyResponse.failedStaffCourses.Add(staffcourseDetails.GetAttributeValue<string>("mshied_name"));
					_logger?.Trace("The Staff Survey instance is not a valid instance");
					continue;
				}
				if (staffCourseStateCode.Value == (int)mshied_coursesectionState.Inactive)
				{
					createStaffSurveyResponse.failedStaffCourses.Add(staffcourseDetails.GetAttributeValue<string>("mshied_name"));
					_logger?.Trace($"This staff course is Inactive :{staffcourseDetails.GetAttributeValue<string>("mshied_name")}");
					continue;
				}
				_logger?.Trace("creating new staffSruvey record");
				staffSurvey.Id = _orgService.Create(staffSurvey);
				#endregion staffSurvey creation				
			}
			return createStaffSurveyResponse;
		}

		/// <summary>
		/// used to create staff survey responses
		/// </summary>
		/// <param name="staffCourseStudents">list of staff course students</param>
		/// <param name="staffSurvey">staff survey instance</param>
		private void CreateStaffSurveyResponses(EntityCollection staffCourseStudents, cmc_staffsurvey staffSurvey)
		{
			var multipleRecordCreateRequest = new ExecuteTransactionRequest()
			{
				// Create an empty organization request collection.
				Requests = new OrganizationRequestCollection(),
				ReturnResponses = true
			};
			foreach (var student in staffCourseStudents.Entities)
			{
				var suveyResponseEntity = new cmc_staffsurveyresponse
				{
					cmc_contactid = student.ToEntityReference(),
					cmc_staffsurveyId = staffSurvey.ToEntityReference()
				};
				CreateRequest createRequest = new CreateRequest { Target = suveyResponseEntity };
				multipleRecordCreateRequest.Requests.Add(createRequest);
				_logger?.Trace("Student Name: {0} - {1} ", student.Id, student.GetAttributeValue<string>("fullname"));
			}
			_logger.Trace($"executing the multipleRecordCreateRequest to associate the students to the survey response");
			_orgService.Execute(multipleRecordCreateRequest);
			_logger.Trace($"multipleRecordCreateRequest executed successfully");
		}

		/// <summary>
		/// used to retrieve the survey response for the given survey id
		/// </summary>
		/// <param name="surveyId">staff survey entity reference</param>
		/// <returns>returns collection of survey responses</returns>
		private EntityCollection RetrieveSurveyResponses(EntityReference surveyId)
		{
			_logger?.Trace("Retrieving Staff Survey Response");
			return _orgService.RetrieveMultipleAll($@"
                <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                  <entity name='cmc_staffsurveyquestionresponse'>
                    <attribute name='cmc_staffsurveyquestionresponseid' />
                    <filter type='and'>
                      <condition attribute='cmc_staffsurveyresponseid' operator='eq'  value='{surveyId.Id}' />
                    </filter>
                  </entity>
                </fetch>");
		}


		/// <summary>
		/// used to copy the staff survey template questions
		/// </summary>
		/// <param name="surveyTemplateQuestions">list of staff survey questions</param>
		/// <returns>returns a entity reference collection of staff survey questions</returns>
		private EntityReferenceCollection CopyStaffSurveyTemplateQuestions(IEnumerable<cmc_staffsurveyquestion> surveyTemplateQuestions)
		{
			var staffSurveyQuestions = new EntityReferenceCollection();
			foreach (var surveyTemplateQuestion in surveyTemplateQuestions)
			{

				var staffSurveyQuestion = new cmc_staffsurveyquestion();

				staffSurveyQuestion.cmc_staffsurveyquestionname = surveyTemplateQuestion.cmc_staffsurveyquestionname;

				staffSurveyQuestion.cmc_choice = surveyTemplateQuestion.cmc_choice;

				staffSurveyQuestion.cmc_questionorder = surveyTemplateQuestion.cmc_questionorder;

				staffSurveyQuestion.Attributes.Add("cmc_questiontype", new OptionSetValue((int)surveyTemplateQuestion.cmc_QuestionType.Value));

				staffSurveyQuestion.Id = _orgService.Create(staffSurveyQuestion);
				_logger?.Trace("Adding question : " + staffSurveyQuestion.Id + " with name " + staffSurveyQuestion.cmc_staffsurveyquestionname + " to the list");
				staffSurveyQuestions.Add(new EntityReference("cmc_staffsurveyquestion", staffSurveyQuestion.Id));

			}
			return staffSurveyQuestions;
		}

		/// <summary>
		/// used to retrieve staff course students
		/// </summary>
		/// <param name="StaffCourse">staff course entity reference</param>
		/// <returns>returns an entity collection of staff course students</returns>
		private EntityCollection RetrieveStaffCourseStudents(EntityReference StaffCourse)
		{
			var staffCourseStudents = new EntityReferenceCollection();
			_logger?.Trace("Retrieving Staff Course Course History Student's");

			return _orgService.RetrieveMultipleAll($@"<fetch distinct='true'>
            <entity name='contact'>
               <attribute name='fullname' />
               <attribute name='contactid' />
				<filter type='and'>
					  <condition attribute='statecode' operator='eq' value='0' />
				</filter >
					<link-entity name='mshied_coursehistory' from='mshied_studentid' to='contactid' link-type='inner' alias='ab'>
                        <filter type='and'>
                            <condition attribute='mshied_coursesectionid' operator='eq' value='{StaffCourse.Id}'/>
                        </filter>
                    </link-entity>
            </entity>
         </fetch>");
		}


		/// <summary>
		/// used to get the staff survey start date
		/// </summary>
		/// <param name="surveyTemplate">staff survey template entity</param>
		/// <param name="academicPeriodId">academic period id</param>
		/// <returns>returns datetime</returns>
		private DateTime? GetStaffSurveyStartDate(Entity surveyTemplate, Guid academicPeriodId)
		{
			DateTime? startDate = null;
			var staffSurveyTemplateEntity = surveyTemplate.ToEntity<cmc_staffsurveytemplate>();
			switch (staffSurveyTemplateEntity.cmc_startdatecalculationtype)
			{
				case cmc_staffsurveytemplate_cmc_startdatecalculationtype.Calculated:

					var startdays = (staffSurveyTemplateEntity.cmc_startdatenumberofdays.HasValue
										   && staffSurveyTemplateEntity.cmc_startdatenumberofdays.Value > 0) ? staffSurveyTemplateEntity.cmc_startdatenumberofdays.Value : 0;
					switch (staffSurveyTemplateEntity.cmc_startdatecalculationfield)
					{
						case cmc_staffsurveytemplate_cmc_startdatecalculationfield.AssignmentDate:
							startDate = DateTime.Now;
							break;
						case cmc_staffsurveytemplate_cmc_startdatecalculationfield.StartDateofAcademicPeriod:
							var cmcStartAcademicPeriod = GetAcademicPeriodDates(academicPeriodId);
							_logger.Trace($"Fetched academic period");
							if (cmcStartAcademicPeriod.mshied_StartDate != null)
							{
								startDate = cmcStartAcademicPeriod.mshied_StartDate;
								_logger?.Trace($"StartDateofAcademicPeriod:{startDate} for {academicPeriodId}");
							}
							else
							{
								startDate = null;
								_logger?.Trace($"StartDateofAcademicPeriod is null for {academicPeriodId}");
							}
							break;
						case cmc_staffsurveytemplate_cmc_startdatecalculationfield.EndDateofAcademicPeriod:

							var cmcEndAcademicPeriod = GetAcademicPeriodDates(academicPeriodId);
							_logger.Trace($"Fetched academic period");
							if (cmcEndAcademicPeriod.mshied_EndDate != null)
							{
								startDate = cmcEndAcademicPeriod.mshied_EndDate;
								_logger?.Trace($"EndDateofAcademicPeriod:{startDate} for {academicPeriodId}");
							}
							else
							{
								startDate = null;
								_logger?.Trace($"EndDateofAcademicPeriod is null for {academicPeriodId}");
							}

							break;

					}
					// add the days.
					switch (staffSurveyTemplateEntity.cmc_startdatedaysdirection)
					{
						case cmc_staffsurveytemplate_cmc_startdatedaysdirection.After:
							startDate = startDate?.AddDays(startdays); // adding the days
							_logger?.Trace($"Added After {startdays} in start date  for {academicPeriodId}");
							break;
						case cmc_staffsurveytemplate_cmc_startdatedaysdirection.Before:
							startDate = startDate?.AddDays(-startdays); // substracting the days
							_logger?.Trace($"Substraced Before {startdays} in start date  for {academicPeriodId}");
							break;
					}
					_logger?.Trace($"calculation of start date -> calculated start date {startDate}.");
					break;
				case cmc_staffsurveytemplate_cmc_startdatecalculationtype.Static:
					startDate = staffSurveyTemplateEntity.cmc_startdatestatic;
					_logger?.Trace($"calculation of start date -> static start date {startDate}.");
					break;
			}
			return startDate;
		}


		/// <summary>
		/// used to get the staff survey due date
		/// </summary>
		/// <param name="surveyTemplate">staff survey template entity</param>
		/// <param name="academicPeriodId">academic period id</param>
		/// <returns>returns datetime</returns>
		private DateTime? GetStaffSurveyDueDate(Entity surveyTemplate, Guid academicPeriodId)
		{
			DateTime? dueDate = null;
			var staffSurveyTemplateEntity = surveyTemplate.ToEntity<cmc_staffsurveytemplate>();
			switch (staffSurveyTemplateEntity.cmc_duedatecalculationtype)
			{
				case cmc_staffsurveytemplate_cmc_duedatecalculationtype.Calculated:

					var duedays = (staffSurveyTemplateEntity.cmc_duedatenumberofdays.HasValue
										   && staffSurveyTemplateEntity.cmc_duedatenumberofdays.Value > 0) ? staffSurveyTemplateEntity.cmc_duedatenumberofdays.Value : 0;
					switch (staffSurveyTemplateEntity.cmc_duedatecalculationfield)
					{
						case cmc_staffsurveytemplate_cmc_duedatecalculationfield.AssignmentDate:
							dueDate = DateTime.Now;
							break;
						case cmc_staffsurveytemplate_cmc_duedatecalculationfield.StartDateofAcademicPeriod:
							var cmcStartAcademicPeriod = GetAcademicPeriodDates(academicPeriodId);
							_logger.Trace($"Fetched academic period");
							if (cmcStartAcademicPeriod.mshied_StartDate != null)
							{
								dueDate = cmcStartAcademicPeriod.mshied_StartDate;
								_logger?.Trace($"StartDateofAcademicPeriod:{dueDate} for {academicPeriodId}");
							}
							else
							{
								dueDate = null;
								_logger?.Trace($"StartDateofAcademicPeriod is null for {academicPeriodId}");
							}
							break;
						case cmc_staffsurveytemplate_cmc_duedatecalculationfield.EndDateofAcademicPeriod:
							var cmcEndAcademicPeriod = GetAcademicPeriodDates(academicPeriodId);
							_logger.Trace($"Fetched academic period");
							if (cmcEndAcademicPeriod.mshied_EndDate != null)
							{
								dueDate = cmcEndAcademicPeriod.mshied_EndDate;
								_logger?.Trace($"EndDateofAcademicPeriod:{dueDate} for {academicPeriodId}");
							}
							else
							{
								dueDate = null;
								_logger?.Trace($"EndDateofAcademicPeriod is null for {academicPeriodId}");
							}
							break;
					}
					// add the days.
					switch (staffSurveyTemplateEntity.cmc_duedatedaysdirection)
					{
						case cmc_staffsurveytemplate_cmc_duedatedaysdirection.After:
							dueDate = dueDate?.AddDays(duedays); // adding the days
							_logger?.Trace($"Added After {duedays} in due date  for {academicPeriodId}");
							break;
						case cmc_staffsurveytemplate_cmc_duedatedaysdirection.Before:
							dueDate = dueDate?.AddDays(-duedays); // substracting the days
							_logger?.Trace($"Substraced Before {duedays} in due date  for {academicPeriodId}");
							break;
					}
					_logger?.Trace($"calculation of due date -> calculated due date {dueDate}.");
					break;
				case cmc_staffsurveytemplate_cmc_duedatecalculationtype.Static:
					dueDate = staffSurveyTemplateEntity.cmc_duedatestatic;
					_logger?.Trace($"calculation of due date -> static due date {dueDate}.");
					break;
			}
			//we want to expire the record on the end of the day means 23 hr 59 mins.
			dueDate = dueDate?.AddMinutes(1439);
			return dueDate;
		}



		/// <summary>
		/// used to get the academic period dates
		/// </summary>
		/// <param name="academicperiodId">academic period id</param>
		/// <returns>returns academic period record</returns>
		private mshied_academicperiod GetAcademicPeriodDates(Guid academicperiodId)
		{
			_logger.Trace("Fetching Academic Period");
			return (_orgService.RetrieveMultipleAll($@"<fetch>
                                    <entity name ='mshied_academicperiod'>                                        
                                        <attribute name ='mshied_startdate'/>
                                        <attribute name ='mshied_enddate'/>                                        
                                                <filter type ='and'>
                                                    <condition attribute ='mshied_academicperiodid' value ='{academicperiodId}'  operator ='eq'/>
                                                </filter>
                                    </entity>
                             </fetch>")?.Entities)?.Cast<mshied_academicperiod>()?.FirstOrDefault();
		}


		/// <summary>
		/// used to validate the survey record
		/// </summary>
		/// <param name="staffCourseId">staff course id</param>
		/// <param name="startDate">start date</param>
		/// <param name="dueDate">due date</param>
		/// <returns>returns if the survey record is valid or not</returns>
		private Boolean IsStaffSurveyValid(Guid staffCourseId, DateTime startDate, DateTime dueDate)
		{
			_logger?.Trace("Validate DupCheck started .");
			_logger?.Trace($"Start Date - {startDate}  - Due Date  {dueDate} - Converted Start Date - {startDate.ToUniversalTime().Date}  - Current date {DateTime.UtcNow.Date}");
			Boolean isValid = true;
			if (startDate.ToUniversalTime().Date < DateTime.UtcNow.Date)
			{
				isValid = false;
				_logger?.Trace($"Start Date - {startDate.ToUniversalTime().Date}  is less than current date {DateTime.UtcNow.Date}.");
			}
			else if (startDate > dueDate)
			{
				isValid = false;
				_logger?.Trace($"Start Date - {startDate}  is greater than Due date {dueDate}.");
			}
			else if (IsSurveyDuplicate(staffCourseId, startDate, dueDate))
			{
				isValid = false;
				_logger?.Trace("Some other staff survey exists for same period of staff survay template.");
			}
			return isValid;
		}


		/// <summary>
		/// Method for Checking  Duplicate Survey  
		/// </summary>
		/// <param name="staffCourseId">staff course id</param>
		/// <param name="startDate">start date</param>
		/// <param name="dueDate">due date</param>
		/// <returns></returns>
		private Boolean IsSurveyDuplicate(Guid staffCourseId, DateTime startDate, DateTime dueDate)
		{
			_logger?.Trace("Fetching Existing Staff Surveys for StaffCourseID : {0}, StartDate : {1}, DueDate : {2}", staffCourseId, startDate, dueDate);
			var fetchXml = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                <entity name='cmc_staffsurvey'>
                    <attribute name='cmc_staffsurveyid' />
                        <filter type='and'>
                            <condition attribute='cmc_coursesectionid' operator='eq'  value='{staffCourseId}' />
                                <filter type='or'>
                                    <filter type='and'>
                                        <condition attribute='cmc_startdate' operator='on-or-before' value='{startDate.ToString("yyyy-MM-dd")}z' />
                                        <condition attribute='cmc_duedate' operator='on-or-after' value='{startDate.ToString("yyyy-MM-dd")}z' />
                                    </filter>
                                    <filter type='and'>
                                        <condition attribute='cmc_startdate' operator='on-or-before' value='{dueDate.ToString("yyyy-MM-dd")}z' />
                                        <condition attribute='cmc_duedate' operator='on-or-after' value='{dueDate.ToString("yyyy-MM-dd")}z' />
                                    </filter>
                                    <filter type='and'>
                                        <condition attribute='cmc_startdate' operator='on-or-after' value='{startDate.ToString("yyyy-MM-dd")}z' />
                                        <condition attribute='cmc_duedate' operator='on-or-before' value='{dueDate.ToString("yyyy-MM-dd")}z' />
                                    </filter>
                                </filter>
                                <condition attribute='statuscode' operator='in'>
                                <value>{(int)cmc_staffsurvey_statuscode.InProgress}</value>
                                <value>{(int)cmc_staffsurvey_statuscode.New}</value>
                            </condition>
                        </filter>
                </entity>
            </fetch>";
			var staffSurveys = _orgService.RetrieveMultipleAll(fetchXml);
			_logger?.Trace(staffSurveys.Entities.Count().ToString());
			return staffSurveys.Entities.Count() > 0 ? true : false;
		}

		/// <summary>
		/// used to get the staff survey template questions
		/// </summary>
		/// <param name="staffSurveyTemplate">staff survey template entity reference</param>
		/// <returns>returns list of staff survey questions</returns>
		private IEnumerable<cmc_staffsurveyquestion> GetTemplateQuestions(EntityReference staffSurveyTemplate)
		{
			_logger.Trace($"Preparing the fetch xml query for getting the template questions");
			IEnumerable<cmc_staffsurveyquestion> staffSurveyQuestions = new List<cmc_staffsurveyquestion>();
			var fetch = $@"<fetch>
                      <entity name='cmc_staffsurveyquestion'>
                          <attribute name='cmc_staffsurveyquestionname'/>
                         <attribute name='cmc_choice' />
                         <attribute name='cmc_staffsurveyquestionid' />
                         <attribute name='cmc_questiontype' />
                        <attribute name='cmc_questionorder' />
                        <filter type='and'>
                          <condition attribute='cmc_staffsurveytemplateid' operator='eq' value='{staffSurveyTemplate.Id}'/>            
                        </filter>
                      </entity>
                    </fetch>";
			_logger.Trace($"calling the RetrieveMultiple method");
			var entityCollection = _orgService.RetrieveMultiple(new FetchExpression(fetch))?.Entities;
			_logger.Trace($"RetrieveMultiple method called");
			if (entityCollection != null && entityCollection.Count() > 0)
			{
				_logger.Trace($"casting the entity collection to cmc_staffsurveyquestion");
				staffSurveyQuestions = entityCollection.Cast<cmc_staffsurveyquestion>();
			}

			return staffSurveyQuestions;

		}

		#region Update Staff Survey Completion/Cancelled Dates 
		/// <summary>
		/// Update Staff Survey Completion/Cancelled Date 
		/// </summary>
		/// <param name="context"></param>
		public void UpdateStaffSurveyCompletedCancellationDate(Core.Xrm.ServerExtension.Core.IExecutionContext context)
		{
			_logger.Trace($"Updating Staff Survey Completed/Cancellation Date");
			var serviceProvider = context.XrmServiceProvider;
			var pluginContext = serviceProvider.GetPluginExecutionContext();

			if (!pluginContext.IsValidCall("cmc_staffsurvey"))
				throw new InvalidPluginExecutionException(_retrieveMultiLingualValues.Get("PluginIsNotConfiguredcorrectly"));

			var staffSurvey = pluginContext.GetPostEntityImage<cmc_staffsurvey>("PostImage");
			var stateCode = staffSurvey.statecode;

			_logger.Trace($"Staff Survey state Code - {stateCode}");

			if (stateCode == cmc_staffsurveyState.Active)
			{
				_logger.Trace($"Clearing Staff Survey Completed/Cancelled date and Cancellation Comment");
				_orgService.Update(new cmc_staffsurvey
				{
					cmc_staffsurveyId = staffSurvey.Id,
					cmc_completedcancelleddate = null,
					cmc_cancellationcomment = null

				});
			}
			else if (stateCode == cmc_staffsurveyState.Inactive)
			{
				_logger.Trace($"Setting Staff Survey Completed/Cancelled date - {DateTime.UtcNow}");
				_orgService.Update(new cmc_staffsurvey
				{
					cmc_staffsurveyId = staffSurvey.Id,
					cmc_completedcancelleddate = DateTime.UtcNow
				});
			}
			_logger.Trace($"Staff Survey Completed/Cancelled date Updated ");
		}

		#endregion

		#region Send StaffSurvey Remainder Email to Faculty
		/// <summary>
		/// Send Remainder Email to Faculty.
		/// </summary>
		public void SendStaffSurveyReminderEmail()
		{
			_logger.Trace($"Inside Function to send an Remainder email to faculty");
			var configurationDetails = _retriveConfigurationDetails.GetActiveConfiguration();


			if (configurationDetails == null) return;

			if (!configurationDetails.Contains("cmc_staffsurveysendreminderemailnumberofdays") ||
				!configurationDetails.Contains("cmc_staffsurveyemailworkflow"))
			{
				return;
			}

			var numberOfDays = configurationDetails.GetAttributeValue<int>("cmc_staffsurveysendreminderemailnumberofdays");
			var staffSurveyList = GetValidStaffSurveyList(Convert.ToDouble(numberOfDays));

			if (staffSurveyList == null) return;
			_logger.Info($"Staff Survey's count :  {staffSurveyList.Count}");


			var workflow = configurationDetails.GetAttributeValue<EntityReference>("cmc_staffsurveyemailworkflow");
			if (workflow == null) return;
			string workflowName = workflow.Name;
			_logger.Info($"Workflow name is {workflowName} ");
			var workflowDetails = GetEmailWorkFlow(workflowName);
			if (workflowDetails == null) return;
			var workflowId = workflowDetails.Id;
			var executeMultipleBuffer = new ExecuteMultipleBuffer(_orgService);
			staffSurveyList.ForEach(a =>
			{
				_logger.Info(
					$"Staff Survey name is {a.cmc_staffsurveyname} and workflow name is {workflowName} ");
				executeMultipleBuffer.Execute(new ExecuteWorkflowRequest
				{
					EntityId = a.Id,
					WorkflowId = workflowId
				});
			});
			executeMultipleBuffer.Flush();



		}

		public List<cmc_staffsurvey> GetValidStaffSurveyList(double numberOfDays)
		{
			var queryExpression = new QueryExpression("cmc_staffsurvey");
			queryExpression.ColumnSet.AddColumns("statuscode", "cmc_startdate", "cmc_staffsurveyid", "cmc_staffsurveyname", "cmc_duedate");
			queryExpression.Criteria.FilterOperator = LogicalOperator.Or;

			var validStaffSurveysForStartDateTodayFilter = new FilterExpression();
			validStaffSurveysForStartDateTodayFilter.AddCondition("statuscode", ConditionOperator.Equal, (int)cmc_staffsurvey_statuscode.New);
			validStaffSurveysForStartDateTodayFilter.AddCondition("cmc_startdate", ConditionOperator.OnOrBefore,
				DateTime.UtcNow.Date.ToString("yyyy-MM-dd") + "Z");

			var validStaffSurveyListForDueDateFilter = new FilterExpression();
			validStaffSurveyListForDueDateFilter.AddCondition("statuscode", ConditionOperator.Equal, (int)cmc_staffsurvey_statuscode.InProgress);
			validStaffSurveyListForDueDateFilter.AddCondition("cmc_duedate", ConditionOperator.On,
				DateTime.UtcNow.Date.AddDays(numberOfDays).ToString("yyyy-MM-dd") + "Z");

			queryExpression.Criteria.AddFilter(validStaffSurveyListForDueDateFilter);
			queryExpression.Criteria.AddFilter(validStaffSurveysForStartDateTodayFilter);
			var staffSurveyList = _orgService.RetrieveMultipleAll(queryExpression);
			return staffSurveyList.Entities.Count <= 0 ? null : staffSurveyList.Entities.Cast<cmc_staffsurvey>().ToList();
		}

		public Workflow GetEmailWorkFlow(string workflowName)
		{
			var fetch = $@" <fetch top = '1'> 
                        <entity name='workflow'>
                        <attribute name='workflowid'/>                        
                        <filter>
                        <condition attribute='name' operator='eq' value='{ workflowName}'/>               
                         <condition attribute='statuscode' operator='eq' value='2'/>
                         <condition attribute='type' operator='eq' value='1' />
                        </filter>                        
                    </entity>
                </fetch>";
			var workflow = _orgService.RetrieveMultiple(new FetchExpression(fetch));
			return workflow.Entities.Count <= 0 ? null : workflow.Entities.Cast<Workflow>().ToList().FirstOrDefault();
		}

		#endregion

		#region Update Overdue Staff Surveys
		/// <summary>
		/// Update the Survey which Crossed Duedate
		/// </summary>
		public void UpdateSurveysCrossedDuedate()
		{
			_logger.Trace($"Inside update function to change overdue Staffsurveys");
			var overdueStafffSurveys = GetOverdueStaffSurveys();
			if (overdueStafffSurveys?.Any() == true)
			{
				_logger.Trace($"Found {overdueStafffSurveys.Count} active StaffSurveys which crossed duedate.");
				foreach (var overdueSurvey in overdueStafffSurveys)
				{
					overdueSurvey.statuscode = cmc_staffsurvey_statuscode.Overdue;
				}

				ExecuteBulkEntities.BulkUpdateBatch(_orgService, overdueStafffSurveys.Cast<Entity>().ToList());
			}
			else
				_logger.Trace($"No Active StaffSurveys found which crossed duedate.");
		}
		/// <summary>
		/// Returns Survey which Crossed Duedate
		/// </summary>
		/// <returns>cmc_staffsurvey</returns>
		private List<cmc_staffsurvey> GetOverdueStaffSurveys()
		{
			_logger?.Trace("Fetching Active Staffsurveys which crossed duedate");
			var fetch = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                          <entity name='cmc_staffsurvey'>
                            <attribute name='cmc_staffsurveyid' />
                            <attribute name='cmc_duedate' />
                            <filter type='and'>
                            <condition attribute = 'cmc_duedate' operator='le' value= '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }' /> 
                              <condition attribute='statuscode' operator='in'>
                                <value>{(int)cmc_staffsurvey_statuscode.New}</value>
                                <value>{(int)cmc_staffsurvey_statuscode.InProgress}</value>
                              </condition>
                            </filter>
                          </entity>
                        </fetch>";
			var overDuestaffSurveys = _orgService.RetrieveMultipleAll(fetch);
			var data = overDuestaffSurveys.Entities.Count <= 0 ? null : overDuestaffSurveys.Entities.Cast<cmc_staffsurvey>().ToList();
			return data;
		}
		#endregion

	}
}
