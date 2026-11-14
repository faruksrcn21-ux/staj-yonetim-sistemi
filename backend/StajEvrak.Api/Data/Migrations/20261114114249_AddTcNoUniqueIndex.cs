using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajEvrak.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTcNoUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_admin_test_dataset_tc_no",
                table: "admin_test_dataset",
                column: "tc_no",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_admin_test_dataset_tc_no",
                table: "admin_test_dataset");
        }
    }
}
