using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using InfraOps.Api.Contracts.Requests.Auth;
using InfraOps.Api.Contracts.Requests.EntityTypes;
using InfraOps.Api.Contracts.Requests.Inventory;
using InfraOps.Api.Contracts.Requests.PreventiveExecutions;
using InfraOps.Api.Contracts.Requests.PreventiveTemplates;
using InfraOps.Api.Contracts.Responses.Auth;
using InfraOps.Api.Contracts.Responses.EntityTypes;
using InfraOps.Api.Contracts.Responses.Inventory;
using InfraOps.Api.Contracts.Responses.PreventiveExecutions;
using InfraOps.Api.Contracts.Responses.PreventiveTemplates;
using InfraOps.Api.Tests.Infrastructure;

namespace InfraOps.Api.Tests.PreventiveExecutions;

public sealed class PreventiveExecutionEndpointsTests : IClassFixture<InfraOpsApiFactory>
{
    private readonly InfraOpsApiFactory _factory;

    public PreventiveExecutionEndpointsTests(InfraOpsApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_RequireAuthentication_When_ListingPreventiveExecutionsWithoutToken()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/api/preventive-executions");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_StartSaveSubmitAndReturnPreventiveExecution_When_UserIsAuthorized()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var entityType = await CreateEntityTypeAsync(client, $"execution-{Guid.NewGuid():N}");
        var metadata = await GetInventoryFormMetadataAsync(client);
        var (regionId, siteId) = ResolveLocationContext(metadata);
        var inventoryItem = await CreateInventoryItemAsync(client, entityType.Id, regionId, siteId);
        var template = await CreatePreventiveTemplateAsync(client, entityType.Id, $"execution-template-{Guid.NewGuid():N}");

        using var formResponse = await client.GetAsync($"/api/preventive-executions/form-definition/{inventoryItem.Id}");
        var formPayload = await formResponse.Content.ReadFromJsonAsync<PreventiveExecutionFormDefinitionResponse>();

        using var startResponse = await client.PostAsJsonAsync(
            "/api/preventive-executions/start",
            new StartPreventiveExecutionRequest(inventoryItem.Id));
        var started = await startResponse.Content.ReadFromJsonAsync<PreventiveExecutionResponse>();

        using var draftResponse = await client.PutAsJsonAsync(
            $"/api/preventive-executions/{started!.Id}/draft",
            new SavePreventiveExecutionDraftRequest(
                [
                    new PreventiveExecutionAnswerRequest("equipmentClean", "true", null),
                    new PreventiveExecutionAnswerRequest("inputVoltage", "220", null)
                ]));
        var draft = await draftResponse.Content.ReadFromJsonAsync<PreventiveExecutionResponse>();

        using var getDraftResponse = await client.GetAsync($"/api/preventive-executions/{started.Id}");
        var persistedDraft = await getDraftResponse.Content.ReadFromJsonAsync<PreventiveExecutionResponse>();

        using var submitResponse = await client.PostAsJsonAsync(
            $"/api/preventive-executions/{started.Id}/submit",
            new SubmitPreventiveExecutionRequest(
                [
                    new PreventiveExecutionAnswerRequest("equipmentClean", "yes", null),
                    new PreventiveExecutionAnswerRequest("activeAlarm", "yes", null),
                    new PreventiveExecutionAnswerRequest("inputVoltage", "220", null)
                ]));
        var submitted = await submitResponse.Content.ReadFromJsonAsync<PreventiveExecutionResponse>();

        using var getResponse = await client.GetAsync($"/api/preventive-executions/{started.Id}");
        using var listResponse = await client.GetAsync($"/api/preventive-executions?status=submitted&inventoryItemId={inventoryItem.Id:D}");
        var listRawContent = await listResponse.Content.ReadAsStringAsync();
        Assert.True(listResponse.IsSuccessStatusCode, listRawContent);
        var listed = await listResponse.Content.ReadFromJsonAsync<IReadOnlyCollection<PreventiveExecutionSummaryResponse>>();

        Assert.Equal(HttpStatusCode.OK, formResponse.StatusCode);
        Assert.NotNull(formPayload);
        Assert.Equal(template.Id, formPayload.PreventiveTemplateId);
        Assert.Equal(HttpStatusCode.Created, startResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, draftResponse.StatusCode);
        Assert.NotNull(draft);
        Assert.Equal("draft", draft.Status);
        Assert.Contains(draft.Answers, x => x.ItemKey == "equipmentClean" && x.Value == "true");
        Assert.Contains(draft.Answers, x => x.ItemKey == "inputVoltage" && x.Value == "220");
        Assert.Equal(HttpStatusCode.OK, getDraftResponse.StatusCode);
        Assert.NotNull(persistedDraft);
        Assert.Contains(persistedDraft.Answers, x => x.ItemKey == "equipmentClean" && x.Value == "true");
        Assert.Contains(persistedDraft.Answers, x => x.ItemKey == "inputVoltage" && x.Value == "220");
        Assert.Equal(HttpStatusCode.OK, submitResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.NotNull(submitted);
        Assert.Equal("submitted", submitted.Status);
        Assert.Contains(submitted.Answers, x => x.ItemKey == "inputVoltage" && x.Value == "220");
        Assert.Contains(listed!, x => x.Id == started.Id);
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
                [new EntityFieldDefinitionRequest(null, "serialNumber", "Serial Number", "text", 1, true, true, null, null, [])]));

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<EntityTypeResponse>())!;
    }

    private static async Task<InventoryFormMetadataResponse> GetInventoryFormMetadataAsync(HttpClient client)
    {
        using var response = await client.GetAsync("/api/inventory/form-metadata");
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<InventoryFormMetadataResponse>())!;
    }

    private static (Guid RegionId, Guid SiteId) ResolveLocationContext(InventoryFormMetadataResponse metadata)
    {
        var region = metadata.Regions.First();
        var site = metadata.Sites.First(x => x.RegionId == region.Id);

        return (region.Id, site.Id);
    }

    private static async Task<InventoryItemResponse> CreateInventoryItemAsync(
        HttpClient client,
        Guid entityTypeId,
        Guid regionId,
        Guid siteId)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/inventory",
            new CreateInventoryItemRequest(
                entityTypeId,
                regionId,
                siteId,
                "UPS-01",
                "operational",
                new DateOnly(2024, 4, 1),
                [new InventoryAttributeValueRequest("serialNumber", $"serial-{Guid.NewGuid():N}".Substring(0, 18))]));

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<InventoryItemResponse>())!;
    }

    private static async Task<PreventiveTemplateResponse> CreatePreventiveTemplateAsync(
        HttpClient client,
        Guid entityTypeId,
        string code)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/preventive-templates",
            new CreatePreventiveTemplateRequest(
                entityTypeId,
                "Quarterly UPS Inspection",
                code,
                null,
                [
                    new PreventiveTemplateSectionRequest(
                        null,
                        "Visual Inspection",
                        1,
                        true,
                        [
                            new PreventiveChecklistItemRequest(null, "equipmentClean", "Equipment clean?", "yesNo", 1, true, true, null, false, false, false, null, null, []),
                            new PreventiveChecklistItemRequest(null, "activeAlarm", "Any active alarm?", "yesNo", 2, true, true, null, true, true, false, null, null, [])
                        ]),
                    new PreventiveTemplateSectionRequest(
                        null,
                        "Electrical Measurements",
                        2,
                        true,
                        [
                            new PreventiveChecklistItemRequest(null, "inputVoltage", "Input voltage", "numeric", 1, true, true, null, true, false, false, 210, 240, [])
                        ])
                ]));

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<PreventiveTemplateResponse>())!;
    }
}
