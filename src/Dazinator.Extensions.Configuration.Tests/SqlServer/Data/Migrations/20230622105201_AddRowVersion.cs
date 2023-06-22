#nullable disable

namespace Dazinator.Extensions.Configuration.Tests.Migrations
{
    using System;
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class AddRowVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Configs",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Configs");
    }
}
