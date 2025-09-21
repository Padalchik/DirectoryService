using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangePositionConfigurationHasForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_department_positions_positions_department_id",
                table: "department_positions");

            migrationBuilder.CreateIndex(
                name: "IX_department_positions_position_id",
                table: "department_positions",
                column: "position_id");

            migrationBuilder.AddForeignKey(
                name: "FK_department_positions_positions_position_id",
                table: "department_positions",
                column: "position_id",
                principalTable: "positions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_department_positions_positions_position_id",
                table: "department_positions");

            migrationBuilder.DropIndex(
                name: "IX_department_positions_position_id",
                table: "department_positions");

            migrationBuilder.AddForeignKey(
                name: "FK_department_positions_positions_department_id",
                table: "department_positions",
                column: "department_id",
                principalTable: "positions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
