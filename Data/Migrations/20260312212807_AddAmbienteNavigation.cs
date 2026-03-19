using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConfortAnimal.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAmbienteNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Avaliacoes_ambienteId",
                table: "Avaliacoes",
                column: "ambienteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Avaliacoes_Ambiente_ambienteId",
                table: "Avaliacoes",
                column: "ambienteId",
                principalTable: "Ambiente",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Avaliacoes_Ambiente_ambienteId",
                table: "Avaliacoes");

            migrationBuilder.DropIndex(
                name: "IX_Avaliacoes_ambienteId",
                table: "Avaliacoes");
        }
    }
}
