using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LojaWeb_2.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarProdutoIdEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProdutoId1",
                table: "Estoques",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Estoques_ProdutoId1",
                table: "Estoques",
                column: "ProdutoId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Estoques_Produtos_ProdutoId1",
                table: "Estoques",
                column: "ProdutoId1",
                principalTable: "Produtos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Estoques_Produtos_ProdutoId1",
                table: "Estoques");

            migrationBuilder.DropIndex(
                name: "IX_Estoques_ProdutoId1",
                table: "Estoques");

            migrationBuilder.DropColumn(
                name: "ProdutoId1",
                table: "Estoques");
        }
    }
}
