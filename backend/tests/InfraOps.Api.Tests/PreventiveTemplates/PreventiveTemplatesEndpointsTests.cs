using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using InfraOps.Api.Contracts.Requests.Auth;
using InfraOps.Api.Contracts.Requests.EntityTypes;
using InfraOps.Api.Contracts.Requests.PreventiveTemplates;
using InfraOps.Api.Contracts.Responses.Auth;
using InfraOps.Api.Contracts.Responses.EntityTypes;
using InfraOps.Api.Contracts.Responses.PreventiveTemplates;
using InfraOps.Api.Tests.Infrastructure;

namespace InfraOps.Api.Tests.PreventiveTemplates;

public sealed class PreventiveTemplatesEndpointsTests : IClassFixture<InfraOpsApiFactory>
{
    private readonly InfraOpsApiFactory _factory;

    public PreventiveTemplatesEndpointsTests(InfraOpsApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_RequireAuthentication_When_ListingPreventiveTemplatesWithoutToken()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/api/preventive-templates");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnPreventiveTemplateFormMetadata_When_AdminUserIsAuthorized()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var entityType = await CreateEntityTypeAsync(client, $"preventive-metadata-{Guid.NewGuid():N}");

        using var response = await client.GetAsync("/api/preventive-templates/form-metadata");
        var payload = await response.Content.ReadFromJsonAsync<PreventiveTemplateFormMetadataResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Contains(payload.EntityTypes, x => x.Id == entityType.Id && x.Code == entityType.Code);
    }

    [Fact]
    public async Task Should_CreatePreventiveTemplate_When_AdminUserIsAuthorized()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var entityType = await CreateEntityTypeAsync(client, $"preventive-create-{Guid.NewGuid():N}");

        using var response = await client.PostAsJsonAsync(
            "/api/preventive-templates",
            CreatePreventiveTemplateRequest(entityType.Id, "UPS Quarterly Inspection", "ups-quarterly-inspection"));
        var rawContent = await response.Content.ReadAsStringAsync();
        var payload = await response.Content.ReadFromJsonAsync<PreventiveTemplateResponse>();

        Assert.True(response.IsSuccessStatusCode, rawContent);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(entityType.Id, payload.EntityTypeId);
        Assert.Equal("UPS Quarterly Inspection", payload.Name);
        Assert.Single(payload.Sections);
        Assert.Equal(2, payload.Sections.Single().ChecklistItems.Count);
    }

    [Fact]
    public async Task Should_GetListAndFilterPreventiveTemplates_When_AdminUserIsAuthorized()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var entityType = await CreateEntityTypeAsync(client, $"preventive-list-{Guid.NewGuid():N}");
        var createdTemplate = await CreatePreventiveTemplateAsync(
            client,
            entityType.Id,
            "UPS Semiannual Inspection",
            "ups-semiannual-inspection");

        using var getResponse = await client.GetAsync($"/api/preventive-templates/{createdTemplate.Id}");
        using var listResponse = await client.GetAsync(
            $"/api/preventive-templates?entityTypeId={entityType.Id:D}&search={Uri.EscapeDataString(createdTemplate.Code)}");
        using var byEntityTypeResponse = await client.GetAsync($"/api/preventive-templates/by-entity-type/{entityType.Id:D}");
        var getPayload = await getResponse.Content.ReadFromJsonAsync<PreventiveTemplateResponse>();
        var listPayload = await listResponse.Content.ReadFromJsonAsync<IReadOnlyCollection<PreventiveTemplateSummaryResponse>>();
        var byEntityTypePayload = await byEntityTypeResponse.Content.ReadFromJsonAsync<IReadOnlyCollection<PreventiveTemplateResponse>>();

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, byEntityTypeResponse.StatusCode);
        Assert.NotNull(getPayload);
        Assert.NotNull(listPayload);
        Assert.NotNull(byEntityTypePayload);
        Assert.Equal(createdTemplate.Id, getPayload.Id);
        Assert.Contains(listPayload, x => x.Id == createdTemplate.Id && x.ChecklistItemCount == 2);
        Assert.Contains(byEntityTypePayload, x => x.Id == createdTemplate.Id && x.Sections.Count == 1);
    }

    [Fact]
    public async Task Should_UpdateAndDeactivatePreventiveTemplate_When_AdminUserIsAuthorized()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var entityType = await CreateEntityTypeAsync(client, $"preventive-update-{Guid.NewGuid():N}");
        var createdTemplate = await CreatePreventiveTemplateAsync(
            client,
            entityType.Id,
            "UPS Monthly Inspection",
            "ups-monthly-inspection");
        var existingSection = createdTemplate.Sections.Single();
        var existingItem = existingSection.ChecklistItems.Single(x => x.ItemKey == "equipmentClean");

        using var updateResponse = await client.PutAsJsonAsync(
            $"/api/preventive-templates/{createdTemplate.Id}",
            new UpdatePreventiveTemplateRequest(
                "UPS Monthly Inspection Updated",
                "ups-monthly-inspection-updated",
                "Updated checklist.",
                [new PreventiveTemplateSectionRequest(
                    existingSection.Id,
                    "Electrical Measurements",
                    1,
                    true,
                    [new PreventiveChecklistItemRequest(
                        existingItem.Id,
                        existingItem.ItemKey,
                        "Equipment clean and accessible?",
                        "yesNo",
                        1,
                        true,
                        true,
                        "Inspect external cleanliness and access clearance.",
                        true,
                        true,
                        false,
                        null,
                        null,
                        [])])]));

        using var deactivateResponse = await client.PostAsync($"/api/preventive-templates/{createdTemplate.Id}/deactivate", null);
        using var getResponse = await client.GetAsync($"/api/preventive-templates/{createdTemplate.Id}");
        var updatePayload = await updateResponse.Content.ReadFromJsonAsync<PreventiveTemplateResponse>();
        var getPayload = await getResponse.Content.ReadFromJsonAsync<PreventiveTemplateResponse>();

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, deactivateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(updatePayload);
        Assert.NotNull(getPayload);
        Assert.Equal("UPS Monthly Inspection Updated", updatePayload.Name);
        Assert.False(getPayload.IsActive);
        Assert.Equal("Electrical Measurements", getPayload.Sections.Single().Title);
        Assert.Contains(getPayload.Sections.Single().ChecklistItems, x => x.IsCritical);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var tokens = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        return client;
    }

    private static async Task<TokenResponse> LoginAsync(HttpClient client)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("admin@infraops.local", "InfraOps.Admin!123"));

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<TokenResponse>())!;
    }

    private static async Task<EntityTypeResponse> CreateEntityTypeAsync(HttpClient client, string code)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/entity-types",
            new CreateEntityTypeRequest(
                "UPS",
                code,
                "Critical power backup assets.",
                [new EntityFieldDefinitionRequest(
                    null,
                    "serialNumber",
                    "Serial Number",
                    "text",
                    1,
                    true,
                    true,
                    null,
                    null,
                    [])]));

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<EntityTypeResponse>())!;
    }

    private static CreatePreventiveTemplateRequest CreatePreventiveTemplateRequest(
        Guid entityTypeId,
        string name,
        string code)
    {
        return new CreatePreventiveTemplateRequest(
            entityTypeId,
            name,
            code,
            "Preventive checklist definition for UPS assets.",
            [new PreventiveTemplateSectionRequest(
                null,
                "Visual Inspection",
                1,
                true,
                [
                    new PreventiveChecklistItemRequest(
                        null,
                        "equipmentClean",
                        "Equipment clean?",
                        "yesNo",
                        1,
                        true,
                        true,
                        null,
                        false,
                        false,
                        false,
                        null,
                        null,
                        []),
                    new PreventiveChecklistItemRequest(
                        null,
                        "batteryCondition",
                        "Battery condition",
                        "select",
                        2,
                        true,
                        true,
                        null,
                        false,
                        true,
                        false,
                        null,
                        null,
                        [
                            new PreventiveChecklistOptionRequest(null, "good", "Good", 1),
                            new PreventiveChecklistOptionRequest(null, "warning", "Warning", 2),
                            new PreventiveChecklistOptionRequest(null, "critical", "Critical", 3)
                        ])
                ])]);
    }

    private static async Task<PreventiveTemplateResponse> CreatePreventiveTemplateAsync(
        HttpClient client,
        Guid entityTypeId,
        string name,
        string code)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/preventive-templates",
            CreatePreventiveTemplateRequest(entityTypeId, name, code));

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<PreventiveTemplateResponse>())!;
    }
}
