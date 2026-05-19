namespace InfraOps.Application.EntityTypes.Support;

public sealed record EntityFieldDefinitionInput(
    Guid? Id,
    string FieldKey,
    string DisplayLabel,
    string FieldType,
    int DisplayOrder,
    bool IsRequired,
    bool IsActive,
    string? Placeholder,
    string? HelpText,
    IReadOnlyCollection<EntityFieldOptionInput> Options);
