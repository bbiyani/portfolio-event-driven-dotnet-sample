using Pensions360.Domain.Entities;
using Pensions360.Domain.ValueObjects;

namespace Pensions360.Application.Abstractions;

public sealed record ProviderFileRow(
    string Nino,
    string ProviderPotId,
    DateTime ValuationDate,
    decimal Balance);

public interface IPensionPotStateStore
{
    Task<IReadOnlyList<ProviderFileRow>> GetProviderFileRowsAsync(
        string blobContainer,
        string blobName,
        CancellationToken cancellationToken);

    Task<bool> CitizenExistsAsync(Nino nino, CancellationToken cancellationToken);
    Task UpsertCitizenAsync(Citizen citizen, CancellationToken cancellationToken);

    Task<PensionPot?> GetPensionPotAsync(
        Nino nino,
        string providerCode,
        string providerPotId,
        CancellationToken cancellationToken);

    Task UpsertPensionPotAsync(PensionPot pot, CancellationToken cancellationToken);
}
