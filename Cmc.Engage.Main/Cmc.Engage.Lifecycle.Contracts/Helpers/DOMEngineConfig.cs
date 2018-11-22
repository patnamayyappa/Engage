using System;
using System.Collections.Generic;

namespace Cmc.Engage.Lifecycle
{
    public class DOMEngineConfig
    {
        public string EntityLogicalName { get; set; }
        public DateTime UserLocalTime { get; set; }
        public DateTime UtcNow { get; set; } = DateTime.UtcNow;
        public bool CreatePosts { get; set; }

        public string DomMasterRetrievePagingCookie;
        public int DomMasterRetrievePage;

        public Dictionary<Guid, List<Guid>> DomMasterListMembersPendingAssignment;
    }
}
