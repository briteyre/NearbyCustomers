using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreCodeCamp.Data.Migrations
{
    public partial class RenameMonikerToCity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename the column at the database level to preserve existing data
            migrationBuilder.RenameColumn(
                name: "Moniker",
                table: "Camps",
                newName: "City");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert the rename if the migration is rolled back
            migrationBuilder.RenameColumn(
                name: "City",
                table: "Camps",
                newName: "Moniker");
        }
    }
}
