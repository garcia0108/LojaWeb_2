using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LojaWeb_2.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarFornecedorMovimentacaoEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FornecedorId",
                table: "MovimentacoesEstoque",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_FornecedorId",
                table: "MovimentacoesEstoque",
                column: "FornecedorId");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimentacoesEstoque_Fornecedores_FornecedorId",
                table: "MovimentacoesEstoque",
                column: "FornecedorId",
                principalTable: "Fornecedores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimentacoesEstoque_Fornecedores_FornecedorId",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropIndex(
                name: "IX_MovimentacoesEstoque_FornecedorId",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropColumn(
                name: "FornecedorId",
                table: "MovimentacoesEstoque");
        }
    }
}
