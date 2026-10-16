using System.Net;
using System.Net.Http.Json;
using Tapio.Application.Common;

namespace Tapio.API.Tests;

public class AuthTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email = "x@test.com", password = "wrong" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_CreatesBusinessUser()
    {
        var email = $"owner{Guid.NewGuid():N}@test.com";
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Test",
            lastName = "Owner",
            email,
            phone = "5551112233",
            password = "Test1234!",
            businessName = "Test Cafe",
            category = "Cafe",
            businessPhone = "5551112233",
            address = "Test Address",
            city = "Istanbul",
            district = "Kadikoy"
        });

        var raw = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, raw);
        var body = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<RegisterTestDto>>(raw, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.True(body?.Success);
        Assert.Equal("Test Cafe", body?.Data?.Business.Name);
    }

    private record RegisterTestDto(string AccessToken, UserDto User, BusinessDto Business);
    private record UserDto(Guid Id, string Email);
    private record BusinessDto(string Name, string Slug);
}
