using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace P133Allup.Migrations
{
    public partial class AddedUsersTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Seria",
                table: "Products",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4)",
                oldMaxLength: 4);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Seria",
                table: "Products",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(4)",
                oldMaxLength: 4,
                oldNullable: true);
        }
    }
}
