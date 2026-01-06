namespace Pensions360.Application.Pensions.Dtos;

public sealed record PensionPotDto(
    Guid PensionPotId,
    string ProviderName,
    decimal EstimatedValue,
    DateTime ValuationDate);
