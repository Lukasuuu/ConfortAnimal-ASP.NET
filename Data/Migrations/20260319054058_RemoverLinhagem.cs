using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConfortAnimal.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoverLinhagem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Linhagem",
                table: "Animais");

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacoes_bovinoId",
                table: "Avaliacoes",
                column: "bovinoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Avaliacoes_Animais_bovinoId",
                table: "Avaliacoes",
                column: "bovinoId",
                principalTable: "Animais",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Avaliacoes_Animais_bovinoId",
                table: "Avaliacoes");

            migrationBuilder.DropIndex(
                name: "IX_Avaliacoes_bovinoId",
                table: "Avaliacoes");

            migrationBuilder.AddColumn<string>(
                name: "Linhagem",
                table: "Animais",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
