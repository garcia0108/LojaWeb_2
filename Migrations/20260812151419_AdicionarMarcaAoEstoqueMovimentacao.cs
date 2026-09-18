using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LojaWeb_2.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarMarcaAoEstoqueMovimentacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Adiciona MarcaId temporariamente como opcional
            migrationBuilder.AddColumn<int>(
                name: "MarcaId",
                table: "MovimentacoesEstoque",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MarcaId",
                table: "Estoques",
                type: "int",
                nullable: true);


            // =====================================================
            // PREENCHE A MARCA DOS ESTOQUES ANTIGOS
            // usando a marca cadastrada no Produto
            // =====================================================

            migrationBuilder.Sql(@"
        UPDATE e
        SET e.MarcaId = p.MarcaId
        FROM Estoques e
        INNER JOIN Produtos p ON e.ProdutoId = p.Id
        WHERE e.MarcaId IS NULL;
    ");


            // =====================================================
            // PREENCHE A MARCA DAS MOVIMENTAÇÕES ANTIGAS
            // usando a marca cadastrada no Produto
            // =====================================================

            migrationBuilder.Sql(@"
        UPDATE m
        SET m.MarcaId = p.MarcaId
        FROM MovimentacoesEstoque m
        INNER JOIN Produtos p ON m.ProdutoId = p.Id
        WHERE m.MarcaId IS NULL;
    ");


            // =====================================================
            // ÍNDICES
            // =====================================================

            migrationBuilder.CreateIndex(
                name: "IX_Estoques_MarcaId",
                table: "Estoques",
                column: "MarcaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_MarcaId",
                table: "MovimentacoesEstoque",
                column: "MarcaId");


            // =====================================================
            // FOREIGN KEYS
            // =====================================================

            migrationBuilder.AddForeignKey(
                name: "FK_Estoques_Marcas_MarcaId",
                table: "Estoques",
                column: "MarcaId",
                principalTable: "Marcas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimentacoesEstoque_Marcas_MarcaId",
                table: "MovimentacoesEstoque",
                column: "MarcaId",
                principalTable: "Marcas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Estoques_Marcas_MarcaId",
                table: "Estoques");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimentacoesEstoque_Marcas_MarcaId",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropIndex(
                name: "IX_MovimentacoesEstoque_MarcaId",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropIndex(
                name: "IX_Estoques_MarcaId",
                table: "Estoques");

            migrationBuilder.DropColumn(
                name: "MarcaId",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropColumn(
                name: "MarcaId",
                table: "Estoques");
        }
    }
}
