using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.EntityTypes.Dtos;

namespace InfraOps.Application.EntityTypes.Queries.GetEntityTypeById;

public sealed record GetEntityTypeByIdQuery(Guid EntityTypeId) : IQuery<EntityTypeDetailsDto>;
