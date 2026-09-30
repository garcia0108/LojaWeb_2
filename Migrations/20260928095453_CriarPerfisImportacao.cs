using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LojaWeb_2.Migrations
{
    /// <inheritdoc />
    public partial class CriarPerfisImportacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PerfisImportacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TamanhosNaHorizontal = table.Column<bool>(type: "bit", nullable: false),
                    UsaLetrasAF = table.Column<bool>(type: "bit", nullable: false),
                    CampoTorax = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CampoComprimento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CampoBarra = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CampoOmbro = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CampoManga = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CampoAbertura = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfisImportacao", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PerfisImportacao");
        }
    }
}
