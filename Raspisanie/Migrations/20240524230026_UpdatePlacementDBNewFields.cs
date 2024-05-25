using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raspisanie.Migrations
{
    public partial class UpdatePlacementDBNewFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SecondAuditoriaId",
                table: "Placement",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Placement_SecondAuditoriaId",
                table: "Placement",
                column: "SecondAuditoriaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Placement_Auditoria_SecondAuditoriaId",
                table: "Placement",
                column: "SecondAuditoriaId",
                principalTable: "Auditoria",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Placement_Auditoria_SecondAuditoriaId",
                table: "Placement");

            migrationBuilder.DropIndex(
                name: "IX_Placement_SecondAuditoriaId",
                table: "Placement");

            migrationBuilder.DropColumn(
                name: "SecondAuditoriaId",
                table: "Placement");
        }
    }
}
