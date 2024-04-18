using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raspisanie.Migrations
{
    public partial class AddTeacherTableToDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Teacher",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AuditoryId = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_Teacher_AuditoryId",
                table: "Teacher",
                column: "AuditoryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Teacher");
        }
    }
}
