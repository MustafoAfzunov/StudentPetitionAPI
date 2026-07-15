using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentPetitionAPI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPetitionQueryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Petitions_StudentId",
                table: "Petitions");

            migrationBuilder.CreateIndex(
                name: "IX_Petitions_PetitionType_CreatedAt",
                table: "Petitions",
                columns: new[] { "PetitionType", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Petitions_Status_CreatedAt",
                table: "Petitions",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Petitions_StudentId_CreatedAt",
                table: "Petitions",
                columns: new[] { "StudentId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Petitions_PetitionType_CreatedAt",
                table: "Petitions");

            migrationBuilder.DropIndex(
                name: "IX_Petitions_Status_CreatedAt",
                table: "Petitions");

            migrationBuilder.DropIndex(
                name: "IX_Petitions_StudentId_CreatedAt",
                table: "Petitions");

            migrationBuilder.CreateIndex(
                name: "IX_Petitions_StudentId",
                table: "Petitions",
                column: "StudentId");
        }
    }
}
