using System;

namespace Cmc.Engage.Common
{
    public class WeekRange
    {
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }

        public WeekRange(DateTime startDate, DateTime endDate)
        {
            StartDate = startDate;
            EndDate = endDate;
        }
    }
}
