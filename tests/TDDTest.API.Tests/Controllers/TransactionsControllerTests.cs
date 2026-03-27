using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TDDTest.API.Models;
using TDDTest.API.Models.Requests;
using TDDTest.API.Tests.Infrastructure;
using TDDTest.Application.DTOs;
using TDDTest.Application.Transactions.Commands.TransferBalance;

namespace TDDTest.API.Tests.Controllers;

public sealed class TransactionsControllerTests : IClassFixture<BankingWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TransactionsControllerTests(BankingWebApplicationFactory factory)
        => _client = factory.CreateClient();

    // ── POST /api/transactions/transfer ─────────────────────────────────────

    [Fact]
    public async Task Transfer_ValidRequest_Returns200WithTransferResult()
    {
        var sourceId = await CreateAccountAsync("Source Account", "src@test.com", 1000m);
        var destId = await CreateAccountAsync("Dest Account", "dst@test.com", 0m);

        var request = new TransferBalanceRequest(sourceId, destId, 300m, "Test transfer");
        var response = await _client.PostAsJsonAsync("/api/transactions/transfer", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<TransferResultDto>>();
        body!.Success.Should().BeTrue();
        body.Data!.Amount.Should().Be(300m);
        body.Data.SourceBalanceAfter.Should().Be(700m);
        body.Data.DestinationBalanceAfter.Should().Be(300m);
        body.Data.SourceTransactionId.Should().NotBe(Guid.Empty);
        body.Data.DestinationTransactionId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Transfer_InsufficientFunds_Returns422()
    {
        var sourceId = await CreateAccountAsync("Poor", "poor@test.com", 50m);
        var destId = await CreateAccountAsync("Rich", "rich@test.com", 0m);

        var request = new TransferBalanceRequest(sourceId, destId, 500m, "Overspend");
        var response = await _client.PostAsJsonAsync("/api/transactions/transfer", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Transfer_SameSourceAndDestination_Returns400()
    {
        var id = await CreateAccountAsync("Self", "self@test.com", 100m);

        var request = new TransferBalanceRequest(id, id, 50m, "Self transfer");
        var response = await _client.PostAsJsonAsync("/api/transactions/transfer", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Transfer_SourceAccountNotFound_Returns404()
    {
        var destId = await CreateAccountAsync("Dest", "dest@test.com", 0m);

        var request = new TransferBalanceRequest(Guid.NewGuid(), destId, 100m, "Ghost transfer");
        var response = await _client.PostAsJsonAsync("/api/transactions/transfer", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Transfer_ZeroAmount_Returns400()
    {
        var srcId = await CreateAccountAsync("Src", "s2@test.com", 100m);
        var dstId = await CreateAccountAsync("Dst", "d2@test.com", 0m);

        var request = new TransferBalanceRequest(srcId, dstId, 0m, "Zero amount");
        var response = await _client.PostAsJsonAsync("/api/transactions/transfer", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── GET /api/transactions/{id} ───────────────────────────────────────────

    [Fact]
    public async Task GetTransaction_ExistingTransaction_Returns200WithData()
    {
        var accountId = await CreateAccountAsync("Tx Owner", "txowner@test.com", 200m);
        var txId = await GetFirstTransactionIdAsync(accountId);

        var response = await _client.GetAsync($"/api/transactions/{txId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<TransactionDto>>();
        body!.Success.Should().BeTrue();
        body.Data!.Id.Should().Be(txId);
        body.Data.AccountId.Should().Be(accountId);
        body.Data.Amount.Should().Be(200m);
        body.Data.Type.Should().Be("Deposit");
    }

    [Fact]
    public async Task GetTransaction_NonExistentId_Returns404()
    {
        var response = await _client.GetAsync($"/api/transactions/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Transfer_VerifyTransactionDetailsAfterTransfer()
    {
        var srcId = await CreateAccountAsync("Verify Src", "vsrc@test.com", 500m);
        var dstId = await CreateAccountAsync("Verify Dst", "vdst@test.com", 0m);

        var transferRequest = new TransferBalanceRequest(srcId, dstId, 200m, "Verify transfer");
        var transferResponse = await _client.PostAsJsonAsync("/api/transactions/transfer", transferRequest);
        transferResponse.EnsureSuccessStatusCode();

        var transferBody = await transferResponse.Content.ReadFromJsonAsync<ApiResponse<TransferResultDto>>();
        var srcTxId = transferBody!.Data!.SourceTransactionId;

        var txResponse = await _client.GetAsync($"/api/transactions/{srcTxId}");
        txResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var txBody = await txResponse.Content.ReadFromJsonAsync<ApiResponse<TransactionDto>>();
        txBody!.Data!.Type.Should().Be("TransferDebit");
        txBody.Data.Amount.Should().Be(200m);
        txBody.Data.AccountId.Should().Be(srcId);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<Guid> CreateAccountAsync(string name, string email, decimal deposit)
    {
        var response = await _client.PostAsJsonAsync("/api/accounts",
            new CreateAccountRequest(name, email, deposit));
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AccountDto>>();
        return body!.Data!.Id;
    }

    private async Task<Guid> GetFirstTransactionIdAsync(Guid accountId)
    {
        var response = await _client.GetAsync($"/api/accounts/{accountId}/transactions");
        response.EnsureSuccessStatusCode();
        var body = await response.Content
            .ReadFromJsonAsync<ApiResponse<Application.Transactions.Queries.GetAccountTransactions.PagedResult<TransactionDto>>>();
        return body!.Data!.Items.First().Id;
    }
}
