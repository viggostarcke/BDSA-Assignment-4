namespace Assignment.Infrastructure.Tests;

public class WorkItemRepositoryTests : IDisposable
{
    private KanbanContext _context;
    private IWorkItemRepository _repository;

    public WorkItemRepositoryTests()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var builder = new DbContextOptionsBuilder<KanbanContext>();
        builder.UseSqlite(connection);
        var context = new KanbanContext(builder.Options);
        context.Database.EnsureCreated();

        var tag = new Tag("Tag1");
        context.Tags.AddRange(tag, new Tag("Tag2"));
        var user = new User("User1", "user1@itu.dk");
        context.Users.AddRange(user, new User("User2", "user@itu.dk"));
        var workitem1 = new WorkItem("WorkItem1");
        workitem1.State = State.New;
        workitem1.Tags.Add(new Tag("Tag1"));
        workitem1.AssignedTo = user;
        var workitem2 = new WorkItem("WorkItem2");
        workitem2.State = State.Active;
        var workitem3 = new WorkItem("WorkItem3");
        workitem3.State = State.Resolved;
        
        context.Items.AddRange(workitem1, workitem2, workitem3);
        context.SaveChanges();

        _context = context;
        _repository = new WorkItemRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public void Create_given_WorkItem_returns_Created_with_WorkItem()
    {
        //given
        var (response, id) = _repository.Create(new WorkItemCreateDTO("newWorkItem", 5, "description", new List<string>()));

        var entity = _context.Items.FirstOrDefault(i => i.Id == id);

        //then
        id.Should().Be(5);
        response.Should().Be(Response.Created);

        entity.Created.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(5));
        entity.StateUpdated.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(5));
        
    }

    [Fact]
    public void Create_given_existing_WorkItem_returns_Conflict_with_existing_WorkItem()
    {
        var (response, id) = _repository.Create(new WorkItemCreateDTO("newWorkItem", 1, "description", new List<string>()));

        id.Should().Be(1);
        response.Should().Be(Response.Conflict);
    }

    [Fact]
    public void Delete_non_existing_WorkItem_returns_NotFound()
    {
        // Given
        var response = _repository.Delete(4);

        // Then
        response.Should().Be(Response.NotFound);
    }

    [Fact]
    public void Delete_existing_WorkItem_with_State_New_returns_Deleted()
    {
        // Given
        var response = _repository.Delete(1);

        // Then
        response.Should().Be(Response.Deleted);
    }

    [Fact]
    public void Delete_existing_WorkItem_with_State_Active_returns_BadRequest()
    {
        // Given
        var response = _repository.Delete(2);

        // Then
        response.Should().Be(Response.BadRequest);
    }

    [Fact]
    public void Delete_existing_WorkItem_with_State_Resolved_returns_Conflict()
    {
        // Given
        var response = _repository.Delete(3);

        // Then
        response.Should().Be(Response.Conflict);
    }

    [Fact]
    public void Read_should_return_TaskDetailsDTO_for_existing_task()
    {
        // Given
        //var entity = _repository.Read();
        //var expected = 

        // Then
        //entity.Should().BeEquivalentTo();
    }

    [Fact]
    public void Read_should_return_WorkItemDetailsDTO_for_existing_WorkItem()
    {
        //given
        var entity = _repository.Read();
        var expected = new List<WorkItemDTO>() {new WorkItemDTO(1, "WorkItem1", null, null, State.New), new WorkItemDTO(1, "WorkItem2", null, null, State.Active), new WorkItemDTO(1, "WorkItem3", null, null, State.Resolved)};

        //then
        entity.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ReadByState_should_return_all_WorkItem_with_Active_State()
    {
        // Given
        var entity = _repository.ReadByState(State.Active);
        var expected = new List<WorkItemDTO>() { new WorkItemDTO(1, "WorkItem2", null, null, State.Active) };

        // Then
        entity.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ReadByTag_should_return_all_WorkItem_with_given_tag()
    {
        // Given
        var entity = _repository.ReadByTag("Tag1");
        var expected = new List<WorkItemDTO>() { new WorkItemDTO(1, "WorkItem1", "User1", new List<string>() { "Tag1" }, State.New) };

        // Then
        entity.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ReadByUser_should_return_all_WorkItem_with_given_user()
    {
        // Given
        var entity = _repository.ReadByUser(1);
        var expected = new List<WorkItemDTO>() { new WorkItemDTO(1, "WorkItem1", "User1", new List<string>() { "Tag1" }, State.New) };

        // Then
        entity.Should().BeEquivalentTo(expected);

    }

    [Fact]
    public void ReadRemoved_should_return_all_WorkItem_with_State_Removed()
    {
         // Given
        var entity = _repository.ReadRemoved();
        var expected = new List<WorkItemDTO>() {};

        // Then
        entity.Should().BeEquivalentTo(expected);

    }

    [Fact]
    public void Update_existing_task_should_return_updated()
    {
        // Given
        var response = _repository.Update(new WorkItemUpdateDTO(1, "WorkItem1", null, null, new List<string>(), State.New));

        // Then
        response.Should().Be(Response.Updated);

    }

    [Fact]
    public void Update_non_existing_task_should_return_notfound()
    {
        // Given
        var response = _repository.Update(new WorkItemUpdateDTO(11, "WorkItem1", null, null, new List<string>(), State.New));

        // Then
        response.Should().Be(Response.NotFound);
    }

}
