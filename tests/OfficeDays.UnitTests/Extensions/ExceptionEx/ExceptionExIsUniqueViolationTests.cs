using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OfficeDays.Extensions;

namespace OfficeDays.UnitTests.Extensions;

public sealed class ExceptionExIsUniqueViolationTests
{
    [Fact]
    public void Should_ReturnTrue_WhenIsIsUniqueViolation()
    {
        // arrange
        var sqlEx = new SqliteException("", 19);
        var exception = new DbUpdateException("", sqlEx);
        
        // act
        var result = exception.IsUniqueViolation();

        // assert
        Assert.True(result);
    }

    [Fact]
    public void Should_ReturnFalse_WhenInnerExceptionIsNotSqliteException()
    {
        // arrange
        var innerEx = new Exception();
        var exception = new DbUpdateException("", innerEx);
        
        // act
        var result = exception.IsUniqueViolation();

        // assert
        Assert.False(result);
    }
    
    [Fact]
    public void Should_ReturnFalse_WhenInnerExceptionIsSqliteException_ButCodeIsNotUniqueViolation()
    {
        // arrange
        var sqlEx = new SqliteException("", 123);
        var exception = new DbUpdateException("", sqlEx);
        
        // act
        var result = exception.IsUniqueViolation();

        // assert
        Assert.False(result);
    }
}
