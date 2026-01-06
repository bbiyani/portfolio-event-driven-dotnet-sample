namespace Pensions360.Application.Abstractions;

public interface IPensionIngestionService
{
    Task ProcessProviderFileAsync(
        string providerCode,
        string blobContainer,
        string blobName,
        DateTime uploadedAtUtc,
        CancellationToken cancellationToken = default);
}
