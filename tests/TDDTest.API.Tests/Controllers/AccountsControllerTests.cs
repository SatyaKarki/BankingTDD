using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TDDTest.API.Models;
using TDDTest.API.Models.Requests;
using TDDTest.API.Tests.Infrastructure;
using TDDTest.Application.DTOs;

namespace TDDTest.API.Tests.Controllers;

public sealed class AccountsControllerTests : IClassFixture<BankingWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AccountsControllerTests(BankingWebApplicationFactory factory)
        => _client = factory.CreateClient();

    // ── POST /api/accounts ───────────────────────────────────────────────────

    [Fact]
    public async Task CreateAccount_ValidRequest_Returns201WithAccountDto()
    {
        var request = new CreateAccountRequest("Integration Test User", "it@test.com", 500m);

        var response = await _client.PostAsJsonAsync("/api/accounts", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AccountDto>>();
        body!.Success.Should().BeTrue();
        body.Data!.OwnerName.Should().Be("Integration Test User");
        body.Data.Balance.Should().Be(500m);
        body.Data.Status.Should().Be("Active");
        body.Data.AccountNumber.Should().StartWith("ACC-");
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAccount_InvalidEmail_Returns400WithValidationErrors()
    {
        var request = new CreateAccountRequest("Valid Name", "not-an-email", 0m);

        var response = await _client.PostAsJsonAsync("/api/accounts", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateAccount_EmptyOwnerName_Returns400()
    {
        var request = new CreateAccountRequest("", "valid@test.com", 0m);

        var response = await _client.PostAsJsonAsync("/api/accounts", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateAccount_NegativeDeposit_Returns400()
    {
        var request = new CreateAccountRequest("Alice", "alice@test.com", -100m);

        var response = await _client.PostAsJsonAsync("/api/accounts", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── GET /api/accounts/{id} ───────────────────────────────────────────────

    [Fact]
    public async Task GetAccount_ExistingAccount_Returns200WithData()
    {
        var accountId = await CreateAccountAsync("Get Test", "get@test.com", 100m);

        var response = await _client.GetAsync($"/api/accounts/{accountId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AccountDto>>();
        body!.Success.Should().BeTrue();
        body.Data!.Id.Should().Be(accountId);
        body.Data.OwnerName.Should().Be("Get Test");
    }

    [Fact]
    public async Task GetAccount_NonExistentId_Returns404()
    {
        var response = await _client.GetAsync($"/api/accounts/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── GET /api/accounts/{id}/balance ───────────────────────────────────────

    [Fact]
    public async Task GetBalance_ExistingAccount_Returns200WithBalance()
    {
        var accountId = await CreateAccountAsync("Balance User", "bal@test.com", 999m);

        var response = await _client.GetAsync($"/api/accounts/{accountId}/balance");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<BalanceDto>>();
        body!.Success.Should().BeTrue();
        body.Data!.Balance.Should().Be(999m);
        body.Data.Currency.Should().Be("USD");
        body.Data.AccountId.Should().Be(accountId);
    }

    [Fact]
    public async Task GetBalance_NonExistentAccount_Returns404()
    {
        var response = await _client.GetAsync($"/api/accounts/{Guid.NewGuid()}/balance");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── GET /api/accounts/{id}/transactions ──────────────────────────────────

    [Fact]
    public async Task GetAccountTransactions_AccountWithTransactions_ReturnsPagedList()
    {
        var accountId = await CreateAccountAsync("TxList User", "txlist@test.com", 100m);

        var response = await _client.GetAsync($"/api/accounts/{accountId}/transactions");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content
            .ReadFromJsonAsync<ApiResponse<Application.Transactions.Queries.GetAccountTransactions.PagedResult<TransactionDto>>>();

        body!.Success.Should().BeTrue();
        body.Data!.TotalCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetAccountTransactions_NonExistentAccount_Returns404()
    {
        var response = await _client.GetAsync($"/api/accounts/{Guid.NewGuid()}/transactions");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Helper ───────────────────────────────────────────────────────────────

    private async Task<Guid> CreateAccountAsync(string name, string email, decimal deposit)
    {
        var response = await _client.PostAsJsonAsync("/api/accounts",
            new CreateAccountRequest(name, email, deposit));
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AccountDto>>();
        return body!.Data!.Id;
    }
}
