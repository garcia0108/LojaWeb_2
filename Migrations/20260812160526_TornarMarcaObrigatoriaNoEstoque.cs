using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LojaWeb_2.Migrations
{
    /// <inheritdoc />
    public partial class TornarMarcaObrigatoriaNoEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
        name: "MarcaId",
        table: "Estoques",
        type: "int",
        nullable: false,
        oldClrType: typeof(int),
        oldType: "int",
        oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MarcaId",
                table: "MovimentacoesEstoque",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
        name: "MarcaId",
        table: "Estoques",
        type: "int",
        nullable: true,
        oldClrType: typeof(int),
        oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "MarcaId",
                table: "MovimentacoesEstoque",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

        }
    }
}
