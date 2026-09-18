using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LojaWeb_2.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarImagemPrincipalPorCor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ImagemPrincipalId",
                table: "ProdutoCores",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProdutoCores_ImagemPrincipalId",
                table: "ProdutoCores",
                column: "ImagemPrincipalId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProdutoCores_ProdutoImagens_ImagemPrincipalId",
                table: "ProdutoCores",
                column: "ImagemPrincipalId",
                principalTable: "ProdutoImagens",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProdutoCores_ProdutoImagens_ImagemPrincipalId",
                table: "ProdutoCores");

            migrationBuilder.DropIndex(
                name: "IX_ProdutoCores_ImagemPrincipalId",
                table: "ProdutoCores");

            migrationBuilder.DropColumn(
                name: "ImagemPrincipalId",
                table: "ProdutoCores");
        }
    }
}
