using System;
using System.Collections.Generic;
using System.Linq;

namespace Cmc.Engage.Communication
{
    public class StaffAvailability
    {
        public Guid UserId { get; set; }
        public Guid UserLocationId { get; set; }
        public decimal Duration { get; set; }
        public List<DateRange> DateRanges { get; set; }

        public StaffAvailability() { }

        public StaffAvailability(Guid userId, Guid userLocationId, decimal duration)
        {
            UserId = userId;
            UserLocationId = userLocationId;
            Duration = duration;
            DateRanges = new List<DateRange>();
        }

        public void AddDateRanges(List<DateRange> dateRanges)
        {
            DateRanges.AddRange(dateRanges.ToList());
        }
    }

    public class DateRange
    {
        public string Start { get; set; }
        public string End { get; set; }
        public bool IsConflictingAppointmentRange { get; set; }
    }
}
