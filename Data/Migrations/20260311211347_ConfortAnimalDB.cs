using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConfortAnimal.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConfortAnimalDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ambiente",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    temperatura = table.Column<double>(type: "float", nullable: false),
                    umidade = table.Column<double>(type: "float", nullable: false),
                    local = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dataRegisto = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProprietarioId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ambiente", x => x.id);
                    table.ForeignKey(
                        name: "FK_Ambiente_AspNetUsers_ProprietarioId",
                        column: x => x.ProprietarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Animais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Peso = table.Column<double>(type: "float", nullable: false),
                    Idade = table.Column<int>(type: "int", nullable: false),
                    ProprietarioId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Discriminator = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Raca = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Linhagem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProdutividadeLeite = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Animais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Animais_AspNetUsers_ProprietarioId",
                        column: x => x.ProprietarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Avaliacoes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    bovinoId = table.Column<int>(type: "int", nullable: false),
                    ambienteId = table.Column<int>(type: "int", nullable: false),
                    valorITU = table.Column<double>(type: "float", nullable: false),
                    resultado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dataAvaliacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProprietarioId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avaliacoes", x => x.id);
                    table.ForeignKey(
                        name: "FK_Avaliacoes_AspNetUsers_ProprietarioId",
                        column: x => x.ProprietarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ambiente_ProprietarioId",
                table: "Ambiente",
                column: "ProprietarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Animais_ProprietarioId",
                table: "Animais",
                column: "ProprietarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacoes_ProprietarioId",
                table: "Avaliacoes",
                column: "ProprietarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ambiente");

            migrationBuilder.DropTable(
                name: "Animais");

            migrationBuilder.DropTable(
                name: "Avaliacoes");
        }
    }
}
