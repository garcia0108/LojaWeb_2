using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LojaWeb_2.Migrations
{
    /// <inheritdoc />
    public partial class AtualizaModelo : Migration
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

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Produtos",
                newName: "Nome");

            migrationBuilder.AlterColumn<int>(
                name: "ProdutoId",
                table: "Estoques",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Estoques_ProdutoId",
                table: "Estoques",
                column: "ProdutoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Estoques_Produtos_ProdutoId",
                table: "Estoques",
                column: "ProdutoId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Estoques_Produtos_ProdutoId",
                table: "Estoques");

            migrationBuilder.DropIndex(
                name: "IX_Estoques_ProdutoId",
                table: "Estoques");

            migrationBuilder.RenameColumn(
                name: "Nome",
                table: "Produtos",
                newName: "Name");

            migrationBuilder.AlterColumn<string>(
                name: "ProdutoId",
                table: "Estoques",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

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
