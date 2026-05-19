using FluentValidation;
using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Abstractions.Persistence;
using InfraOps.Application.Abstractions.Time;
using InfraOps.Application.Common.Exceptions;
using InfraOps.Application.Identity.Abstractions;
using InfraOps.Application.Inventory.Abstractions;
using InfraOps.Application.PreventiveExecutions.Abstractions;
using InfraOps.Application.PreventiveExecutions.Dtos;
using InfraOps.Application.PreventiveExecutions.Support;
using InfraOps.Application.PreventiveTemplates.Abstractions;
using InfraOps.Domain.Common.Exceptions;
using InfraOps.Domain.PreventiveExecutions.Entities;

namespace InfraOps.Application.PreventiveExecutions.Commands.StartPreventiveExecution;

public sealed class StartPreventiveExecutionCommandHandler
    : ICommandHandler<StartPreventiveExecutionCommand, PreventiveExecutionDetailsDto>
{
    private readonly IValidator<StartPreventiveExecutionCommand> _validator;
    private readonly IPreventiveExecutionRepository _preventiveExecutionRepository;
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly IPreventiveTemplateRepository _preventiveTemplateRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public StartPreventiveExecutionCommandHandler(
        IValidator<StartPreventiveExecutionCommand> validator,
        IPreventiveExecutionRepository preventiveExecutionRepository,
        IInventoryItemRepository inventoryItemRepository,
        IPreventiveTemplateRepository preventiveTemplateRepository,
        ICurrentUser currentUser,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _validator = validator;
        _preventiveExecutionRepository = preventiveExecutionRepository;
        _inventoryItemRepository = inventoryItemRepository;
        _preventiveTemplateRepository = preventiveTemplateRepository;
        _currentUser = currentUser;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<PreventiveExecutionDetailsDto> Handle(
        StartPreventiveExecutionCommand command,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var currentUserId = _currentUser.UserId
            ?? throw new ApplicationUnauthorizedException("Authenticated user context is required.");

        var inventoryItem = await _inventoryItemRepository.GetByIdAsync(command.InventoryItemId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Inventory item was not found.");

        var activeTemplate = (await _preventiveTemplateRepository.ListByEntityTypeAsync(
                inventoryItem.EntityTypeId,
                true,
                cancellationToken))
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault()
            ?? throw new DomainRuleException("Inventory item entity type does not have an active preventive template.");

        var execution = PreventiveExecution.CreateDraft(
            Guid.NewGuid(),
            inventoryItem,
            activeTemplate,
            currentUserId,
            _clock.UtcNow);

        await _preventiveExecutionRepository.AddAsync(execution, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdExecution = await _preventiveExecutionRepository.GetByIdAsync(execution.Id, cancellationToken)
            ?? execution;

        return PreventiveExecutionMappings.ToDetailsDto(createdExecution);
    }
}
