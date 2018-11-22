using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Contracts
{
    public partial class cmc_scoringfactor: IConditionEntity
    {
        public string cmc_attributename { get; set; }
        public OptionSetValue cmc_conditiontype { get; set; }
        public string cmc_min { get; set; }
        public string cmc_max { get; set; }
        public string cmc_value { get; set; }
    }
}
