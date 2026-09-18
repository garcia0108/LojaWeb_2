using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LojaWeb_2.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarDadosFreteNaVenda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CepEntrega",
                table: "Vendas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrazoEntregaDias",
                table: "Vendas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoFrete",
                table: "Vendas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorFrete",
                table: "Vendas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CepEntrega",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "PrazoEntregaDias",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "TipoFrete",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "ValorFrete",
                table: "Vendas");
        }
    }
}
