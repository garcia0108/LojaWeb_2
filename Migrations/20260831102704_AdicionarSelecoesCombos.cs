using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LojaWeb_2.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarSelecoesCombos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CarrinhoComboSelecoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CarrinhoComboItemId = table.Column<int>(type: "int", nullable: false),
                    ComboItemId = table.Column<int>(type: "int", nullable: false),
                    ProdutoId = table.Column<int>(type: "int", nullable: false),
                    Cor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Tamanho = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarrinhoComboSelecoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarrinhoComboSelecoes_CarrinhoComboItens_CarrinhoComboItemId",
                        column: x => x.CarrinhoComboItemId,
                        principalTable: "CarrinhoComboItens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarrinhoComboSelecoes_ComboItens_ComboItemId",
                        column: x => x.ComboItemId,
                        principalTable: "ComboItens",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CarrinhoComboSelecoes_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarrinhoComboSelecoes_CarrinhoComboItemId",
                table: "CarrinhoComboSelecoes",
                column: "CarrinhoComboItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CarrinhoComboSelecoes_ComboItemId",
                table: "CarrinhoComboSelecoes",
                column: "ComboItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CarrinhoComboSelecoes_ProdutoId",
                table: "CarrinhoComboSelecoes",
                column: "ProdutoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarrinhoComboSelecoes");
        }
    }
}
