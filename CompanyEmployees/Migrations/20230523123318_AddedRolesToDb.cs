using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompanyEmployees.Migrations
{
    public partial class AddedRolesToDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "690e451d-60dc-48e4-8752-ed1e107d0566", "b5ec6b77-2ca3-48f9-9ea3-440e5f35eb13", "Administrator", "ADMINISTRATOR" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "bca85d18-cd99-4684-8058-04c745b564fd", "6b2e8017-8463-4f91-b586-7ee9e4aac989", "Manager", "MANAGER" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "690e451d-60dc-48e4-8752-ed1e107d0566");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "bca85d18-cd99-4684-8058-04c745b564fd");
        }
    }
}
