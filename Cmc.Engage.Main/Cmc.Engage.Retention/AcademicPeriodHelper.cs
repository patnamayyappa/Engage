using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Cmc.Engage.Retentions
{
    public static class AcademicPeriodHelper
    {
        public static cmc_academicperiod GetCurrentAcademicPeriod(IOrganizationService orgService, Guid studentId)
        {
            var now = DateTime.UtcNow;
            var fetch = $@"<fetch>
              <entity name='cmc_academicperiod'>
                <attribute name='cmc_startdate' />
                <attribute name='cmc_enddate' />
                <attribute name='cmc_academicperiodid' />
	            <filter type='and'>
                  <condition attribute='cmc_startdate' operator='on-or-before' value='{now}' />
                  <condition attribute='cmc_enddate' operator='on-or-after' value='{now}' />
                  <condition attribute='statecode' operator='eq' value='0' />
                </filter>
                <link-entity name='cmc_academicprogress' from='cmc_academicperiodid' to='cmc_academicperiodid'>
                  <filter type='and'>
                    <condition attribute='cmc_studentid' operator='eq' value='{studentId}' />
                  </filter>
                </link-entity>
              </entity>
            </fetch>";

            return orgService.RetrieveMultiple(new FetchExpression(fetch)).Entities.FirstOrDefault()?.ToEntity<cmc_academicperiod>();
        }

        public static cmc_academicperiod GetDefaultAcaedmicPeriod()
        {
            return new cmc_academicperiod()
            {
                cmc_startdate = new DateTime(1753, 1, 1),
                cmc_enddate = new DateTime(1753, 1, 1)
            }; ;
        }
    }
}
