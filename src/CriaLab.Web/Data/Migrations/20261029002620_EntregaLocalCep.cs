using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CriaLab.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class EntregaLocalCep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "entrega_local",
                table: "faixas_cep",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "entrega_local_descricao",
                table: "faixas_cep",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "entrega_local_dias",
                table: "faixas_cep",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "entrega_local_preco",
                table: "faixas_cep",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "entrega_local",
                table: "faixas_cep");

            migrationBuilder.DropColumn(
                name: "entrega_local_descricao",
                table: "faixas_cep");

            migrationBuilder.DropColumn(
                name: "entrega_local_dias",
                table: "faixas_cep");

            migrationBuilder.DropColumn(
                name: "entrega_local_preco",
                table: "faixas_cep");
        }
    }
}
