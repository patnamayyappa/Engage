using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;

namespace Cmc.Engage.Lifecycle.Plugins
{
    /// <summary>
    /// updating the trip activity latitude and longitude   
    /// </summary>
    public class TripActivityUpdateLatitudeLongitudePlugin : PluginBase
    {
        /// <summary>
        /// used to update the address latitude and longitude.
        /// </summary>
        /// <param name="unsecuredParameters"></param>
        /// <param name="securedParameters"></param>
        public TripActivityUpdateLatitudeLongitudePlugin(string unsecuredParameters, string securedParameters)
            : base(unsecuredParameters, securedParameters) { }
        protected override void Execute(IExecutionContext context)
        {
            var tripActivityService = context.IocScope.Resolve<ITripActivityService>();
            tripActivityService.TripActivityUpdateLatitudeLongitude(context);
        }
    }
}
