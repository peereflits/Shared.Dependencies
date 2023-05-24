using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Peereflits.Shared.Dependencies.Extensions.Test;

public class ValidateTypeRegistrationsTests
{
    internal class SingleTypeRegistrationRulesProvider : IProvideTypeRegistrationRules
    {
        public IEnumerable<TypeRegistrationRule> Execute()
        {
            yield return new TypeRegistrationRule<ITest>(false);
        }
    }

    internal class DoubleTypeRegistrationRulesProvider : IProvideTypeRegistrationRules
    {
        public IEnumerable<TypeRegistrationRule> Execute()
        {
            yield return new TypeRegistrationRule<ITest>(true);
        }
    }

    private interface ITest { }
    private class Test : ITest { }
    private class TestAlternative : ITest { }

    [Fact]
    public void WhenValidateMissingRegistration_ExpectedSingle_ItShouldThrow()
    {
        var services = new ServiceCollection();

        Assert.Throws<UnregisteredTypeException>(services.Validate<SingleTypeRegistrationRulesProvider>);
    }

    [Fact]
    public void WhenValidateSingleRegistration_ExpectedSingle_ItShouldSucceed()
    {
        var services = new ServiceCollection();

        services.AddScoped<ITest, Test>();

        services.Validate<SingleTypeRegistrationRulesProvider>();
    }

    [Fact]
    public void WhenValidateDoubleRegistration_ExpectedSingle_ItShouldThrow()
    {
        var services = new ServiceCollection();

        services.AddScoped<ITest, Test>();
        services.AddScoped<ITest, TestAlternative>();

        Assert.Throws<MultipleTypeRegistrationsException>(services.Validate<SingleTypeRegistrationRulesProvider>);
    }

    [Fact]
    public void WhenValidateSingleRegistrationAndImplementationRegistration_ExpectedSingle_ItShouldSucceed()
    {
        var services = new ServiceCollection();

        services.AddScoped<ITest, Test>();
        services.AddScoped<Test, Test>();

        services.Validate<SingleTypeRegistrationRulesProvider>();
    }

    [Fact]
    public void WhenValidateMissingRegistration_ExpectedAtLeastOne_ItShouldThrow()
    {
        var services = new ServiceCollection();

        Assert.Throws<UnregisteredTypeException>(services.Validate<DoubleTypeRegistrationRulesProvider>);
    }

    [Fact]
    public void WhenValidateSingleRegistration_ExpectedAtLeastOne_ItShouldSucceed()
    {
        var services = new ServiceCollection();

        services.AddScoped<ITest, Test>();

        services.Validate<DoubleTypeRegistrationRulesProvider>();
    }

    [Fact]
    public void WhenValidateDoubleRegistration_ExpectedAtLeastOne_ItShouldSucceed()
    {
        var services = new ServiceCollection();

        services.AddScoped<ITest, Test>();
        services.AddScoped<ITest, TestAlternative>();

        services.Validate<DoubleTypeRegistrationRulesProvider>();
    }
}