using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Identity.Plugin.Tests.Utilities
{
    public sealed class ServiceCollectionMock
    {
        private readonly Mock<IServiceCollection> _serviceCollectionMock;
        private ServiceCollectionVerifier ServiceCollectionVerifier { get; set; }
        public IServiceCollection ServiceCollection => _serviceCollectionMock.Object;

        public ServiceCollectionMock()
        {
            _serviceCollectionMock = new Mock<IServiceCollection>();
            ServiceCollectionVerifier = new ServiceCollectionVerifier(_serviceCollectionMock);
        }

        public void ContainsSingletonService<TService, TInstance>()
        {
            ServiceCollectionVerifier.ContainsSingletonService<TService, TInstance>();
        }

        public void ContainsTransientService<TService, TInstance>()
        {
            ServiceCollectionVerifier.ContainsTransientService<TService, TInstance>();
        }

        public void ContainsScopedService<TService, TInstance>()
        {
            ServiceCollectionVerifier.ContainsTransientService<TService, TInstance>();
        }
    }

    public sealed class ServiceCollectionVerifier
    {
        private readonly Mock<IServiceCollection> _serviceCollectionMock;

        public ServiceCollectionVerifier(Mock<IServiceCollection> collectionMock)
        {
            _serviceCollectionMock = new Mock<IServiceCollection>();
        }

        public void ContainsSingletonService<TService, TInstance>()
        {
            IsRegistered<TService, TInstance>(ServiceLifetime.Singleton);
        }

        public void ContainsTransientService<TService, TInstance>()
        {
            IsRegistered<TService, TInstance>(ServiceLifetime.Transient);
        }

        public void ContainsScopedService<TService, TInstance>()
        {
            IsRegistered<TService, TInstance>(ServiceLifetime.Scoped);
        }

        private void IsRegistered<TService, TInstance>(ServiceLifetime lifetime)
        {
            _serviceCollectionMock
                .Verify(serviceCollection => serviceCollection.Add(
                    It.Is<ServiceDescriptor>(serviceDescriptor => serviceDescriptor.Is<TService, TInstance>(lifetime))));
        }
    }
    public static class ServiceDescriptionExtensions
    {
        public static bool Is<TService, TInstance>(this ServiceDescriptor serviceDescriptor, ServiceLifetime lifetime)
        {
            return serviceDescriptor.ServiceType == typeof(TService) &&
                   serviceDescriptor.ImplementationType == typeof(TInstance) &&
                   serviceDescriptor.Lifetime == lifetime;
        }
    }
}