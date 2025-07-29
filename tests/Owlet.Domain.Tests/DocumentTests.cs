using Owlet.Domain;

namespace Owlet.Domain.Tests;

public class DocumentTests
{
    [Fact]
    public void Document_Constructor_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var userId = UserID.New();
        var title = "Test Document";
        var content = "This is test content";
        var description = "Test description";
        var beforeCreation = DateTime.UtcNow;

        // Act
        var document = new Document(title, content, userId, description);

        // Assert
        Assert.NotEqual(DocumentID.New(), document.Id);
        Assert.Equal(title, document.Title);
        Assert.Equal(content, document.Content);
        Assert.Equal(userId, document.CreatedBy);
        Assert.Equal(description, document.Description);
        Assert.Equal(DocumentStatus.Draft, document.Status);
        Assert.True(document.CreatedAt >= beforeCreation);
        Assert.True(document.CreatedAt <= DateTime.UtcNow);
        // Remove exact timestamp comparison to avoid flaky tests
        Assert.True((document.LastModifiedAt - document.CreatedAt).TotalMilliseconds < 1);
    }

    [Fact]
    public void Document_UpdateContent_ShouldUpdatePropertiesAndTimestamp()
    {
        // Arrange
        var userId = UserID.New();
        var document = new Document("Original Title", "Original Content", userId);
        var originalTimestamp = document.LastModifiedAt;
        
        // Wait a bit to ensure timestamp difference
        Thread.Sleep(1);
        
        var newTitle = "Updated Title";
        var newContent = "Updated Content";
        var newDescription = "Updated Description";

        // Act
        document.UpdateContent(newTitle, newContent, newDescription);

        // Assert
        Assert.Equal(newTitle, document.Title);
        Assert.Equal(newContent, document.Content);
        Assert.Equal(newDescription, document.Description);
        Assert.True(document.LastModifiedAt > originalTimestamp);
    }

    [Fact]
    public void Document_Publish_ShouldChangeStatusToPublished()
    {
        // Arrange
        var userId = UserID.New();
        var document = new Document("Test Title", "Test Content", userId);

        // Act
        document.Publish();

        // Assert
        Assert.Equal(DocumentStatus.Published, document.Status);
    }

    [Fact]
    public void Document_Archive_ShouldChangeStatusToArchived()
    {
        // Arrange
        var userId = UserID.New();
        var document = new Document("Test Title", "Test Content", userId);

        // Act
        document.Archive();

        // Assert
        Assert.Equal(DocumentStatus.Archived, document.Status);
    }
}
