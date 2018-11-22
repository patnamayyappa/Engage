using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Contracts
{
    public class AssignSuccessPlanResponse
    {
        public List<string> DuplicateList { get; set; }
        public string AssignSuccessPlanDialogMessage { get; set; }
        public int TotalCount { get; set; }

        /// <summary>
        /// to store student names when no academic period is null, maintain when assign of due date calculated.
        /// </summary>
        public List<string> StudentNamesWithoutAcademicPeriod { get; set; }

        public List<EntityReference> SuccessPlanIds { get; set; }

        public AssignSuccessPlanResponse()
        {
            StudentNamesWithoutAcademicPeriod=new List<string>();
            SuccessPlanIds=new List<EntityReference>();
            DuplicateList=new List<string>();
        }
    }
    
}