using InfraOps.Infrastructure.Persistence.Seeding;

namespace InfraOps.Infrastructure.Tests.Persistence.Seeding;

public sealed class DemoSeedLabelsTests
{
    [Fact]
    public void Should_ReturnEnglishLabels_When_LocaleIsNotPortuguese()
    {
        var labels = DemoSeedLabels.ForLocale("en-US");

        Assert.Equal("Visual Inspection", labels.VisualInspectionSection);
        Assert.Equal("Equipment clean?", labels.EquipmentCleanQuestion);
        Assert.Equal("Any active alarm?", labels.ActiveAlarmQuestion);
        Assert.Equal("Good", labels.GoodOption);
    }

    [Fact]
    public void Should_ReturnPortugueseLabels_When_LocaleIsPortugueseBrazil()
    {
        var labels = DemoSeedLabels.ForLocale("pt-BR");

        Assert.Equal("Inspeção visual", labels.VisualInspectionSection);
        Assert.Equal("Equipamento limpo?", labels.EquipmentCleanQuestion);
        Assert.Equal("Existe alarme ativo?", labels.ActiveAlarmQuestion);
        Assert.Equal("Bom", labels.GoodOption);
    }

    [Fact]
    public void Should_FallbackToEnglishLabels_When_LocaleIsUnknown()
    {
        var labels = DemoSeedLabels.ForLocale("fr-FR");

        Assert.Equal("Quarterly UPS Inspection", labels.UpsTemplateName);
        Assert.Equal("Operational Readings", labels.OperationalReadingsSection);
    }
}
