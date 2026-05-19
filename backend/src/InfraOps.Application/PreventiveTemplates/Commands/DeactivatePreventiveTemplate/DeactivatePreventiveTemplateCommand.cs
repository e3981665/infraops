using InfraOps.Application.Abstractions.Messaging;

namespace InfraOps.Application.PreventiveTemplates.Commands.DeactivatePreventiveTemplate;

public sealed record DeactivatePreventiveTemplateCommand(Guid PreventiveTemplateId) : ICommand;
