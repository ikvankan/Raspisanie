using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raspisanie.Migrations
{
    public partial class AddFieldAuditoriaId_fergnKeey_ToTablePlacement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AuditoriaId",
                table: "Placement",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Placement_AuditoriaId",
                table: "Placement",
                column: "AuditoriaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Placement_Auditoria_AuditoriaId",
                table: "Placement",
                column: "AuditoriaId",
                principalTable: "Auditoria",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Placement_Auditoria_AuditoriaId",
                table: "Placement");

            migrationBuilder.DropIndex(
                name: "IX_Placement_AuditoriaId",
                table: "Placement");

            migrationBuilder.DropColumn(
                name: "AuditoriaId",
                table: "Placement");
        }
    }
}
