using Assignment.Infrastructure;

namespace Assignment.Infrastructure.Tests;

public class TagRepositoryTests :IDisposable
{
    private KanbanContext _context;
    private ITagRepository _repository;

    public TagRepositoryTests() 
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var builder = new DbContextOptionsBuilder<KanbanContext>();
        builder.UseSqlite(connection);
        var context = new KanbanContext(builder.Options);
        context.Database.EnsureCreated();

        var tag = new Tag("Tag1");
        context.Tags.AddRange(tag, new Tag("Tag2"));
        context.Users.AddRange(new User("User1", "user1@itu.dk"), new User("User2", "user2@itu.dk"));
        context.Items.AddRange(new WorkItem("WorkItem1"), new WorkItem("WorkItem2"));
        context.SaveChanges();

        _context = context;
        _repository = new TagRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public void Create_given_Tag_returns_Created_with_Tag()
    {
        // Given
        var (response, id) = _repository.Create(new TagCreateDTO("Tag3"));

        // When
        response.Should().Be(Response.Created);

        // Then
        id.Should().Be(3);
    }

    [Fact]
    public void Create_given_existing_Tag_returns_Conflict_with_existing_Tag()
    {
        var (response, id) = _repository.Create(new TagCreateDTO("Tag1"));

        response.Should().Be(Response.Conflict);

        id.Should().Be(1);
    }

    [Fact]
    public void Delete_non_existing_tag_should_give_not_found()
    {
        // Given
        var response = _repository.Delete(4, false);
    
        // Then
        response.Should().Be(Response.NotFound);
    }

    [Fact]
    public void Delete_existing_tag_in_use_without_force_should_give_conflict()
    {
        // Given
        var response = _repository.Delete(1, false);
    
        // Then
        response.Should().Be(Response.Conflict);
    }

    [Fact]
    public void Delete_existing_tag_in_use_with_force_should_give_deleted()
    {
        // Given
        var response = _repository.Delete(1, true);
    
        // Then
        response.Should().Be(Response.Deleted);
    }

    [Fact]
    public void Read_with_existing_tag_returning_TagDTO()
    {
        // Arrange
        List<TagDTO> list = new();
        var entity = _context.Tags;
        foreach (var t in entity)
        {
            list.Add(new TagDTO(t.Id, t.Name));
        }

        // Given
        var tag = _repository.Read();
    
        // Then
        tag.Should().BeEquivalentTo(list.AsReadOnly());
    }


    [Fact]
    public void Update_non_existing_tag_should_return_notfound()
    {
        // Given
        var response = _repository.Update(new TagUpdateDTO(3, "tag3"));
    
        // Then
        response.Should().Be(Response.NotFound);
    }

    [Fact]
    public void Update_existing_tag_should_return_updated()
    {
        // Given
        var response = _repository.Update(new TagUpdateDTO(1, "newTag1"));
    
        // Then
        response.Should().Be(Response.Updated);
    }
}
