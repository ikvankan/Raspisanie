using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raspisanie.Migrations
{
    public partial class fixForegnnKeeyPredmetToGroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Group_Predmet_PredmetId",
                table: "Group");

            migrationBuilder.DropIndex(
                name: "IX_Group_PredmetId",
                table: "Group");

            migrationBuilder.DropColumn(
                name: "PredmetId",
                table: "Group");

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Predmet",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Predmet_GroupId",
                table: "Predmet",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Predmet_Group_GroupId",
                table: "Predmet",
                column: "GroupId",
                principalTable: "Group",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Predmet_Group_GroupId",
                table: "Predmet");

            migrationBuilder.DropIndex(
                name: "IX_Predmet_GroupId",
                table: "Predmet");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Predmet");

            migrationBuilder.AddColumn<int>(
                name: "PredmetId",
                table: "Group",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Group_PredmetId",
                table: "Group",
                column: "PredmetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Group_Predmet_PredmetId",
                table: "Group",
                column: "PredmetId",
                principalTable: "Predmet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
