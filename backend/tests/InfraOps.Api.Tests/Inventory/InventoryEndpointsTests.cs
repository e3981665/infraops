using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using InfraOps.Api.Contracts.Requests.Auth;
using InfraOps.Api.Contracts.Requests.EntityTypes;
using InfraOps.Api.Contracts.Requests.Inventory;
using InfraOps.Api.Contracts.Responses.Auth;
using InfraOps.Api.Contracts.Responses.EntityTypes;
using InfraOps.Api.Contracts.Responses.Inventory;
using InfraOps.Api.Tests.Infrastructure;

namespace InfraOps.Api.Tests.Inventory;

public sealed class InventoryEndpointsTests : IClassFixture<InfraOpsApiFactory>
{
    private readonly InfraOpsApiFactory _factory;

    public InventoryEndpointsTests(InfraOpsApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_RequireAuthentication_When_ListingInventoryWithoutToken()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/api/inventory");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnInventoryFormDefinition_When_InventoryReaderIsAuthorized()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var entityType = await CreateEntityTypeAsync(client, $"inventory-form-{Guid.NewGuid():N}");

        using var response = await client.GetAsync($"/api/inventory/form-definition/{entityType.Id}");
        var rawContent = await response.Content.ReadAsStringAsync();
        var payload = await response.Content.ReadFromJsonAsync<InventoryFormDefinitionResponse>();

        Assert.True(response.IsSuccessStatusCode, rawContent);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(entityType.Id, payload.EntityTypeId);
        Assert.Equal("UPS", payload.EntityTypeName);
        Assert.Equal(2, payload.FieldDefinitions.Count);
        Assert.Contains(payload.FieldDefinitions, x => x.FieldKey == "serialNumber" && x.IsRequired);
        Assert.Contains(payload.FieldDefinitions, x => x.FieldKey == "phaseType" && x.Options.Count == 2);
    }

    [Fact]
    public async Task Should_CreateInventoryItem_When_AuthorizedUserIsValid()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var entityType = await CreateEntityTypeAsync(client, $"inventory-create-{Guid.NewGuid():N}");
        var metadata = await GetFormMetadataAsync(client);
        var (regionId, siteId, regionName, siteName) = ResolveLocationContext(metadata);

        using var response = await client.PostAsJsonAsync(
            "/api/inventory",
            new CreateInventoryItemRequest(
                entityType.Id,
                regionId,
                siteId,
                "UPS Room A",
                "operational",
                new DateOnly(2024, 4, 1),
                [
                    new InventoryAttributeValueRequest("serialNumber", "UPS-0001"),
                    new InventoryAttributeValueRequest("phaseType", "single-phase")
                ]));

        var rawContent = await response.Content.ReadAsStringAsync();
        var payload = await response.Content.ReadFromJsonAsync<InventoryItemResponse>();

        Assert.True(response.IsSuccessStatusCode, rawContent);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(entityType.Id, payload.EntityTypeId);
        Assert.Equal("UPS Room A", payload.DisplayName);
        Assert.Equal("operational", payload.Status);
        Assert.Equal(regionName, payload.RegionName);
        Assert.Equal(siteName, payload.SiteName);
        Assert.Equal(2, payload.AttributeValues.Count);
    }

    [Fact]
    public async Task Should_GetAndListInventoryItems_When_AuthorizedUserIsValid()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var entityType = await CreateEntityTypeAsync(client, $"inventory-list-{Guid.NewGuid():N}");
        var metadata = await GetFormMetadataAsync(client);
        var (regionId, siteId, _, _) = ResolveLocationContext(metadata);
        var createdItem = await CreateInventoryItemAsync(
            client,
            entityType.Id,
            regionId,
            siteId,
            $"UPS Room {Guid.NewGuid():N}".Substring(0, 17));

        using var getResponse = await client.GetAsync($"/api/inventory/{createdItem.Id}");
        using var listResponse = await client.GetAsync(
            $"/api/inventory?entityTypeId={entityType.Id:D}&search={Uri.EscapeDataString(createdItem.DisplayName)}");
        var getPayload = await getResponse.Content.ReadFromJsonAsync<InventoryItemResponse>();
        var listPayload = await listResponse.Content.ReadFromJsonAsync<IReadOnlyCollection<InventoryItemSummaryResponse>>();

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.NotNull(getPayload);
        Assert.NotNull(listPayload);
        Assert.Equal(createdItem.Id, getPayload.Id);
        Assert.Contains(listPayload, x => x.Id == createdItem.Id && x.DisplayName == createdItem.DisplayName);
    }

    [Fact]
    public async Task Should_UpdateAndDeactivateInventoryItem_When_AuthorizedUserIsValid()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var entityType = await CreateEntityTypeAsync(client, $"inventory-update-{Guid.NewGuid():N}");
        var metadata = await GetFormMetadataAsync(client);
        var (regionId, siteId, _, _) = ResolveLocationContext(metadata);
        var createdItem = await CreateInventoryItemAsync(client, entityType.Id, regionId, siteId, "UPS Room Edit");

        using var updateResponse = await client.PutAsJsonAsync(
            $"/api/inventory/{createdItem.Id}",
            new UpdateInventoryItemRequest(
                regionId,
                siteId,
                "UPS Room Updated",
                "maintenance",
                new DateOnly(2025, 5, 1),
                [
                    new InventoryAttributeValueRequest("serialNumber", "UPS-0002"),
                    new InventoryAttributeValueRequest("phaseType", "three-phase")
                ]));

        using var deactivateResponse = await client.PostAsync($"/api/inventory/{createdItem.Id}/deactivate", null);
        using var getResponse = await client.GetAsync($"/api/inventory/{createdItem.Id}");
        var updatePayload = await updateResponse.Content.ReadFromJsonAsync<InventoryItemResponse>();
        var getPayload = await getResponse.Content.ReadFromJsonAsync<InventoryItemResponse>();

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, deactivateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(updatePayload);
        Assert.NotNull(getPayload);
        Assert.Equal("UPS Room Updated", updatePayload.DisplayName);
        Assert.Equal("maintenance", updatePayload.Status);
        Assert.False(getPayload.IsActive);
        Assert.Contains(getPayload.AttributeValues, x => x.FieldKey == "phaseType" && x.Value == "three-phase");
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
            new LoginRequest("admin@infraops.local", "DemoOnly-Admin-Local"));

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
                [
                    new EntityFieldDefinitionRequest(
                        null,
                        "serialNumber",
                        "Serial Number",
                        "text",
                        1,
                        true,
                        true,
                        "Printed on the nameplate",
                        "Used by inventory and preventive flows.",
                        []),
                    new EntityFieldDefinitionRequest(
                        null,
                        "phaseType",
                        "Phase Type",
                        "select",
                        2,
                        false,
                        true,
                        null,
                        null,
                        [
                            new EntityFieldOptionRequest(null, "single-phase", "Single-phase", 1),
                            new EntityFieldOptionRequest(null, "three-phase", "Three-phase", 2)
                        ])
                ]));

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<EntityTypeResponse>())!;
    }

    private static async Task<InventoryFormMetadataResponse> GetFormMetadataAsync(HttpClient client)
    {
        using var response = await client.GetAsync("/api/inventory/form-metadata");

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<InventoryFormMetadataResponse>())!;
    }

    private static (Guid RegionId, Guid SiteId, string RegionName, string SiteName) ResolveLocationContext(
        InventoryFormMetadataResponse metadata)
    {
        var region = metadata.Regions.First();
        var site = metadata.Sites.First(x => x.RegionId == region.Id);

        return (region.Id, site.Id, region.Name, site.Name);
    }

    private static async Task<InventoryItemResponse> CreateInventoryItemAsync(
        HttpClient client,
        Guid entityTypeId,
        Guid regionId,
        Guid siteId,
        string displayName)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/inventory",
            new CreateInventoryItemRequest(
                entityTypeId,
                regionId,
                siteId,
                displayName,
                "operational",
                new DateOnly(2024, 4, 1),
                [
                    new InventoryAttributeValueRequest("serialNumber", $"serial-{Guid.NewGuid():N}".Substring(0, 18)),
                    new InventoryAttributeValueRequest("phaseType", "single-phase")
                ]));

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<InventoryItemResponse>())!;
    }
}
