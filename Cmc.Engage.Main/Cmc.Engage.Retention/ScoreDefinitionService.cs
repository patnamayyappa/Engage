using System;
using System.Linq;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;
using Cmc.Engage.Models;
using Cmc.Engage.Common;

namespace Cmc.Engage.Retention
{
    public class ScoreDefinitionService : IScoreDefinitionService
    {
        private readonly ILogger _tracer;
        private IOrganizationService _orgService;
        private ILanguageService _languageService;
        public ScoreDefinitionService(ILogger tracer, IOrganizationService orgService, ILanguageService languageService)
        {
            _tracer = tracer ?? throw new ArgumentException(nameof(tracer));
            _orgService = orgService;
            _languageService = languageService;
        }
        public void AssociateDisassociateScoreDefinition(IExecutionContext context)
        {
            _tracer.Info($"Entered into ModifyRetrieveScoreDefination");

            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();

            if (pluginContext.InputParameters.Contains("Target"))
            {
                _tracer.Info($"InputParameters");
                var targetEntity = (EntityReference)pluginContext.InputParameters["Target"];
                _tracer.Info($"AssociateScoreDefination : Target Id is :{targetEntity.Id} and schema name is {targetEntity.LogicalName}");
                if (targetEntity.LogicalName == cmc_scoredefinition.EntityLogicalName)
                {                  
                    EntityReferenceCollection relatedEntityReferenceCollection = null;                                    
                    // related entities collection
                    if (pluginContext.InputParameters.Contains("RelatedEntities"))
                    {
                        relatedEntityReferenceCollection = (EntityReferenceCollection)pluginContext.InputParameters["RelatedEntities"];
                        _tracer.Info($"AssociateScoreDefinition : Related Entities Count :{relatedEntityReferenceCollection.Count} and related instance {relatedEntityReferenceCollection.FirstOrDefault()?.LogicalName}");
                        var relatedEntity = relatedEntityReferenceCollection[0];

                        var scoreDefinitionEntity = (cmc_scoredefinition)_orgService.Retrieve(targetEntity.LogicalName, targetEntity.Id, new ColumnSet(true));
                        _tracer.Info($"target entity Id:{scoreDefinitionEntity.Id} name of scoredefination {scoreDefinitionEntity.cmc_scoredefinitionname} and base entity {scoreDefinitionEntity.cmc_baseentity}");


                        if (relatedEntity.LogicalName == "list")
                        {
                            var marketingListEntity = (List)_orgService.Retrieve(relatedEntity.LogicalName, relatedEntity.Id, new ColumnSet(true));
                            _tracer.Info($"target entity Id:{relatedEntity.Id}:{marketingListEntity.CreatedFromCode}:{marketingListEntity.ListName}");

                            _tracer.Info($" The conditon value :{marketingListEntity.CreatedFromCode.ToString().ToLower()} and {scoreDefinitionEntity.cmc_baseentity.ToLower()}");

                            if (marketingListEntity.CreatedFromCode.ToString().ToLower() != scoreDefinitionEntity.cmc_baseentity.ToLower())
                            {
                                _tracer.Info($"The scoringFactor baseentiy {marketingListEntity.CreatedFromCode} and scoreDefinition baseentity {scoreDefinitionEntity.cmc_baseentity} is not equal,It's not Associated");
                                throw new InvalidPluginExecutionException(_languageService.Get("The Student Group's base entity is different from the Score Definition's base entity. Select a Student Group that has the same base entity as the Score Definition."));
                            }
                        }
                        if (relatedEntity.LogicalName == "cmc_scoringfactor")
                        {
                            var scoringFactorEntiy = (cmc_scoringfactor)_orgService.Retrieve(relatedEntity.LogicalName, relatedEntity.Id, new ColumnSet(true));
                            _tracer.Info($"target entity Id:{relatedEntity.Id}:{scoringFactorEntiy.cmc_baseentity}:{scoringFactorEntiy.cmc_scoringfactorname}");

                            if (scoringFactorEntiy.cmc_baseentity.ToString().ToLower() != scoreDefinitionEntity.cmc_baseentity.ToLower())
                            {
                                _tracer.Info($"The scoringFactor baseentiy {scoringFactorEntiy.cmc_baseentity} and scoreDefinition baseentity {scoreDefinitionEntity.cmc_baseentity} is not equal, It's not Associated");                               
                                throw new InvalidPluginExecutionException(_languageService.Get("The Scoring Factor's base entity is different from the Score Definition's base entity. Select a Scoring Factor that has the same base entity as the score definition."));
                            }
                        }
                    }
                }

            }
        }

    }
}
