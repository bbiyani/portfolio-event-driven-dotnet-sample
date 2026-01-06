using MediatR;
using Pensions360.Application.Pensions.Dtos;

namespace Pensions360.Application.Pensions.Queries;

public sealed record FindPensionsQuery(string Nino) : IRequest<IReadOnlyList<PensionPotDto>>;
