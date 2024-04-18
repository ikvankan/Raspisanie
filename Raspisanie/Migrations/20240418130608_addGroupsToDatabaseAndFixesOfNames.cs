using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raspisanie.Migrations
{
    public partial class addGroupsToDatabaseAndFixesOfNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Specialnosti");

            migrationBuilder.RenameColumn(
                name: "hours",
                table: "Predmet",
                newName: "Hours");

            migrationBuilder.CreateTable(
                name: "Specialnost",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specialnost", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Course = table.Column<int>(type: "int", nullable: false),
                    SpecialnostId = table.Column<int>(type: "int", nullable: false),
                    AuditoriaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Groups_Auditoria_AuditoriaId",
                        column: x => x.AuditoriaId,
                        principalTable: "Auditoria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Groups_Specialnost_SpecialnostId",
                        column: x => x.SpecialnostId,
                        principalTable: "Specialnost",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Groups_AuditoriaId",
                table: "Groups",
                column: "AuditoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_SpecialnostId",
                table: "Groups",
                column: "SpecialnostId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "Specialnost");

            migrationBuilder.RenameColumn(
                name: "Hours",
                table: "Predmet",
                newName: "hours");

            migrationBuilder.CreateTable(
                name: "Specialnosti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specialnosti", x => x.Id);
                });
        }
    }
}
