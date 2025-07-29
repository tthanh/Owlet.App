using System.Collections.ObjectModel;

namespace Owlet.Domain;

/// <summary>
/// Represents a node in the document tree structure. This is an aggregate root for tree management.
/// Documents are referenced by ID to maintain proper aggregate boundaries.
/// </summary>
public class DocumentNode
{
    private readonly List<DocumentNode> _children = new();

    public DocumentNodeID Id { get; private set; }
    public string Name { get; private set; } = null!;
    public DocumentNodeType Type { get; private set; }
    public DocumentNodeID? ParentId { get; private set; }
    public DocumentNode? Parent { get; private set; }
    public DocumentID? DocumentId { get; private set; } // Reference to Document aggregate
    public UserID CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime LastModifiedAt { get; private set; }
    public int Order { get; private set; }
    
    public IReadOnlyCollection<DocumentNode> Children => _children.AsReadOnly();
    public bool IsFolder => Type == DocumentNodeType.Folder;
    public bool IsDocument => Type == DocumentNodeType.Document;
    public bool IsRoot => Parent == null;
    public int Level => IsRoot ? 0 : Parent!.Level + 1;

    private DocumentNode() { } // EF Core constructor

    /// <summary>
    /// Creates a new folder node
    /// </summary>
    public DocumentNode(string name, UserID createdBy, DocumentNode? parent = null, int order = 0)
    {
        Id = DocumentNodeID.New();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Type = DocumentNodeType.Folder;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        Order = order;
        
        SetParent(parent);
    }

    /// <summary>
    /// Creates a new document node that references an existing document
    /// </summary>
    public DocumentNode(string name, DocumentID documentId, UserID createdBy, DocumentNode? parent = null, int order = 0)
    {
        Id = DocumentNodeID.New();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Type = DocumentNodeType.Document;
        DocumentId = documentId;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        Order = order;
        
        SetParent(parent);
    }

    public void Rename(string newName)
    {
        Name = newName ?? throw new ArgumentNullException(nameof(newName));
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Associates this node with a different document (only valid for document nodes)
    /// </summary>
    public void ChangeDocument(DocumentID newDocumentId)
    {
        if (Type != DocumentNodeType.Document)
        {
            throw new InvalidOperationException("Can only change document for document nodes.");
        }

        DocumentId = newDocumentId;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Move(DocumentNode? newParent, int newOrder = 0)
    {
        if (newParent != null && WouldCreateCycle(newParent))
        {
            throw new InvalidOperationException("Moving this node would create a cycle in the tree.");
        }

        // Remove from current parent
        Parent?._children.Remove(this);
        
        SetParent(newParent);
        Order = newOrder;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Reorder(int newOrder)
    {
        Order = newOrder;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void AddChild(DocumentNode child)
    {
        if (Type != DocumentNodeType.Folder)
        {
            throw new InvalidOperationException("Only folder nodes can have children.");
        }

        if (child.WouldCreateCycle(this))
        {
            throw new InvalidOperationException("Adding this child would create a cycle in the tree.");
        }

        child.SetParent(this);
    }

    public void RemoveChild(DocumentNode child)
    {
        if (_children.Remove(child))
        {
            child.SetParent(null);
        }
    }

    /// <summary>
    /// Deletes this node and all its descendants
    /// Note: This does NOT delete the referenced documents - that's handled separately
    /// </summary>
    public void Delete()
    {
        // Remove from parent first
        Parent?._children.Remove(this);
        
        // Recursively delete all children
        var childrenToDelete = _children.ToList();
        foreach (var child in childrenToDelete)
        {
            child.Delete();
        }
        
        _children.Clear();
        Parent = null;
        ParentId = null;
    }

    public IEnumerable<DocumentNode> GetAllDescendants()
    {
        foreach (var child in _children)
        {
            yield return child;
            foreach (var descendant in child.GetAllDescendants())
            {
                yield return descendant;
            }
        }
    }

    public IEnumerable<DocumentNode> GetAncestors()
    {
        var current = Parent;
        while (current != null)
        {
            yield return current;
            current = current.Parent;
        }
    }

    public string GetPath()
    {
        if (IsRoot)
            return Name;
        
        return string.Join("/", GetAncestors().Reverse().Select(n => n.Name).Concat(new[] { Name }));
    }

    /// <summary>
    /// Gets all document node IDs within this subtree
    /// </summary>
    public IEnumerable<DocumentID> GetAllDocumentIds()
    {
        if (IsDocument && DocumentId.HasValue)
            yield return DocumentId.Value;

        foreach (var descendant in GetAllDescendants().Where(d => d.IsDocument && d.DocumentId.HasValue))
        {
            yield return descendant.DocumentId!.Value;
        }
    }

    private void SetParent(DocumentNode? parent)
    {
        Parent = parent;
        ParentId = parent?.Id;
        
        if (parent != null && !parent._children.Contains(this))
        {
            parent._children.Add(this);
        }
    }

    private bool WouldCreateCycle(DocumentNode potentialAncestor)
    {
        return GetAllDescendants().Contains(potentialAncestor);
    }
}

/// <summary>
/// Represents the type of a document node
/// </summary>
public enum DocumentNodeType
{
    Folder,
    Document
}