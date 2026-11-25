using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tooded_EFCore.Migrations
{
    /// <inheritdoc />
    public partial class LisaOstud : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ostud",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Kuupaev = table.Column<DateTime>(type: "TEXT", nullable: false),
                    KasutajaId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ostud", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ostud_Kasutajad_KasutajaId",
                        column: x => x.KasutajaId,
                        principalTable: "Kasutajad",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OstuRead",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OstId = table.Column<int>(type: "INTEGER", nullable: false),
                    ToodeId = table.Column<int>(type: "INTEGER", nullable: true),
                    Toodenimetus = table.Column<string>(type: "TEXT", nullable: false),
                    Kogus = table.Column<int>(type: "INTEGER", nullable: false),
                    Hind = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OstuRead", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OstuRead_Ostud_OstId",
                        column: x => x.OstId,
                        principalTable: "Ostud",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OstuRead_Toodetabel_ToodeId",
                        column: x => x.ToodeId,
                        principalTable: "Toodetabel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ostud_KasutajaId",
                table: "Ostud",
                column: "KasutajaId");

            migrationBuilder.CreateIndex(
                name: "IX_OstuRead_OstId",
                table: "OstuRead",
                column: "OstId");

            migrationBuilder.CreateIndex(
                name: "IX_OstuRead_ToodeId",
                table: "OstuRead",
                column: "ToodeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OstuRead");

            migrationBuilder.DropTable(
                name: "Ostud");
        }
    }
}
