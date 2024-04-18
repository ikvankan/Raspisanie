using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raspisanie.Migrations
{
    public partial class AddDbAuditoriaAndForeignKeyAuditoriaId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AuditoryID",
                table: "Teacher",
                newName: "AuditoryId");

            migrationBuilder.CreateTable(
                name: "Auditoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Proektor = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditoria", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Teacher_AuditoryId",
                table: "Teacher",
                column: "AuditoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Teacher_Auditoria_AuditoryId",
                table: "Teacher",
                column: "AuditoryId",
                principalTable: "Auditoria",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teacher_Auditoria_AuditoryId",
                table: "Teacher");

            migrationBuilder.DropTable(
                name: "Auditoria");

            migrationBuilder.DropIndex(
                name: "IX_Teacher_AuditoryId",
                table: "Teacher");

            migrationBuilder.RenameColumn(
                name: "AuditoryId",
                table: "Teacher",
                newName: "AuditoryID");
        }
    }
}
