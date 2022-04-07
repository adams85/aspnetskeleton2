using Xunit;

namespace WebApp.Service.Proxy.Tests.IntegrationTests;

[CollectionDefinition(nameof(ServiceHostCollection))]
public class ServiceHostCollection : ICollectionFixture<ServiceHostFixture> { }
