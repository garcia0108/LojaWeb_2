using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LojaWeb_2.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarDadosItemVenda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cor",
                table: "ItensVenda",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MarcaId",
                table: "ItensVenda",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Tamanho",
                table: "ItensVenda",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ItensVenda_MarcaId",
                table: "ItensVenda",
                column: "MarcaId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItensVenda_Marcas_MarcaId",
                table: "ItensVenda",
                column: "MarcaId",
                principalTable: "Marcas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItensVenda_Marcas_MarcaId",
                table: "ItensVenda");

            migrationBuilder.DropIndex(
                name: "IX_ItensVenda_MarcaId",
                table: "ItensVenda");

            migrationBuilder.DropColumn(
                name: "Cor",
                table: "ItensVenda");

            migrationBuilder.DropColumn(
                name: "MarcaId",
                table: "ItensVenda");

            migrationBuilder.DropColumn(
                name: "Tamanho",
                table: "ItensVenda");
        }
    }
}
