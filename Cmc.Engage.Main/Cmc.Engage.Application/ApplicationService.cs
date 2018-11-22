using Cmc.Core.Xrm.ServerExtension.Core;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;
using System.Collections.Generic;

namespace Cmc.Engage.Application
{
    public class ApplicationService : IApplicationService
    {
        private IOrganizationService _orgService;
        private readonly ILogger _logger;

        public ApplicationService(IOrganizationService orgService, ILogger logger)
        {
            _orgService = orgService;
            _logger = logger;
        }

        #region SetupApplicationRequirementsOnApplicationPlugin
        public void SetupApplicationRequirements(IExecutionContext executionContext)
        {
            _logger.Trace("Starting SetupApplicationRequirementsOnApplicationPlugin");
            var serviceProvider = executionContext.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();

            _logger.Trace("Retrieving Target.");
            var target = pluginContext.GetInputParameter<Entity>("Target").ToEntity<cmc_application>();
            if (pluginContext.MessageName.ToLower() == "create")
            {
                _logger.Trace("Executing logic on create.");
                SetupNewApplicationRequirements(target);
            }
            else if (pluginContext.MessageName.ToLower() == "update")
            {
                _logger.Trace("Executing logic on update.");
                UpdateRelatedTestScoreRequirements(target);
            }

            _logger.Trace("Exiting SetupApplicationRequirementsOnApplicationPlugin");
        }

        private void SetupNewApplicationRequirements(cmc_application application)
        {
            var applicationId = application.ToEntityReference();
            if (application.cmc_applicationregistration == null)
            {
                _logger.Trace("Application Registration is missing from the Application. No Application Requirements will be created.");
                return;
            }

            _logger.Trace($"Retrieving Application Requirement Definition Details related to Application Registration {application.cmc_applicationregistration.Id}");
            // Do not create Application Requirement Definiton Details of type Transcript at this time
            var applicationRequirementDefinitionDetails = _orgService.RetrieveMultipleAll(
                $@"<fetch version='1.0' output-format='xml-platform' mapping='logical'>
                      <entity name='cmc_applicationrequirementdefinitiondetail'>
                        <attribute name='cmc_applicationrequirementdefinitiondetailid' />
                        <attribute name='cmc_requirementtype' />
                        <attribute name='cmc_statusonsubmit' />
                        <attribute name='cmc_testscoretype' />
                        <attribute name='cmc_testsourcetype' />
                        <filter>
                          <condition attribute='statecode' operator='eq' value='{(int)cmc_applicationrequirementdefinitiondetailState.Active}' />
                          <condition attribute='cmc_requirementtype' operator='not-in'>
                            <value>{(int)cmc_applicationrequirementtype.OfficialTranscript}</value>
                            <value>{(int)cmc_applicationrequirementtype.UnofficialTranscript}</value>
                          </condition>
                        </filter>
                        <link-entity name='cmc_applicationrequirementdefinition' from='cmc_applicationrequirementdefinitionid' to='cmc_applicationrequirementdefinition' link-type='inner'>
                          <link-entity name='cmc_applicationdefinition' from='cmc_requirementdefinitionid' to='cmc_applicationrequirementdefinitionid' link-type='inner'>
                            <link-entity name='cmc_applicationdefinitionversion' from='cmc_applicationdefinitionid' to='cmc_applicationdefinitionid' link-type='inner'>
                              <link-entity name='cmc_applicationregistration' from='cmc_applicationdefinitionversionid' to='cmc_applicationdefinitionversionid' link-type='inner'>
                                <filter type='and'>
                                  <condition attribute='cmc_applicationregistrationid' operator='eq' value='{application.cmc_applicationregistration.Id}' />
                                </filter>
                              </link-entity>
                            </link-entity>
                          </link-entity>
                        </link-entity>
                      </entity>
                    </fetch>").Entities;

            CreateApplicationRequirements(applicationRequirementDefinitionDetails, applicationId);
        }

        private void CreateApplicationRequirements(IEnumerable<Entity> applicationRequirementDefinitionDetails, EntityReference applicationId)
        {
            var testSourceTypes = new HashSet<int>();
            var testScoreTypes = new HashSet<Guid>();
            var applicationRequirementsBySourceAndType = new Dictionary<string, List<cmc_applicationrequirement>>();
            foreach (var record in applicationRequirementDefinitionDetails)
            {
                var applicationRequirementDefinitionDetail = record.ToEntity<cmc_applicationrequirementdefinitiondetail>();

                _logger.Trace($"Creating Application Requirement for Definition Detail {applicationRequirementDefinitionDetail.Id}");
                // The InitializeFromRequest will use the relationship between Applicaiton
                // Requirement and Application Definition Detail to create a new Application Requirement
                // record with several fields prepoulated.
                var applicationRequirement = ((InitializeFromResponse)_orgService.Execute(new InitializeFromRequest()
                {
                    TargetEntityName = cmc_applicationrequirement.EntityLogicalName,
                    // XRM Fake Easy requires TargetFieldType All. Ideally this would be create.
                    TargetFieldType = TargetFieldType.All,
                    EntityMoniker = applicationRequirementDefinitionDetail.ToEntityReference()
                })).Entity.ToEntity<cmc_applicationrequirement>();
                applicationRequirement.cmc_applicationid = applicationId;

                if (applicationRequirementDefinitionDetail.cmc_requirementtype?.Value == (int)cmc_applicationrequirementtype.TestScore &&
                     applicationRequirementDefinitionDetail.cmc_testsourcetype != null &&
                     applicationRequirementDefinitionDetail.cmc_testscoretype != null)
                {
                    _logger.Trace("Application Requirement is for a Test Score, this will be created later.");
                    var testSourceType = applicationRequirementDefinitionDetail.cmc_testsourcetype.Value;
                    testSourceTypes.Add(testSourceType);

                    var testScoreType = applicationRequirementDefinitionDetail.cmc_testscoretype.Id;
                    testScoreTypes.Add(testScoreType);

                    var key = BuildTestScoreKey(testScoreType, testSourceType);
                    if (applicationRequirementsBySourceAndType.ContainsKey(key) == false)
                    {
                        applicationRequirementsBySourceAndType.Add(key, new List<cmc_applicationrequirement>());
                    }

                    applicationRequirementsBySourceAndType[key].Add(applicationRequirement);
                    continue;
                }

                _logger.Trace("Creating non-Test Score Application Requirement.");
                _orgService.Create(applicationRequirement);
            }

            if (applicationRequirementsBySourceAndType.Count <= 0)
            {
                _logger.Trace("No Test Score Application Requirements need to be created.");
                return;
            }

            _logger.Trace("Retrieving Test Scores.");
            var testScores = RetrieveTestScores(applicationId, testSourceTypes, testScoreTypes);

            _logger.Trace($"Found {testScores.Count()} Test Scores.");
            // Two edge cases will be handled with this logic:
            // 1. If multiple Application Requirement Definitions exist with the same Test Source and
            //    Test Type, this plugin will create multiple records for the same Test Score.
            // 2. If two Test Scores exist for the same Contact with the same Test Type and Test Source,
            //    the newer Test Score will be used to create the Application Requirement record. 
            foreach(var record in testScores)
            {
                var testScore = record.ToEntity<mshied_testscore>();
                var testScoreId = testScore.ToEntityReference();
                var key = BuildTestScoreKey(testScore.mshied_TestTypeId.Id, (int)testScore.mshied_TestSource);

                _logger.Trace($"Processing Test Scores for key {key}");
                if (applicationRequirementsBySourceAndType.ContainsKey(key) == false)
                {
                    _logger.Trace("No Test Scores for this key or it was already processed. Moving on to the next one.");
                    continue;
                }

                _logger.Trace("Creating Application Requirements for this key.");
                foreach (var applicationRequirement in applicationRequirementsBySourceAndType[key])
                {
                    SetTestScoreFieldsOnApplicationRequirement(applicationRequirement, testScoreId);
                    _orgService.Create(applicationRequirement);
                }

                applicationRequirementsBySourceAndType.Remove(key);
            }

            var unmatchedApplicationRequirements = applicationRequirementsBySourceAndType.SelectMany(kvp => kvp.Value);
            _logger.Trace($"Creating {unmatchedApplicationRequirements.Count()} Application Requirements that could not find a matching Test Score.");
            foreach(var applicationRequirement in unmatchedApplicationRequirements)
            {
                _orgService.Create(applicationRequirement);
            }
        }

        private void UpdateRelatedTestScoreRequirements(cmc_application application)
        {
            _logger.Trace("Retrieving previously setup Test Score Application Requirements that are missing Test Scores.");
            var testScoreRequirements = _orgService.RetrieveMultipleAll(
                $@"<fetch version='1.0' output-format='xml-platform' mapping='logical'>
                      <entity name='cmc_applicationrequirement'>
                        <attribute name='cmc_applicationrequirementid' />
                        <filter type='and'>
                          <condition attribute='cmc_applicationid' operator='eq' value='{application.cmc_applicationId}' />
                          <condition attribute='cmc_requirementtype' operator='eq' value='{(int)cmc_applicationrequirementtype.TestScore}' />
                          <condition attribute='cmc_testscoreid' operator='null' />
                          <condition attribute='statecode' operator='eq' value='{(int)cmc_applicationrequirementdefinitiondetailState.Active}' />
                        </filter>
                        <link-entity name='cmc_applicationrequirementdefinitiondetail' from='cmc_applicationrequirementdefinitiondetailid' to='cmc_requirementdefinitiondetailid' link-type='inner' alias='cmc_applicationrequirementdefinitiondetail'>
                          <attribute name='cmc_testsourcetype' />
                          <attribute name='cmc_testscoretype' />
                          <attribute name='cmc_applicationrequirementdefinitiondetailid' />
                          <filter type='and'>
                            <condition attribute='cmc_requirementtype' operator='eq' value='{(int)cmc_applicationrequirementtype.TestScore}' />
                            <condition attribute='cmc_testsourcetype' operator='not-null' />
                            <condition attribute='cmc_testscoretype' operator='not-null' />
                          </filter>
                        </link-entity>
                      </entity>
                    </fetch>");

            if (testScoreRequirements.Entities.Count == 0)
            {
                _logger.Trace("No Application Requirements found that are missing a Test Score.");
                return;
            }

            var applicationId = application.ToEntityReference();
            var testSourceTypes = new HashSet<int>();
            var testScoreTypes = new HashSet<Guid>();
            var applicationRequirementsBySourceAndType = new Dictionary<string, List<cmc_applicationrequirement>>();
            _logger.Trace("Gathering Test Score values.");
            foreach (var record in testScoreRequirements.Entities)
            {
                _logger.Trace($"Looking for a Test Score for Application Requirement {record.Id}.");
                var applicationRequirement = record.ToEntity<cmc_applicationrequirement>();
                var testScoreType = applicationRequirement.GetAliasedAttributeValue<EntityReference>("cmc_applicationrequirementdefinitiondetail.cmc_testscoretype").Id;
                var testSourceType = applicationRequirement.GetAliasedAttributeValue<OptionSetValue>("cmc_applicationrequirementdefinitiondetail.cmc_testsourcetype").Value;
                testSourceTypes.Add(testSourceType);
                testScoreTypes.Add(testScoreType);
                var key = BuildTestScoreKey(testScoreType, testSourceType);

                if (applicationRequirementsBySourceAndType.ContainsKey(key) == false)
                {
                    applicationRequirementsBySourceAndType.Add(key, new List<cmc_applicationrequirement>());
                }

                applicationRequirementsBySourceAndType[key].Add(applicationRequirement);
            }

            _logger.Trace("Retrieving Test Scores.");
            var testScores = RetrieveTestScores(applicationId, testSourceTypes, testScoreTypes);

            _logger.Trace($"Found {testScores.Count()} Test Scores.");
            foreach (var record in testScores)
            {
                var testScore = record.ToEntity<mshied_testscore>();
                var testScoreId = testScore.ToEntityReference();
                var key = BuildTestScoreKey(testScore.mshied_TestTypeId.Id, (int)testScore.mshied_TestSource);

                _logger.Trace($"Processing Test Scores for key {key}");
                if (applicationRequirementsBySourceAndType.ContainsKey(key) == false)
                {
                    _logger.Trace("No Test Scores for this key or it was already processed. Moving on to the next one.");
                    continue;
                }

                _logger.Trace("Updating Application Requirements for this key.");
                // Two edge cases will be handled with this logic:
                // 1. If multiple Application Requirement Definitions exist with the same Test Source and
                //    Test Type, this plugin will create multiple records for the same Test Score.
                // 2. If two Test Scores exist for the same Contact with the same Test Type and Test Source,
                //    the newer Test Score will be used to create the Application Requirement record. 
                foreach (var applicationRequirement in applicationRequirementsBySourceAndType[key])
                {
                    var updateApplicationRequirement = new cmc_applicationrequirement()
                    {
                        Id = applicationRequirement.Id
                    };

                    SetTestScoreFieldsOnApplicationRequirement(updateApplicationRequirement, testScoreId);
                    _orgService.Update(updateApplicationRequirement);
                }

                applicationRequirementsBySourceAndType.Remove(key);
            }
        }

        private IEnumerable<Entity> RetrieveTestScores(EntityReference applicationId, HashSet<int> testSourceTypes, HashSet<Guid> testScoreTypes)
        {
            // Logic relies on the Test Source Type Option Set on Test Score and Application
            // Requirement Definition Detail having matching values.
            return _orgService.RetrieveMultiple(new FetchExpression(
                $@"<fetch version='1.0' output-format='xml-platform' mapping='logical'>
                      <entity name='mshied_testscore'>
                        <attribute name='mshied_testscoreid' />
                        <attribute name='mshied_testsource' />
                        <attribute name='mshied_testtypeid' />
                        <order attribute='mshied_testdate' descending='true' />
                        <filter>
                          <condition attribute='statecode' operator='eq' value='{(int)mshied_testscoreState.Active}' />
                          <condition attribute='mshied_testsource' operator='in'>
                            <value>{string.Join("</value><value>", testSourceTypes)}</value>
                          </condition>
                          <condition attribute='mshied_testtypeid' operator='in'>
                             <value>{string.Join("</value><value>", testScoreTypes)}</value>
                          </condition>
                        </filter>
                        <link-entity name='contact' from='contactid' to='mshied_studentid' link-type='inner'>
                          <link-entity name='cmc_applicationregistration' from='cmc_contactid' to='contactid' link-type='inner'>
                            <link-entity name='cmc_application' from='cmc_applicationregistration' to='cmc_applicationregistrationid' link-type='inner'>
                              <filter type='and'>
                                <condition attribute='cmc_applicationid' operator='eq' value='{applicationId.Id}' />
                              </filter>
                            </link-entity>
                          </link-entity>
                        </link-entity>
                      </entity>
                    </fetch>")).Entities;
        }

        private string BuildTestScoreKey(Guid testScoreType, int testSourceType)
        {
            return $"{testSourceType}_{testScoreType}";
        }

        private void SetTestScoreFieldsOnApplicationRequirement(cmc_applicationrequirement applicationRequirement, EntityReference testScoreId)
        {
            _logger.Trace("Test Score found. Setting Test Score on Application Requirement and marking it Submitted.");
            applicationRequirement.cmc_testscoreid = testScoreId;
            applicationRequirement.cmc_requirementstatus = new OptionSetValue((int)cmc_applicationrequirementstatus.Received);
            applicationRequirement.cmc_receiveddatetime = DateTime.UtcNow;
        }
        #endregion

        #region SetDefaultFieldsOnApplicationPlugin
        public void SetDefaultFields(IExecutionContext executionContext)
        {
            _logger.Trace("Starting SetDefaultFieldsOnApplicationPlugin");
            var serviceProvider = executionContext.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();

            _logger.Trace("Retrieving Target.");
            var target = pluginContext.GetTargetEntity<cmc_application>();
            cmc_application preImage = null;

            if (pluginContext.PreEntityImages.Contains("Target"))
            {
                preImage = pluginContext.GetPreEntityImage<cmc_application>("Target");
            }

            if (preImage != null && preImage.cmc_applicationregistration?.Id == target.cmc_applicationregistration?.Id)
            {
                _logger.Trace("Application Registration has not changed. Exiting early.");
                return;
            }

            SetDefaultFields(target, preImage);

            _logger.Trace("Exiting SetDefaultFieldsOnApplicationPlugin");
        }

        private void SetDefaultFields(cmc_application application, cmc_application preImage)
        {
            if (application.cmc_applicationregistration == null)
            {
                _logger.Trace("Application Registration is null. Clearing Contact.");
                application.cmc_contactid = null;
                return;
            }

            _logger.Trace("Retrieving Application Registrations.");
            var applicationRegistration = _orgService.Retrieve<cmc_applicationregistration>(
                application.cmc_applicationregistration,
                new ColumnSet("cmc_contactid", "cmc_applicationstatus"));

            // Set Contact on Application if one of the following is true
            // 1. Contact on Application was not just updated to the Application Registration's Contact
            // 2. Contact is not being updated on this transaction and it was not previously the 
            // Application Registration's Contact
            var latestContactId = application.Contains("cmc_application") || preImage == null
                                  ? application.cmc_contactid
                                  : preImage.cmc_contactid;
            if (latestContactId?.Id != applicationRegistration.cmc_contactid?.Id)
            {
                _logger.Trace("Setting the Contact Id.");
                application.cmc_contactid = applicationRegistration.cmc_contactid;
            }

            // Only set Applicaiton Status if it hasn't been set already previously set.
            // If it was just cleared out though, still overwrite it.
            if (application.cmc_applicationstatus == null && 
                (application.Contains("cmc_applicationstatus") == true || 
                preImage?.cmc_applicationstatus == null))
            {
                _logger.Trace("Setting Application Status.");
                application.cmc_applicationstatus = applicationRegistration.cmc_applicationstatus;
            }
        }
        #endregion
    }
}
