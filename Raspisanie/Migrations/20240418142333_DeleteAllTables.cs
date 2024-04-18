using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raspisanie.Migrations
{
    public partial class DeleteAllTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Group");

            migrationBuilder.DropTable(
                name: "Predmet");

            migrationBuilder.DropTable(
                name: "Specialnost");

            migrationBuilder.DropTable(
                name: "Teacher");

            migrationBuilder.DropTable(
                name: "Auditoria");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Auditoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LekciaOrPractica = table.Column<bool>(type: "bit", nullable: false),
                    Proektor = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditoria", x => x.Id);
                });

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
                name: "Teacher",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditoryId = table.Column<int>(type: "int", nullable: false),
                    TeacherName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teacher", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teacher_Auditoria_AuditoryId",
                        column: x => x.AuditoryId,
                        principalTable: "Auditoria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Group",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditoriaId = table.Column<int>(type: "int", nullable: false),
                    SpecialnostId = table.Column<int>(type: "int", nullable: false),
                    Course = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Group", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Group_Auditoria_AuditoriaId",
                        column: x => x.AuditoriaId,
                        principalTable: "Auditoria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Group_Specialnost_SpecialnostId",
                        column: x => x.SpecialnostId,
                        principalTable: "Specialnost",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Predmet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    Hours = table.Column<int>(type: "int", nullable: false),
                    PredmetName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Predmet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Predmet_Teacher_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teacher",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Group_AuditoriaId",
                table: "Group",
                column: "AuditoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Group_SpecialnostId",
                table: "Group",
                column: "SpecialnostId");

            migrationBuilder.CreateIndex(
                name: "IX_Predmet_TeacherId",
                table: "Predmet",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Teacher_AuditoryId",
                table: "Teacher",
                column: "AuditoryId");
        }
    }
}
