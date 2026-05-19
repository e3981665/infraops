using InfraOps.Domain.Common.Exceptions;
using InfraOps.Domain.EntityTypes.ValueObjects;

namespace InfraOps.Domain.Locations.Entities;

public sealed class Region
{
    private Region()
    {
    }

    public Region(Guid id, string code, string name)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Region id is required.");
        }

        Id = id;
        Code = EntityTypeCode.Create(code).Value;
        Name = NormalizeName(name);
        IsActive = true;
    }

    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainRuleException("Region name is required.");
        }

        var normalizedName = name.Trim();

        if (normalizedName.Length > 120)
        {
            throw new DomainRuleException("Region name cannot exceed 120 characters.");
        }

        return normalizedName;
    }
}
