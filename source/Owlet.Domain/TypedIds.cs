namespace Owlet.Domain;

/// <summary>
/// Strongly typed identifier for User entities
/// </summary>
public readonly record struct UserID(Guid Value)
{
    public static UserID New() => new(Guid.NewGuid());
    public static implicit operator Guid(UserID id) => id.Value;
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly typed identifier for Document entities
/// </summary>
public readonly record struct DocumentID(Guid Value)
{
    public static DocumentID New() => new(Guid.NewGuid());
    public static implicit operator Guid(DocumentID id) => id.Value;
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly typed identifier for DocumentNode entities
/// </summary>
public readonly record struct DocumentNodeID(Guid Value)
{
    public static DocumentNodeID New() => new(Guid.NewGuid());
    public static implicit operator Guid(DocumentNodeID id) => id.Value;
    public override string ToString() => Value.ToString();
}