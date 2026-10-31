using AutoMapper;
using FitZone.Application;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FitZone.Testing;

public class MappingProfileTests
{
    [Fact]
    public void MappingProfile_Configuration_IsValid()
    {
        var config = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingProfile>(),
            NullLoggerFactory.Instance);

        config.AssertConfigurationIsValid();
    }
}