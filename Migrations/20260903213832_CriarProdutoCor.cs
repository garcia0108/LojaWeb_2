using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LojaWeb_2.Migrations
{
    /// <inheritdoc />
    public partial class CriarProdutoCor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<int>(
                name: "ProdutoCorId",
                table: "ProdutoImagens",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProdutoCores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProdutoId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProdutoCores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProdutoCores_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProdutoImagens_ProdutoCorId",
                table: "ProdutoImagens",
                column: "ProdutoCorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProdutoCores_ProdutoId",
                table: "ProdutoCores",
                column: "ProdutoId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProdutoImagens_ProdutoCores_ProdutoCorId",
                table: "ProdutoImagens",
                column: "ProdutoCorId",
                principalTable: "ProdutoCores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProdutoImagens_ProdutoCores_ProdutoCorId",
                table: "ProdutoImagens");

            migrationBuilder.DropTable(
                name: "ProdutoCores");

            migrationBuilder.DropIndex(
                name: "IX_ProdutoImagens_ProdutoCorId",
                table: "ProdutoImagens");

            migrationBuilder.DropColumn(
                name: "ProdutoCorId",
                table: "ProdutoImagens");

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
    }
}
