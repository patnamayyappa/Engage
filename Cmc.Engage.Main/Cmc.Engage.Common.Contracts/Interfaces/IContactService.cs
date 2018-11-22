using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Engage.Contracts;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Common
{
   public interface IContactService
    {
        void ComputeMilesFromHome(IExecutionContext context);
        void CreateUpdateStudentOwner(IExecutionContext context);
        void SetLegacyStudent(IExecutionContext context);
        AssignSuccessPlanResponse AssignSuccessPlan(EntityReference studentId, EntityReference successPlanTemplateId);
        void CreateSuccessPlansForSelectedStudent(Core.Xrm.ServerExtension.Core.IExecutionContext context);
        void PredictRetentionAction(IExecutionContext context);
        void CampusDistanceCalculatorLogic();
        List<Contact> RetriveMembersForStaticType(Guid listId, Guid? ownerId, IOrganizationService organizationService);
        List<Contact> RetriveMembersForDynamicType(string query, Guid? ownerId, IOrganizationService organizationService);
        void SuccessPlanAssignmentLogic(IOrganizationService organizationService);
        void SetContactImage();
        void SetStudentFlag(Core.Xrm.ServerExtension.Core.IExecutionContext context);
    }
}
