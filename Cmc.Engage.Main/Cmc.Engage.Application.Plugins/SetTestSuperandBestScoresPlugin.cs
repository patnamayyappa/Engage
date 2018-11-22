using System;
using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Application.Plugins
{
    public class SetTestSuperandBestScoresPlugin : PluginBase, IPlugin
    {
        public SetTestSuperandBestScoresPlugin(string unsecuredParameters, string securedParameters)
            : base(unsecuredParameters, securedParameters)
        {

        }

        protected override void Execute(Core.Xrm.ServerExtension.Core.IExecutionContext context)
        {
            var setTestScore = context.IocScope.Resolve<ITestScoreService>();
            setTestScore.SetTestSuperandBestScores(context);
        }
    }
}
