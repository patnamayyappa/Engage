using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core.IoC;
using Cmc.Core.Xrm.ServerExtension.Functions;


namespace Cmc.Engage.FunctionExtensions
{
    /// <summary>
    /// Common class for all azure functions to resolve autofac dependancy using service locator
    /// </summary>
    public static class FunctionExtensions
    {
        private static List<Type> RegistrationModules { get; set; }
        private static object lockObject = new object();

        /// <summary>
        /// Creating service locator for registering autofac dependancy modules.
        /// </summary>
        /// <param name="registrationModules"></param>
        /// <returns></returns>
        public static ILifetimeScope GetServiceLocator(List<Type> registrationModules)
        {
            lock (lockObject)
            {
                if (RegistrationModules == null) { RegistrationModules = new List<Type>(); }
                foreach (Type moduleType in registrationModules)
                {
                    if (!RegistrationModules.Distinct().Contains(moduleType))
                    {
                        RegistrationModules.Add(moduleType);
                    }
                }
                ServiceLocator.SetContainer(RegisterAutoFactModules);

            }

            return ServiceLocator.Default;
        }

        /// <summary>
        /// Registering autofac dependancy modules.
        /// </summary>
        /// <returns></returns>
        private static IContainer RegisterAutoFactModules()
        {
            var containerBuilder = new ContainerBuilder();
            if (RegistrationModules?.Any() != true) throw new Exception("There is no module for registering autofac dependancy for azure function.");
            foreach (var type in RegistrationModules)
            {
                containerBuilder.RegisterModule((StaticRegistrationModule)Activator.CreateInstance(type));
            }

            return containerBuilder.Build();
        }

    }
}
