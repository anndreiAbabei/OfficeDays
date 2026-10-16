using NSubstitute;
using OfficeDays.Domain;
using OfficeDays.Features.Users.CreateRequirement;
using OfficeDays.Features.Users.UpdateRequirement;
using OfficeDays.Features.Users.GetRequirements;
using OfficeDays.Services;

namespace OfficeDays.UnitTests.Features.Requirements;

public sealed class RequirementMappingTests
{
    [Theory]
    [InlineData("0001-01-01", true)]
    [InlineData("2027-01-01", false)]
    public void Each_endpoint_maps_all_fields_to_its_own_response(string date, bool isBaseline)
    {
        var row = new UserRequiredOfficePercentage
        {
            Id = Guid.NewGuid(), UserId = Guid.NewGuid(), StartingFrom = DateOnly.Parse(date),
            RequiredOfficePercentage = 60, CreatedAt = new DateTimeOffset(2026, 10, 16, 9, 0, 0, TimeSpan.Zero)
        };
        var created = CreateRequirementMapping.ToViewModel(row);
        var updated = UpdateRequirementMapping.ToViewModel(row);
        var item = GetRequirementsMapping.ToViewModel(row);
        var expected = (row.Id, row.StartingFrom, row.RequiredOfficePercentage, row.CreatedAt, isBaseline);
        Assert.Equal(expected, (created.Id, created.StartingFrom, created.RequiredOfficePercentage, created.CreatedAt, created.IsBaseline));
        Assert.Equal(expected, (updated.Id, updated.StartingFrom, updated.RequiredOfficePercentage, updated.CreatedAt, updated.IsBaseline));
        Assert.Equal(expected, (item.Id, item.StartingFrom, item.RequiredOfficePercentage, item.CreatedAt, item.IsBaseline));
    }

    [Fact]
    public void List_maps_the_users_local_date_and_preserves_requirement_order()
    {
        var user = new User { Username = "user", NormalizedUsername = "USER", PasswordHash = "hash", TimeZoneId = "Europe/Bucharest" };
        var dates = Substitute.For<IUserDateService>();
        var today = new DateOnly(2027, 1, 1);
        dates.Today(user).Returns(today);
        UserRequiredOfficePercentage[] rows =
        [
            new() { Id = Guid.NewGuid(), StartingFrom = today, RequiredOfficePercentage = 60 },
            new() { Id = Guid.NewGuid(), StartingFrom = DateOnly.MinValue, RequiredOfficePercentage = 50 }
        ];
        var response = user.ToViewModel(rows, dates);
        Assert.Equal(today, response.Today);
        Assert.Equal(rows.Select(x => x.Id), response.Items.Select(x => x.Id));
        Assert.True(response.Items.Last().IsBaseline);
        Assert.Empty(user.ToViewModel([], dates).Items);
    }
}
