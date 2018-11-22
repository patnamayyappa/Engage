
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Engage.Models;

namespace Cmc.Engage.Common
{
    /// <summary>
    /// Contract for configuration service
    /// </summary>
    public interface IConfigurationService
    {
        
        Dictionary<string, string> GetConfigurationDetails(List<string> keyNamesList);
        /// <summary>
        /// Get All Active Configuration 
        /// </summary>
        /// <returns></returns>
        cmc_configuration GetActiveConfiguration();
    }
}
