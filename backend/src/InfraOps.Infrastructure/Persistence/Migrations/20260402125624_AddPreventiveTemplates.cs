using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraOps.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPreventiveTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "preventive_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Code = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_preventive_templates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_preventive_templates_entity_types_EntityTypeId",
                        column: x => x.EntityTypeId,
                        principalTable: "entity_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "preventive_template_sections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PreventiveTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_preventive_template_sections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_preventive_template_sections_preventive_templates_Preventiv~",
                        column: x => x.PreventiveTemplateId,
                        principalTable: "preventive_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "preventive_checklist_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PreventiveTemplateSectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Label = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    ItemType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    HelpText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsCritical = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresCommentOnFailure = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresPhotoOnFailure = table.Column<bool>(type: "boolean", nullable: false),
                    MinimumValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    MaximumValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_preventive_checklist_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_preventive_checklist_items_preventive_template_sections_Pre~",
                        column: x => x.PreventiveTemplateSectionId,
                        principalTable: "preventive_template_sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "preventive_checklist_options",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PreventiveChecklistItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Label = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_preventive_checklist_options", x => x.Id);
                    table.ForeignKey(
                        name: "FK_preventive_checklist_options_preventive_checklist_items_Pre~",
                        column: x => x.PreventiveChecklistItemId,
                        principalTable: "preventive_checklist_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_preventive_checklist_items_PreventiveTemplateSectionId_Disp~",
                table: "preventive_checklist_items",
                columns: new[] { "PreventiveTemplateSectionId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_preventive_checklist_items_PreventiveTemplateSectionId_Item~",
                table: "preventive_checklist_items",
                columns: new[] { "PreventiveTemplateSectionId", "ItemKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_preventive_checklist_options_PreventiveChecklistItemId_Disp~",
                table: "preventive_checklist_options",
                columns: new[] { "PreventiveChecklistItemId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_preventive_checklist_options_PreventiveChecklistItemId_Value",
                table: "preventive_checklist_options",
                columns: new[] { "PreventiveChecklistItemId", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_preventive_template_sections_PreventiveTemplateId_DisplayOr~",
                table: "preventive_template_sections",
                columns: new[] { "PreventiveTemplateId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_preventive_templates_Code",
                table: "preventive_templates",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_preventive_templates_EntityTypeId",
                table: "preventive_templates",
                column: "EntityTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "preventive_checklist_options");

            migrationBuilder.DropTable(
                name: "preventive_checklist_items");

            migrationBuilder.DropTable(
                name: "preventive_template_sections");

            migrationBuilder.DropTable(
                name: "preventive_templates");
        }
    }
}
