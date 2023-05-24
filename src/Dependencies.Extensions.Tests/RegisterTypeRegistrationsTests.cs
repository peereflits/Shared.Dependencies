using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Peereflits.Shared.Dependencies.Extensions.Test;

public class RegisterTypeRegistrationsTests
{
    private interface ITestService { }

    private class TestService : ITestService { }

    private class TypeRegistrationsProvider : IProvideTypeRegistrations
    {
        private readonly List<TypeRegistration> registrations = new()
                                                                {
                                                                    new TypeRegistration<ITestService, TestService>(Lifetime.Scoped),
                                                                    new TypeRegistration<ITestService>(provider => new TestService(), Lifetime.Scoped),
                                                                };

        public IEnumerable<TypeRegistration> Execute() => registrations;
    }

    [Fact]
    public void WhenAddTypeRegistrations_ItShouldAddServices()
    {
        var services = Substitute.For<IServiceCollection>();

        services.AddTypeRegistrations<TypeRegistrationsProvider>();

        services.Received().Add(Arg.Is<ServiceDescriptor>(d => d.ServiceType == typeof(ITestService) && d.ImplementationType == typeof(TestService)));
        services.Received().Add(Arg.Is<ServiceDescriptor>(d => d.ServiceType == typeof(ITestService) && d.ImplementationFactory != null));
    }

    [Fact]
    public void WhenAddServiceSingleton_ItShouldAddSingleton()
    {
        var services = Substitute.For<IServiceCollection>();

        services.AddService(Lifetime.Singleton, typeof(ITestService), typeof(TestService));

        services.Received().Add(Arg.Is<ServiceDescriptor>(d => d.ServiceType == typeof(ITestService) && d.ImplementationType == typeof(TestService) && d.Lifetime == ServiceLifetime.Singleton));
    }

    [Fact]
    public void WhenAddServiceScoped_ItShouldAddScoped()
    {
        var services = Substitute.For<IServiceCollection>();

        services.AddService(Lifetime.Scoped, typeof(ITestService), typeof(TestService));

        services.Received().Add(Arg.Is<ServiceDescriptor>(d => d.ServiceType == typeof(ITestService) && d.ImplementationType == typeof(TestService) && d.Lifetime == ServiceLifetime.Scoped));
    }

    [Fact]
    public void WhenAddServiceInstance_ItShouldAddTransient()
    {
        var services = Substitute.For<IServiceCollection>();

        services.AddService(Lifetime.Instance, typeof(ITestService), typeof(TestService));

        services.Received().Add(Arg.Is<ServiceDescriptor>(d => d.ServiceType == typeof(ITestService) && d.ImplementationType == typeof(TestService) && d.Lifetime == ServiceLifetime.Transient));
    }

    [Fact]
    public void WhenAddServiceFactorySingleton_ItShouldAddSingleton()
    {
        var services = Substitute.For<IServiceCollection>();
        Func<IServiceProvider, object> factory = p => new TestService();

        services.AddService(Lifetime.Singleton, typeof(ITestService), factory);

        services.Received().Add(Arg.Is<ServiceDescriptor>(d => d.ServiceType == typeof(ITestService) && d.ImplementationFactory == factory && d.Lifetime == ServiceLifetime.Singleton));
    }

    [Fact]
    public void WhenAddServiceFactoryScoped_ItShouldAddScoped()
    {
        var services = Substitute.For<IServiceCollection>();
        Func<IServiceProvider, object> factory = p => new TestService();

        services.AddService(Lifetime.Scoped, typeof(ITestService), factory);

        services.Received().Add(Arg.Is<ServiceDescriptor>(d => d.ServiceType == typeof(ITestService) && d.ImplementationFactory == factory && d.Lifetime == ServiceLifetime.Scoped));
    }

    [Fact]
    public void WhenAddServiceFactoryInstance_ItShouldAddTransient()
    {
        var services = Substitute.For<IServiceCollection>();
        Func<IServiceProvider, object> factory = p => new TestService();

        services.AddService(Lifetime.Instance, typeof(ITestService), factory);

        services.Received().Add(Arg.Is<ServiceDescriptor>(d => d.ServiceType == typeof(ITestService) && d.ImplementationFactory == factory && d.Lifetime == ServiceLifetime.Transient));
    }
}