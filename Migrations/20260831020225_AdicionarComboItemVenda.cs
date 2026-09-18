using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LojaWeb_2.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarComboItemVenda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ComboId",
                table: "ItensVenda",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItensVenda_ComboId",
                table: "ItensVenda",
                column: "ComboId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItensVenda_Combos_ComboId",
                table: "ItensVenda",
                column: "ComboId",
                principalTable: "Combos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItensVenda_Combos_ComboId",
                table: "ItensVenda");

            migrationBuilder.DropIndex(
                name: "IX_ItensVenda_ComboId",
                table: "ItensVenda");

            migrationBuilder.DropColumn(
                name: "ComboId",
                table: "ItensVenda");
        }
    }
}
