using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OfficeDays.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHolidayJurisdictions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BankHolidays_Date",
                table: "BankHolidays");

            migrationBuilder.CreateTable(
                name: "HolidayJurisdictions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 6, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HolidayJurisdictions", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "HolidayJurisdictions",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[] { 1, "RO", "Romania" });

            migrationBuilder.AddColumn<int>(
                name: "HolidayJurisdictionId",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "HolidayJurisdictionId",
                table: "BankHolidays",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Users_HolidayJurisdictionId",
                table: "Users",
                column: "HolidayJurisdictionId");

            migrationBuilder.CreateIndex(
                name: "IX_BankHolidays_HolidayJurisdictionId_Date",
                table: "BankHolidays",
                columns: new[] { "HolidayJurisdictionId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HolidayJurisdictions_Code",
                table: "HolidayJurisdictions",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BankHolidays_HolidayJurisdictions_HolidayJurisdictionId",
                table: "BankHolidays",
                column: "HolidayJurisdictionId",
                principalTable: "HolidayJurisdictions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_HolidayJurisdictions_HolidayJurisdictionId",
                table: "Users",
                column: "HolidayJurisdictionId",
                principalTable: "HolidayJurisdictions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankHolidays_HolidayJurisdictions_HolidayJurisdictionId",
                table: "BankHolidays");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_HolidayJurisdictions_HolidayJurisdictionId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "HolidayJurisdictions");

            migrationBuilder.DropIndex(
                name: "IX_Users_HolidayJurisdictionId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_BankHolidays_HolidayJurisdictionId_Date",
                table: "BankHolidays");

            migrationBuilder.DropColumn(
                name: "HolidayJurisdictionId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "HolidayJurisdictionId",
                table: "BankHolidays");

            migrationBuilder.CreateIndex(
                name: "IX_BankHolidays_Date",
                table: "BankHolidays",
                column: "Date",
                unique: true);
        }
    }
}
