using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Utilities;
using Cmc.Engage.Common.Utilities.Helpers;
using Cmc.Engage.Contracts;
using Cmc.Engage.Models;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.WebServiceClient;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;
using System.Security.Cryptography;
using System.Net;
using System.IO;
using static Cmc.Engage.Common.Utilities.Constants.Constants;

namespace Cmc.Engage.Common
{
    public class ContactService : IContactService
    {

        private readonly ILogger _logger;
        private IOrganizationService _orgService;
        private readonly IBingMapService _bingMapService;
        private readonly ILanguageService _retrieveMultiLingualValues;

        private readonly Guid _alumniRoleId = new Guid("3BB3FEDD-0333-E811-A95F-000D3A11FE32");

        private readonly IConfigurationService _retriveConfigurationDetails;
        public ContactService(ILogger tracer, IBingMapService bingMapService, IOrganizationService orgService, IConfigurationService retriveConfigurationDetails, ILanguageService retrieveMultiLingualValues)
        {
            _logger = tracer;
            _bingMapService = bingMapService;
            _retriveConfigurationDetails = retriveConfigurationDetails;
            _orgService = orgService;
            _retrieveMultiLingualValues = retrieveMultiLingualValues;
        }


        private readonly string[] _successPlanIgnoreFields =
        {
            "cmc_successplanname", "cmc_successplanid", "cmc_successplantemplateid"
        };

        private readonly string[] _toDoIgnoreFields =
        {
            "cmc_todoid", "cmc_successplanid", "cmc_successplantodotemplateid"
        };
        private static string AssignSuccessPlanDialogSuccessMessageForNoDuplicates =
            "AssignSuccessPlanDialog_SuccessMessageForNoDuplicates";

        private static string AssignSuccessPlanDialogSuccessMessageForAllDuplicates =
            "AssignSuccessPlanDialog_SuccessMessageForAllDuplicates";

        private static string AssignSuccessPlanDialogSuccessMessageForSomeDuplicates =
            "AssignSuccessPlanDialog_SuccessMessageForSomeDuplicates";

        private static string AssignSuccessPlanDialogSuccessMessageForSingleStudent =
            "AssignSuccessPlanDialog_SuccessMessageForSingleStudent";

        private static string AssignSuccessPlanDialogFailureMessageForSingleStudent =
            "AssignSuccessPlanDialog_FailureMessageForSingleStudent";


        #region Assign Success Plan
        public AssignSuccessPlanResponse AssignSuccessPlan(EntityReference studentId, EntityReference successPlanTemplateId)
        {
            _logger.Trace("Reading In RunActivity method of BussinessLogicWorkflowActivities new Framework demo");
            AssignSuccessPlanResponse result = null;
            try
            {
                _logger.Trace("Reading In CreateSuccessPlansForSelectedStudents method of BussinessLogicCommon new Framework demo");
                result = CreateSuccessPlansForSelectedStudents(successPlanTemplateId,
                    new EntityCollection(new List<Entity>
                    {
                    new Entity(studentId.LogicalName, studentId.Id)
                    }));
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw (ex);

            }

            return result;
        }
        #endregion

        #region Create Student Success Plan
        public void CreateSuccessPlansForSelectedStudent(Core.Xrm.ServerExtension.Core.IExecutionContext context)
        {
            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();
            //_orgService = serviceProvider.CreateOrganizationServiceAsCurrentUser();
            var target = pluginContext.GetInputParameter<EntityReference>("Target");
            var students = pluginContext.GetInputParameter<EntityCollection>("Students");
            var result = CreateSuccessPlansForSelectedStudents(target, students);
            _logger.Trace("Reading output" + JsonConvert.SerializeObject(result));
            pluginContext.OutputParameters["SuccessPlanResponse"] = JsonConvert.SerializeObject(result);
        }

        public AssignSuccessPlanResponse CreateSuccessPlansForSelectedStudents(EntityReference successPlanTemplateId, EntityCollection students)
        {
            _logger?.Trace("Create SuccessPlans For SelectedStudents...");

            var validstudents = new EntityCollection();
            var assignSuccessPlanDialogModel = new AssignSuccessPlanResponse();
            if (students == null || students.Entities == null || !students.Entities.Any()) return assignSuccessPlanDialogModel;

            var successPlanTemplate = _orgService.Retrieve(successPlanTemplateId, new ColumnSet(true));
            var succesPlanTemplateName = successPlanTemplate.GetAttributeValue<string>("cmc_successplantemplatename");
            _logger?.Trace($"Creating Success Plans using the Success Plan Template '{succesPlanTemplateName}'.");
            var studentsHavingSameSuccessPlanTemplate =
                GetAllStudentsHavingSameSuccessPlanTemplate(successPlanTemplateId.Id,
                    students.Entities.Select(a => a.Id).ToList());

            if (studentsHavingSameSuccessPlanTemplate == null)
                validstudents = students;

            else
            {
                foreach (var student in students.Entities)
                {
                    //checking same success template already assign or not for student

                    if (studentsHavingSameSuccessPlanTemplate.ContainsKey(student.Id))
                    {
                        if (studentsHavingSameSuccessPlanTemplate.TryGetValue(student.Id, out var fullname))
                        {
                            _logger?.Trace($"Assigning SuccessPlan to '{fullname}'.");
                            assignSuccessPlanDialogModel.DuplicateList.Add(fullname);
                        }
                    }
                    else
                    {
                        validstudents.Entities.Add(student);
                    }
                    // end
                }
            }

            assignSuccessPlanDialogModel.TotalCount = students.Entities.Count;

            if (validstudents.Entities.Count > 0)
            {
                assignSuccessPlanDialogModel.SuccessPlanIds = CreateSuccessPlans(successPlanTemplateId, validstudents, assignSuccessPlanDialogModel);
            }


            if (students.Entities.Count == 1)

                assignSuccessPlanDialogModel.AssignSuccessPlanDialogMessage = assignSuccessPlanDialogModel.DuplicateList.Count == 0 ? AssignSuccessPlanDialogSuccessMessageForSingleStudent : AssignSuccessPlanDialogFailureMessageForSingleStudent;

            else
            {
                if (assignSuccessPlanDialogModel.DuplicateList.Count == 0)
                    assignSuccessPlanDialogModel.AssignSuccessPlanDialogMessage =
                        AssignSuccessPlanDialogSuccessMessageForNoDuplicates;
                else if (assignSuccessPlanDialogModel.DuplicateList.Count == students.Entities.Count)
                    assignSuccessPlanDialogModel.AssignSuccessPlanDialogMessage =
                        AssignSuccessPlanDialogSuccessMessageForAllDuplicates;
                else
                    assignSuccessPlanDialogModel.AssignSuccessPlanDialogMessage =
                        AssignSuccessPlanDialogSuccessMessageForSomeDuplicates;
            }

            return assignSuccessPlanDialogModel;
        }


        public List<EntityReference> CreateSuccessPlans(EntityReference successPlanTemplateId, EntityCollection students, AssignSuccessPlanResponse assignSuccessPlanDialogModel)
        {
            var successPlanIds = new List<EntityReference>();
            _logger?.Trace("Retrieving Success Plan");
            var successPlanTemplate = _orgService.Retrieve(successPlanTemplateId, new ColumnSet(true));

            var successPlanToDoTemplates = RetrieveSuccessPlanToDoTemplates(successPlanTemplateId);
            // get the current academic periods for the students.
            var studentsCurrentAcademicPeriodForStudents = GetCurrentAcademicPeriodForStudents(students.Entities); // to calculate the due date for the to do task.

            // get update list of the students, to create the success plan.
            students = GetValidStudentsAssociatedToAcademicPeriod(students, successPlanToDoTemplates, studentsCurrentAcademicPeriodForStudents, assignSuccessPlanDialogModel);

            if (students?.Entities?.Count > 0)
            {
                _logger?.Trace("Retrieving fields to copy");
                var studentSuccessPlanFields = CloneEntityCommon.RetrieveUpdateableFields(cmc_successplan.EntityLogicalName, _successPlanIgnoreFields, _orgService, _logger);
                var toDoFields = CloneEntityCommon.RetrieveUpdateableFields(
                    cmc_todo.EntityLogicalName, _toDoIgnoreFields,
                    _orgService, _logger);
                _logger?.Trace("Creating Student Success Plans for all Students");
                foreach (var student in students.Entities)
                {
                    var studentName = studentsCurrentAcademicPeriodForStudents?.FirstOrDefault(r => r.Key.Equals(student.Id)).Value?.Item1;
                    var studentId = student.ToEntityReference();
                    _logger?.Trace($"Creating Student Success Plan for student '{studentName}' and successplantemplate name is '{successPlanTemplateId.Name}'");

                    var successPlan =
                        CloneEntityCommon.CloneEntity<cmc_successplan>(successPlanTemplate, studentSuccessPlanFields);
                    successPlan.cmc_assignedtoid = studentId;
                    successPlan.cmc_successplantemplateid = successPlanTemplate.ToEntityReference();

                    _logger?.Trace(successPlan.LogicalName);
                    successPlan.Id = _orgService.Create(successPlan);
                    var studentSuccessPlanId = successPlan.ToEntityReference();


                    _logger?.Trace($"Creating To Dos for student '{studentName}'");
                    foreach (var successPlanToDoTemplate in successPlanToDoTemplates.Entities)
                    {
                        var toDo = CloneEntityCommon.CloneEntity<cmc_todo>(successPlanToDoTemplate, toDoFields);
                        var isDueDateComputed = true;
                        toDo.cmc_assignedtostudentid = student.ToEntityReference();
                        toDo.cmc_successplanid = studentSuccessPlanId;
                        toDo.cmc_todoname = successPlanToDoTemplate.GetAttributeValue<string>("cmc_successplantodotemplatename");
                        toDo.cmc_successplantodotemplateid = successPlanToDoTemplate.ToEntityReference();
                        _logger?.Trace($"conversion successPlanToDoTemplate entity reference to Entity");
                        var successPlanEntity = successPlanToDoTemplate.ToEntity<cmc_successplantodotemplate>();
                        _logger?.Trace($"section for calculation of due date for {successPlanEntity.cmc_duedatecalculationtype.ToString()} for the todo template {toDo.cmc_todoname}");
                        switch (successPlanEntity.cmc_duedatecalculationtype)
                        {
                            case cmc_successplantodotemplate_cmc_duedatecalculationtype.Calculated:
                                _logger?.Trace($"calculation of due date -> field name :{successPlanEntity.cmc_duedatecalculationfield.ToString()}");

                                var duedays = (successPlanEntity.cmc_duedatenumberofdays.HasValue
                                               && successPlanEntity.cmc_duedatenumberofdays.Value > 0) ? successPlanEntity.cmc_duedatenumberofdays.Value : 0;

                                DateTime? dueDate = null;
                                switch (successPlanEntity.cmc_duedatecalculationfield)
                                {
                                    case cmc_successplantodotemplate_cmc_duedatecalculationfield.StartofAcademicPeriod:
                                        var cmcStartdate = studentsCurrentAcademicPeriodForStudents[studentId.Id].Item2; // get from the list
                                        if (cmcStartdate != null)
                                            dueDate = cmcStartdate.Value;
                                        else
                                        {
                                            isDueDateComputed = false;
                                            _logger?.Trace(
                                                $"calculation of due date -> start of academic period is value is empty/null for {studentName} and to do name is {toDo.cmc_todoname}.");
                                        }

                                        break;
                                    case cmc_successplantodotemplate_cmc_duedatecalculationfield.EndofAcademicPeriod:
                                        var cmcEnddate = studentsCurrentAcademicPeriodForStudents[studentId.Id].Item3; // get from the list
                                        if (cmcEnddate != null)
                                            dueDate = cmcEnddate.Value;
                                        else
                                        {
                                            isDueDateComputed = false;
                                            _logger?.Trace($"calculation of due date -> end of academic period is value is empty/null for {studentName} and to do name is {toDo.cmc_todoname}.");
                                        }
                                        break;
                                    case cmc_successplantodotemplate_cmc_duedatecalculationfield.AssignmentDate:
                                        dueDate = DateTime.Now;
                                        break;
                                }

                                // add the days.
                                switch (successPlanEntity.cmc_duedatedaysdirection)
                                {
                                    case cmc_successplantodotemplate_cmc_duedatedaysdirection.After:
                                        dueDate = dueDate?.AddDays(duedays); // adding the days
                                        break;
                                    case cmc_successplantodotemplate_cmc_duedatedaysdirection.Before:
                                        dueDate = dueDate?.AddDays(-duedays); // substracting the days
                                        break;
                                }
                                _logger?.Trace($"calculation of due date -> computed due date is  :{dueDate?.ToShortDateString()} and due date is computed properly '{isDueDateComputed}'");
                                toDo.cmc_duedate = isDueDateComputed ? dueDate : null;
                                break;
                            case cmc_successplantodotemplate_cmc_duedatecalculationtype.Static:
                                _logger?.Trace($"calculation of due date -> static due date set to the form.");
                                toDo.cmc_duedate = successPlanEntity.cmc_duedatestatic;
                                break;
                        }

                        _orgService.Create(toDo);
                    }

                    successPlanIds.Add(studentSuccessPlanId);
                }
            }

            return successPlanIds;
        }

        /// <summary>
        /// uses to return the valid student information to create the success plan to do from the temaplates. 
        /// </summary>
        /// <param name="students">students</param>
        /// <param name="successPlanToDoTemplates">to do templates</param>
        /// <param name="studentsCurrentAcademicPeriodForStudents">students with academic periods</param>
        /// <param name="assignSuccessPlanDialogModel">response to return back with some details.</param>
        /// <returns>updated students with academic period.</returns>
        private EntityCollection GetValidStudentsAssociatedToAcademicPeriod(EntityCollection students, EntityCollection successPlanToDoTemplates, Dictionary<Guid, Tuple<string, DateTime?, DateTime?>> studentsCurrentAcademicPeriodForStudents, AssignSuccessPlanResponse assignSuccessPlanDialogModel)
        {
            _logger?.Trace($"GetValidStudentsAssociatedToAcademicPeriod : entered with students count is {students.Entities.Count} ");
            if (assignSuccessPlanDialogModel?.DuplicateList != null)
            {
                _logger?.Trace($"DuplicateList : Duplicate students count is {assignSuccessPlanDialogModel.DuplicateList?.Count} ");
            }

            if (successPlanToDoTemplates.Entities.Cast<cmc_successplantodotemplate>().Any(tm => tm.cmc_duedatecalculationtype == cmc_successplantodotemplate_cmc_duedatecalculationtype.Calculated &&
                                                                                                (tm.cmc_duedatecalculationfield == cmc_successplantodotemplate_cmc_duedatecalculationfield.StartofAcademicPeriod ||
                                                                                                tm.cmc_duedatecalculationfield == cmc_successplantodotemplate_cmc_duedatecalculationfield.EndofAcademicPeriod)))
            {
                _logger?.Trace($"GetValidStudentsAssociatedToAcademicPeriod : some to do tasks are dependent on academic period.");

                var studentsNoAcademicPeriod = studentsCurrentAcademicPeriodForStudents.Where(r => !(r.Value.Item2.HasValue && r.Value.Item3.HasValue)).ToArray();
                // if all the students not set with academic period
                if (studentsNoAcademicPeriod != null && studentsNoAcademicPeriod.Any())
                {
                    // get all not valid users 
                    if (assignSuccessPlanDialogModel != null) // send the screen to invalid users list. 
                    {
                        assignSuccessPlanDialogModel.StudentNamesWithoutAcademicPeriod = new List<string>(students.Entities.Where(r => studentsNoAcademicPeriod.Select(p => p.Key).Contains(r.Id)).SelectMany(r => studentsNoAcademicPeriod.Where(p => p.Key == r.Id).Select(p => p.Value.Item1))); // create the list with append the names of not valid users.
                        _logger?.Trace($"GetValidStudentsAssociatedToAcademicPeriod : number of students not associated with academic period {assignSuccessPlanDialogModel.StudentNamesWithoutAcademicPeriod?.Count}.");
                    }
                    students = new EntityCollection(students.Entities.Where(r => !studentsNoAcademicPeriod.Select(p => p.Key).Contains(r.Id)).ToList()); // modify the valid students to associate the successplan.
                }
                else
                {
                    _logger?.Trace($"GetValidStudentsAssociatedToAcademicPeriod : All students are associated with accedemic periods. ");
                }
            }

            _logger?.Trace($"GetValidStudentsAssociatedToAcademicPeriod : returned valid students associated with academic period {students.Entities.Count}.");

            return students;
        }

        /// <summary>
        /// to retrive the current academic year for all the students , this plugin is called in azure functions. to avoid multiple calls to service, 
        /// fetching all the students and adding to dictionary.
        /// </summary>
        /// <param name="studentCollection">collection of the students.</param>
        /// <returns>academic period</returns>
        private Dictionary<Guid, Tuple<string, DateTime?, DateTime?>> GetCurrentAcademicPeriodForStudents(DataCollection<Entity> studentCollection)
        {
            _logger?.Trace($"GetCurrentAcademicPeriodForStudents for number of students is {studentCollection.Count} ");
            var sbCondition = new StringBuilder();
            // loop through all student records and add the conditions
            studentCollection.ToList().ForEach(student => sbCondition.Append($"<condition attribute='contactid' operator='eq' value='{student.Id}' />"));
            _logger?.Trace($"GetCurrentAcademicPeriodForStudents:  students filter conditions {sbCondition} ");
            // get the associated Academic period.
            var fetchAcademicPeriod = $@"<fetch>
                                            <entity name='contact'>
                                                <attribute name='contactid' />
                                                <attribute name='fullname' />
                                                <filter type='or'>
                                                  {sbCondition}
                                                </filter>
                                                <link-entity name='mshied_academicperiod' from='mshied_academicperiodid' to='mshied_currentacademicperiodid' visible='false' link-type='outer' alias='ap'>
                                                  <attribute name='mshied_startdate' />
                                                  <attribute name='mshied_enddate' />
                                                </link-entity>
                                              </entity>
                                         </fetch>";


            var studentsAcademicPeriod = _orgService.RetrieveMultipleAll(fetchAcademicPeriod)?.Entities;
            var dicStudentAcademicPeriods = studentsAcademicPeriod?.Cast<Contact>()?.ToDictionary(r => r.Id, r => new Tuple<string, DateTime?, DateTime?>(r.FullName, r.GetAliasedAttributeValue<DateTime?>("ap.mshied_startdate"), r.GetAliasedAttributeValue<DateTime?>("ap.mshied_enddate")));
            _logger?.Trace($"GetCurrentAcademicPeriodForStudents:  students count after the execution of fetch xml is {dicStudentAcademicPeriods?.Count}.");
            return dicStudentAcademicPeriods;
        }

        private EntityCollection RetrieveSuccessPlanToDoTemplates(EntityReference successPlanTemplateId)
        {
            _logger?.Trace("Retrieving Success Plan To Do Templates");

            return _orgService.RetrieveMultipleAll($@"
                <fetch count='5000' aggregate='false' distinct='false' mapping='logical'>
                  <entity name='cmc_successplantodotemplate'>
                    <all-attributes />
                    <filter>
                      <condition attribute='cmc_successplantemplateid' operator='eq' value='{successPlanTemplateId.Id}' />
                    </filter>
                  </entity>
                </fetch>");
        }

        public Dictionary<Guid, string> GetAllStudentsHavingSameSuccessPlanTemplate(Guid successPlanTemplateId, List<Guid> studentIds)
        {
            _logger?.Trace("Get All Students Having SameSuccessPlan Template");
            if (studentIds == null || !studentIds.Any()) return null;

            var fetch = $@"<fetch distinct='true'>
               <entity name='contact'>            
                 <attribute name='fullname'/>
                 <attribute name='contactid'/>    
                 <filter type='and'>
                   <condition attribute='contactid' operator='in'>
                     <value>{string.Join(" </value><value> ", studentIds)}</value>
                    </condition>
                    </filter>
                    <link-entity name = 'cmc_successplan' from = 'cmc_assignedtoid' to = 'contactid' >
                    <link-entity name = 'cmc_successplantemplate' from = 'cmc_successplantemplateid' to = 'cmc_successplantemplateid' >
                    <filter type='and'>
                    <condition attribute = 'cmc_successplantemplateid' operator= 'eq' value = '{successPlanTemplateId}'/>
               </filter>
            </link-entity>
                </link-entity>
                </entity>
                </fetch>";

            var data = _orgService.RetrieveMultipleAll(fetch);
            if (data?.Entities == null || data.Entities.Count <= 0) return null;
            return data.Entities.Cast<Contact>().ToList().ToDictionary(a => a.Id, a => a.FullName);
        }
        #endregion


        #region ComputeMilesFromHome      
        public void ComputeMilesFromHome(IExecutionContext context)
        {
            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();
            //_orgService = serviceProvider.CreateOrganizationServiceAsCurrentUser();
            if (!pluginContext.IsValidCall("contact"))
                throw new InvalidPluginExecutionException(_retrieveMultiLingualValues.Get("PluginIsNotConfiguredcorrectly"));
            var preImage = pluginContext.GetPreEntityImage<Contact>("PreImage");
            var postImage = pluginContext.GetPostEntityImage<Contact>("PostImage");
            UpdateMilesFromHome(preImage, postImage, context);
        }



        private void UpdateMilesFromHome(Contact preContact, Contact contact, IExecutionContext context)
        {
            _logger.Trace("Update Miles From Home Method called");

            if (preContact.Address1_Latitude != null)
                _logger.Trace("Old Latitude" + preContact.Address1_Latitude.Value);
            else _logger.Trace("Old Latitude is null");

            if (preContact.Address1_Longitude != null)
                _logger.Trace("Old Longitude" + preContact.Address1_Longitude.Value);
            else _logger.Trace("Old Longitude is null");

            if (contact.Address1_Latitude != null)
                _logger.Trace("New Latitude" + contact.Address1_Latitude.Value);
            else _logger.Trace("New Latitude is null");

            if (contact.Address1_Longitude != null)
                _logger.Trace("New Longitude" + contact.Address1_Longitude.Value);
            else _logger.Trace("New Longitude is null");

            if (preContact.ContactId == null || contact.ContactId == null) return;
            if (!IsCurrentCampusOrLatitudeOrLongitudeChanged(preContact, contact)) return;
            var latitudeLongitudeForCampus = GetLatitudeLongitudeForCampus(contact.Id);
            if (latitudeLongitudeForCampus == null || contact.Address1_Latitude == null || contact.Address1_Longitude == null)
            {
                _logger.Trace("New distance is null ");
                _orgService.Update(new Contact
                {
                    ContactId = contact.Id,
                    cmc_milesfromcampus = null
                });
                return;
            }
            var distance = CalculateDistanceInMiles(contact.Address1_Latitude.Value, contact.Address1_Longitude.Value,
                latitudeLongitudeForCampus.Latitude, latitudeLongitudeForCampus.Longitude, context);

            if (preContact.cmc_milesfromcampus != null)
                _logger.Trace("Old distance was " + preContact.cmc_milesfromcampus.Value);

            if (preContact.cmc_milesfromcampus.Equals((decimal)distance)) return;
            _logger.Trace("New distance  " + distance);
            var updateContact = new Contact
            {
                ContactId = contact.Id,
                cmc_milesfromcampus = (decimal)distance
            };
            _orgService.Update(updateContact);
        }


        private bool IsCurrentCampusOrLatitudeOrLongitudeChanged(Contact preContact, Contact contact)
        {
            _logger.Trace("Is Current Campus Or Latitude Or LongitudeChanged Method called");
            var currentCampusId = contact.ParentCustomerId?.Id;
            var preCurrentCampusId = preContact.ParentCustomerId?.Id;

            return !currentCampusId.Equals(preCurrentCampusId) ||
                   !contact.Address1_Latitude.Equals(preContact.Address1_Latitude) ||
                   !contact.Address1_Longitude.Equals(preContact.Address1_Longitude);
        }

        private double CalculateDistanceInMiles(double sourceLatitude, double sourceLongitude,
            double destinationLatitude,
            double destinationLongitude, IExecutionContext context)
        {
            _logger.Trace("Calculate Distance In Miles Method called");

            var distanceInMilesUsingBingMap =
                _bingMapService.GetDistanceInMiles(sourceLatitude, sourceLongitude, destinationLatitude,
                    destinationLongitude, context);
            if (distanceInMilesUsingBingMap == null || distanceInMilesUsingBingMap.Value <= 0)
                return CalculateDistanceInMilesUsingPointToPointCalculation(sourceLatitude, sourceLongitude,
                    destinationLatitude, destinationLongitude);
            _logger.Trace("distance using Bingmap " + distanceInMilesUsingBingMap.Value);
            return distanceInMilesUsingBingMap.Value;
        }

        private double CalculateDistanceInMilesUsingPointToPointCalculation(double sourceLatitude,
            double sourceLongitude,
            double destinationLatitude,
            double destinationLongitude)
        {
            _logger.Trace("Calculate Distance In Miles Using PointToPoint Calculation Method called");
            var sourceGeoCoordinate = new GeoCoordinate
            {
                Latitude = sourceLatitude,
                Longitude = sourceLongitude
            };
            var destinationGeoCoordinate = new GeoCoordinate
            {
                Latitude = destinationLatitude,
                Longitude = destinationLongitude
            };

            _logger.Trace("distance using PointToPointCalculation in Miles " +
                           ConvertMetersToMiles(sourceGeoCoordinate.GetDistanceTo(destinationGeoCoordinate)));
            return ConvertMetersToMiles(sourceGeoCoordinate.GetDistanceTo(destinationGeoCoordinate));
        }

        private double ConvertMetersToMiles(double meters)
        {
            _logger.Trace("distance using PointToPointCalculation in meters " + meters);
            return meters / 1609.344;
        }

        private LatitudeLongitude GetLatitudeLongitudeForCampus(Guid contactId)
        {
            _logger.Trace("Get LatitudeLongitude For Campus Method called");

            var fetch =
                $@"
  <fetch distinct='false' mapping='logical' output-format='xml-platform' version='1.0' top='1'>						
    <entity name='account'>                      
      <attribute name='address1_latitude' />  
      <attribute name='address1_longitude' />
      <link-entity name='contact' link-type='inner' to='accountid' from='parentcustomerid'>
        <filter type='and'>  
          <condition attribute='contactid' operator='eq' value='{contactId}'/>                     
        </filter>
      </link-entity>
    </entity>
  </fetch>";

            var keyDetails = _orgService.RetrieveMultiple(new FetchExpression(fetch));

            var data = keyDetails.Entities.Count <= 0 ? null : keyDetails.Entities.First().ToEntity<Account>();
            if (data?.Address1_Latitude == null || data.Address1_Longitude == null)
                return null;

            _logger.Trace("Campus Latitude" + data.Address1_Latitude.Value);
            _logger.Trace("Campus Longitude" + data.Address1_Longitude.Value);
            return new LatitudeLongitude
            {
                Latitude = data.Address1_Latitude.Value,
                Longitude = data.Address1_Longitude.Value
            };
        }

        #endregion

        #region CreateUpdateStudentOwner

        public void CreateUpdateStudentOwner(IExecutionContext context)
        {
            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();

            _logger.Trace("Start CreateUpdateStudentOwner Plugin. Test");

            var target = pluginContext.GetInputParameter<Entity>("Target").ToEntity<Contact>();
            Contact preImage = null;

            if (pluginContext.PostEntityImages.Contains("PostImage"))
            {
                target = pluginContext.PostEntityImages["PostImage"].ToEntity<Contact>();
            }
            if (pluginContext.PreEntityImages.Contains("PreImage"))
            {
                preImage = pluginContext.PreEntityImages["PreImage"].ToEntity<Contact>();
            }

            CreateUpdateRelatedSuccessNetworkRecords(target, preImage);

            _logger.Trace("End CreateUpdateStudentOwner Plugin.");
        }



        private void CreateUpdateRelatedSuccessNetworkRecords(Contact contact, Contact preImage)
        {
            // on create
            if (preImage == null)
            {
                if (IsStudentType(contact))
                {
                    CreateContactOwnerAsAdvisorOnStudentSuccessNetworkRecord(contact);
                }
                else
                {
                    _logger.Trace("Contact must be Student to create an associated Student Success Network.");
                }
            }
            // on update
            else
            {   // was student
                if (IsStudentType(preImage))
                {
                    // student to non-student
                    if (!IsStudentType(contact))
                    {
                        DeactivateContactOwnerAsAdvisorOnStudentSuccessNetworkRecord(contact, preImage);
                    }
                    // student to student
                    else
                    {
                        if (contact.OwnerId?.Id != preImage.OwnerId?.Id)
                        {
                            DeactivateContactOwnerAsAdvisorOnStudentSuccessNetworkRecord(contact, preImage);
                            CreateContactOwnerAsAdvisorOnStudentSuccessNetworkRecord(contact);
                        }
                        else
                        {
                            _logger.Trace("Unhandled edge case on Student found.");
                        }
                    }
                }
                // was non-student
                else
                {
                    // non-student to student
                    if (IsStudentType(contact))
                    {
                        CreateContactOwnerAsAdvisorOnStudentSuccessNetworkRecord(contact);
                    }
                    else
                    {
                        _logger.Trace("Contact must be Student to create an associated Student Success Network.");
                    }
                }
            }
        }

        private void CreateContactOwnerAsAdvisorOnStudentSuccessNetworkRecord(Contact contact)
        {
            _logger.Trace("Creating new associated Student Success Network.");

            if (contact.OwnerId?.LogicalName != SystemUser.EntityLogicalName)
            {
                _logger.Trace($"The Owner of the Contact is a {contact.OwnerId?.LogicalName} rather than a User. A Student Success Network will not be created.");
                return;
            }

            var studentSuccessNetwork = new cmc_successnetwork();

            studentSuccessNetwork.cmc_staffmemberid = contact.OwnerId;
            studentSuccessNetwork.cmc_studentid = new EntityReference(contact.LogicalName, contact.Id);        
            studentSuccessNetwork.cmc_staffroleid = GetAdvisorRoleEntityReference();
            try
            {
                studentSuccessNetwork.Id = _orgService.Create(studentSuccessNetwork);
            }
            catch (Exception ex)
            {
                _logger.Error($@"Creating associated Student Success Network produced the following error: {ex}");
                return;
            }
            _logger.Trace($@"Successfully created Student Success Network with Id {studentSuccessNetwork.Id}.");
        }

        private EntityReference GetAdvisorRoleEntityReference()
        {
            _logger.Trace($"Inside Function Get Advisor Role EntityReference");
            const string fetchXml = @"
            <fetch>
              <entity name='cmc_title'>
                <attribute name='cmc_titleid' />
                <filter type='and'>
                  <condition attribute='cmc_titlename' operator='eq' value='Advisor'/>
                </filter>
              </entity>
            </fetch>";

            var advisoryRoleDetails = _orgService.RetrieveMultipleAll(fetchXml);
            return advisoryRoleDetails.Entities.Count <= 0 ? null : advisoryRoleDetails.Entities.Cast<cmc_title>().ToList().FirstOrDefault()?.ToEntityReference();
        }

        private void DeactivateContactOwnerAsAdvisorOnStudentSuccessNetworkRecord(Contact contact, Contact preImage)
        {
            _logger.Trace("Student OwnerId changed," +
                $@"deactivating the last related Student Success Network associated with the previous OwnerId {preImage.OwnerId.Id}.");

            if (preImage.OwnerId.LogicalName != SystemUser.EntityLogicalName)
            {
                _logger.Trace($"Previous Owner was a {preImage.OwnerId.LogicalName} rather than a User. No Student Success Network will be deactivated.");
                return;
            }

            var fetchLastRelatedActiveOwnerAsAdvisorStudentSuccessNetworkXml = $@"
                <fetch>
                  <entity name='cmc_successnetwork'>
                    <attribute name='cmc_successnetworkid' />
                    <filter type='and'>
                      <condition attribute='cmc_studentid' operator='eq' value='{preImage.Id}' />
                      <condition attribute='cmc_staffmemberid' operator='eq' value='{preImage.OwnerId.Id}' />
                    </filter>
                    <link-entity name='cmc_title' from='cmc_titleid' to='cmc_staffroleid' alias='ab'>
                        <filter type='and'>
                            <condition attribute='cmc_titlename' operator='eq' value='Advisor' />
                        </filter>
                    </link-entity> 
                  </entity>
                </fetch>";
            IEnumerable<Entity> lastActiveStudentSuccessNetworkResult;

            _logger.Trace($"Attempting to fetch last active associated Student Success Network record.");
            try
            {
                lastActiveStudentSuccessNetworkResult =
                    _orgService.RetrieveMultipleAll(fetchLastRelatedActiveOwnerAsAdvisorStudentSuccessNetworkXml).Entities;
            }
            catch (Exception ex)
            {
                _logger.Error($"Fetch of last active associated Student Success Network record has produced the following error: {ex}");
                return;
            }

            foreach (var studentSuccessNetwork in lastActiveStudentSuccessNetworkResult)
            {
                try
                {
                    _orgService.Update(new cmc_successnetwork()
                    {
                        Id = studentSuccessNetwork.Id,
                        statecode = cmc_successnetworkState.Inactive,
                        statuscode = cmc_successnetwork_statuscode.Inactive
                    });
                }
                catch (Exception ex)
                {
                    _logger.Error($"Update of last active associated Student Success Network record has produced the following error: {ex}");
                    return;
                }
            }
        }

        private  bool IsStudentType(Contact entity)
        {
            return entity.cmc_isstudent == null ? false : (bool)entity.cmc_isstudent;
        }
        /// <summary>
        /// Set the the isStudent Flag if student option is selected in Contact type Option and vice versa.
        /// </summary>
        /// <param name="executionContext"></param>
        public void SetStudentFlag(IExecutionContext executionContext)
        {
            _logger.Trace($"Inside Function To set Student Flag");
            var serviceProvider = executionContext.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();

            Contact entity = pluginContext.GetTargetEntity<Contact>();
            bool isStudentSelected = false, isstudent = false;bool? flagValue = null;
            OptionSetValueCollection contactTypes = (OptionSetValueCollection)entity.mshied_ContactType;

            /*on Update Step system will automatically passes the field which are updated in the current context.
            If Field is not updated that field will not be available in context 
            below code returns if no change made to Contacttype hence no update required for the boolen field.
            In case of create Scenario Default 'NO' value will be set.
             */
            if (contactTypes == null){_logger.Trace($"No changes made to ContactType option terminating the process "); return;}

            if (pluginContext.MessageName.ToLower() == "update")
            {
                var preImage = pluginContext.GetPreEntityImage<Contact>("PreImage");
                isstudent = (bool)preImage.cmc_isstudent;
            }

            _logger.Trace($"Contact Entity ID : {entity.Id}");

            isStudentSelected = contactTypes.Any(x => x.Value == (int)ContactType.Student);

            _logger.Trace($" Is Student Option Selected: {isStudentSelected}");
            
            if (!isStudentSelected && isstudent)
            {
                entity.cmc_isstudent = false;
                flagValue = false;
            }
            else if (isStudentSelected && !isstudent)
            {
                entity.cmc_isstudent = true;
                flagValue = true;
            }
            
                _logger.Trace( flagValue == null ? "No Change in flag value" :"Flag Value set to {0}", flagValue);

            _logger.Trace($"End of Function To set Student Flag");
        }
        #endregion

        #region SetLegacyStudent

        public void SetLegacyStudent(IExecutionContext context)
        {
            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();

            if (pluginContext.MessageName.ToLower() != "delete" && !pluginContext.IsValidCall(Connection.EntityLogicalName))
            {
                throw new InvalidPluginExecutionException(_retrieveMultiLingualValues.Get("PluginIsNotConfiguredcorrectly"));
            }

            Connection preImage  = null;
            if (pluginContext.MessageName.ToLower() != "create")
            {
                preImage = pluginContext.GetPreEntityImage<Connection>("Target");
            }

            if (pluginContext.MessageName.ToLower() == "create")
            {
                var target = pluginContext.GetTargetEntity<Connection>();
                HandleSetLegacyStudentOnCreate(target);
            }
            else if (pluginContext.MessageName.ToLower() == "update")
            {
                var postImage = pluginContext.GetPostEntityImage<Connection>("Target");
                HandleSetLegacyStudentOnUpdate(preImage, postImage);
            }
            else if (pluginContext.MessageName.ToLower() == "delete")
            {
                HandleSetLegacyStudentOnDelete(preImage);
            }
        }

        public void HandleSetLegacyStudentOnCreate(Connection connection)
        {
            _logger.Trace($"Start {nameof(HandleSetLegacyStudentOnCreate)}.");

            if (IsLegacyCheckDisabled())
            {
                _logger.Trace($"{_retriveConfigurationDetails.GetActiveConfiguration().cmc_stoplegacycheck} is set to true. Legacy will not be checked.");
                return;
            }
            HandleContactLegacyFlags(connection, true);

            _logger.Trace($"End {nameof(HandleSetLegacyStudentOnCreate)}.");
        }

        public void HandleSetLegacyStudentOnDelete(Connection connection)
        {
            _logger.Trace($"Start {nameof(HandleSetLegacyStudentOnDelete)}.");

            if (IsLegacyCheckDisabled())
            {
                _logger.Trace($"{_retriveConfigurationDetails.GetActiveConfiguration().cmc_stoplegacycheck} is set to true. Legacy will not be checked.");
                return;
            }

            if (connection.StateCode == ConnectionState.Inactive)
            {
                _logger.Trace("Connection is inactive. The Legacy flag will not be unchecked.");
                return;
            }

            HandleContactLegacyFlags(connection, false);

            _logger.Trace($"End {nameof(HandleSetLegacyStudentOnDelete)}.");
        }

        public void HandleSetLegacyStudentOnUpdate(Connection prevConnection, Connection updatedConnection)
        {
            var connectedTo = updatedConnection.Record2Id;
            var connectedFrom = updatedConnection.Record1Id;
            _logger.Trace($"Start {nameof(HandleSetLegacyStudentOnUpdate)}.");

            if (IsLegacyCheckDisabled())
            {
                _logger.Trace($"{_retriveConfigurationDetails.GetActiveConfiguration().cmc_stoplegacycheck} is set to true. Legacy will not be checked.");
                return;
            }

            if ((prevConnection.Record2Id?.Id == connectedTo?.Id &&
                 prevConnection.Record1Id?.Id == connectedFrom?.Id) &&
                 (prevConnection.Record2RoleId?.Id == updatedConnection.Record2RoleId?.Id &&
                 prevConnection.Record1RoleId?.Id == updatedConnection.Record1RoleId?.Id) &&
                 (prevConnection.StateCode.GetValueOrDefault() == updatedConnection.StateCode.GetValueOrDefault()))
            {
                _logger.Trace("Conditions to update Contact as Legacy have not been met. Exiting.");
                return;
            }

            if (prevConnection.StateCode.Value == ConnectionState.Active &&
                updatedConnection.StateCode.Value == ConnectionState.Inactive)
            {
                _logger.Trace("Connection has been deactivated, handling legacy flags...");

                // Use the PreImage just in case other attributes on the Connection changed
                HandleContactLegacyFlags(prevConnection, false, true);
                return;
            }
            else if (prevConnection.StateCode.Value == ConnectionState.Inactive &&
                updatedConnection.StateCode.Value == ConnectionState.Active)
            {
                _logger.Trace("Connection has been activated, handling legacy flags...");

                HandleContactLegacyFlags(updatedConnection, true, true);
                return;
            }
            else if (prevConnection.StateCode.Value == ConnectionState.Inactive &&
                updatedConnection.StateCode.Value == ConnectionState.Inactive)
            {
                _logger.Trace("Connection is inactive, no legacy flags will be handled...");
                return;
            }

            // If the records change, treat this as a delete and a create
            if (prevConnection.Record1Id?.Id != connectedFrom?.Id ||
                prevConnection.Record2Id?.Id != connectedTo?.Id)
            {
                _logger.Trace("Connect From or Connected To changed, handling Legacy flags on the old and new values...");
                HandleContactLegacyFlags(prevConnection, false, true);
                HandleContactLegacyFlags(updatedConnection, true, true);
                return;
            }

            var isNonAlumiRoleToAlumniRole = IsNonAlumiRoleToAlumniRole(prevConnection, updatedConnection);
            if (isNonAlumiRoleToAlumniRole)
            {
                _logger.Trace("Connection role was updated from another role to Alumni role, handling legacy flags...");

                HandleContactLegacyFlags(updatedConnection, true, true);
                return;
            }

            var isAlumniRoleToNonAlumniRole = IsAlumniRoleToNonAlumniRole(prevConnection, updatedConnection);
            if (isAlumniRoleToNonAlumniRole)
            {
                _logger.Trace("Connection role was updated from Alumni role to another role, handling legacy flags...");

                HandleContactLegacyFlags(prevConnection, false, true);
                return;
            }

            var isNonFamilyRoleToFamilyRole = IsNonFamilyRoleToFamilyRole(prevConnection, updatedConnection);
            if (isNonFamilyRoleToFamilyRole)
            {
                _logger.Trace("Connection role was updated from non-Family role to a Family role, handling legacy flags...");

                HandleContactLegacyFlags(updatedConnection, true, true);
                return;
            }

            var isFamilyRoleToNonFamilyRole = IsFamilyRoleToNonFamilyRole(prevConnection, updatedConnection);
            if (isFamilyRoleToNonFamilyRole)
            {
                _logger.Trace("Connection role was updated from a Family role to a non-Family role, handling legacy flags...");

                HandleContactLegacyFlags(prevConnection, false, true);
                return;
            }

            _logger.Trace("Conditions to update Contact as Legacy have not been met. Exiting.");
        }

        private bool IsLegacyCheckDisabled()
        {
            var configurationData = _retriveConfigurationDetails.GetActiveConfiguration();
            _logger.Trace("Checking null values for StopCheckLegacy");
            bool value = configurationData.cmc_stoplegacycheck ?? false;

            return value;
        }

        private bool IsConnectionFamily(Guid? connectionRoleId)
        {
            return IsConnectionFamily(new List<Guid?> { connectionRoleId });
        }

        private bool IsConnectionFamily(List<Guid?> connectionRoleIds)
        {
            return _orgService.RetrieveMultiple(new FetchExpression($@"
                <fetch top='{connectionRoleIds.Count}'>
                  <entity name='connectionrole'>
                    <attribute name='connectionroleid' />
                    <filter>
                      <condition attribute='connectionroleid' operator='in'>
                        <value>{string.Join("</value><value>", connectionRoleIds)}</value>
                      </condition>
                      <condition attribute='category' operator='eq' value='{(int)connectionrole_category.Family}' />
                    </filter>
                  </entity>
                </fetch>")).Entities.ToList().Count == connectionRoleIds.Count;
        }

        private bool IsFamilyRoleToNonFamilyRole(Connection prevConnection, Connection updatedConnection)
        {
            var connectionRoleIds = new List<Guid?>();

            if (prevConnection.Record2RoleId?.Id == updatedConnection.Record2RoleId?.Id &&
                prevConnection.Record1RoleId?.Id == updatedConnection.Record1RoleId?.Id)
            {
                return false;
            }

            var isPrevRole1Family = IsConnectionFamily(prevConnection.Record1RoleId?.Id);
            var isUpdatedRole1Family = IsConnectionFamily(updatedConnection.Record1RoleId?.Id);

            if (isPrevRole1Family && !isUpdatedRole1Family)
            {
                return true;
            }

            var isPrevRole2Family = IsConnectionFamily(prevConnection.Record2RoleId?.Id);
            var isUpdatedRole2Family = IsConnectionFamily(updatedConnection.Record2RoleId?.Id);

            if (isPrevRole2Family && !isUpdatedRole2Family)
            {
                return true;
            }

            return false;
        }

        private bool IsNonFamilyRoleToFamilyRole(Connection prevConnection, Connection updatedConnection)
        {
            var connectionRoleIds = new List<Guid?>();

            if (prevConnection.Record2RoleId?.Id == updatedConnection.Record2RoleId?.Id &&
                prevConnection.Record1RoleId?.Id == updatedConnection.Record1RoleId?.Id)
            {
                return false;
            }

            var isPrevRole1Family = IsConnectionFamily(prevConnection.Record1RoleId?.Id);
            var isUpdatedRole1Family = IsConnectionFamily(updatedConnection.Record1RoleId?.Id);

            if (!isPrevRole1Family && isUpdatedRole1Family)
            {
                return true;
            }

            var isPrevRole2Family = IsConnectionFamily(prevConnection.Record2RoleId?.Id);
            var isUpdatedRole2Family = IsConnectionFamily(updatedConnection.Record2RoleId?.Id);

            if (!isPrevRole2Family && isUpdatedRole2Family)
            {
                return true;
            }

            return false;
        }

        private bool IsNonAlumiRoleToAlumniRole(Connection prevConnection, Connection updatedConnection)
        {
            return (prevConnection.Record2RoleId?.Id != _alumniRoleId &&
                    updatedConnection.Record2RoleId?.Id == _alumniRoleId) ||
                   (prevConnection.Record1RoleId?.Id != _alumniRoleId &&
                    updatedConnection.Record1RoleId?.Id == _alumniRoleId);
        }

        private bool IsAlumniRoleToNonAlumniRole(Connection prevConnection, Connection updatedConnection)
        {
            return (prevConnection.Record2RoleId?.Id == _alumniRoleId &&
                    updatedConnection.Record2RoleId?.Id != _alumniRoleId) ||
                   (prevConnection.Record1RoleId?.Id == _alumniRoleId &&
                    updatedConnection.Record1RoleId?.Id != _alumniRoleId);
        }

        private bool RelatedContactHasActiveAlumniConnection(EntityReference contactId)
        {
            return _orgService.RetrieveMultiple(new FetchExpression($@"
                <fetch top='1'>
                  <entity name='connection'>
                    <attribute name='connectionid' />
                    <filter type='and'>
                      <condition attribute='record1roleid' operator='eq' value='{_alumniRoleId}' />
                      <condition attribute='record2roleid' operator='eq' value='{_alumniRoleId}' />
                      <condition attribute='statecode' operator='eq' value='{(int)ConnectionState.Active}' />
                    </filter>
                    <link-entity name='contact' from='contactid' to='record1id' link-type='inner' alias='contact'>
                      <filter type='and'>
                        <condition attribute='contactid' operator='eq' value='{contactId?.Id}' />
                      </filter>
                    </link-entity>
                    <link-entity name='account' from='accountid' to='record2id' link-type='inner' alias='campus'>
                      <filter type='and'>
                        <condition attribute='mshied_accounttype' operator='eq' value='{(int)mshied_account_mshied_accounttype.Campus}' />
                      </filter>
                    </link-entity>
                  </entity>
                </fetch>")).Entities.ToList().Count > 0;
        }

        private bool RelatedContactsHaveActiveAlumniConnection(EntityReference contactId)
        {
            return _orgService.RetrieveMultiple(new FetchExpression($@"
                <fetch top='1'>
                  <entity name='connection'>
                    <attribute name='connectionid' />
                    <filter type='and'>
                      <condition attribute='record1roleid' operator='eq' value='{_alumniRoleId}' />
                      <condition attribute='record2roleid' operator='eq' value='{_alumniRoleId}' />
                      <condition attribute='statecode' operator='eq' value='{(int)ConnectionState.Active}' />
					  <filter type='or'>
                        <filter type='and'>
                            <condition entityname='role1' attribute='category' operator='not-null' />
                            <condition entityname='role2' attribute='category' operator='not-null' />
                        </filter>
                        <filter type='and'>
                            <condition entityname='role1' attribute='category' operator='null' />
                            <condition entityname='role2' attribute='category' operator='not-null' />
                        </filter>
                        <filter type='and'>
                            <condition entityname='role1' attribute='category' operator='not-null' />
                            <condition entityname='role2' attribute='category' operator='null' />
                        </filter>
                      </filter>
                    </filter>
                    <link-entity name='contact' from='contactid' to='record1id' link-type='inner' alias='contact'>
						<attribute name='contactid' />
                      <link-entity name='connection' from='record2id' to='contactid' link-type='inner' alias='connection'>
					  	<attribute name='connectionid' />
					  	<filter type='and'>
	                      <condition attribute='record1id' operator='eq' value='{contactId.Id}'/>
	                      <condition attribute='record1objecttypecode' operator='eq' value='{(int)connection_record1objecttypecode.Contact}' />  
	                      <condition attribute='statecode' operator='eq' value='{(int)ConnectionState.Active}' />                         
	                    </filter>
	                    <link-entity name='connectionrole' from='connectionroleid' to='record1roleid' link-type='outer' alias='role1'>
						  <attribute name='category' />
	                      <filter type='and'>
	                        <condition attribute='category' operator='eq' value='{(int)connectionrole_category.Family}' />
	                      </filter>
	                    </link-entity>
	                    <link-entity name='connectionrole' from='connectionroleid' to='record2roleid' link-type='outer' alias='role2'>
	                      <attribute name='category' />
						  <filter type='and'>
	                        <condition attribute='category' operator='eq' value='{(int)connectionrole_category.Family}' />
	                      </filter>
	                    </link-entity>
					  </link-entity>
                    </link-entity>
                    <link-entity name='account' from='accountid' to='record2id' link-type='inner' alias='campus'>
                      <filter type='and'>
                        <condition attribute='mshied_accounttype' operator='eq' value='{(int)mshied_account_mshied_accounttype.Campus}' />
                      </filter>
                    </link-entity>
                  </entity>
                </fetch>")).Entities.ToList().Count > 0;
        }

        private void HandleLegacyOnFamilyContactsOfConnectionFrom(EntityReference contactId, bool legacyValue)
        {
            var fetch = $@"
                <fetch>
                  <entity name='connection'>
                    <attribute name='record2id' />
                    <attribute name='record2roleid' />
                    <attribute name='connectionid' />
                    <order attribute='record2id' descending='false' />
                    <filter type='and'>
                      <condition attribute='record1id' operator='eq' value='{contactId?.Id}'/>
                      <condition attribute='record1objecttypecode' operator='eq' value='{(int)connection_record1objecttypecode.Contact}' />
                      <condition attribute='statecode' operator='eq' value='{(int)ConnectionState.Active}' />
                      <filter type='or'>
                        <filter type='and'>
                            <condition entityname='role1' attribute='category' operator='not-null' />
                            <condition entityname='role2' attribute='category' operator='not-null' />
                        </filter>
                        <filter type='and'>
                            <condition entityname='role1' attribute='category' operator='null' />
                            <condition entityname='role2' attribute='category' operator='not-null' />
                        </filter>
                        <filter type='and'>
                            <condition entityname='role1' attribute='category' operator='not-null' />
                            <condition entityname='role2' attribute='category' operator='null' />
                        </filter>
                      </filter>
                    </filter>
                    <link-entity name='connectionrole' from='connectionroleid' to='record1roleid' link-type='outer' alias='role1'>
	                  <attribute name='category' />
                      <filter type='and'>
                        <condition attribute='category' operator='eq' value='{(int)connectionrole_category.Family}' />
                      </filter>
                    </link-entity>
                    <link-entity name='connectionrole' from='connectionroleid' to='record2roleid' link-type='outer' alias='role2'>
	                  <attribute name='category' />
                      <filter type='and'>
                        <condition attribute='category' operator='eq' value='{(int)connectionrole_category.Family}' />
                      </filter>
                    </link-entity>
                    <link-entity name='contact' from='contactid' to='record2id' link-type='inner' alias='record2'>
                        <filter>
							<condition attribute='mshied_legacy' operator='ne' value='{legacyValue}' />
						</filter>
                    </link-entity>
                  </entity>
                </fetch>";

            _logger.Trace($"Pulling all family Connections where Contact {contactId?.Id} is Record 1 Role.");
            var results = _orgService.RetrieveMultipleAll(fetch).Entities.ToList();

            if (results.Count > 0)
            {
                _logger.Trace($"Contact {contactId} has {results.Count} Family connections. Updating related Contacts Legacy.");

                foreach (var connection in results)
                {
                    var updatedContact = new Entity(Contact.EntityLogicalName);
                    updatedContact.Id = connection.GetAttributeValue<EntityReference>("record2id").Id;
                    updatedContact["mshied_legacy"] = legacyValue;

                    _orgService.Update(updatedContact);
                }
            }
        }

        public void HandleContactConnection(EntityReference connectedFrom, EntityReference connectedTo, bool checkLegacy)
        {
            _logger.Trace("Retrieving the Contact in Record 1.");
            var contact = _orgService.Retrieve<Contact>(connectedFrom,
                new ColumnSet("contactid", "mshied_legacy"));

            if (contact.mshied_Legacy == checkLegacy)
            {
                _logger.Trace($"Contact {contact?.Id} Legacy is already set to {checkLegacy}. Exiting early.");
                return;
            }

            // If the connection is being added then check if the related contact is an alumni
            //      If so, the legacy field will be checked
            if (checkLegacy == true &&
                RelatedContactHasActiveAlumniConnection(connectedTo) == false)
            {
                _logger.Trace($"Contact in Record 2, {connectedTo?.Id} is not an Alumni on any Campus.");
                return;
            }
            // If the connection is being removed then check if the From contact is connected to any other alumnis
            //      If so, the legacy field will be unchecked
            else if (checkLegacy == false &&
                RelatedContactsHaveActiveAlumniConnection(connectedFrom) == true)
            {
                _logger.Trace($"Connected From still has an active Alumni Connection.");
                return;
            }

            _logger.Trace($"Setting Contact {connectedFrom.Id} Legacy field to {checkLegacy}.");
            _orgService.Update(new Contact()
            {
                ContactId = contact.ContactId,
                mshied_Legacy = checkLegacy
            });
        }

        public void HandleAccountConnection(EntityReference connectedFrom, EntityReference connectedTo, bool checkLegacy)
        {
            var campus = _orgService.Retrieve<Account>(connectedTo,
                new ColumnSet("accountid", "mshied_accounttype"));

            if ((bool)!campus?.mshied_AccountType.Value.Equals(mshied_account_mshied_accounttype.Campus))
            {
                _logger.Trace($"Account in Record 1, {connectedTo?.Id} is not a Campus.");
                return;
            }

            _logger.Trace($"Contact in Record 2, {connectedFrom.Id} is an Alumni of Campus {campus.AccountId}.");
            HandleLegacyOnFamilyContactsOfConnectionFrom(connectedFrom, checkLegacy);
        }

        private void HandleContactLegacyFlags(Connection connection, bool checkLegacy, bool isUpdate = false)
        {
            var connectedTo = connection.Record2Id;
            var connectedToRole = connection.Record2RoleId;

            var connectedFrom = connection.Record1Id;
            var connectedFromRole = connection.Record1RoleId;

            switch (connectedTo?.LogicalName)
            {
                // Criteria for checking Legacy Checkbox of the Contact in Connected From (Record 1)
                case Contact.EntityLogicalName when connectedFrom?.LogicalName == Contact.EntityLogicalName:
                    // Null Connection Roles are allowed, but if both Connection Roles are specified,
                    // both must have a Category of Family
                    if (checkLegacy == true &&
                        IsConnectionFamily((new List<Guid?>
                        {
                            connectedToRole?.Id,
                            connectedFromRole?.Id
                        }.Where(roleId => roleId != null).ToList())) == false
                    )
                    {
                        _logger.Trace("Connection is not a Family Connection.");
                        return;
                    }

                    HandleContactConnection(connectedFrom, connectedTo, checkLegacy);

                    if (isUpdate)
                    {
                        _logger.Trace("Update Message. The reverse Contact Connection will be checked too.");
                        HandleContactConnection(connectedTo, connectedFrom, checkLegacy);
                    }
                    break;
                // Criteria for checking the Legacy Checkbox for all Contact connections related to Connected From (Record 1)
                case Account.EntityLogicalName when connectedFrom?.LogicalName == Contact.EntityLogicalName && connectedToRole?.Id == _alumniRoleId && connectedFromRole?.Id == _alumniRoleId:
                    {
                        _logger.Trace("Connection is from a Contact to an Account and both Record 1 and Record 2 have Alumni roles.");
                        HandleAccountConnection(connectedFrom, connectedTo, checkLegacy);
                        break;
                    }
                case Contact.EntityLogicalName when connectedFrom?.LogicalName == Account.EntityLogicalName && connectedToRole?.Id == _alumniRoleId && connectedFromRole?.Id == _alumniRoleId && isUpdate:
                    {
                        _logger.Trace("Connection is from an Account to a Contact and both Record 1 and Record 2 have Alumni roles and this is an Update.");
                        HandleAccountConnection(connectedTo, connectedFrom, checkLegacy);
                        break;
                    }
                default:
                    _logger.Trace("The record in Record 1 is not a Contact or Account or the record in Record 2 is not a Contact or Account.");
                    break;
            }
        }

        #endregion

        #region Predict Retention

        public void PredictRetentionAction(IExecutionContext context)
        {
            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();
            //_orgService = serviceProvider.CreateSystemOrganizationService();
            _logger.Trace($"Entered plugin: {nameof(PredictRetentionAction)}");
            Guid contactId = pluginContext.ParseGuidInput("ContactId").Value;
            var outcome = Predict(contactId);
            pluginContext.OutputParameters["RetentionProbability"] = outcome;
        }

        private ContactFactors RetrieveContactFactors(Guid contactId)
        {
            var fetchXml =
            $@"<fetch top='1'>
			    <entity name='contact'>
				    <attribute name='cmc_recentsat' />
				    <attribute name='cmc_recentact' />
				    <attribute name='gendercode' />
				    <attribute name='cmc_age' />
				    <attribute name='mshied_firstgeneration' />
				    <link-entity name='mshied_academicperioddetails' from='mshied_studentid' to='contactid' alias='ap' link-type='outer'>
					    <attribute name='cmc_residence' />
					    <attribute name='mshied_attendancetype' />
					    <attribute name='mshied_midtermdeficiency' />
				    </link-entity>
				    <link-entity name='mshied_previouseducation' from='mshied_studentid' to='contactid' alias='pe' link-type='outer'>
					    <attribute name='mshied_rank' />
				    </link-entity>
				    <filter>
					    <condition operator='eq' attribute='contactid' value='{contactId}' />
				    </filter>
			    </entity>
		    </fetch>";

            var contact = _orgService.RetrieveMultiple(new FetchExpression(fetchXml)).Entities.FirstOrDefault();
            if (contact == null)
                return null;

            var result = new ContactFactors();
            result.Age = GetString(contact.GetAttributeValue<int>("cmc_age"));
            result.AttendanceType = GetString(contact.GetAliasedAttributeValue<OptionSetValue>("ap.mshied_attendancetype"));
            result.FirstGeneration = GetString(contact.GetAttributeValue<bool>("mshied_firstgeneration"));
            result.GenderCode = GetString(contact.GetAttributeValue<OptionSetValue>("gendercode"));
            result.Rank = GetString(contact.GetAliasedAttributeValue<int>("pe.mshied_rank"));
            result.MidTermDeficiency = GetString(contact.GetAliasedAttributeValue<bool>("ap.mshied_midtermdeficiency"));
            result.RecentAct = GetString(contact.GetAttributeValue<int>("cmc_recentact"));
            result.RecentSat = GetString(contact.GetAttributeValue<int>("cmc_recentsat"));
            result.Residence = GetString(contact.GetAliasedAttributeValue<OptionSetValue>("ap.cmc_residence"));

            return result;
        }

        public string GetString(object value)
        {
            if (value == null)
                return "-1";

            if (value.GetType() == typeof(OptionSetValue))
                return ((OptionSetValue)value).Value.ToString();
            if (value.GetType() == typeof(int) ||
                value.GetType() == typeof(bool))
                return value.ToString();

            return "-1";
        }

        public decimal Predict(Guid studentId)
        {
            var apiKey = _retriveConfigurationDetails.GetActiveConfiguration().cmc_retentionpredictionapikey;
            var apiUrl = _retriveConfigurationDetails.GetActiveConfiguration().cmc_retentionpredictionapiurl;

            var contactFactors = RetrieveContactFactors(studentId);

            if (contactFactors == null)
                return 0;

            using (var client = new HttpClient())
            {//should not use HTTPClient insted should use WebRequest (as used in other features like Gravatar etc.)
                var scoreRequest = new
                {
                    Inputs = new Dictionary<string, StringTable>() {
                        {
                            "input1",
                            new StringTable()
                            {
                                ColumnNames = new string[] {"cmc_droppedstatus", "cmc_recentsat", "cmc_recentact", "cmc_currentretentionscore", "gendercode", "cmc_age", "mshied_firstgeneration", "cmc_residence", "mshied_attendancetype", "mshied_midtermdeficiency", "mshied_rank"},
                                Values = new string[,] {  { "value", contactFactors.RecentSat, contactFactors.RecentAct, "0", contactFactors.GenderCode, contactFactors.Age, contactFactors.FirstGeneration, contactFactors.Residence, contactFactors.AttendanceType, contactFactors.MidTermDeficiency, contactFactors.Rank } }
                            }
                        },
                    },
                    GlobalParameters = new Dictionary<string, string>()
                    {
                    }
                };

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.BaseAddress = new Uri(apiUrl);

                HttpResponseMessage response = client.PostAsync("", new StringContent(JsonConvert.SerializeObject(scoreRequest), Encoding.UTF8, "application/json")).Result;
                decimal? retentionOutcome = null;
                if (response != null && response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    var output = JsonConvert.DeserializeObject<PredictionResult>(result);
                    retentionOutcome = decimal.Parse(output.Results.Output1.Value.Values[0][9]);

                    decimal retentionProbability = 0;
                    if (retentionOutcome.HasValue)
                    {
                        retentionProbability = retentionOutcome.Value * 100;
                        retentionProbability = Math.Round(retentionProbability, 2);
                    }

                    return retentionProbability;
                }
            }

            return 0;
        }
        #endregion

        #region  CampusDistanceCalculatorLogic

        public void CampusDistanceCalculatorLogic()
        {


            UpdateDistance();
        }



        public void UpdateDistance()
        {
            UpdateDistanceForContactBasedOnCampusAddressChnge();
            UpdateDistanceForContactsHavingValidAddresses();
        }

        private void UpdateDistanceForContactBasedOnCampusAddressChnge()
        {
            var contactDetailsForUpdatedCampus = GetAllContactDetailsForUpdatedCampus();
            if (contactDetailsForUpdatedCampus == null) return;
            UpdateDistanceInMilesForContact(contactDetailsForUpdatedCampus, true);
        }

        private void UpdateDistanceForContactsHavingValidAddresses()
        {
            var contactsHavingValidAddressesButDistanceIsNull = GetContactsHavingValidAddressesButDistanceIsNull();
            if (contactsHavingValidAddressesButDistanceIsNull == null) return;
            UpdateDistanceInMilesForContact(contactsHavingValidAddressesButDistanceIsNull, false);
        }

        private void UpdateDistanceInMilesForContact(List<Contact> contactEntities,
            bool flagForUpdatingaccountaddressupdatedField)
        {
            var contactList = new List<Contact>();
            var accountsById = new Dictionary<Guid, Account>();
            foreach (var data in contactEntities)
            {
                var campusLongitude = data.GetAliasedAttributeValue<double>("ai.address1_longitude");
                var campusLatitude = data.GetAliasedAttributeValue<double>("ai.address1_latitude");
                var contactLatitude = data.Address1_Latitude;
                var contactLongitude = data.Address1_Longitude;
                if (contactLatitude == null || contactLongitude == null) continue;
                var distanceInMiles = CalculateDistanceInMilesUsingPointToPointCalculation(contactLatitude.Value,
                    contactLongitude.Value, campusLatitude, campusLongitude);

                contactList.Add(new Contact
                {
                    ContactId = data.Id,
                    cmc_milesfromcampus = (decimal)distanceInMiles
                });
                if (!flagForUpdatingaccountaddressupdatedField) continue;
                var accounntId = data.GetAliasedAttributeValue<Guid>("ai.accountid");

                if (accountsById.ContainsKey(accounntId)) continue;
                accountsById.Add(accounntId, new Account
                {
                    AccountId = accounntId,
                    cmc_accountaddressupdated = null
                });
            }

            ExecuteBulkEntities.BulkUpdateBatch(_orgService, contactList.Cast<Entity>().ToList());
            if (flagForUpdatingaccountaddressupdatedField)
                ExecuteBulkEntities.BulkUpdateBatch(_orgService, accountsById.Values.Cast<Entity>().ToList());
        }




        private List<Contact> GetAllContactDetailsForUpdatedCampus()
        {
            var fetch = $@"<fetch>
                                     <entity name='contact' >
                                         <attribute name='address1_latitude' />               
                                         <attribute name='address1_longitude' />
                                         <attribute name='contactid' />
                                         <filter type='and' >
                                             <condition attribute='address1_latitude' operator='not-null' />
                                             <condition attribute='address1_longitude' operator='not-null' />
                                         </filter>
                                         <link-entity name='account' from='accountid' to='parentcustomerid' alias='ai'>
                                             <attribute name='address1_longitude' />
                                             <attribute name='address1_latitude' />
                                             <attribute name='accountid' />
                                             <filter type='and' >
                                                 <condition attribute='address1_latitude' operator='not-null' />
                                                 <condition attribute='address1_longitude' operator='not-null' />
                                                 <condition attribute='cmc_accountaddressupdated' operator='eq' value='{
                    (int)cmc_account_cmc_accountaddressupdated.Yes
                }'  />
                                             </filter> 
                                         </link-entity> 
                                     </entity> 
                                 </fetch>";
            var addressDetails = _orgService.RetrieveMultipleAll(fetch);
            return addressDetails.Entities.Count <= 0 ? null : addressDetails.Entities.Cast<Contact>().ToList();
        }

        private List<Contact> GetContactsHavingValidAddressesButDistanceIsNull()
        {
            const string fetch =
                @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' >
                                     <entity name='contact' >             
                                         <attribute name='contactid' />
                                         <attribute name='address1_longitude' />
                                         <attribute name='address1_latitude' />            
                                         <filter type='and' >
                                             <condition attribute='address1_latitude' operator='not-null' />
                                             <condition attribute='address1_longitude' operator='not-null' />
                                             <condition attribute='cmc_milesfromcampus' operator='null' />
                                         </filter>
                                         <link-entity name='account' from='accountid' to='parentcustomerid' link-type='inner' alias='ai' >
                                             <attribute name='address1_longitude' />
                                             <attribute name='address1_latitude' />
                                             <filter type='and' >
                                                 <condition attribute='address1_latitude' operator='not-null' />
                                                 <condition attribute='address1_longitude' operator='not-null' />
                                             </filter>
                                         </link-entity>
                                     </entity>
                                 </fetch>";

            var addressDetails = _orgService.RetrieveMultipleAll(fetch);
            return addressDetails.Entities.Count <= 0 ? null : addressDetails.Entities.Cast<Contact>().ToList();
        }
        #endregion

        #region RetriveMembersFromMarketngList
        public List<Contact> RetriveMembersForStaticType(Guid listId, Guid? ownerId, IOrganizationService organizationService)
        {

            if (ownerId != null && organizationService!=null)
                SetCallerId(ownerId.Value, organizationService);

            var fetch = $@"<fetch distinct='true'> 
                <entity name='contact'>
                    <attribute name='fullname' />
                        <attribute name='contactid'/>
                     <filter>
                        <condition attribute='statecode' operator='eq' value='0' />
                    </filter>
                    <link-entity name='listmember' to='contactid' from='entityid'>
                        <link-entity name='list' to='listid' from='listid'>
                            <filter type='and' >
                                <condition attribute='type' operator='eq' value='0' />
                                <condition attribute='listid' operator='eq' value='{
                    listId
                }'/>               
                            </filter>
                        </link-entity>
                    </link-entity>
                    <link-entity name='mshied_academicperiod' from='mshied_academicperiodid' to='mshied_currentacademicperiodid'>
                    <filter>
                    <condition attribute='statecode' operator='eq' value='0' />
                    </filter>
                    </link-entity>
                </entity>
            </fetch>";
            var contactList = organizationService.RetrieveMultipleAll(fetch);

            SetCallerId(Guid.Empty, organizationService);
            return contactList.Entities.Count <= 0 ? null : contactList.Entities.Cast<Contact>().ToList();
        }

        public List<Contact> RetriveMembersForDynamicType(string query, Guid? ownerId, IOrganizationService organizationService)
        {
            if (string.IsNullOrEmpty(query)) return null;

            if (ownerId != null && organizationService != null)
                SetCallerId(ownerId.Value, organizationService);

            var queryExpression = ((FetchXmlToQueryExpressionResponse)_orgService.Execute(
                new FetchXmlToQueryExpressionRequest
                {
                    FetchXml = query
                })).Query;

            if (queryExpression.Criteria.FilterOperator == LogicalOperator.And)
            {
                queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            }
            else
            {
                var originalQueryExpression = queryExpression;
                queryExpression = new QueryExpression();
                queryExpression.ColumnSet = originalQueryExpression.ColumnSet;
                queryExpression.Distinct = originalQueryExpression.Distinct;
                queryExpression.EntityName = originalQueryExpression.EntityName;
                queryExpression.LinkEntities.AddRange(originalQueryExpression.LinkEntities);
                queryExpression.NoLock = originalQueryExpression.NoLock;
                queryExpression.Orders.AddRange(originalQueryExpression.Orders);
                queryExpression.PageInfo = originalQueryExpression.PageInfo;
                queryExpression.TopCount = originalQueryExpression.TopCount;
                queryExpression.Criteria.AddFilter(originalQueryExpression.Criteria);
                queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            }
            var academicPeriodLink = queryExpression.AddLink("mshied_academicperiod", "mshied_currentacademicperiodid",
                "mshied_academicperiodid");

            academicPeriodLink.LinkCriteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            var dynamicListMembers = organizationService.RetrieveMultipleAll(queryExpression);

            SetCallerId(Guid.Empty, organizationService);
            return dynamicListMembers.Entities.Count <= 0 ? null : dynamicListMembers.Entities.Cast<Contact>().ToList();
        }
        /// <summary>
        /// The Organization Service instance retrieved from the DI is a static instance and shared across all Azure functions.
        /// In cases where we would need to impersonate the user, we need to perform the operations using a new instance of Organization Service,
        /// so that it does not impact the other Azure functions.
        /// The newly created Organization Service can be used to set the Caller Id to the user to whom to impersonate. 
        /// </summary>
        /// <param name="callerId"></param>
        /// <param name="organizationService"></param>
        private void SetCallerId(Guid callerId, IOrganizationService organizationService)
        {
            switch (organizationService)
            {
                case OrganizationWebProxyClient _:
                    ((OrganizationWebProxyClient)organizationService).CallerId = callerId;
                    break;
                case OrganizationServiceProxy _:
                    ((OrganizationServiceProxy)organizationService).CallerId = callerId;
                    break;
            }
        }

        #endregion

        #region SuccessPlanAssignmentLogic

        public void SuccessPlanAssignmentLogic(IOrganizationService organizationService)
        {
            AssignSuccessPlan(organizationService);
        }

        private void AssignSuccessPlan(IOrganizationService organizationService)
        {
            _logger.Info($"Retrieving active success plan assignment and student groups");
            var studentGroups = RetrieveSuccessPlanStudentGroups();
            if (studentGroups == null || studentGroups.Any() != true)
            {
                _logger.Info("No groups found.");
                return;
            }

            //var retriveMemberslogic = new ContactService(null, null, _orgService, null);
            foreach (var assignment in studentGroups)
                try
                {
                    var listType = assignment.Type ?? false;
                    var successplanassignmentid =
                        assignment.GetAliasedAttributeValue<Guid>("spl.cmc_successplanassignmentid");
                    var successplantemplateid =
                        assignment.GetAliasedAttributeValue<EntityReference>("sp.cmc_successplantemplateid");
                    var successplantemplatename = assignment.GetAliasedAttributeValue<string>("spt.cmc_successplantemplatename");
                    var ownerdetails =
                        assignment.OwnerId;
                    Guid? ownerid = null;
                    if (ownerdetails != null)
                    {
                        if (ownerdetails.LogicalName == SystemUser.EntityLogicalName)
                        {
                            ownerid = ownerdetails.Id;
                        }
                    }
                    var memberList = listType ? RetriveMembersForDynamicType(assignment.Query, ownerid, organizationService) : RetriveMembersForStaticType(assignment.Id, ownerid, organizationService);

                    if (memberList == null || successplantemplateid == null) continue;
                    var students = RemoveDuplicates(memberList, successplantemplateid, successplantemplatename);

                    _logger.Info($"{students.Count} students matched assignment'");
                    using (var executeMultipleBuffer = new ExecuteMultipleBuffer(_orgService))
                    {
                        foreach (var student in students)
                        {
                            var alreadyAssigned = WasSuccessPlanAssigned(successplanassignmentid, student.Id);
                            if (alreadyAssigned)
                            {
                                _logger.Info($"Success Plan Template {successplantemplatename} was already assigned to Student ID {student.FullName} Continuing...");
                                continue;
                            }

                            //var logic = new ContactService(null, null, _orgService, null);
                            CreateSuccessPlans(successplantemplateid,
                              new EntityCollection(new List<Entity>
                              {
                                    new Entity(Contact.EntityLogicalName, student.Id)
                              }), null);
                            _logger.Info($"Success Plan Template {successplantemplatename} is assigned to Student name {student.FullName} Continuing...");
                            var associateRequest = new AssociateRequest
                            {
                                Relationship = new Relationship("cmc_contact_successplanassignment"),
                                Target = new EntityReference(Contact.EntityLogicalName, student.Id),
                                RelatedEntities = new EntityReferenceCollection
                                {
                                    new EntityReference(cmc_successplanassignment.EntityLogicalName,
                                        successplanassignmentid)
                                }
                            };

                            _logger.Info("Associating Student with Success Plan Assignment...");
                            _orgService.Execute(associateRequest);
                        }
                    }

                    _logger.Info($"Finished processing Assignment...");
                }
                catch (Exception e)
                {
                    _logger.Fatal($"Error while processing Assignment: {e}");
                }
        }

        private List<Contact> RemoveDuplicates(List<Contact> students, EntityReference successPlanTemplateId, string successplanTemplateName)
        {
            var validstudents = new List<Contact>();
            if (students?.Any() != true || successPlanTemplateId == null) return validstudents;
            //var logic = new ContactService(null, null, _orgService, null);
            var studentsHavingSameSuccessPlanTemplate =
                GetAllStudentsHavingSameSuccessPlanTemplate(successPlanTemplateId.Id,
                    students.Select(a => a.Id).ToList());
            if (studentsHavingSameSuccessPlanTemplate != null)
            {
                foreach (var student in students)
                {
                    if (!studentsHavingSameSuccessPlanTemplate.ContainsKey(student.Id))
                    {
                        validstudents.Add(student);
                    }
                    else
                    {
                    }
                    _logger.Info($"Success Plan Template {successplanTemplateName} already exists for this Student {student.FullName}  therefore not re-assigned.");
                }
            }
            else
            {
                return students;
            }
            return validstudents;
        }

        private bool WasSuccessPlanAssigned(Guid successPlanAssignmentId, Guid contactId)
        {
            var fetch =
                $@"<fetch>
			        <entity name='cmc_contact_successplanassignment'>
                        <filter>
                            <condition attribute='contactid' operator='eq' value='{contactId}' />
                            <condition attribute='cmc_successplanassignmentid' operator='eq' value='{
                        successPlanAssignmentId
                    }' />
                        </filter>
			        </entity>
		        </fetch>";

            return _orgService.RetrieveMultiple(new FetchExpression(fetch)).Entities.Count > 0;
        }
        private List<List> RetrieveSuccessPlanStudentGroups()
        {
            var fetch =
               $@"<fetch>
			        <entity name='list'>
                                 <attribute name='type' />
                                 <attribute name='listid'/>
                                 <attribute name='query' />
                                  <attribute name='listname' />
                                  <attribute name='ownerid' />
				        <link-entity name='cmc_successplanassignment_list' to='listid' from='listid' alias='spl'>	
                         <attribute name='cmc_successplanassignmentid' />
                           <link-entity name='cmc_successplanassignment' to='cmc_successplanassignmentid' from='cmc_successplanassignmentid' alias='sp'>
                            <attribute name='cmc_successplantemplateid' />
                    <link-entity name='cmc_successplantemplate' to='cmc_successplantemplateid' from='cmc_successplantemplateid' alias='spt'>
                     <attribute name='cmc_successplantemplatename' />
                            </link-entity>
                            <filter>
                           <condition attribute='statecode' operator='eq' value='0' />
                                </filter>
				            </link-entity>
				        </link-entity>
                        <filter>
                            <condition attribute='statecode' operator='eq' value='0' />
                            <condition attribute ='cmc_marketinglisttype' operator='eq' value=' { (int)cmc_list_cmc_marketinglisttype.StudentGroup}' />
                            <condition attribute ='createdfromcode' operator='eq' value=' { (int)list_createdfromcode.Contact}' />
                        </filter>
			        </entity>
		        </fetch>";
            var list = _orgService.RetrieveMultipleAll(fetch);
            return list.Entities.Count <= 0 ? null : list.Entities.Cast<List>().ToList();
        }
        #endregion

        #region Update Contact Image
        /// <summary>
        ///  Function to set Image for contact from various integration.
        /// </summary>
        public void SetContactImage()
        {
            _logger.Trace($"Inside Function to set Contact Image.");
            UpdateContactImageData();
            List<Contact> contacts = GetContactforUpdate();
            if (contacts == null) { _logger.Trace($"No contact found for Image Update."); return; }

            cmc_configuration configurations = GetConfigurationsforImageIntegration();
            if (configurations == null) { _logger?.Trace("No configurations Found."); return; }

            contacts = SetGravatharImage(contacts, configurations);
            //++++Enhancement integration methods caller should be added respectively++++.

            if (contacts.Count > 0)
                SetIntegrationFlag(contacts); //Should be a finall method after all integration function process are done. 

        }
        private List<Contact> GetContactforUpdate()
        {
            var fetch = $@"<fetch >
                  <entity name='contact' >
                    <attribute name='contactid' />
                    <attribute name='emailaddress1' />	
                    <filter type='and' >
                      <condition attribute='statuscode' operator='eq' value='{(int)contact_statuscode.Active}' />
                      <condition attribute='emailaddress1' operator='not-null' />
                      <condition attribute='cmc_imagesource' operator='null'/>
                      <condition attribute='entityimage' operator='null' />
                      <condition attribute='cmc_autoupdatepicture' operator='neq' value='0' />
                    </filter>
                  </entity>
                </fetch>";
            var contactDetails = _orgService.RetrieveMultipleAll(fetch);
            return contactDetails.Entities.Count <= 0 ? null : contactDetails.Entities.Cast<Contact>().ToList();
        }
        /// <summary>
        /// Default Method in the pattern to set flag to not found to contact instance for which image has been not found by any integration.
        /// </summary>
        /// <param name="contacts"></param>
        private void SetIntegrationFlag(List<Contact> contacts)
        {
            foreach (var contact in contacts)
            {
                contact.cmc_imagesource = cmc_contact_cmc_imagesource.NoMatchFound;
            }
            ExecuteBulkEntities.BulkUpdateBatch(_orgService, contacts.ToList<Entity>());
        }

        /// <summary>
        /// Clean up the record for which mannual update has been happened.
        /// </summary>
        public void UpdateContactImageData()
        {
            var fetch = $@" <fetch top='50'>
                              <entity name='contact' >
                                <attribute name='entityimage_timestamp' />
                                <attribute name='cmc_externalimagetimestamp' />
                                <filter type='and' >
                                  <condition attribute='statuscode' operator='eq' value='{(int)contact_statuscode.Active}' />
                                  <condition attribute='cmc_externalimagetimestamp' operator='not-null' /> 
                                  <condition attribute='cmc_imagesource' operator='not-null' />
                                  <condition attribute='cmc_imagesource' operator='neq' value='{(int)cmc_contact_cmc_imagesource.ManualUpdate}' />
                                  <filter type='or' >
                                    <condition attribute='entityimage_timestamp' operator='null' />
                                    <condition attribute='entityimage_timestamp' operator='not-null' />
                                  </filter>
                                </filter>
                              </entity>
                            </fetch>";
            var contactDetails = _orgService.RetrieveMultiple(new FetchExpression(fetch));
            var contacts = contactDetails.Entities.Count <= 0 ? null : contactDetails.Entities.Cast<Contact>().ToList();

            if (contacts != null)
            {
                List<Contact> updatedContacts = new List<Contact>();
                foreach (var contact in contacts)
                {
                    /*cmc_externalimagetimestamp is set only by azure function
                     if difference found between the timestamps (mannual updation) cmc_externalimagetimestamp will be cleared.  */
                    if (contact.EntityImage_Timestamp.ToString() != contact.cmc_externalimagetimestamp)
                    {
                        updatedContacts.Add(new Contact()
                        {
                            Id = contact.Id,
                            cmc_externalimagetimestamp = null,
                            cmc_imagesource = cmc_contact_cmc_imagesource.ManualUpdate,
                        });

                    }
                }
                if(updatedContacts.Count > 0)
                    ExecuteBulkEntities.BulkUpdateBatch(_orgService, updatedContacts.Cast<Entity>().ToList());
            }

        }
        /// <summary>
        /// Get Configuration values used for Image Integration.
        /// </summary>
        /// <returns>Key Value Pair</returns>
        public cmc_configuration GetConfigurationsforImageIntegration()
        {
            var fetchConfigData =
                    $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' >
                         <entity name='cmc_configuration' >
                            <attribute name='cmc_imageintegrationgravatarurl' />
                            <attribute name='cmc_imageintegrationgravatarrequired' />
                            
                            <filter type='and' >
                                <condition attribute='statecode' operator='eq' value='0' />
                            </filter>
                         </entity>
                        </fetch>";
            var configurationDetails = _orgService.RetrieveMultipleAll(fetchConfigData);
            return configurationDetails.Entities.Count <= 0 ? null : configurationDetails.Entities.Cast<cmc_configuration>().FirstOrDefault();
        }
        /// <summary>
        /// Common funtion to refine and copy timestamp to the entities for which image has found.
        /// </summary>
        /// <param name="contacts"></param>
        /// <param name="matchedContacts"></param>
        /// <returns></returns>
        public List<Contact> FilterContactWithoutImage(List<Contact> contacts, List<Contact> matchedContacts)
        {
            var contactList = new List<Contact>();
            foreach (var matchedContact in matchedContacts)
            {
                Contact timestamp = _orgService.Retrieve("contact", matchedContact.Id, new ColumnSet(true)).ToEntity<Contact>();
                contactList.Add(new Contact()
                {
                    Id = timestamp.Id,
                    cmc_externalimagetimestamp = timestamp.EntityImage_Timestamp.ToString(),
                });
                contacts.Remove(matchedContact);
            }
            ExecuteBulkEntities.BulkUpdateBatch(_orgService, contactList.Cast<Entity>().ToList());
            return contacts;
        }
        /// <summary>
        /// Get the Image associated with contact from gravatar if found and sets.
        /// </summary>
        /// <param name="contacts">Set of contact which dont have image.</param>
        /// <param name="cofigurations">System Level Configurationt to Settings used for Integration.</param>
        /// <returns></returns>
        public List<Contact> SetGravatharImage(List<Contact> contacts, cmc_configuration configurations)
        {
            bool isGravatarRequired = configurations.GetAttributeValue<bool>("cmc_imageintegrationgravatarrequired");
            string gravatarUrl = configurations.GetAttributeValue<string>("cmc_imageintegrationgravatarurl");
            if (!isGravatarRequired) { _logger.Trace($"Gravatar integration is turned off."); return contacts; }
            if (gravatarUrl == null || gravatarUrl == String.Empty) { _logger.Trace($"Gravatar Url cannot not be empty."); return contacts; };

            _logger.Trace($"Configuration key values for Image Integration Is Gravatar Required : '{isGravatarRequired}' and Image Integration Gravatar Url : '{gravatarUrl}'");

            List<Contact> matchedContacts = new List<Contact>();
            foreach (var contact in contacts)
            {
                byte[] imageArray;
                string emailHash = GetEmailHashValue(contact.EMailAddress1);
                //Create Gravatar URL by configuration Gravatar value and Current contact email's Hash.
                HttpWebRequest lxRequest = (HttpWebRequest)WebRequest.Create(string.Format(gravatarUrl, emailHash));
                try
                {//gravatar returns 404 error if image not found to handle that try catch has been used.
                    using (HttpWebResponse lxResponse = (HttpWebResponse)lxRequest.GetResponse())
                    {
                        using (BinaryReader lxBR = new BinaryReader(lxResponse.GetResponseStream()))
                        {
                            //If image found set that to EntityImage field which calculate values for entityImage_timestamp and entityImage_url of the current instance.
                            //long size = lxBR.BaseStream.Length;
                            imageArray = lxBR.ReadBytes(5000000);
                            if (imageArray.Length > 0)
                            {
                                contact.EntityImage = imageArray;
                                contact.cmc_imagesource = cmc_contact_cmc_imagesource.Gravatar;
                                _orgService.Update(contact);
                                matchedContacts.Add(contact); // contact whose images are found with this integration are added into local list  and removed contact collection so other interation (if any) can process contact whose image are not found.
                            }

                        }
                    }
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.ProtocolError &&
                        ex.Response != null)
                    {
                        var resp = (HttpWebResponse)ex.Response;
                        if (resp.StatusCode == HttpStatusCode.NotFound)
                        {
                            _logger.Trace($"Gravatar image not found for - {contact.EMailAddress1}");
                        }

                    }
                    else
                    {
                        _logger.Trace($"{ex.Message}");
                    }
                }

            }
            return FilterContactWithoutImage(contacts, matchedContacts);
        }
        /// <summary>
        /// Convert the passed Email string to a byte array and compute the hash
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static string GetEmailHashValue(string email)
        {
            MD5 md5Hasher = MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(email));
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string.  
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }



        #endregion
    }
}
