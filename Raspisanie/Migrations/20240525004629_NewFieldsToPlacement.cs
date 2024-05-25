using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raspisanie.Migrations
{
    public partial class NewFieldsToPlacement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SecondPredmetId",
                table: "Placement",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SecondTeacherId",
                table: "Placement",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TeacherId",
                table: "Placement",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Placement_SecondPredmetId",
                table: "Placement",
                column: "SecondPredmetId");

            migrationBuilder.CreateIndex(
                name: "IX_Placement_SecondTeacherId",
                table: "Placement",
                column: "SecondTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Placement_TeacherId",
                table: "Placement",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Placement_Predmet_SecondPredmetId",
                table: "Placement",
                column: "SecondPredmetId",
                principalTable: "Predmet",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Placement_Teacher_SecondTeacherId",
                table: "Placement",
                column: "SecondTeacherId",
                principalTable: "Teacher",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Placement_Teacher_TeacherId",
                table: "Placement",
                column: "TeacherId",
                principalTable: "Teacher",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Placement_Predmet_SecondPredmetId",
                table: "Placement");

            migrationBuilder.DropForeignKey(
                name: "FK_Placement_Teacher_SecondTeacherId",
                table: "Placement");

            migrationBuilder.DropForeignKey(
                name: "FK_Placement_Teacher_TeacherId",
                table: "Placement");

            migrationBuilder.DropIndex(
                name: "IX_Placement_SecondPredmetId",
                table: "Placement");

            migrationBuilder.DropIndex(
                name: "IX_Placement_SecondTeacherId",
                table: "Placement");

            migrationBuilder.DropIndex(
                name: "IX_Placement_TeacherId",
                table: "Placement");

            migrationBuilder.DropColumn(
                name: "SecondPredmetId",
                table: "Placement");

            migrationBuilder.DropColumn(
                name: "SecondTeacherId",
                table: "Placement");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "Placement");
        }
    }
}
