using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cmc.Engage.Retention
{
	public class AssignStaffSurveyResponse
	{
		public List<string> failedStaffCourses { get; set; }
		public AssignStaffSurveyResponse()
		{
			failedStaffCourses = new List<string>();
		}
	}
}
