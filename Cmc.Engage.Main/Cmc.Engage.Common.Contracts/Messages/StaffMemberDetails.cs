using System;
using System.Collections.Generic;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Common
{
    public class StaffMemberDetails
    {
        public Guid? StaffMemberId { get; private set; }
        public string StaffMemberName { get; set; }
        public string StaffRoleName { get; private set; }
        public string StaffPhoneNumber { get; private set; }
        public string StaffEmail { get; private set; }
        public string StaffBio { get; private set; }
        public Guid? StaffDepartmentId { get; private set; }
        public IEnumerable<Week> OfficeHours { get; private set; }
        public IEnumerable<EntityReference> StaffLocations { get; private set; }

        public StaffMemberDetails(cmc_successnetwork successNetwork, IEnumerable<Week> weeks, IEnumerable<EntityReference> locations)
        {
            StaffMemberId = successNetwork.cmc_staffmemberid?.Id;
            StaffMemberName = successNetwork.cmc_staffmemberid?.Name;
            StaffRoleName = successNetwork.cmc_staffroleid?.Name;
            StaffPhoneNumber = successNetwork.Contains("systemuser.address1_telephone1") ? (string)successNetwork.GetAttributeValue<AliasedValue>("systemuser.address1_telephone1").Value : null;
            StaffEmail = successNetwork.Contains("systemuser.internalemailaddress") ? (string)successNetwork.GetAttributeValue<AliasedValue>("systemuser.internalemailaddress").Value : null;
            StaffBio = successNetwork.Contains("systemuser.cmc_bio") ? (string)successNetwork.GetAttributeValue<AliasedValue>("systemuser.cmc_bio").Value : null; ;
            StaffDepartmentId = successNetwork.Contains("systemuser.cmc_departmentid") ? ((EntityReference)successNetwork.GetAttributeValue<AliasedValue>("systemuser.cmc_departmentid").Value)?.Id : null;
            OfficeHours = weeks;
            StaffLocations = locations;
        }
    }
}
