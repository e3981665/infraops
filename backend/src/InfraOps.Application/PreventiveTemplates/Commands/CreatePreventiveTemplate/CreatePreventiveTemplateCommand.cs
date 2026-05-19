using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.PreventiveTemplates.Dtos;
using InfraOps.Application.PreventiveTemplates.Support;

namespace InfraOps.Application.PreventiveTemplates.Commands.CreatePreventiveTemplate;

public sealed record CreatePreventiveTemplateCommand(
    Guid EntityTypeId,
    string Name,
    string Code,
    string? Description,
    IReadOnlyCollection<PreventiveTemplateSectionInput> Sections) : ICommand<PreventiveTemplateDetailsDto>;
