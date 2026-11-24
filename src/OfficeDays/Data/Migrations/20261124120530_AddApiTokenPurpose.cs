using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OfficeDays.Data.Migrations;

/// <inheritdoc />
public partial class _20261124120530_AddApiTokenPurpose : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Purpose",
            table: "ApiTokens",
            type: "TEXT",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Purpose",
            table: "ApiTokens");
    }
}
