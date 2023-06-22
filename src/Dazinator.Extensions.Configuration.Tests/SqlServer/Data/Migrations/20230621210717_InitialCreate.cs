#nullable disable

namespace Dazinator.Extensions.Configuration.Tests.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.CreateTable(
                name: "Configs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfigSectionPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Json = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Configs", x => x.Id));//migrationBuilder.Sql("ALTER DATABASE CURRENT SET MULTI_USER;", suppressTransaction: true);

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(
                name: "Configs");
    }
}
