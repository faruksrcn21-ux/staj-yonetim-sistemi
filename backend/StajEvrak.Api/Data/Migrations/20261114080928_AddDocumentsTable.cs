using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajEvrak.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "documents",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<int>(type: "INTEGER", nullable: false),
                    file_name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    file_path = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    file_extension = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    parsed_data = table.Column<string>(type: "TEXT", nullable: true),
                    manual_form_data = table.Column<string>(type: "TEXT", nullable: true),
                    is_ready_for_submission = table.Column<bool>(type: "INTEGER", nullable: false),
                    comparison_report = table.Column<string>(type: "TEXT", nullable: true),
                    uploaded_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documents", x => x.id);
                    table.ForeignKey(
                        name: "FK_documents_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_documents_status",
                table: "documents",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_documents_user_id",
                table: "documents",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "documents");
        }
    }
}
