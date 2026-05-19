using InfraOps.Application.Abstractions.Messaging;

namespace InfraOps.Application.PreventiveTemplates.Commands.ActivatePreventiveTemplate;

public sealed record ActivatePreventiveTemplateCommand(Guid PreventiveTemplateId) : ICommand;
