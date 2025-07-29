namespace Owlet.Domain.Services;

/// <summary>
/// Domain service that coordinates between DocumentNode and Document aggregates
/// </summary>
public class DocumentTreeService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentNodeRepository _documentNodeRepository;

    public DocumentTreeService(IDocumentRepository documentRepository, IDocumentNodeRepository documentNodeRepository)
    {
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        _documentNodeRepository = documentNodeRepository ?? throw new ArgumentNullException(nameof(documentNodeRepository));
    }

    /// <summary>
    /// Creates a new document with its corresponding node in the tree
    /// </summary>
    public async Task<DocumentNode> CreateDocumentWithNodeAsync(
        string nodeName, 
        string documentTitle, 
        string documentContent, 
        UserID createdBy, 
        DocumentNode? parent = null, 
        string? description = null, 
        int order = 0)
    {
        // Create the document (its own aggregate)
        var document = new Document(documentTitle, documentContent, createdBy, description);
        await _documentRepository.AddAsync(document);

        // Create the node that references the document
        var documentNode = new DocumentNode(nodeName, document.Id, createdBy, parent, order);
        await _documentNodeRepository.AddAsync(documentNode);

        return documentNode;
    }

    /// <summary>
    /// Deletes a document tree node and optionally the referenced document
    /// </summary>
    public async Task DeleteDocumentNodeAsync(DocumentNode node, bool deleteReferencedDocument = false)
    {
        // Get all document IDs that would be affected
        var documentIds = node.GetAllDocumentIds().ToList();

        // Delete the node (and its children)
        node.Delete();
        await _documentNodeRepository.RemoveAsync(node);

        // Optionally delete the referenced documents
        if (deleteReferencedDocument)
        {
            foreach (var documentId in documentIds)
            {
                var document = await _documentRepository.GetByIdAsync(documentId);
                if (document != null)
                {
                    await _documentRepository.RemoveAsync(document);
                }
            }
        }
    }

    /// <summary>
    /// Gets document nodes with their associated document data
    /// </summary>
    public async Task<IEnumerable<DocumentNodeWithDocument>> GetDocumentNodesWithContentAsync(DocumentNode parentNode)
    {
        var result = new List<DocumentNodeWithDocument>();
        var documentNodes = parentNode.GetAllDescendants().Where(n => n.IsDocument).ToList();

        foreach (var node in documentNodes)
        {
            if (node.DocumentId.HasValue)
            {
                var document = await _documentRepository.GetByIdAsync(node.DocumentId.Value);
                if (document != null)
                {
                    result.Add(new DocumentNodeWithDocument(node, document));
                }
            }
        }

        return result;
    }
}

/// <summary>
/// Repository interface for Document aggregate
/// </summary>
public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(DocumentID id);
    Task AddAsync(Document document);
    Task RemoveAsync(Document document);
    Task SaveChangesAsync();
}

/// <summary>
/// Repository interface for DocumentNode aggregate
/// </summary>
public interface IDocumentNodeRepository
{
    Task<DocumentNode?> GetByIdAsync(DocumentNodeID id);
    Task<IEnumerable<DocumentNode>> GetRootNodesAsync(UserID userId);
    Task AddAsync(DocumentNode node);
    Task RemoveAsync(DocumentNode node);
    Task SaveChangesAsync();
}

/// <summary>
/// Read model that combines DocumentNode and Document data
/// </summary>
public record DocumentNodeWithDocument(DocumentNode Node, Document Document);