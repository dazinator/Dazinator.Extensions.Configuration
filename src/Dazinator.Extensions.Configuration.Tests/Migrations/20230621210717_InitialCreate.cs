using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

#nullable disable

namespace Dazinator.Extensions.Configuration.Tests.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /// migrationBuilder.AlterDatabase<AlterDatabaseOperation>().migrationBuilder.ActiveProvider, (builder) =>
            //{
            //    builder.EnableRetryOnFailure();
            //})

            //  var v = this.na<YourDbContext>();

            // Retrieve the database connection
            ///  var connection = dbContext.Database.GetDbConnection() as SqlConnection;

            // Retrieve the database name
            //  var databaseName = connection.Database;


        ///    migrationBuilder.Sql("ALTER DATABASE CURRENT SET SINGLE_USER WITH ROLLBACK IMMEDIATE;", suppressTransaction: true);
            //var dbName = this.TargetModel.GetDatabaseName();
         //   migrationBuilder.Sql($"ALTER DATABASE CURRENT SET ENABLE_BROKER");

         

            migrationBuilder.CreateTable(
                name: "Configs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfigSectionPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Json = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configs", x => x.Id);
                });

            //migrationBuilder.Sql("ALTER DATABASE CURRENT SET MULTI_USER;", suppressTransaction: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Configs");
        }
    }
}
