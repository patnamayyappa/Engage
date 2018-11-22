using System;
using System.Collections.Generic;

namespace Cmc.Engage.Common
{
    public class Week
    {
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public IEnumerable<Dictionary<String, object>> Locations { get; private set; }

        public Week(WeekRange weekRange, IEnumerable<Dictionary<String, object>> locations)
        {
            StartDate = weekRange.StartDate;
            EndDate = weekRange.EndDate;
            Locations = locations;
        }
    }
}
