using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common;
using Cmc.Engage.Common.Utilities.Helpers;
using Cmc.Engage.Models;
using Cmc.Engage.Lifecycle;
using Cmc.Engage.Retention.FetchBuilderSupport;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Entity = Microsoft.Xrm.Sdk.Entity;


namespace Cmc.Engage.Retention
{
	public class RetentionScoreCalculatorService : IRetentionScoreCalculatorService
	{
		IDictionary<Guid, IEnumerable<cmc_scoringfactor>> mapOfScoreDefinitionToListOfScoringFactors;
		EntityConditionEvaluator _contactEvaluator;
		int _createCount;
		int _updateCount;
		int _skippedCount;
		int _deactivatedCount;
		IDictionary<string, EntityMetadata> mapOfEntityNameToEntityMetadata;
		private readonly ILogger _logger;
		private IOrganizationService _orgService;
		private IDOMService _domService;
		private readonly IBingMapService _bingMapService;

		private readonly Guid _alumniRoleId = new Guid("3BB3FEDD-0333-E811-A95F-000D3A11FE32");

		private readonly IConfigurationService _retriveConfigurationDetails;
		public RetentionScoreCalculatorService(ILogger tracer,IBingMapService bingMapService, IOrganizationService orgService, IConfigurationService retriveConfigurationDetails, IDOMService domService)
		{
			_logger = tracer;
			_bingMapService = bingMapService;
			_retriveConfigurationDetails = retriveConfigurationDetails ??
										   throw new ArgumentException(nameof(retriveConfigurationDetails));
			_orgService = orgService;
			_domService = domService;
		}
		public void RetentionScoreLogic()
		{
			CalculateRetentionScores();
		}

		private void CalculateRetentionScores()
		{
			_logger.Info($"Retrieving active factor filters");
			var activeMarketingLists = RetrieveActiveFactorFilters();

			_logger.Info($"Retrieving active factors");
			var activeScoringFactors = RetrieveActiveScoringFactors();

			var mapOfScoreDefintionToListOfMarketingLists = activeMarketingLists.GroupBy(f => (Guid)((AliasedValue)f["ac.cmc_scoredefinitionid"]).Value);
			mapOfScoreDefinitionToListOfScoringFactors = activeScoringFactors.GroupBy(f => (Guid)((AliasedValue)f["cmc_scoredefinition_scoringfactor1.cmc_scoredefinitionid"]).Value).ToDictionary(g => g.Key, g => g.AsEnumerable());

			mapOfEntityNameToEntityMetadata = new Dictionary<string, EntityMetadata>();
			_contactEvaluator = new EntityConditionEvaluator(mapOfEntityNameToEntityMetadata);

			foreach (var scoreDefinitionToListOfMarketingListsKeyValuePair in mapOfScoreDefintionToListOfMarketingLists)
			{
				try
				{
					var scoreDefinitionInstance = _orgService.Retrieve(cmc_scoredefinition.EntityLogicalName, scoreDefinitionToListOfMarketingListsKeyValuePair.Key, new ColumnSet("cmc_baseentity", "cmc_scheduleinterval", "cmc_datelastrun", "cmc_scoredefinitionname")).ToEntity<cmc_scoredefinition>();
					if (scoreDefinitionInstance.cmc_datelastrun==null ||
						(scoreDefinitionInstance.cmc_scheduleinterval == null || 
						scoreDefinitionInstance.cmc_scheduleinterval.Equals(0))||
						scoreDefinitionInstance.cmc_datelastrun?.AddDays((int)scoreDefinitionInstance.cmc_scheduleinterval).Date == DateTime.UtcNow.Date
						
						)
						CalculateRetentionScoresForScoreDefinition(scoreDefinitionToListOfMarketingListsKeyValuePair);
					else
					{
						_logger.Info($"score definition last run date:{scoreDefinitionInstance.cmc_datelastrun}.");
						_logger.Info($"score definition schedule interval:{scoreDefinitionInstance.cmc_scheduleinterval}.");
						_logger.Info($"current date:{DateTime.Today}");
						_logger.Info($"score definition :{scoreDefinitionInstance.cmc_scoredefinitionname} is not picked as it did not match the schedule criteria.");
					}
				}
				catch (Exception e)
				{
					_logger.Fatal($"Error while processing the ScoreDefinition Id:{scoreDefinitionToListOfMarketingListsKeyValuePair.Key} with the exception:{e}");
				}
			}
		}

		private EntityMetadata LoadEntityMetadata(string entityName)
		{
			EntityMetadata entityMetadata;
			if (!mapOfEntityNameToEntityMetadata.TryGetValue(entityName, out entityMetadata))
			{
				_logger.Info($"Loading {entityName} attribute metadata");
				entityMetadata = _orgService.GetEntityMetadata(entityName);
				mapOfEntityNameToEntityMetadata.Add(entityName, entityMetadata);//som
			}

			return entityMetadata;
		}

		private void CalculateRetentionScoresForScoreDefinition(IGrouping<Guid, List> scoreDefinitionFilters)
		{
			_logger.Info($"Processing student group...");
		
			//get the target entity and the target attribute of the Score Definition.
			var scoreDefinitionInstance = _orgService.Retrieve(cmc_scoredefinition.EntityLogicalName, scoreDefinitionFilters.Key, new ColumnSet("cmc_baseentity", "cmc_targetattributename")).ToEntity<cmc_scoredefinition>();
			_logger.Info($"splitting the attribute name:{scoreDefinitionInstance.cmc_targetattributename} of the score definition:{scoreDefinitionInstance.Id}");
			string[] parts = scoreDefinitionInstance.cmc_targetattributename.Split('.');
			scoreDefinitionInstance.cmc_targetattributename = parts.Last();
			IEnumerable<cmc_scoringfactor> scoringFactors;
			if (!mapOfScoreDefinitionToListOfScoringFactors.TryGetValue(scoreDefinitionFilters.Key, out scoringFactors))
			{
				scoringFactors = Enumerable.Empty<cmc_scoringfactor>();
			}
			_logger.Info($"Preparing the  scoring factors query for the score definition");
			// get the fetchbuilder expression for the groupfactor
			var getScoringFactorsBuilder = GetScoringFactorsQueryExpression(scoringFactors, scoreDefinitionInstance);
			if (!getScoringFactorsBuilder.Item1)
			{
				_logger.Error($"Retention score histories: no group factors have valid attribute names.");
				return;
			}
			var fb = getScoringFactorsBuilder.Item2;
			_logger.Info($"Retention score: group factors fetch expression : {fb.ToString()}");

			// fetching associated students from student group list into two steps.
			// step 1: get all students from static groups having link entity relation conditions
			// step 2: get all students from dynamic groups by execution of query seperately, cause query expressions are different , and we cannot get students by combining all query expressions.


			// Step 1 :get all static type of marketing list id's
			var staticMarketingLists = scoreDefinitionFilters.Where(factorFilter => factorFilter.Type != null &&
																						   (
																							(bool)!factorFilter.Type)) // static type of marketing list
																	   .Select(r => (object)r.ListId);
			_logger.Info($"Retention score : Static type conditions to the fetch builder  count is : {staticMarketingLists.Count()}");
			
			var instances = new List<Entity>();
			var scoringFactorsQuery = fb.ToString();
			if (staticMarketingLists.Any())
			{
				//fetch the students using Fetch expression builder.
				_logger.Debug($"FetchXml before adding static marketing list condition: {fb.ToString()}");

				// add the staic list filter condition.
				var entityList = fb.FindOrAddEntity(GetRelatedEntity(scoreDefinitionInstance));
				entityList.AddCondition("listid", "in", staticMarketingLists.ToArray());
				_logger.Debug($"after adding static {scoreDefinitionInstance.LogicalName} fetchxml: {fb}");
				instances.AddRange(RetrieveAll<Entity>(fb).ToList());
			}

			_logger.Debug($"Fetch count of  {scoreDefinitionInstance.LogicalName} instances for static  fetchxml: {instances.Count()}");
			// convert to SerializeObject to create a the copy from the string.
			var scoringFactorExpression = ((FetchXmlToQueryExpressionResponse)_orgService.Execute(new FetchXmlToQueryExpressionRequest { FetchXml = scoringFactorsQuery })).Query; // get the copy 

			// Step 2: get dynamic group student list and execute each student group with groupfactors and store the students into collection.
			scoreDefinitionFilters.Where(factorFilter => factorFilter.Type != null &&
												(
												 (bool)factorFilter.Type)).ToList().ForEach(factorFilter =>
												 {

													 _logger.Debug($"adding dynamic {scoreDefinitionInstance.LogicalName} fetchxml: {factorFilter.Query}");

													 // convert the fetch expression to Icondition , so adding only the conditions rather than entities
													 var dynamicStudentGroupQuery = ((FetchXmlToQueryExpressionResponse)_orgService.Execute(new FetchXmlToQueryExpressionRequest { FetchXml = factorFilter.Query })).Query;

													 // add the columns
													 if (scoringFactorExpression.ColumnSet.Columns.Any())
														 dynamicStudentGroupQuery.ColumnSet.Columns.AddRange(scoringFactorExpression.ColumnSet.Columns);

													 // adding the Conditions
													 if (scoringFactorExpression.Criteria.Conditions.Any())
														 dynamicStudentGroupQuery.Criteria.Conditions.AddRange(scoringFactorExpression.Criteria.Conditions);

													 // adding the link entities
													 if (scoringFactorExpression.LinkEntities.Any())
														 dynamicStudentGroupQuery.LinkEntities.AddRange(scoringFactorExpression.LinkEntities);

													 _logger.Debug($"final dynamic {scoreDefinitionInstance.LogicalName} fetchxml: {dynamicStudentGroupQuery.ToString()}");

													 instances.AddRange(_orgService.RetrieveMultipleAll(dynamicStudentGroupQuery).Entities.Cast<Entity>());

												 });

			_logger.Debug($"final {scoreDefinitionInstance.LogicalName} count: {instances.Count} after execution of all dynamic groups.");

			_createCount = 0;
			_updateCount = 0;
			_skippedCount = 0;
			_deactivatedCount = 0;

			var usedInstanceIds = new HashSet<Guid>();
			using (var executeMultipleBuffer = new ExecuteMultipleBuffer(_orgService))
			{
				foreach (var instance in instances) // pick only the distinct students.
				{
					if (!usedInstanceIds.Contains(instance.Id))
					{
						usedInstanceIds.Add(instance.Id); // add to hash set to aviod the duplicates.
						_logger.Info($"calculating the Retention score for {instance.LogicalName} with id:{instance.Id}");
						int score = CalculateRetentionScoreForScoreDefinition(instance, scoreDefinitionFilters.Key, scoringFactors, executeMultipleBuffer);
						_logger.Info($"updating the score anf retention score history for {instance.LogicalName} with id:{instance.Id}");
						UpdateRetentionScoreHistoryRecords(instance, scoreDefinitionInstance, scoreDefinitionFilters.Key, executeMultipleBuffer, score);
					}
				}
			}

			_logger.Info($"Retention score histories: {_createCount} created, {_skippedCount} already matched, {_deactivatedCount} deactivated");
		}

		private Tuple<bool, FetchBuilder> GetScoringFactorsQueryExpression(IEnumerable<cmc_scoringfactor> groupFactors, cmc_scoredefinition scoreDefinition)
		{

			var fb = new FetchBuilder();
			fb.EntityName = scoreDefinition.cmc_baseentity;
			fb.AddAttribute(scoreDefinition.cmc_targetattributename);
			fb.AddCondition("statecode", "eq", 0);
			int aliasCount = 0;
			bool isScoreFactorAttributeAdded = false;
			foreach (var scoringFactor in groupFactors.Where(f => f.cmc_attributename != null))
			{
				var attributePath = ParseAttributeSchemaChain(scoringFactor);
				if (attributePath.RelatedEntities.Count == 0)
				{
					LoadEntityMetadata(scoreDefinition.cmc_baseentity);
					if (mapOfEntityNameToEntityMetadata.ContainsKey(scoreDefinition.cmc_baseentity) &&
						mapOfEntityNameToEntityMetadata[scoreDefinition.cmc_baseentity].Attributes.Any(r => r.LogicalName == attributePath.AttributeName))
					{
						isScoreFactorAttributeAdded = true;
						fb.AddAttribute(attributePath.AttributeName);
					}					
					else
					{
						_logger.Warn($"Retention score histories: group factors not have valid attribute name {attributePath.AttributeName}.");
					}
				}
				else
				{
					if (attributePath.RelatedEntities.All(re => IsPrimaryIdAttribute(re.EntityName, re.FromAttribute)))
					{
						var entity = (ILinkEntity)fb.FindOrAddEntity(attributePath.RelatedEntities, "outer");
						if (entity.Alias == null)
						{
							entity.Alias = $"e{++aliasCount}";
						}
						_contactEvaluator.AddAttributeAlias(attributePath.ToString(), $"{entity.Alias}.{attributePath.AttributeName}");

						if (mapOfEntityNameToEntityMetadata.ContainsKey(entity.EntityName) && mapOfEntityNameToEntityMetadata[entity.EntityName].Attributes.Any(r => r.LogicalName == attributePath.AttributeName))
						{
							isScoreFactorAttributeAdded = true;
							entity.AddAttribute(attributePath.AttributeName);

						}
						else
						{
							_logger.Info($"Retention score histories: group factors not have valid attribute name {attributePath.AttributeName}.");
						}
					}
				}
			}

			return new Tuple<bool, FetchBuilder>(isScoreFactorAttributeAdded, fb);
		}
		/// <summary>
		/// returns the Attribute Path for the scoring factor
		/// </summary>
		/// <param name="conditionEntity"></param>
		/// <returns></returns>
		private AttributePath ParseAttributeSchemaChain(cmc_scoringfactor conditionEntity)
		{
			// example path: account(accountid=parentcustomerid).contact(primarycontactid=contactid).fullname
			if (string.IsNullOrWhiteSpace(conditionEntity.cmc_attributename))
				throw new InvalidOperationException($"Blank attribute is not allowed."); ;

			string[] parts = conditionEntity.cmc_attributename.Split('.');
			AttributePath result = new AttributePath();
			result.AttributeName = parts.Last();
			foreach (var relationship in _domService.GetEntityChainForAttributeSchemaString(conditionEntity.cmc_attributename))
			{
				result.RelatedEntities.Add(new RelatedEntity
				{

					EntityName = relationship.TargetEntity,
					FromAttribute = relationship.TargetAttribute,
					ToAttribute = relationship.SourceAttribute
				});
			}
			return result;
		}
		private bool IsPrimaryIdAttribute(string entityName, string attributeName)
		{
			EntityMetadata entityMetadata = LoadEntityMetadata(entityName);
			return entityMetadata.PrimaryIdAttribute == attributeName;
		}

		private int CalculateRetentionScoreForScoreDefinition(Entity instance, Guid factorGroupId, IEnumerable<cmc_scoringfactor> groupFactors, ExecuteMultipleBuffer executeMultipleBuffer)
		{
			int score = 0;

			var childEntityFactorGroups = from f in groupFactors
										  let path = ParseAttributeSchemaChain(f)
										  where
											 path.RelatedEntities.Count > 0 &&
											 path.RelatedEntities.Any(re => !IsPrimaryIdAttribute(re.EntityName, re.FromAttribute))
										  group f by path.ToString(true) into g
										  select g;

			var studentFactors = from f in groupFactors
								 let path = ParseAttributeSchemaChain(f)
								 where
									path.RelatedEntities.Count == 0 ||
									path.RelatedEntities.All(re => IsPrimaryIdAttribute(re.EntityName, re.FromAttribute))
								 select f;


			// fetch related child records to evaluate them
			foreach (var group in childEntityFactorGroups)
			{
				FetchBuilder childQuery = BuildChildRecordQuery(instance, group);

				_logger.Debug($"Child record query: {childQuery}");

				var children = RetrieveAll<Entity>(childQuery);
				var evaluator = new EntityConditionEvaluator(mapOfEntityNameToEntityMetadata);
				foreach (var factor in group)
				{
					_logger.Debug($"Evaluating {factor.cmc_scoringfactorname}...");
					foreach (Entity child in children)
					{
						if (evaluator.IsMatch(child, factor, ParseAttributeSchemaChain(factor)))
						{
							_logger.Debug($"Matched (add {factor.cmc_points} points)");
							score += factor.cmc_points ?? 0;
							break;
						}
					}
				}
			}

			foreach (var factor in studentFactors)
			{
				_logger.Debug($"Evaluating {factor.cmc_scoringfactorname}...");
				if (_contactEvaluator.IsMatch(instance, factor,ParseAttributeSchemaChain(factor)))
				{
					_logger.Debug($"Matched (add {factor.cmc_points} points)");
					score += factor.cmc_points ?? 0;
				}
			}
			_logger.Debug($"Calculated score for {instance.Id}: {score}.");

			return score;
		}

		private  FetchBuilder BuildChildRecordQuery(Entity instance, IEnumerable<cmc_scoringfactor> factors)
		{
			// factors are already grouped by path to the entity, so just use the first
			var path = ParseAttributeSchemaChain(factors.First());

			// use a stack to make it easier to work backwards
			var relatedEntities = new Stack<RelatedEntity>(path.RelatedEntities);

			var top = relatedEntities.Pop();

			FetchBuilder childQuery = new FetchBuilder();
			childQuery.EntityName = top.EntityName;

			// add all the attributes for this group to the query
			childQuery.AddAttributes(factors.Select(f => ParseAttributeSchemaChain(f).AttributeName));

			// add link entities to the query as needed
			RelatedEntity bottom = top;
			IEntity entity = childQuery;
			while (relatedEntities.Count > 0)
			{
				RelatedEntity previous = bottom;
				bottom = relatedEntities.Pop();
				entity = entity.AddLinkEntity(bottom.EntityName, previous.ToAttribute, previous.FromAttribute);
			}

			// we might need one more link entity at the end, depending on the relationship direction
			string lookupAttribute = bottom.FromAttribute;
			

			// only retrieve rows for this student
			entity.AddCondition(lookupAttribute, "eq", instance.Id);
			return childQuery;
		}

		private void LogResult<T>(string message, Task<T> t, Func<T, string> successFormatter = null)
		{
			switch (t.Status)
			{
				case TaskStatus.RanToCompletion:
					string result = successFormatter?.Invoke(t.Result);
					if (result != null)
					{
						_logger.Debug($"Succeeded {message}: {result}");
					}
					else
					{
						_logger.Debug($"Succeeded {message}");
					}
					break;
				case TaskStatus.Canceled:
					_logger.Warn($"Aborted {message} due to previous errors");
					break;
				case TaskStatus.Faulted:
					_logger.Error($"Failed {message}: {t.Exception}.");
					break;

				default:
					Console.WriteLine($"Unexpected task status: {t.Status}");
					break;
			}
		}

		private void UpdateRetentionScoreHistoryRecords(Entity instance, cmc_scoredefinition scoreDefinitionInstance, Guid factorGroupId, ExecuteMultipleBuffer executeMultipleBuffer, int score)
		{
			Entity instanceChanges = new Entity(instance.LogicalName)
			{
				Id = instance.Id
			};
			instanceChanges["cmc_currentretentionscoredate"] = DateTime.UtcNow;
			_logger.Info($"checking if the new score calculated is different from the current score for {instance.LogicalName} with id:{instance.Id} for the target attribute:{scoreDefinitionInstance.cmc_targetattributename}");
			if (instance.GetAttributeValue<int>(scoreDefinitionInstance.cmc_targetattributename) != score)
			{
				_logger.Info($"updating the new score for {instance.LogicalName} with id:{instance.Id} for the target attribute:{scoreDefinitionInstance.cmc_targetattributename}");
				instanceChanges[scoreDefinitionInstance.cmc_targetattributename] = score;
			}

			scoreDefinitionInstance.cmc_datelastrun = DateTime.UtcNow;
			executeMultipleBuffer.Update(scoreDefinitionInstance,t=> LogResult($"updating the score definition :{scoreDefinitionInstance.cmc_scoredefinitionname} last run date)", t));
			_logger.Info($"updating the  {instance.LogicalName} with id :{instance.Id} with the date and score");
			executeMultipleBuffer.Update(instanceChanges, t => LogResult($"updating instance {instanceChanges.LogicalName} ({instanceChanges.Id})", t));
			// query for active history records
			var historyQuery = new FetchBuilder();
			historyQuery.EntityName = cmc_retentionscorehistory.EntityLogicalName;
			historyQuery.AddAttribute("cmc_retentionscorehistoryid");
			historyQuery.AddAttribute("cmc_score");
			historyQuery.AddAttribute("cmc_scoredefinitionid");
			historyQuery.AddCondition("statecode", "eq", 0);
			historyQuery.AddCondition(GetField(instance), "eq", instance.Id);

			var activeHistories = _orgService.RetrieveMultiple(new FetchExpression(historyQuery.ToString()));

			// if there is already an active record with the same score and group id, leave it
			var match = (from h in activeHistories.Entities.Cast<cmc_retentionscorehistory>()
						 where h.cmc_score == score && h.cmc_scoredefinitionid?.Id == factorGroupId
						 select h).FirstOrDefault(); if (match != null)
			{
				activeHistories.Entities.Remove(match);
				match.cmc_lastcalculateddate= DateTime.UtcNow;
				executeMultipleBuffer.Update(match, t => LogResult($"updating instance {match.LogicalName} ({match.Id})", t));
				_skippedCount++;
			}
			else
			{
				// otherwise create a new record
				var newRecord = new cmc_retentionscorehistory();
				newRecord[GetField(instance)] = instance.ToEntityReference();
				newRecord.cmc_scoredefinitionid = new EntityReference(cmc_scoredefinition.EntityLogicalName, factorGroupId);
				newRecord.cmc_score = score;
				newRecord.cmc_targetattribute = scoreDefinitionInstance.cmc_targetattributename;
				newRecord.cmc_lastcalculateddate= DateTime.UtcNow;
				executeMultipleBuffer.Create(newRecord, t =>
				{
					 LogResult($"creating retention score hisory for instance {instance.LogicalName} ({instance.Id})", t, r => $"new id: {((CreateResponse)r).id}");
					if (t.IsCompleted) _createCount++;
				});
			}

			// deactivate any previous history records
			foreach (var activeHistory in activeHistories.Entities.Cast<cmc_retentionscorehistory>())
			{
				activeHistory.statecode = cmc_retentionscorehistoryState.Inactive;

				executeMultipleBuffer.Update(activeHistory, t =>
				{
					
					 LogResult($"deactivating old retentions score for entity {instance.LogicalName} with Id: {instance.Id}", t);
				
					if (t.IsCompleted) _deactivatedCount++;
				});
			}
		}
		/// <summary>
		/// gets the relationship field in RetentionScoreHistory for the entity
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		private string GetField(Entity instance)
		{
			var newRecord = new cmc_retentionscorehistory();
			switch (instance.LogicalName)
			{
				case Contact.EntityLogicalName: return "cmc_studentid";
				case Account.EntityLogicalName: return "cmc_accountid";
				case Lead.EntityLogicalName: return "cmc_lifecycleid";
				case Opportunity.EntityLogicalName: return "cmc_inboundinterestid";
			}
			return string.Empty;
		}

		private Guid GetCurrentAcademicPeriodId()
		{
			var fetch = String.Format(
				 @"<fetch>
                      <entity name='mshied_academicperiod'>
	                    <filter type='and'>
                          <condition attribute='mshied_startdate' operator='on-or-before' value='{0}' />
                          <condition attribute='mshied_enddate' operator='on-or-after' value='{0}' />
                          <condition attribute='statecode' operator='eq' value='0' />
                        </filter>
                      </entity>
                    </fetch>", DateTime.UtcNow.ToString("yyyy-MM-dd") + "Z");

			return _orgService.RetrieveMultiple(new FetchExpression(fetch)).Entities.FirstOrDefault().Id;
		}

		private List<RelatedEntity> GetRelatedEntity(cmc_scoredefinition scoredefinition)
		{
			var toAttribute = "contactid";
			switch (scoredefinition.cmc_baseentity.ToLower())
			{
				case "account": toAttribute = "accountid"; break;
				case "contact": toAttribute = "contactid"; break;
				case "lead": toAttribute = "leadid"; break;
				case "opportunity": toAttribute = "opportunityid"; break;
			}
			return new List<RelatedEntity>()
				{
					new RelatedEntity {EntityName = "listmember", FromAttribute = "entityid", ToAttribute = toAttribute}
				};
		}
		private IEnumerable<T> RetrieveAll<T>(FetchBuilder fetchBuilder)
			where T : Entity
		{
			EntityCollection collection;
			do
			{
				collection = _orgService.RetrieveMultiple(new FetchExpression(fetchBuilder.ToString()));
				fetchBuilder.Page++;
				fetchBuilder.PagingCookie = collection.PagingCookie;
				foreach (T entity in collection.Entities) yield return entity;
			}
			while (collection.MoreRecords);
		}

		private IEnumerable<List> RetrieveActiveFactorFilters()
		{
			var fetch =
				$@"<fetch>
			        <entity name='list'>
				    <link-entity name='cmc_scoredefinition_list' from='listid' to='listid'>
                        <all-attributes />
                        <link-entity name='cmc_scoredefinition' from='cmc_scoredefinitionid' to='cmc_scoredefinitionid' alias='ac'>
                        <all-attributes />
                        <filter type='and'>
                            <condition attribute='statecode' operator='eq' value='0' />
                        </filter>
                        </link-entity>
                        </link-entity>
                    <filter>
                        <condition attribute='statecode' operator='eq' value='0' />                
                    </filter>
			        </entity>
		        </fetch>";

			return _orgService.RetrieveMultiple(new FetchExpression(fetch)).Entities.Cast<List>();
		}

		private IEnumerable<cmc_scoringfactor> RetrieveActiveScoringFactors()
		{
			var fetch =
				@"<fetch>
			        <entity name='cmc_scoringfactor'>
				        <link-entity name='cmc_scoredefinition_scoringfactor' to='cmc_scoringfactorid' from='cmc_scoringfactorid'>	
					        <all-attributes />
                            <link-entity name='cmc_scoredefinition' to='cmc_scoredefinitionid' from='cmc_scoredefinitionid'>	
					            <all-attributes />
                                <filter>
                                    <condition attribute='statecode' operator='eq' value='0' />
                                </filter>
				            </link-entity>
				        </link-entity>
                        <filter>
                            <condition attribute='statecode' operator='eq' value='0' />
                        </filter>
			        </entity>
		        </fetch>";

			return _orgService.RetrieveMultiple(new FetchExpression(fetch)).Entities.Cast<cmc_scoringfactor>();
		}
	}
}
