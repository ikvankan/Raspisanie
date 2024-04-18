using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raspisanie.Migrations
{
    public partial class AddAuditoriaTableToDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Auditoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Proektor = table.Column<bool>(type: "bit", nullable: false),
                    LekciaOrPractica = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditoria", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Auditoria");
        }
    }
}
