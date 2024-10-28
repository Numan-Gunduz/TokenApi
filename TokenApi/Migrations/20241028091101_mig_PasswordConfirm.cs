using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TokenApi.Migrations
{
    /// <inheritdoc />
    public partial class mig_PasswordConfirm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordConfirm",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordConfirm",
                table: "Users");
        }
    }
}
