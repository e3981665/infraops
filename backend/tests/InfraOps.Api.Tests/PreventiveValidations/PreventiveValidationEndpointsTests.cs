using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using InfraOps.Api.Contracts.Requests.Auth;
using InfraOps.Api.Contracts.Requests.EntityTypes;
using InfraOps.Api.Contracts.Requests.Inventory;
using InfraOps.Api.Contracts.Requests.PreventiveExecutions;
using InfraOps.Api.Contracts.Requests.PreventiveTemplates;
using InfraOps.Api.Contracts.Requests.PreventiveValidations;
using InfraOps.Api.Contracts.Responses.Auth;
using InfraOps.Api.Contracts.Responses.EntityTypes;
using InfraOps.Api.Contracts.Responses.Inventory;
using InfraOps.Api.Contracts.Responses.PreventiveExecutions;
using InfraOps.Api.Contracts.Responses.PreventiveTemplates;
using InfraOps.Api.Tests.Infrastructure;

namespace InfraOps.Api.Tests.PreventiveValidations;

public sealed class PreventiveValidationEndpointsTests : IClassFixture<InfraOpsApiFactory>
{
    private readonly InfraOpsApiFactory _factory;

    public PreventiveValidationEndpointsTests(InfraOpsApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_RequireAuthentication_When_ListingPreventiveValidationsWithoutToken()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/api/preventive-validations");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ListAndApproveSubmittedExecution_When_UserIsValidator()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var execution = await CreateSubmittedExecutionAsync(client);

        using var listResponse = await client.GetAsync("/api/preventive-validations");
        var listed = await listResponse.Content.ReadFromJsonAsync<IReadOnlyCollection<PreventiveExecutionSummaryResponse>>();

        using var detailResponse = await client.GetAsync($"/api/preventive-validations/{execution.Id}");
        var detail = await detailResponse.Content.ReadFromJsonAsync<PreventiveExecutionResponse>();

        using var approveResponse = await client.PostAsJsonAsync(
            $"/api/preventive-validations/{execution.Id}/approve",
            new ApprovePreventiveExecutionRequest(null));
        var approved = await approveResponse.Content.ReadFromJsonAsync<PreventiveExecutionResponse>();

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.Contains(listed!, x => x.Id == execution.Id);
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
        Assert.Equal("submitted", detail!.Status);
        Assert.Equal(HttpStatusCode.OK, approveResponse.StatusCode);
        Assert.Equal("approved", approved!.Status);
        Assert.Single(approved.ValidationHistory);
    }

    [Fact]
    public async Task Should_RejectSubmittedExecutionWithReason_When_UserIsValidator()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var execution = await CreateSubmittedExecutionAsync(client);

        using var response = await client.PostAsJsonAsync(
            $"/api/preventive-validations/{execution.Id}/reject",
            new RejectPreventiveExecutionRequest("Measurements require correction."));
        var rejected = await response.Content.ReadFromJsonAsync<PreventiveExecutionResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("rejected", rejected!.Status);
        Assert.Equal("Measurements require correction.", rejected.ValidationHistory.Single().Comment);
    }

    [Fact]
    public async Task Should_RequestReworkWithReason_When_UserIsValidator()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var execution = await CreateSubmittedExecutionAsync(client);

        using var response = await client.PostAsJsonAsync(
            $"/api/preventive-validations/{execution.Id}/request-rework",
            new RequestPreventiveReworkRequest("Add missing alarm explanation."));
        var rework = await response.Content.ReadFromJsonAsync<PreventiveExecutionResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("reworkRequested", rework!.Status);
        Assert.Equal("Add missing alarm explanation.", rework.ValidationHistory.Single().Comment);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_ApprovingDraftExecution()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var draft = await CreateDraftExecutionAsync(client);

        using var response = await client.PostAsJsonAsync(
            $"/api/preventive-validations/{draft.Id}/approve",
            new ApprovePreventiveExecutionRequest(null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_UserDoesNotHaveValidationPermission()
    {
        using var adminClient = await CreateAuthenticatedClientAsync();
        var execution = await CreateSubmittedExecutionAsync(adminClient);

        using var technicianClient = _factory.CreateClient();
        var technicianTokens = await LoginAsync(
            technicianClient,
            "technician@infraops.local",
            "DemoOnly-Tech-Local");
        technicianClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            technicianTokens.AccessToken);

        using var response = await technicianClient.PostAsJsonAsync(
            $"/api/preventive-validations/{execution.Id}/approve",
            new ApprovePreventiveExecutionRequest(null));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var tokens = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        return client;
    }

    private static Task<TokenResponse> LoginAsync(HttpClient client)
    {
        return LoginAsync(client, "admin@infraops.local", "DemoOnly-Admin-Local");
    }

    private static async Task<TokenResponse> LoginAsync(
        HttpClient client,
        string email,
        string password)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, password));

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<TokenResponse>())!;
    }

    private static async Task<PreventiveExecutionResponse> CreateSubmittedExecutionAsync(HttpClient client)
    {
        var execution = await CreateDraftExecutionAsync(client);

        using var submitResponse = await client.PostAsJsonAsync(
            $"/api/preventive-executions/{execution.Id}/submit",
            new SubmitPreventiveExecutionRequest(
                [
                    new PreventiveExecutionAnswerRequest("equipmentClean", "yes", null),
                    new PreventiveExecutionAnswerRequest("activeAlarm", "yes", null),
                    new PreventiveExecutionAnswerRequest("inputVoltage", "220", null)
                ]));

        submitResponse.EnsureSuccessStatusCode();

        return (await submitResponse.Content.ReadFromJsonAsync<PreventiveExecutionResponse>())!;
    }

    private static async Task<PreventiveExecutionResponse> CreateDraftExecutionAsync(HttpClient client)
    {
        var entityType = await CreateEntityTypeAsync(client, $"validation-{Guid.NewGuid():N}");
        var metadata = await GetInventoryFormMetadataAsync(client);
        var (regionId, siteId) = ResolveLocationContext(metadata);
        var inventoryItem = await CreateInventoryItemAsync(client, entityType.Id, regionId, siteId);
        await CreatePreventiveTemplateAsync(client, entityType.Id, $"validation-template-{Guid.NewGuid():N}");

        using var startResponse = await client.PostAsJsonAsync(
            "/api/preventive-executions/start",
            new StartPreventiveExecutionRequest(inventoryItem.Id));

        startResponse.EnsureSuccessStatusCode();

        return (await startResponse.Content.ReadFromJsonAsync<PreventiveExecutionResponse>())!;
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
