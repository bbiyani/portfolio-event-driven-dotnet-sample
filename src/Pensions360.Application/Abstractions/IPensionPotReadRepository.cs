using Pensions360.Application.Pensions.Dtos;
using Pensions360.Domain.ValueObjects;

namespace Pensions360.Application.Abstractions;

public interface IPensionPotReadRepository
{
    Task<IReadOnlyList<PensionPotDto>> GetPotsByNinoAsync(Nino nino, CancellationToken cancellationToken = default);
}
