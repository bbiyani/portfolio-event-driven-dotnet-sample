using System.Globalization;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Pensions360.Application.Abstractions;
using Pensions360.Domain.Entities;
using Pensions360.Domain.ValueObjects;

namespace Pensions360.Infrastructure.Persistence;

public sealed class CosmosPensionPotStateStore : IPensionPotStateStore
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly CosmosClient _cosmosClient;
    private readonly Container _potsContainer;
    private readonly Container _citizensContainer;

    public CosmosPensionPotStateStore(
        BlobServiceClient blobServiceClient,
        CosmosClient cosmosClient,
        IConfiguration configuration)
    {
        _blobServiceClient = blobServiceClient;
        _cosmosClient = cosmosClient;

        var dbName = configuration["Cosmos:Database"] ?? "PensionsDb";
        var potsContainerName = configuration["Cosmos:PensionPotsContainer"] ?? "PensionPots";
        var citizensContainerName = configuration["Cosmos:CitizensContainer"] ?? "Citizens";

        _potsContainer = _cosmosClient.GetContainer(dbName, potsContainerName);
        _citizensContainer = _cosmosClient.GetContainer(dbName, citizensContainerName);
    }

    public async Task<IReadOnlyList<ProviderFileRow>> GetProviderFileRowsAsync(
        string blobContainer,
        string blobName,
        CancellationToken cancellationToken)
    {
        var container = _blobServiceClient.GetBlobContainerClient(blobContainer);
        var blob = container.GetBlobClient(blobName);

        using var stream = await blob.OpenReadAsync(cancellationToken: cancellationToken);
        using var reader = new StreamReader(stream);

        var rows = new List<ProviderFileRow>();
        string? header = await reader.ReadLineAsync();
        if (header is null)
            return rows;

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split(',', StringSplitOptions.TrimEntries);

            var nino = parts[0];
            var providerPotId = parts[2];
            var valuationDate = DateTime.Parse(parts[3], CultureInfo.InvariantCulture);
            var balance = decimal.Parse(parts[4], CultureInfo.InvariantCulture);

            rows.Add(new ProviderFileRow(nino, providerPotId, valuationDate, balance));
        }

        return rows;
    }

    public async Task<bool> CitizenExistsAsync(Nino nino, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _citizensContainer.ReadItemAsync<dynamic>(
                id: nino.Value,
                partitionKey: new PartitionKey(nino.Value),
                cancellationToken: cancellationToken);

            return response.StatusCode == System.Net.HttpStatusCode.OK;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task UpsertCitizenAsync(Citizen citizen, CancellationToken cancellationToken)
    {
        var doc = new
        {
            id = citizen.Nino.Value,
            nino = citizen.Nino.Value,
            firstName = citizen.FirstName,
            lastName = citizen.LastName,
            dateOfBirth = citizen.DateOfBirth
        };

        await _citizensContainer.UpsertItemAsync(
            doc,
            new PartitionKey(citizen.Nino.Value),
            cancellationToken: cancellationToken);
    }

    public async Task<PensionPot?> GetPensionPotAsync(
        Nino nino,
        string providerCode,
        string providerPotId,
        CancellationToken cancellationToken)
    {
        var query = new QueryDefinition(@"
SELECT * FROM c
WHERE c.nino = @nino AND c.providerCode = @providerCode AND c.providerPotId = @providerPotId")
            .WithParameter("@nino", nino.Value)
            .WithParameter("@providerCode", providerCode)
            .WithParameter("@providerPotId", providerPotId);

        using var iterator = _potsContainer.GetItemQueryIterator<dynamic>(query);
        if (!iterator.HasMoreResults)
            return null;

        var response = await iterator.ReadNextAsync(cancellationToken);
        var doc = response.FirstOrDefault();
        if (doc is null)
            return null;

        Guid id = Guid.Parse((string)doc.id);
        decimal balance = (decimal)doc.currentBalance;
        DateTime valuationDate = (DateTime)doc.valuationDate;

        return new PensionPot(
            id,
            providerPotId,
            providerCode,
            nino,
            new Money(balance, (string)doc.currency),
            valuationDate);
    }

    public async Task UpsertPensionPotAsync(PensionPot pot, CancellationToken cancellationToken)
    {
        var doc = new
        {
            id = pot.Id.ToString(),
            nino = pot.Nino.Value,
            providerCode = pot.ProviderCode,
            providerPotId = pot.ProviderPotId,
            providerName = (string?)null,
            currentBalance = pot.CurrentBalance.Amount,
            currency = pot.CurrentBalance.Currency,
            valuationDate = pot.ValuationDate
        };

        await _potsContainer.UpsertItemAsync(
            doc,
            new PartitionKey(pot.Nino.Value),
            cancellationToken: cancellationToken);
    }
}
