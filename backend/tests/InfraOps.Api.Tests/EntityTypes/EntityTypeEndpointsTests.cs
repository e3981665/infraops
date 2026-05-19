using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using InfraOps.Api.Contracts.Requests.Auth;
using InfraOps.Api.Contracts.Requests.EntityTypes;
using InfraOps.Api.Contracts.Responses.Auth;
using InfraOps.Api.Contracts.Responses.EntityTypes;
using InfraOps.Api.Tests.Infrastructure;

namespace InfraOps.Api.Tests.EntityTypes;

public sealed class EntityTypeEndpointsTests : IClassFixture<InfraOpsApiFactory>
{
    private readonly InfraOpsApiFactory _factory;

    public EntityTypeEndpointsTests(InfraOpsApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_RequireAuthentication_When_ListingEntityTypesWithoutToken()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/api/entity-types");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_CreateEntityType_When_AdminUserIsAuthorized()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var request = CreateRequest("ups-create");

        using var response = await client.PostAsJsonAsync("/api/entity-types", request);
        var rawContent = await response.Content.ReadAsStringAsync();
        var payload = await response.Content.ReadFromJsonAsync<EntityTypeResponse>();

        Assert.True(response.IsSuccessStatusCode, rawContent);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(request.Name, payload.Name);
        Assert.Equal(request.Code, payload.Code);
        Assert.Single(payload.FieldDefinitions);
    }

    [Fact]
    public async Task Should_ReturnCreatedEntityType_When_GetAndListEndpointsAreRequested()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var request = CreateRequest("ups-list");
        using var createResponse = await client.PostAsJsonAsync("/api/entity-types", request);
        var created = await createResponse.Content.ReadFromJsonAsync<EntityTypeResponse>();

        using var getResponse = await client.GetAsync($"/api/entity-types/{created!.Id}");
        using var listResponse = await client.GetAsync("/api/entity-types");
        var getPayload = await getResponse.Content.ReadFromJsonAsync<EntityTypeResponse>();
        var listPayload = await listResponse.Content.ReadFromJsonAsync<IReadOnlyCollection<EntityTypeSummaryResponse>>();

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.NotNull(getPayload);
        Assert.NotNull(listPayload);
        Assert.Contains(listPayload, x => x.Id == created.Id && x.FieldCount == 1);
        Assert.Equal("serialNumber", getPayload.FieldDefinitions.Single().FieldKey);
    }

    [Fact]
    public async Task Should_UpdateAndDeactivateEntityType_When_AdminUserIsAuthorized()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var request = CreateRequest("ups-update");
        using var createResponse = await client.PostAsJsonAsync("/api/entity-types", request);
        var createRawContent = await createResponse.Content.ReadAsStringAsync();
        var created = await createResponse.Content.ReadFromJsonAsync<EntityTypeResponse>();
        Assert.True(createResponse.IsSuccessStatusCode, createRawContent);
        var existingField = created!.FieldDefinitions.Single();

        using var updateResponse = await client.PutAsJsonAsync(
            $"/api/entity-types/{created.Id}",
            new UpdateEntityTypeRequest(
                "UPS System",
                "ups-system",
                "Updated description",
                [
                    new EntityFieldDefinitionRequest(
                        existingField.Id,
                        existingField.FieldKey,
                        "Asset Serial Number",
                        existingField.FieldType,
                        1,
                        true,
                        true,
                        "Printed on the nameplate",
                        null,
                        [])
                ]));

        using var deactivateResponse = await client.PostAsync($"/api/entity-types/{created.Id}/deactivate", null);
        using var getResponse = await client.GetAsync($"/api/entity-types/{created.Id}");
        var payload = await getResponse.Content.ReadFromJsonAsync<EntityTypeResponse>();

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, deactivateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("UPS System", payload.Name);
        Assert.False(payload.IsActive);
        Assert.Equal("Asset Serial Number", payload.FieldDefinitions.Single().DisplayLabel);
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
        using var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "admin@infraops.local",
            "InfraOps.Admin!123"));

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<TokenResponse>())!;
    }

    private static CreateEntityTypeRequest CreateRequest(string code)
    {
        return new CreateEntityTypeRequest(
            code.Equals("ups-create", StringComparison.Ordinal) ? "UPS" : $"UPS {code}",
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
                    "Used by inventory and maintenance workflows.",
                    [])
            ]);
    }
}
