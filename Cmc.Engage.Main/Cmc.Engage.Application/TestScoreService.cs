using Microsoft.Xrm.Sdk;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Core.Xrm.ServerExtension.Core;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk.Query;
using System.Linq;
using System;

namespace Cmc.Engage.Application
{
    public class TestScoreService : ITestScoreService
    {
        private IOrganizationService _orgService;

        private readonly ILogger _logger;
        public TestScoreService(ILogger tracer, IOrganizationService orgService)
        {
            _logger = tracer;
            _orgService = orgService;
        }
        /// <summary>
        /// Main method to set Testscores for Contact.
        /// </summary>
        /// <param name="context"></param>
        public void SetTestSuperandBestScores(IExecutionContext context)
        {
            _logger.Trace("In service to Set Contact Super and Best Scores");

            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();
            var mshied_testscore = pluginContext.GetPostEntityImage<mshied_testscore>("PostImage");

            if (mshied_testscore.mshied_TestTypeId == null) { _logger.Trace("Test Type is not associated."); return; }
            if (mshied_testscore.mshied_StudentID == null) { _logger.Trace("Test is not associated to any contact."); return; }

            mshied_testtype testType = _orgService.Retrieve(mshied_testscore.mshied_TestTypeId, new ColumnSet(true)).ToEntity<mshied_testtype>();

            _logger.Trace($"Test type : {testType["mshied_name"]}");

            if ((testType.mshied_name).ToString() == "ACT")
                SetACTSuperandBestScore(mshied_testscore.mshied_StudentID.Id, mshied_testscore.mshied_TestTypeId.Id);

            else if ((testType.mshied_name).ToString() == "SAT")
                SetSATSuperandBestScore(mshied_testscore.mshied_StudentID.Id, mshied_testscore.mshied_TestTypeId.Id);
            if (testType.cmc_equivalentscoreschemaname == null || testType.cmc_testscoreschemaname == null) { _logger.Trace($"Equivalentscore Schemaname - '{testType.cmc_equivalentscoreschemaname}' or Testscore Schemaname - '{testType.cmc_testscoreschemaname}'is not associated"); return; }
            SetEquivalentScore(mshied_testscore, testType.cmc_equivalentscoreschemaname, testType.cmc_testscoreschemaname);
        }
        /// <summary>
        /// Calculate ACT Test Super and best score for a contact.
        /// </summary>
        /// <param name="contact"></param>
        /// <param name="testtype"></param>
        private void SetACTSuperandBestScore(Guid contact, Guid testtype)
        {
            _logger.Trace($"Calculating ACT Super and Best Scores.");

            Contact ContactEntity = new Contact() { Id = contact };
            var fetch =
                $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' aggregate='true' >
                                <entity name='mshied_testscore' >
                                    <attribute name='mshied_actscience' alias='cmc_science_max' aggregate='max' />
                                    <attribute name='mshied_actmath' alias='cmc_math_max' aggregate='max' />
                                    <attribute name='mshied_actenglish' alias='cmc_english_max' aggregate='max' />
                                    <attribute name='mshied_actreading' alias='cmc_readingscore_max' aggregate='max' />
                                    <attribute name='mshied_actcomposite' alias='cmc_compositescore_max' aggregate='max'/>
                                    <filter type='and' >
                                        <condition attribute='statuscode' operator='eq' value='{(int)mshied_testscore_statuscode.Active}' />
                                        <condition attribute='mshied_studentid' operator='eq' value='{contact}' />
                                        <condition attribute='mshied_testtypeid' operator='eq' value='{testtype}' />
                                        <condition attribute='cmc_includeinscorecalculations' operator='eq' value='1' />
                                    </filter>
                                </entity>
                            </fetch>";
            var testScores = _orgService.RetrieveMultiple(new FetchExpression(fetch));
            if (testScores.Entities.Count <= 0)
            {
                _logger.Trace("No ACT test score marked for score calculation resetting contact's super and best score");
                ContactEntity.cmc_actsuperscore = null;
                ContactEntity.cmc_actbest = null;
                _orgService.Update(ContactEntity);
                return;
            }

            var data = testScores.Entities.FirstOrDefault();

            int scienceMaxScore = data.GetAliasedAttributeValue<int?>("cmc_science_max") ?? 0;
            int mathMaxScore = data.GetAliasedAttributeValue<int?>("cmc_math_max") ?? 0;
            int englishMaxScore = data.GetAliasedAttributeValue<int?>("cmc_english_max") ?? 0;
            int readingMaxScore = data.GetAliasedAttributeValue<int?>("cmc_readingscore_max") ?? 0;
            int compositeMaxScore = data.GetAliasedAttributeValue<int?>("cmc_compositescore_max") ?? 0;

            
            ContactEntity.cmc_actsuperscore =
                Convert.ToInt32((scienceMaxScore + mathMaxScore + englishMaxScore + readingMaxScore) / 4);
            ContactEntity.cmc_actbest = compositeMaxScore;
            _logger.Trace(
                $"ACT test Best Score is : {compositeMaxScore} and Super score is : {ContactEntity.cmc_actsuperscore}");
            _orgService.Update(ContactEntity);
        }
        /// <summary>
        /// Calculate SAT Test Super and best score for a contact.
        /// </summary>
        /// <param name="contact"></param>
        /// <param name="testtype"></param>
        private void SetSATSuperandBestScore(Guid contact, Guid testtype)
        {
            _logger.Trace($"Calculating SAT Super and Best Scores.");

            Contact ContactEntity = new Contact() { Id = contact };
            var fetch = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' aggregate='true' >
                                <entity name='mshied_testscore' >
                                    <attribute name='mshied_satevidencebasedreadingandwritingsection' alias='cmc_evidencebased_max' aggregate='max' />
                                    <attribute name='mshied_satmathsection' alias='cmc_mathsection_max' aggregate='max' />
                                    <attribute name='mshied_sattotalscore' alias='cmc_sattotalscore_max' aggregate='max' />
                                    <filter type='and' >
                                        <condition attribute='statuscode' operator='eq' value='{(int)mshied_testscore_statuscode.Active}' />                                        
                                        <condition attribute='mshied_studentid' operator='eq' value='{contact}' />
                                        <condition attribute='mshied_testtypeid' operator='eq' value='{testtype}' />
                                        <condition attribute='cmc_includeinscorecalculations' operator='eq' value='1' />
                                    </filter>
                                </entity>
                            </fetch>";
            var testScores = _orgService.RetrieveMultiple(new FetchExpression(fetch));
            if (testScores.Entities.Count <= 0)
            {
                _logger.Trace("No SAT test score marked for score calculation resetting contact's super and best score");
                ContactEntity.cmc_satsuperscore = null;
                ContactEntity.cmc_satbest = null;
                _orgService.Update(ContactEntity);
                return;
            }
            var data = testScores.Entities.FirstOrDefault();

            int evidenceBasedMaxScore = data.GetAliasedAttributeValue<int?>("cmc_evidencebased_max") ?? 0;
            int mathSectionMaxScore = data.GetAliasedAttributeValue<int?>("cmc_mathsection_max") ?? 0;
            int satBestScore = data.GetAliasedAttributeValue<int?>("cmc_sattotalscore_max") ?? 0;
            
            ContactEntity.cmc_satsuperscore = evidenceBasedMaxScore + mathSectionMaxScore;
            ContactEntity.cmc_satbest = satBestScore;
            _logger.Trace($"SAT test Best Score is : {satBestScore} and Super score is : {ContactEntity.cmc_satsuperscore}");
            _orgService.Update(ContactEntity);
        }
        /// <summary>
        /// Caculate Equivalent Score for different test type based on provided Consideration schema name.
        /// </summary>
        /// <param name="testScore"></param>
        /// <param name="equivalentscoreSchemaname"></param>
        /// <param name="testscoreSchemaname"></param>
        private void SetEquivalentScore(mshied_testscore testScore,string equivalentscoreSchemaname,string testscoreSchemaname)
        {
            _logger.Trace($"Calculating Equivalent Score.");

            decimal? score = (decimal?)testScore.GetAttributeValue<dynamic>(testscoreSchemaname.Split('.')[1]);
            if(score == null || score ==0){_logger.Trace($"{testscoreSchemaname.Split('.')[1]} don't have any value.");return;}
            _logger.Trace($"Value from the Attribute {testscoreSchemaname} is {score}.");

            var fetch = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='cmc_testscoreconversion'> 
                                <attribute name='cmc_testscoreconversionid' />   
                                <attribute name='cmc_equivalentscore' />
                                <filter type='and'>
                                  <condition attribute='statuscode' operator='eq' value='{(int)cmc_testscoreconversion_statuscode.Active}' />
                                  <condition attribute='cmc_testtypeid' operator='eq' value='{testScore.mshied_TestTypeId.Id}' />
                                  <condition attribute='cmc_minimumscore' operator='le' value='{score}' />
                                  <condition attribute='cmc_maximumscore' operator='ge' value='{score}' />
                                </filter>
                              </entity>
                            </fetch>";
            var testscoreConversion = _orgService.RetrieveMultiple(new FetchExpression(fetch));
            if (testscoreConversion.Entities.Count != 1)
            {
                _logger.Trace($"Found '{testscoreConversion.Entities.Count}' Testscore Conversion records,cannot update Equivalent score.Always should have one equivalent conversion record for a given range");
                return;
            }

            cmc_testscoreconversion equivalentScore = testscoreConversion.Entities.First().ToEntity<cmc_testscoreconversion>();

            if(equivalentScore.cmc_equivalentscore == null){_logger.Trace($"Equivalent Score is null");return;}

            _logger.Trace($"Equivalent Score for the Attribute {equivalentscoreSchemaname} is {equivalentScore.cmc_equivalentscore}.");
            mshied_testscore calculatedTestScore = new mshied_testscore() { Id = testScore.Id };
            calculatedTestScore[equivalentscoreSchemaname.Split('.')[1]] = equivalentScore.cmc_equivalentscore;
            _orgService.Update(calculatedTestScore);
        }

    }
}
