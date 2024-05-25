using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raspisanie.Migrations
{
    public partial class DeletePredmetDBNewFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Predmet_Teacher_SecondTeacherId",
                table: "Predmet");

            migrationBuilder.DropIndex(
                name: "IX_Predmet_SecondTeacherId",
                table: "Predmet");

            migrationBuilder.DropColumn(
                name: "SecondTeacherId",
                table: "Predmet");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SecondTeacherId",
                table: "Predmet",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Predmet_SecondTeacherId",
                table: "Predmet",
                column: "SecondTeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Predmet_Teacher_SecondTeacherId",
                table: "Predmet",
                column: "SecondTeacherId",
                principalTable: "Teacher",
                principalColumn: "Id");
        }
    }
}
