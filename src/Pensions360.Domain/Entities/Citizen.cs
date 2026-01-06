using Pensions360.Domain.ValueObjects;

namespace Pensions360.Domain.Entities;

public sealed class Citizen
{
    public Nino Nino { get; }
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public DateTime? DateOfBirth { get; private set; }

    public Citizen(Nino nino)
    {
        Nino = nino;
    }

    public void UpdateProfile(string? firstName, string? lastName, DateTime? dateOfBirth)
    {
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
    }
}
