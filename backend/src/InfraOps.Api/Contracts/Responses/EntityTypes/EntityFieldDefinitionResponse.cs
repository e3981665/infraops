namespace InfraOps.Api.Contracts.Responses.EntityTypes;

public sealed record EntityFieldDefinitionResponse(
    Guid Id,
    string FieldKey,
    string DisplayLabel,
    string FieldType,
    int DisplayOrder,
    bool IsRequired,
    bool IsActive,
    string? Placeholder,
    string? HelpText,
    IReadOnlyCollection<EntityFieldOptionResponse> Options);
