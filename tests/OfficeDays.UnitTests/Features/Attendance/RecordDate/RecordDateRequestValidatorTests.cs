using NSubstitute;
using OfficeDays.Features.Attendance.RecordDate;
using OfficeDays.Features.Attendance.RecordDate.Contracts;
using OfficeDays.Security;

namespace OfficeDays.UnitTests.Features.Attendance.RecordDate;

public sealed class RecordDateRequestValidatorTests
{
    // [Fact] todo
    // public async Task Should_ReturnTrue()
    // {
    //     // arrange
    //     var expectedDate = DateOnly.FromDateTime(DateTime.Now);
    //     var sut = new RecordDateRequestValidator(TimeProvider.System,
    //                                              Substitute.For<ICurrentUser>(),
    //                                              );
    //     var request = new RecordDateRequest
    //     {
    //         Date = expectedDate
    //     };
    //
    //     // act
    //     var result = await sut.ValidateAsync(request);
    //
    //     // assert
    //     Assert.True(result.IsValid);
    // }
}
