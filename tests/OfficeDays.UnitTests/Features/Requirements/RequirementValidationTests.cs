using OfficeDays.Domain;
using OfficeDays.Features.Users.CreateRequirement;
using OfficeDays.Features.Users.CreateRequirement.Contracts;
using OfficeDays.Features.Users.UpdateRequirement;
using OfficeDays.Features.Users.UpdateRequirement.Contracts;

namespace OfficeDays.UnitTests.Features.Requirements;

public sealed class RequirementValidationTests
{
    [Theory]
    [InlineData(0, true)]
    [InlineData(100, true)]
    [InlineData(60, true)]
    [InlineData(-1, false)]
    [InlineData(101, false)]
    public void Create_and_update_validate_percentage_bounds(int percentage, bool valid)
    {
        var createBody = new CreateRequirementRequestBody { StartingFrom = new DateOnly(2027, 1, 1), RequiredOfficePercentage = percentage };
        var updateBody = new UpdateRequirementRequestBody { StartingFrom = new DateOnly(2027, 1, 1), RequiredOfficePercentage = percentage };
        Assert.Equal(valid, new CreateRequirementRequestValidator(new CreateRequirementRequestBodyValidator()).Validate(new CreateRequirementRequest { Body = createBody }).IsValid);
        Assert.Equal(valid, new UpdateRequirementRequestValidator(new UpdateRequirementRequestBodyValidator()).Validate(new UpdateRequirementRequest { Body = updateBody }).IsValid);
    }

    [Theory]
    [InlineData("2027-01-01", true, true)]
    [InlineData("2027-01-15", false, false)]
    [InlineData("2024-02-29", false, false)]
    [InlineData("0001-01-01", false, true)]
    [InlineData("2020-01-01", true, true)]
    public void Month_validation_allows_baseline_only_for_updates(string date, bool createValid, bool updateValid)
    {
        var createBody = new CreateRequirementRequestBody { StartingFrom = DateOnly.Parse(date), RequiredOfficePercentage = 50 };
        var updateBody = new UpdateRequirementRequestBody { StartingFrom = DateOnly.Parse(date), RequiredOfficePercentage = 50 };
        Assert.Equal(createValid, new CreateRequirementRequestValidator(new CreateRequirementRequestBodyValidator()).Validate(new CreateRequirementRequest { Body = createBody }).IsValid);
        Assert.Equal(updateValid, new UpdateRequirementRequestValidator(new UpdateRequirementRequestBodyValidator()).Validate(new UpdateRequirementRequest { Body = updateBody }).IsValid);
    }

    [Fact]
    public void Missing_bodies_are_rejected_without_throwing()
    {
        Assert.False(new CreateRequirementRequestValidator(new CreateRequirementRequestBodyValidator()).Validate(new CreateRequirementRequest { Body = null! }).IsValid);
        Assert.False(new UpdateRequirementRequestValidator(new UpdateRequirementRequestBodyValidator()).Validate(new UpdateRequirementRequest { Body = null! }).IsValid);
    }

    [Fact]
    public void Only_the_baseline_is_marked_as_initial()
    {
        var row = new UserRequiredOfficePercentage { Id = Guid.NewGuid(), StartingFrom = DateOnly.MinValue, RequiredOfficePercentage = 40 };
        Assert.True(CreateRequirementMapping.ToViewModel(row).IsBaseline);
        Assert.True(UpdateRequirementMapping.ToViewModel(row).IsBaseline);
        row.StartingFrom = new DateOnly(2026, 1, 1);
        Assert.False(CreateRequirementMapping.ToViewModel(row).IsBaseline);
        Assert.False(UpdateRequirementMapping.ToViewModel(row).IsBaseline);
    }
}
