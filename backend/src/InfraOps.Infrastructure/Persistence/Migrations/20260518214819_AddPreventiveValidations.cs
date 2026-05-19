using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraOps.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPreventiveValidations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "preventive_validation_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PreventiveExecutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ValidatorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_preventive_validation_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_preventive_validation_records_preventive_executions_Prevent~",
                        column: x => x.PreventiveExecutionId,
                        principalTable: "preventive_executions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_preventive_validation_records_CreatedAtUtc",
                table: "preventive_validation_records",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_preventive_validation_records_PreventiveExecutionId",
                table: "preventive_validation_records",
                column: "PreventiveExecutionId");

            migrationBuilder.CreateIndex(
                name: "IX_preventive_validation_records_ValidatorUserId",
                table: "preventive_validation_records",
                column: "ValidatorUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "preventive_validation_records");
        }
    }
}
