using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Template.Migrations
{
    public partial class Migration1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Link",
                table: "Menu",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreperationForBeaorStaff",
                table: "Menu",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2023, 11, 29, 15, 20, 41, 66, DateTimeKind.Local).AddTicks(9030), "p9mP0tjhY+V0cnRdvwwCHT7QsbTkSPtbzfQE6JT1aoI=" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Link",
                table: "Menu");

            migrationBuilder.DropColumn(
                name: "PreperationForBeaorStaff",
                table: "Menu");

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2023, 11, 27, 5, 6, 22, 494, DateTimeKind.Local).AddTicks(309), "HiOr780Vl7KlkT5Miwg3a7u/VhSemRgUjYoeiGgI5C0=" });
        }
    }
}
