using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cmc.Engage.Contracts
{
    public interface IConditionEntity
    {
        string cmc_attributename { get; set; }
        OptionSetValue cmc_conditiontype { get; set; }
        string cmc_min { get; set; }
        string cmc_max { get; set; }
        string cmc_value { get; set; }
    }
}
