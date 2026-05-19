namespace InfraOps.Api.Contracts.Requests.EntityTypes;

public sealed record EntityFieldDefinitionRequest(
    Guid? Id,
    string FieldKey,
    string DisplayLabel,
    string FieldType,
    int DisplayOrder,
    bool IsRequired,
    bool IsActive,
    string? Placeholder,
    string? HelpText,
    IReadOnlyCollection<EntityFieldOptionRequest> Options);
