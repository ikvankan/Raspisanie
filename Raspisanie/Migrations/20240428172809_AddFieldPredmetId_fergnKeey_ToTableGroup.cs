using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raspisanie.Migrations
{
    public partial class AddFieldPredmetId_fergnKeey_ToTableGroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
