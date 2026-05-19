using InfraOps.Domain.EntityTypes.Enums;

namespace InfraOps.Domain.EntityTypes.Models;

public sealed record EntityFieldDefinitionDraft(
    Guid? Id,
    string FieldKey,
    string DisplayLabel,
    EntityFieldType FieldType,
    int DisplayOrder,
    bool IsRequired,
    bool IsActive,
    string? Placeholder,
    string? HelpText,
    IReadOnlyCollection<EntityFieldOptionDraft> Options);
