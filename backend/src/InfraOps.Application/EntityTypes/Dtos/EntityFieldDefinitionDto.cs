namespace InfraOps.Application.EntityTypes.Dtos;

public sealed record EntityFieldDefinitionDto(
    Guid Id,
    string FieldKey,
    string DisplayLabel,
    string FieldType,
    int DisplayOrder,
    bool IsRequired,
    bool IsActive,
    string? Placeholder,
    string? HelpText,
    IReadOnlyCollection<EntityFieldOptionDto> Options);
