using System;
using System.Linq;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Cmc.Engage.Common.Utilities
{
    public class AcademicPeriodHelper
    {
        public static mshied_academicperiod GetCurrentAcademicPeriod(IOrganizationService orgService, Guid studentId)
        {
            var now = DateTime.UtcNow;
            var fetch = $@"<fetch>
              <entity name='mshied_academicperiod'>
                <attribute name='mshied_startdate' />
                <attribute name='mshied_enddate' />
                <attribute name='mshied_academicperiodid' />
	            <filter type='and'>
                  <condition attribute='mshied_startdate' operator='on-or-before' value='{now}' />
                  <condition attribute='mshied_enddate' operator='on-or-after' value='{now}' />
                  <condition attribute='statecode' operator='eq' value='0' />
                </filter>
                <link-entity name='mshied_academicperioddetails' from='mshied_academicperiodid' to='mshied_academicperiodid'>
                  <filter type='and'>
                    <condition attribute='mshied_studentid' operator='eq' value='{studentId}' />
                  </filter>
                </link-entity>
              </entity>
            </fetch>";

            var data = orgService.RetrieveMultiple(new FetchExpression(fetch)).Entities.FirstOrDefault();
            //var a=new mshied_academicperiod()
            //{
               
            //}

            return orgService.RetrieveMultiple(new FetchExpression(fetch)).Entities.FirstOrDefault()?.ToEntity<mshied_academicperiod>();
        }

        public static mshied_academicperiod GetDefaultAcaedmicPeriod()
        {
            return new mshied_academicperiod()
            {
                mshied_StartDate = new DateTime(1753, 1, 1),
                mshied_EndDate = new DateTime(1753, 1, 1)
            }; ;
        }
    }
}
