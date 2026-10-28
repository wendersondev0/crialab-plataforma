using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CriaLab.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class ParceiroUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "url",
                table: "parceiros",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "url",
                table: "parceiros");
        }
    }
}
