using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Inventory.Dtos;

namespace InfraOps.Application.Inventory.Queries.GetInventoryFormDefinitionByEntityType;

public sealed record GetInventoryFormDefinitionByEntityTypeQuery(Guid EntityTypeId) : IQuery<InventoryFormDefinitionDto>;
