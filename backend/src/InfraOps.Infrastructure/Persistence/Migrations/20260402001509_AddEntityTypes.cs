using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraOps.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEntityTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "entity_types",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entity_types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "entity_field_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    DisplayLabel = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    FieldType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Placeholder = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    HelpText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entity_field_definitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_entity_field_definitions_entity_types_EntityTypeId",
                        column: x => x.EntityTypeId,
                        principalTable: "entity_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "entity_field_options",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityFieldDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Label = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entity_field_options", x => x.Id);
                    table.ForeignKey(
                        name: "FK_entity_field_options_entity_field_definitions_EntityFieldDe~",
                        column: x => x.EntityFieldDefinitionId,
                        principalTable: "entity_field_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_entity_field_definitions_EntityTypeId_FieldKey",
                table: "entity_field_definitions",
                columns: new[] { "EntityTypeId", "FieldKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_entity_field_options_EntityFieldDefinitionId_Value",
                table: "entity_field_options",
                columns: new[] { "EntityFieldDefinitionId", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_entity_types_Code",
                table: "entity_types",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "entity_field_options");

            migrationBuilder.DropTable(
                name: "entity_field_definitions");

            migrationBuilder.DropTable(
                name: "entity_types");
        }
    }
}
