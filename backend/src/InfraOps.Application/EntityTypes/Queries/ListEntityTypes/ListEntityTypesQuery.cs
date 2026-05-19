using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.EntityTypes.Dtos;

namespace InfraOps.Application.EntityTypes.Queries.ListEntityTypes;

public sealed record ListEntityTypesQuery() : IQuery<IReadOnlyCollection<EntityTypeSummaryDto>>;
