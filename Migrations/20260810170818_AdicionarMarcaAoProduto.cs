using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LojaWeb_2.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarMarcaAoProduto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Adiciona MarcaId temporariamente permitindo NULL
            migrationBuilder.AddColumn<int>(
                name: "MarcaId",
                table: "Produtos",
                type: "int",
                nullable: true);

            // 2. Cria as marcas que ainda não existem,
            //    utilizando os valores antigos da coluna Marca
            migrationBuilder.Sql(@"
        INSERT INTO Marcas (Nome)
        SELECT DISTINCT
            LTRIM(RTRIM(p.Marca))
        FROM Produtos p
        WHERE p.Marca IS NOT NULL
          AND LTRIM(RTRIM(p.Marca)) <> ''
          AND NOT EXISTS
          (
              SELECT 1
              FROM Marcas m
              WHERE LTRIM(RTRIM(m.Nome)) = LTRIM(RTRIM(p.Marca))
          );
    ");

            // 3. Relaciona cada Produto com sua Marca
            migrationBuilder.Sql(@"
        UPDATE p
        SET p.MarcaId = m.Id
        FROM Produtos p
        INNER JOIN Marcas m
            ON LTRIM(RTRIM(m.Nome)) = LTRIM(RTRIM(p.Marca))
        WHERE p.Marca IS NOT NULL
          AND LTRIM(RTRIM(p.Marca)) <> '';
    ");

            // 4. Cria uma marca padrão para produtos
            //    que não possuíam marca
            migrationBuilder.Sql(@"
        IF NOT EXISTS
        (
            SELECT 1
            FROM Marcas
            WHERE Nome = 'Sem Marca'
        )
        BEGIN
            INSERT INTO Marcas (Nome)
            VALUES ('Sem Marca');
        END
    ");

            // 5. Produtos sem marca recebem "Sem Marca"
            migrationBuilder.Sql(@"
        UPDATE Produtos
        SET MarcaId =
        (
            SELECT TOP 1 Id
            FROM Marcas
            WHERE Nome = 'Sem Marca'
        )
        WHERE MarcaId IS NULL;
    ");

            // 6. Agora MarcaId pode se tornar obrigatório
            migrationBuilder.AlterColumn<int>(
                name: "MarcaId",
                table: "Produtos",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // 7. Índice
            migrationBuilder.CreateIndex(
                name: "IX_Produtos_MarcaId",
                table: "Produtos",
                column: "MarcaId");

            // 8. Chave estrangeira Produto → Marca
            migrationBuilder.AddForeignKey(
                name: "FK_Produtos_Marcas_MarcaId",
                table: "Produtos",
                column: "MarcaId",
                principalTable: "Marcas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // 9. Somente agora podemos remover a antiga coluna de texto
            migrationBuilder.DropColumn(
                name: "Marca",
                table: "Produtos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Marca",
                table: "Produtos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.Sql(@"
        UPDATE p
        SET p.Marca = m.Nome
        FROM Produtos p
        INNER JOIN Marcas m
            ON p.MarcaId = m.Id;
    ");

            migrationBuilder.DropForeignKey(
                name: "FK_Produtos_Marcas_MarcaId",
                table: "Produtos");

            migrationBuilder.DropIndex(
                name: "IX_Produtos_MarcaId",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "MarcaId",
                table: "Produtos");
        }
    }
}
