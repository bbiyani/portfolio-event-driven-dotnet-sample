using Microsoft.Azure.Cosmos;
using Pensions360.Application.Abstractions;
using Pensions360.Application.Pensions.Dtos;
using Pensions360.Domain.ValueObjects;

namespace Pensions360.Infrastructure.Persistence;

public sealed class CosmosPensionPotReadRepository : IPensionPotReadRepository
{
    private readonly Container _container;

    public CosmosPensionPotReadRepository(Container container)
    {
        _container = container;
    }

    public async Task<IReadOnlyList<PensionPotDto>> GetPotsByNinoAsync(
        Nino nino,
        CancellationToken cancellationToken = default)
    {
        const string queryText = @"
SELECT c.id, c.providerName, c.currentBalance, c.valuationDate
FROM c
WHERE c.nino = @nino";

        var queryDef = new QueryDefinition(queryText)
            .WithParameter("@nino", nino.Value);

        var result = new List<PensionPotDto>();
        using var iter = _container.GetItemQueryIterator<dynamic>(queryDef);

        while (iter.HasMoreResults)
        {
            foreach (var item in await iter.ReadNextAsync(cancellationToken))
            {
                Guid id = Guid.Parse((string)item.id);
                string providerName = item.providerName;
                decimal balance = (decimal)item.currentBalance;
                DateTime valuationDate = (DateTime)item.valuationDate;

                result.Add(new PensionPotDto(
                    id,
                    providerName,
                    balance,
                    valuationDate));
            }
        }

        return result;
    }
}
