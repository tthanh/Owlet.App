namespace Owlet.Domain;

/// <summary>
/// Represents a document in the document management system
/// </summary>
public class Document
{
    public DocumentID Id { get; private set; }
    public string Title { get; private set; }
    public string Content { get; private set; }
    public UserID CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime LastModifiedAt { get; private set; }
    public string? Description { get; private set; }
    public DocumentStatus Status { get; private set; }

    private Document() { } // EF Core constructor

    public Document(string title, string content, UserID createdBy, string? description = null)
    {
        Id = DocumentID.New();
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Content = content ?? throw new ArgumentNullException(nameof(content));
        CreatedBy = createdBy;
        Description = description;
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        Status = DocumentStatus.Draft;
    }

    public void UpdateContent(string title, string content, string? description = null)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Content = content ?? throw new ArgumentNullException(nameof(content));
        Description = description;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Publish()
    {
        Status = DocumentStatus.Published;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        Status = DocumentStatus.Archived;
        LastModifiedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Represents the status of a document
/// </summary>
public enum DocumentStatus
{
    Draft,
    Published,
    Archived
}
