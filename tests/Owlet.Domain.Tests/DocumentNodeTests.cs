using Owlet.Domain;

namespace Owlet.Domain.Tests;

public class DocumentNodeTests
{
    [Fact]
    public void DocumentNode_CreateFolder_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var userId = UserID.New();
        var folderName = "My Folder";
        var beforeCreation = DateTime.UtcNow;

        // Act
        var folderNode = new DocumentNode(folderName, userId);

        // Assert
        Assert.NotEqual(DocumentNodeID.New(), folderNode.Id);
        Assert.Equal(folderName, folderNode.Name);
        Assert.Equal(DocumentNodeType.Folder, folderNode.Type);
        Assert.Equal(userId, folderNode.CreatedBy);
        Assert.True(folderNode.IsFolder);
        Assert.False(folderNode.IsDocument);
        Assert.True(folderNode.IsRoot);
        Assert.Equal(0, folderNode.Level);
        Assert.Empty(folderNode.Children);
        Assert.Null(folderNode.Parent);
        Assert.Null(folderNode.DocumentId);
        Assert.True(folderNode.CreatedAt >= beforeCreation);
    }

    [Fact]
    public void DocumentNode_CreateDocumentNode_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var userId = UserID.New();
        var documentId = DocumentID.New();
        var nodeName = "Document Node";

        // Act
        var documentNode = new DocumentNode(nodeName, documentId, userId);

        // Assert
        Assert.Equal(nodeName, documentNode.Name);
        Assert.Equal(DocumentNodeType.Document, documentNode.Type);
        Assert.Equal(documentId, documentNode.DocumentId);
        Assert.True(documentNode.IsDocument);
        Assert.False(documentNode.IsFolder);
    }

    [Fact]
    public void DocumentNode_CreateChildNode_ShouldEstablishParentChildRelationship()
    {
        // Arrange
        var userId = UserID.New();
        var parentFolder = new DocumentNode("Parent", userId);
        var childFolder = new DocumentNode("Child", userId, parentFolder);

        // Act & Assert
        Assert.Equal(parentFolder, childFolder.Parent);
        Assert.Equal(parentFolder.Id, childFolder.ParentId);
        Assert.Contains(childFolder, parentFolder.Children);
        Assert.False(childFolder.IsRoot);
        Assert.Equal(1, childFolder.Level);
    }

    [Fact]
    public void DocumentNode_AddChild_ShouldAddToFolderSuccessfully()
    {
        // Arrange
        var userId = UserID.New();
        var parentFolder = new DocumentNode("Parent", userId);
        var childFolder = new DocumentNode("Child", userId);

        // Act
        parentFolder.AddChild(childFolder);

        // Assert
        Assert.Contains(childFolder, parentFolder.Children);
        Assert.Equal(parentFolder, childFolder.Parent);
    }

    [Fact]
    public void DocumentNode_AddChildToDocument_ShouldThrowException()
    {
        // Arrange
        var userId = UserID.New();
        var documentId = DocumentID.New();
        var documentNode = new DocumentNode("Doc Node", documentId, userId);
        var childFolder = new DocumentNode("Child", userId);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => documentNode.AddChild(childFolder));
    }

    [Fact]
    public void DocumentNode_Move_ShouldUpdateParentAndOrder()
    {
        // Arrange
        var userId = UserID.New();
        var oldParent = new DocumentNode("Old Parent", userId);
        var newParent = new DocumentNode("New Parent", userId);
        var child = new DocumentNode("Child", userId, oldParent);

        // Act
        child.Move(newParent, 5);

        // Assert
        Assert.Equal(newParent, child.Parent);
        Assert.Equal(5, child.Order);
        Assert.DoesNotContain(child, oldParent.Children);
        Assert.Contains(child, newParent.Children);
    }

    [Fact]
    public void DocumentNode_MoveToDescendant_ShouldThrowException()
    {
        // Arrange
        var userId = UserID.New();
        var grandparent = new DocumentNode("Grandparent", userId);
        var parent = new DocumentNode("Parent", userId, grandparent);
        var child = new DocumentNode("Child", userId, parent);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => grandparent.Move(child));
    }

    [Fact]
    public void DocumentNode_Rename_ShouldUpdateNameAndTimestamp()
    {
        // Arrange
        var userId = UserID.New();
        var node = new DocumentNode("Original Name", userId);
        var originalTimestamp = node.LastModifiedAt;
        Thread.Sleep(1); // Ensure timestamp difference

        // Act
        node.Rename("New Name");

        // Assert
        Assert.Equal("New Name", node.Name);
        Assert.True(node.LastModifiedAt > originalTimestamp);
    }

    [Fact]
    public void DocumentNode_ChangeDocument_ShouldUpdateDocumentIdAndTimestamp()
    {
        // Arrange
        var userId = UserID.New();
        var originalDocumentId = DocumentID.New();
        var newDocumentId = DocumentID.New();
        var documentNode = new DocumentNode("Doc Node", originalDocumentId, userId);
        var originalTimestamp = documentNode.LastModifiedAt;
        Thread.Sleep(1);

        // Act
        documentNode.ChangeDocument(newDocumentId);

        // Assert
        Assert.Equal(newDocumentId, documentNode.DocumentId);
        Assert.True(documentNode.LastModifiedAt > originalTimestamp);
    }

    [Fact]
    public void DocumentNode_ChangeDocumentOnFolder_ShouldThrowException()
    {
        // Arrange
        var userId = UserID.New();
        var folderNode = new DocumentNode("Folder", userId);
        var documentId = DocumentID.New();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => folderNode.ChangeDocument(documentId));
    }

    [Fact]
    public void DocumentNode_GetPath_ShouldReturnCorrectPath()
    {
        // Arrange
        var userId = UserID.New();
        var root = new DocumentNode("Root", userId);
        var level1 = new DocumentNode("Level1", userId, root);
        var level2 = new DocumentNode("Level2", userId, level1);

        // Act & Assert
        Assert.Equal("Root", root.GetPath());
        Assert.Equal("Root/Level1", level1.GetPath());
        Assert.Equal("Root/Level1/Level2", level2.GetPath());
    }

    [Fact]
    public void DocumentNode_GetAllDescendants_ShouldReturnAllDescendants()
    {
        // Arrange
        var userId = UserID.New();
        var root = new DocumentNode("Root", userId);
        var child1 = new DocumentNode("Child1", userId, root);
        var child2 = new DocumentNode("Child2", userId, root);
        var grandchild = new DocumentNode("Grandchild", userId, child1);

        // Act
        var descendants = root.GetAllDescendants().ToList();

        // Assert
        Assert.Equal(3, descendants.Count);
        Assert.Contains(child1, descendants);
        Assert.Contains(child2, descendants);
        Assert.Contains(grandchild, descendants);
    }

    [Fact]
    public void DocumentNode_GetAncestors_ShouldReturnAllAncestors()
    {
        // Arrange
        var userId = UserID.New();
        var root = new DocumentNode("Root", userId);
        var level1 = new DocumentNode("Level1", userId, root);
        var level2 = new DocumentNode("Level2", userId, level1);

        // Act
        var ancestors = level2.GetAncestors().ToList();

        // Assert
        Assert.Equal(2, ancestors.Count);
        Assert.Contains(level1, ancestors);
        Assert.Contains(root, ancestors);
    }

    [Fact]
    public void DocumentNode_RemoveChild_ShouldRemoveFromParent()
    {
        // Arrange
        var userId = UserID.New();
        var parent = new DocumentNode("Parent", userId);
        var child = new DocumentNode("Child", userId, parent);

        // Act
        parent.RemoveChild(child);

        // Assert
        Assert.DoesNotContain(child, parent.Children);
        Assert.Null(child.Parent);
        Assert.Null(child.ParentId);
    }

    [Fact]
    public void DocumentNode_Reorder_ShouldUpdateOrderAndTimestamp()
    {
        // Arrange
        var userId = UserID.New();
        var node = new DocumentNode("Node", userId);
        var originalTimestamp = node.LastModifiedAt;
        Thread.Sleep(1);

        // Act
        node.Reorder(10);

        // Assert
        Assert.Equal(10, node.Order);
        Assert.True(node.LastModifiedAt > originalTimestamp);
    }

    [Fact]
    public void DocumentNode_Delete_ShouldRemoveFromParentAndDeleteAllChildren()
    {
        // Arrange
        var userId = UserID.New();
        var parent = new DocumentNode("Parent", userId);
        var child1 = new DocumentNode("Child1", userId, parent);
        var child2 = new DocumentNode("Child2", userId, parent);
        var grandchild = new DocumentNode("Grandchild", userId, child1);

        // Act
        child1.Delete();

        // Assert
        Assert.DoesNotContain(child1, parent.Children);
        Assert.Null(child1.Parent);
        Assert.Empty(child1.Children);
        Assert.Null(grandchild.Parent);
    }

    [Fact]
    public void DocumentNode_GetAllDocumentIds_ShouldReturnOnlyDocumentIds()
    {
        // Arrange
        var userId = UserID.New();
        var root = new DocumentNode("Root", userId);
        var folder1 = new DocumentNode("Folder1", userId, root);
        var docId1 = DocumentID.New();
        var docId2 = DocumentID.New();
        var doc1 = new DocumentNode("Doc1", docId1, userId, folder1);
        var doc2 = new DocumentNode("Doc2", docId2, userId, root);
        var folder2 = new DocumentNode("Folder2", userId, folder1);

        // Act
        var documentIds = root.GetAllDocumentIds().ToList();

        // Assert
        Assert.Equal(2, documentIds.Count);
        Assert.Contains(docId1, documentIds);
        Assert.Contains(docId2, documentIds);
    }
}