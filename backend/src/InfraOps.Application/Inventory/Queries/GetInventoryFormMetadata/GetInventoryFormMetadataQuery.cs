using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Inventory.Dtos;

namespace InfraOps.Application.Inventory.Queries.GetInventoryFormMetadata;

public sealed record GetInventoryFormMetadataQuery() : IQuery<InventoryFormMetadataDto>;
