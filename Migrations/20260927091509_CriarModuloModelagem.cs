using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LojaWeb_2.Migrations
{
    /// <inheritdoc />
    public partial class CriarModuloModelagem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ModelagemId",
                table: "Produtos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Modelagens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MarcaId = table.Column<int>(type: "int", nullable: false),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modelagens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Modelagens_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Modelagens_Marcas_MarcaId",
                        column: x => x.MarcaId,
                        principalTable: "Marcas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MedidasModelagem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModelagemId = table.Column<int>(type: "int", nullable: false),
                    Tamanho = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Torax = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Comprimento = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Barra = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Ombro = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ComprimentoManga = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    AberturaManga = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedidasModelagem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedidasModelagem_Modelagens_ModelagemId",
                        column: x => x.ModelagemId,
                        principalTable: "Modelagens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Produtos_ModelagemId",
                table: "Produtos",
                column: "ModelagemId");

            migrationBuilder.CreateIndex(
                name: "IX_MedidasModelagem_ModelagemId",
                table: "MedidasModelagem",
                column: "ModelagemId");

            migrationBuilder.CreateIndex(
                name: "IX_Modelagens_CategoriaId",
                table: "Modelagens",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Modelagens_MarcaId",
                table: "Modelagens",
                column: "MarcaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Produtos_Modelagens_ModelagemId",
                table: "Produtos",
                column: "ModelagemId",
                principalTable: "Modelagens",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Produtos_Modelagens_ModelagemId",
                table: "Produtos");

            migrationBuilder.DropTable(
                name: "MedidasModelagem");

            migrationBuilder.DropTable(
                name: "Modelagens");

            migrationBuilder.DropIndex(
                name: "IX_Produtos_ModelagemId",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "ModelagemId",
                table: "Produtos");
        }
    }
}
