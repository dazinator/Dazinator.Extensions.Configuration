#nullable disable

namespace Dazinator.Extensions.Configuration.Tests.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class AddSeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.InsertData(
                table: "Configs",
                columns: new[] { "Id", "ConfigSectionPath", "Json" },
                values: new object[] { 1, "Test", "{\"Test\":\"Test\"}" });

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DeleteData(
                table: "Configs",
                keyColumn: "Id",
                keyValue: 1);
    }
}
