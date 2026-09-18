using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LojaWeb_2.Migrations
{
    /// <inheritdoc />
    public partial class CorrigirRelacionamentoProdutoCor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProdutoCores_Produtos_ProdutoId",
                table: "ProdutoCores");

            migrationBuilder.AddForeignKey(
                name: "FK_ProdutoCores_Produtos_ProdutoId",
                table: "ProdutoCores",
                column: "ProdutoId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProdutoCores_Produtos_ProdutoId",
                table: "ProdutoCores");

            migrationBuilder.AddForeignKey(
                name: "FK_ProdutoCores_Produtos_ProdutoId",
                table: "ProdutoCores",
                column: "ProdutoId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
