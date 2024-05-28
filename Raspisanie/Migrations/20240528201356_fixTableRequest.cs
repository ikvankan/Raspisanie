using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raspisanie.Migrations
{
    public partial class fixTableRequest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Request_Auditoria_AuditoryId",
                table: "Request");

            migrationBuilder.RenameColumn(
                name: "AuditoryId",
                table: "Request",
                newName: "TeacherId");

            migrationBuilder.RenameIndex(
                name: "IX_Request_AuditoryId",
                table: "Request",
                newName: "IX_Request_TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Request_Teacher_TeacherId",
                table: "Request",
                column: "TeacherId",
                principalTable: "Teacher",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Request_Teacher_TeacherId",
                table: "Request");

            migrationBuilder.RenameColumn(
                name: "TeacherId",
                table: "Request",
                newName: "AuditoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Request_TeacherId",
                table: "Request",
                newName: "IX_Request_AuditoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Request_Auditoria_AuditoryId",
                table: "Request",
                column: "AuditoryId",
                principalTable: "Auditoria",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
