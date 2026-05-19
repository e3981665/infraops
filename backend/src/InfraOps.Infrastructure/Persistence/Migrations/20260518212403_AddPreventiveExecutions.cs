using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraOps.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPreventiveExecutions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "preventive_executions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreventiveTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreventiveTemplateName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    PreventiveTemplateCode = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    EntityTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityTypeName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    EntityTypeCode = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    InventoryItemDisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RegionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegionName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    SiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    SiteName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SubmittedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_preventive_executions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_preventive_executions_inventory_items_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalTable: "inventory_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_preventive_executions_preventive_templates_PreventiveTempla~",
                        column: x => x.PreventiveTemplateId,
                        principalTable: "preventive_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "preventive_execution_answers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PreventiveExecutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Value = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_preventive_execution_answers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_preventive_execution_answers_preventive_executions_Preventi~",
                        column: x => x.PreventiveExecutionId,
                        principalTable: "preventive_executions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "preventive_execution_template_sections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PreventiveExecutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceTemplateSectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_preventive_execution_template_sections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_preventive_execution_template_sections_preventive_execution~",
                        column: x => x.PreventiveExecutionId,
                        principalTable: "preventive_executions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "preventive_execution_template_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PreventiveExecutionTemplateSectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceChecklistItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Label = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    ItemType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    HelpText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsCritical = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresCommentOnFailure = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresPhotoOnFailure = table.Column<bool>(type: "boolean", nullable: false),
                    MinimumValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    MaximumValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_preventive_execution_template_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_preventive_execution_template_items_preventive_execution_te~",
                        column: x => x.PreventiveExecutionTemplateSectionId,
                        principalTable: "preventive_execution_template_sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "preventive_execution_template_options",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PreventiveExecutionTemplateItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Label = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_preventive_execution_template_options", x => x.Id);
                    table.ForeignKey(
                        name: "FK_preventive_execution_template_options_preventive_execution_~",
                        column: x => x.PreventiveExecutionTemplateItemId,
                        principalTable: "preventive_execution_template_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_preventive_execution_answers_PreventiveExecutionId_ItemKey",
                table: "preventive_execution_answers",
                columns: new[] { "PreventiveExecutionId", "ItemKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_preventive_execution_template_items_PreventiveExecutionTem~1",
                table: "preventive_execution_template_items",
                columns: new[] { "PreventiveExecutionTemplateSectionId", "ItemKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_preventive_execution_template_items_PreventiveExecutionTemp~",
                table: "preventive_execution_template_items",
                columns: new[] { "PreventiveExecutionTemplateSectionId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_preventive_execution_template_options_PreventiveExecutionT~1",
                table: "preventive_execution_template_options",
                columns: new[] { "PreventiveExecutionTemplateItemId", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_preventive_execution_template_options_PreventiveExecutionTe~",
                table: "preventive_execution_template_options",
                columns: new[] { "PreventiveExecutionTemplateItemId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_preventive_execution_template_sections_PreventiveExecutionI~",
                table: "preventive_execution_template_sections",
                columns: new[] { "PreventiveExecutionId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_preventive_executions_CreatedAtUtc",
                table: "preventive_executions",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_preventive_executions_CreatedBy",
                table: "preventive_executions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_preventive_executions_EntityTypeId",
                table: "preventive_executions",
                column: "EntityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_preventive_executions_InventoryItemId",
                table: "preventive_executions",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_preventive_executions_PreventiveTemplateId",
                table: "preventive_executions",
                column: "PreventiveTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_preventive_executions_RegionId",
                table: "preventive_executions",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_preventive_executions_SiteId",
                table: "preventive_executions",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_preventive_executions_Status",
                table: "preventive_executions",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "preventive_execution_answers");

            migrationBuilder.DropTable(
                name: "preventive_execution_template_options");

            migrationBuilder.DropTable(
                name: "preventive_execution_template_items");

            migrationBuilder.DropTable(
                name: "preventive_execution_template_sections");

            migrationBuilder.DropTable(
                name: "preventive_executions");
        }
    }
}
