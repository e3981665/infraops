using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.PreventiveTemplates.Dtos;

namespace InfraOps.Application.PreventiveTemplates.Queries.GetPreventiveTemplateFormMetadata;

public sealed record GetPreventiveTemplateFormMetadataQuery() : IQuery<PreventiveTemplateFormMetadataDto>;
