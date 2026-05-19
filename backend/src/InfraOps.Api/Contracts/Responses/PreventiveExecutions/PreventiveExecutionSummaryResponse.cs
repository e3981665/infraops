namespace InfraOps.Api.Contracts.Responses.PreventiveExecutions;

public sealed record PreventiveExecutionSummaryResponse(
    Guid Id,
    Guid InventoryItemId,
    string InventoryItemDisplayName,
    Guid PreventiveTemplateId,
    string PreventiveTemplateName,
    Guid EntityTypeId,
    string EntityTypeName,
    Guid RegionId,
    string RegionName,
    Guid SiteId,
    string SiteName,
    string Status,
    Guid CreatedBy,
    Guid UpdatedBy,
    Guid? SubmittedBy,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? SubmittedAtUtc);
