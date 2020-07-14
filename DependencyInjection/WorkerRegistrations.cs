using Penguin.DependencyInjection.Abstractions.Enums;
using Penguin.DependencyInjection.Abstractions.Interfaces;
using Penguin.DependencyInjection.ServiceProviders;
using Penguin.Reflection;
using Penguin.Workers.Abstractions;
using System;
using DependencyEngine = Penguin.DependencyInjection.Engine;

namespace Penguin.Cms.Modules.Workers.DependencyInjection
{
    public class WorkerRegistrations : IRegisterDependencies
    {
        public void RegisterDependencies(IServiceRegister serviceRegister)
        {
            if (serviceRegister is null)
            {
                throw new ArgumentNullException(nameof(serviceRegister));
            }

            foreach (Type workerType in TypeFactory.GetAllImplementations<IWorker>())
            {
                serviceRegister.Register(workerType, workerType, ServiceLifetime.Scoped);
            }
        }
    }
}