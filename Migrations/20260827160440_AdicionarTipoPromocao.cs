using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LojaWeb_2.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarTipoPromocao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PrecoQuantidadePromocional",
                table: "Produtos",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuantidadeMinimaPromocao",
                table: "Produtos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoPromocao",
                table: "Produtos",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrecoQuantidadePromocional",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "QuantidadeMinimaPromocao",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "TipoPromocao",
                table: "Produtos");
        }
    }
}
