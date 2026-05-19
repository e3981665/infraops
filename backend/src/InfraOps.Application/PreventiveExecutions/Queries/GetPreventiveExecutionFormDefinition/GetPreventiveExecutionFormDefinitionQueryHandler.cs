using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Common.Exceptions;
using InfraOps.Application.Inventory.Abstractions;
using InfraOps.Application.PreventiveExecutions.Dtos;
using InfraOps.Application.PreventiveExecutions.Support;
using InfraOps.Application.PreventiveTemplates.Abstractions;
using InfraOps.Domain.Common.Exceptions;

namespace InfraOps.Application.PreventiveExecutions.Queries.GetPreventiveExecutionFormDefinition;

public sealed class GetPreventiveExecutionFormDefinitionQueryHandler
    : IQueryHandler<GetPreventiveExecutionFormDefinitionQuery, PreventiveExecutionFormDefinitionDto>
{
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly IPreventiveTemplateRepository _preventiveTemplateRepository;

    public GetPreventiveExecutionFormDefinitionQueryHandler(
        IInventoryItemRepository inventoryItemRepository,
        IPreventiveTemplateRepository preventiveTemplateRepository)
    {
        _inventoryItemRepository = inventoryItemRepository;
        _preventiveTemplateRepository = preventiveTemplateRepository;
    }

    public async Task<PreventiveExecutionFormDefinitionDto> Handle(
        GetPreventiveExecutionFormDefinitionQuery query,
        CancellationToken cancellationToken)
    {
        var inventoryItem = await _inventoryItemRepository.GetByIdAsync(query.InventoryItemId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Inventory item was not found.");

        if (!inventoryItem.IsActive)
        {
            throw new DomainRuleException("Preventive execution form requires an active inventory item.");
        }

        var activeTemplate = (await _preventiveTemplateRepository.ListByEntityTypeAsync(
                inventoryItem.EntityTypeId,
                true,
                cancellationToken))
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault()
            ?? throw new DomainRuleException("Inventory item entity type does not have an active preventive template.");

        return PreventiveExecutionMappings.ToFormDefinitionDto(inventoryItem, activeTemplate);
    }
}
