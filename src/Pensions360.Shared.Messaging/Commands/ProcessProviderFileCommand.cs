namespace Pensions360.Shared.Messaging.Commands;

public sealed record ProcessProviderFileCommand(
    string ProviderCode,
    string BlobContainerName,
    string BlobName,
    DateTime UploadedAtUtc);
