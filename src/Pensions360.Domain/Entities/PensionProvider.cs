namespace Pensions360.Domain.Entities;

public sealed class PensionProvider
{
    public Guid Id { get; }
    public string Code { get; }
    public string Name { get; }

    public PensionProvider(Guid id, string code, string name)
    {
        Id = id;
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}
