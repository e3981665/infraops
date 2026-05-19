using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.PreventiveExecutions.Dtos;

namespace InfraOps.Application.PreventiveExecutions.Queries.GetPreventiveExecutionFormDefinition;

public sealed record GetPreventiveExecutionFormDefinitionQuery(Guid InventoryItemId)
    : IQuery<PreventiveExecutionFormDefinitionDto>;
