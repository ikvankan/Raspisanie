using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raspisanie.Migrations
{
    public partial class AddPlacementTableToDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Placement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    PredmetId = table.Column<int>(type: "int", nullable: false),
                    index = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Placement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Placement_Group_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Group",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Placement_Predmet_PredmetId",
                        column: x => x.PredmetId,
                        principalTable: "Predmet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Placement_GroupId",
                table: "Placement",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Placement_PredmetId",
                table: "Placement",
                column: "PredmetId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Placement");
        }
    }
}
