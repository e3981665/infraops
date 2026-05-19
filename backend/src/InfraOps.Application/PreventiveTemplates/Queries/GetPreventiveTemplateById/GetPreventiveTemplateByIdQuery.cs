using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.PreventiveTemplates.Dtos;

namespace InfraOps.Application.PreventiveTemplates.Queries.GetPreventiveTemplateById;

public sealed record GetPreventiveTemplateByIdQuery(Guid PreventiveTemplateId) : IQuery<PreventiveTemplateDetailsDto>;
